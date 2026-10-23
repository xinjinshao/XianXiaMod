using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditTrialRetries(Action<bool,string> check)
    {
        Player original=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();
        int[] freeNpcs=Enumerable.Range(0,Main.maxNPCs).Where(i=>!Main.npc[i].active).ToArray(),freeShots=Enumerable.Range(0,Main.maxProjectiles).Where(i=>!Main.projectile[i].active).ToArray();
        NPC[] oldNpcs=freeNpcs.Select(i=>Main.npc[i]).ToArray();Projectile[] oldShots=freeShots.Select(i=>Main.projectile[i]).ToArray();
        var update=typeof(XianXiaPlayer).GetMethod("UpdateTribulation",BindingFlags.Instance|BindingFlags.NonPublic)!;
        void Clear() {foreach(int i in freeNpcs)Main.npc[i]=new NPC {whoAmI=i};foreach(int i in freeShots)Main.projectile[i]=new Projectile {whoAmI=i};}
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=new AuditOutputSocket(),State=0};
            int[][] durations={new[]{1560,1404,1248,1092},new[]{1800,1620,1440,1260},new[]{2520,2268,2016,1764},new[]{3300,2970,2640,2310},new[]{4440,3996,3552,3108},new[]{4680,4212,3744,3276}};
            foreach(int netMode in new[] {NetmodeID.SinglePlayer,NetmodeID.Server})
            for(int stage=3;stage<=8;stage++) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(324);Clear();
                var actor=new Player {whoAmI=0,active=true,statLife=100};actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=actor;
                var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=(CultivationStage)stage;state.discoveredSpiritualEnergy=true;state.ResetEffects();
                long lastSession=0;
                check(state.TryRetryTribulation(),"native initial retry starts through actual player API");
                for(int attempt=0;attempt<=10;attempt++) {
                    check(state.tribulationTimer==durations[stage-3][Math.Min(attempt,3)]&&state.tribulationAttempts==attempt,"native retry uses actual API and expected insight duration table through ten failures");
                    long session=state.TribulationSession;
                    check(session>lastSession&&state.tribulationStage==stage&&state.tribulationIntensity==stage-1,"native each actual retry rotates authority session while preserving current stage");lastSession=session;
                    var running=state.CaptureSnapshot();check(!state.TryRetryTribulation()&&state.CaptureSnapshot().Equals(running),"native active trial rejects retry without changing resources timer or session");
                    int intensity=state.tribulationIntensity;
                    int interval=state.tribulationKind switch {
                        XianXiaPlayer.TribulationKind.HeartDemon=>Math.Max(90,350-intensity*25),
                        XianXiaPlayer.TribulationKind.HeavenTablet=>Math.Max(60,200-intensity*15),
                        XianXiaPlayer.TribulationKind.DaoSevering=>Math.Max(70,280-intensity*20),
                        _=>Math.Max(38,110-intensity*8)};
                    state.tribulationTimer=interval+1;update.Invoke(state,null);
                    foreach(int i in freeShots.Where(i=>Main.projectile[i].active&&Main.projectile[i].ModProjectile is TribulationWarningLineProjectile).ToArray()) {Main.projectile[i].timeLeft=0;Main.projectile[i].Kill();}
                    var shots=freeShots.Select(i=>Main.projectile[i]).Where(p=>p.active).ToArray();var npcs=freeNpcs.Select(i=>Main.npc[i]).Where(n=>n.active).ToArray();
                    check(shots.Length+npcs.Length>0,"native retry fixture produces actual hazards before fatal hook");
                    foreach(var shot in shots) {if(shot.ModProjectile is BossArrayFieldProjectile)shot.timeLeft=50;check(shot.ModProjectile.CanDamage()==true,"native retry hazard is damaging before failure");}
                    foreach(var npc in npcs) {npc.target=0;int cooldown=0;check(NPCLoader.CanHitPlayer(npc,actor,ref cooldown),"native retry heart contact is damaging before failure");}
                    bool sound=true,dust=true;var reason=PlayerDeathReason.ByCustomReason("XianXia retry audit");state.PreKill(1000,1,false,ref sound,ref dust,ref reason);
                    var failed=state.CaptureSnapshot();actor.dead=true;state.UpdateDead();state.UpdateDead();
                    check(state.CaptureSnapshot().Equals(failed)&&state.tribulationAttempts==Math.Min(10,attempt+1)&&state.tribulationComprehension==0&&state.tribulationWeakness==10800,"native death updates preserve a single capped failure and no comprehension");
                    check(!state.TryRetryTribulation()&&state.CaptureSnapshot().Equals(failed),"native dead player cannot retry or reset penalties");
                    actor.dead=false;Main.netMode=NetmodeID.MultiplayerClient;
                    check(!state.TryRetryTribulation()&&state.CaptureSnapshot().Equals(failed),"native client cannot authoritatively start retry");Main.netMode=netMode;
                    foreach(var shot in shots)check(shot.ModProjectile.CanDamage()==false,"native failed trial hazards become harmless before source AI cleanup");
                    foreach(var npc in npcs) {int cooldown=0;check(!NPCLoader.CanHitPlayer(npc,actor,ref cooldown),"native failed heart contact blocks damage before next AI");}
                    // Begin a fresh session before old hazards receive their cancellation AI.
                    check(state.TryRetryTribulation(),"native living failure can immediately start actual retry with weakness preserved");
                    foreach(var shot in shots) {check(shot.ModProjectile.CanDamage()==false,"native new retry cannot reactivate prior attempt projectile source");shot.ModProjectile.AI();check(shot.timeLeft<=15&&shot.ModProjectile.CanDamage()==false,"native failed old shot enters harmless bounded fade after immediate retry");}
                    foreach(var npc in npcs)check(!NPCLoader.PreAI(npc)&&!npc.active,"native immediate retry does not inherit prior heart NPC");
                    check(state.tribulationWeakness==10800&&state.TribulationSession>session,"native actual retry preserves failure weakness and replaces retired session");
                    // Continue this actual retry in the next loop; only clear the old entity fixtures.
                    Clear();
                }
            }
        } finally {
            for(int i=0;i<freeNpcs.Length;i++)Main.npc[freeNpcs[i]]=oldNpcs[i];for(int i=0;i<freeShots.Length;i++)Main.projectile[freeShots[i]]=oldShots[i];
            Main.player[0]=original;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);
        }
        check(ReferenceEquals(Main.player[0],original)&&freeNpcs.Select(i=>Main.npc[i]).SequenceEqual(oldNpcs)&&freeShots.Select(i=>Main.projectile[i]).SequenceEqual(oldShots),"native retry audit restores all borrowed entity slots");
    }
}
