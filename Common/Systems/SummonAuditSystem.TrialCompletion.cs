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
    private static void AuditTrialCompletion(Action<bool,string> check)
    {
        Player original=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();
        int[] freeNpcs=Enumerable.Range(0,Main.maxNPCs).Where(i=>!Main.npc[i].active).ToArray(),freeShots=Enumerable.Range(0,Main.maxProjectiles).Where(i=>!Main.projectile[i].active).ToArray();
        NPC[] oldNpcs=freeNpcs.Select(i=>Main.npc[i]).ToArray();Projectile[] oldShots=freeShots.Select(i=>Main.projectile[i]).ToArray();
        var begin=typeof(XianXiaPlayer).GetMethod("BeginTribulation",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var update=typeof(XianXiaPlayer).GetMethod("UpdateTribulation",BindingFlags.Instance|BindingFlags.NonPublic)!;
        void Clear() {foreach(int i in freeNpcs)Main.npc[i]=new NPC {whoAmI=i};foreach(int i in freeShots)Main.projectile[i]=new Projectile {whoAmI=i};}
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=new AuditOutputSocket(),State=0};
            foreach(int netMode in new[] {NetmodeID.SinglePlayer,NetmodeID.Server})
            foreach(var stage in new[] {CultivationStage.Foundation,CultivationStage.NascentSoul,CultivationStage.SpiritSevering,CultivationStage.DaoSevering}) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(323);Clear();
                var actor=new Player {whoAmI=0,active=true,statLife=100};actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=actor;
                var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=stage;state.ResetEffects();state.spiritualEnergy=0;state.spiritPressure=80;state.tribulationAttempts=2;
                begin.Invoke(state,new object[] {stage});long session=state.TribulationSession;
                int intensity=state.tribulationIntensity;
                check(state.tribulationTimer>1&&session>0&&state.tribulationStage==(int)stage,"native actual trial begin selects stage and positive authority session");
                int interval=state.tribulationKind switch {
                    XianXiaPlayer.TribulationKind.HeartDemon=>Math.Max(90,350-intensity*25),
                    XianXiaPlayer.TribulationKind.HeavenTablet=>Math.Max(60,200-intensity*15),
                    XianXiaPlayer.TribulationKind.DaoSevering=>Math.Max(70,280-intensity*20),
                    _=>Math.Max(38,110-intensity*8)};
                state.tribulationTimer=interval+1;update.Invoke(state,null);
                check(state.tribulationTimer==interval&&state.TribulationSession==session&&(freeShots.Any(i=>Main.projectile[i].active)||freeNpcs.Any(i=>Main.npc[i].active)),"native positive trial interval still creates real hazards before terminal tick");
                foreach(int i in freeShots.Where(i=>Main.projectile[i].active&&Main.projectile[i].ModProjectile is TribulationWarningLineProjectile).ToArray()) {
                    Main.projectile[i].timeLeft=0;Main.projectile[i].Kill();
                }
                foreach(int i in freeShots.Where(i=>Main.projectile[i].active)) {
                    var shot=Main.projectile[i];if(shot.ModProjectile is BossArrayFieldProjectile)shot.timeLeft=50;
                    check(shot.ModProjectile.CanDamage()==true,"native earlier trial hazards have a genuinely damaging positive control before completion");
                }
                foreach(int i in freeNpcs.Where(i=>Main.npc[i].active)) {
                    Main.npc[i].target=0;int cooldown=0;check(NPCLoader.CanHitPlayer(Main.npc[i],actor,ref cooldown),"native earlier heart NPC remains damaging before trial completion");
                }
                state.tribulationTimer=1;
                var npcBefore=freeNpcs.Select(i=>(Main.npc[i].active,Main.npc[i].type,Main.npc[i].life,Main.npc[i].position)).ToArray();
                var shotBefore=freeShots.Select(i=>(Main.projectile[i].active,Main.projectile[i].type,Main.projectile[i].timeLeft)).ToArray();
                update.Invoke(state,null);
                check(npcBefore.SequenceEqual(freeNpcs.Select(i=>(Main.npc[i].active,Main.npc[i].type,Main.npc[i].life,Main.npc[i].position)))&&shotBefore.SequenceEqual(freeShots.Select(i=>(Main.projectile[i].active,Main.projectile[i].type,Main.projectile[i].timeLeft))),"native terminal trial tick completes without scheduling a zero-session hazard");
                check(state.tribulationTimer==0&&state.TribulationSession==0&&state.tribulationStage==0&&state.tribulationIntensity==0&&state.tribulationKind==XianXiaPlayer.TribulationKind.None&&state.tribulationAttempts==0,"native trial success clears active state and retry attempts");
                check(state.tribulationComprehension==1&&state.maxSpiritualEnergy==XianXiaPlayer.GetMaxSpiritualEnergy(stage)+5&&state.spiritualEnergy==20+intensity*8&&state.spiritPressure==80-(20+intensity*2)&&!state.CanRetryTribulation(),"native trial success grants exactly one comprehension and intended resource recovery");
                var completed=state.CaptureSnapshot();for(int tick=0;tick<3;tick++)update.Invoke(state,null);
                state.HandleTribulationFailure();
                check(state.CaptureSnapshot().Equals(completed)&&state.tribulationComprehension==1,"native completed trial cannot repeat rewards or acquire failure penalty on later callbacks");
                foreach(int i in freeNpcs.Where(i=>Main.npc[i].active))check(!NPCLoader.PreAI(Main.npc[i])&&!Main.npc[i].active,"native successful heart trial invalidates its earlier spawned heart NPC");
                foreach(int i in freeShots.Where(i=>Main.projectile[i].active)) {
                    var shot=Main.projectile[i];check(shot.ModProjectile.CanDamage()==false,"native successful trial immediately disables its previously released hazard");
                    shot.ModProjectile.AI();check(shot.ModProjectile.CanDamage()==false&&shot.timeLeft<=15,"native successful trial hazard enters bounded harmless cancellation");
                }
                Clear();
                var failed=new Player {whoAmI=0,active=true,statLife=100};failed.Center=actor.Center;Main.player[0]=failed;var failure=failed.GetModPlayer<XianXiaPlayer>();failure.cultivationStage=stage;failure.ResetEffects();failure.spiritPressure=80;
                begin.Invoke(failure,new object[] {stage});int cap=failure.maxSpiritualEnergy;bool sound=true,dust=true;var reason=PlayerDeathReason.ByCustomReason("XianXia trial audit");
                check(failure.PreKill(1000,1,false,ref sound,ref dust,ref reason),"native trial failure hook preserves actual player death decision");
                check(failure.tribulationTimer==0&&failure.TribulationSession==0&&failure.tribulationKind==XianXiaPlayer.TribulationKind.None&&failure.tribulationAttempts==1&&failure.tribulationComprehension==0&&failure.spiritualEnergy==cap/4&&failure.spiritPressure==70&&failure.tribulationWeakness==10800&&failed.HasBuff(BuffID.Weak)&&failed.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>()),"native fatal trial hook applies intended failure recovery and penalties without success credit");
                var ended=failure.CaptureSnapshot();failure.PreKill(1000,1,false,ref sound,ref dust,ref reason);failure.HandleTribulationFailure();
                check(failure.CaptureSnapshot().Equals(ended)&&failure.tribulationComprehension==0,"native repeated death and failure callbacks cannot multiply recovery or attempts");
            }
        } finally {
            for(int i=0;i<freeNpcs.Length;i++)Main.npc[freeNpcs[i]]=oldNpcs[i];for(int i=0;i<freeShots.Length;i++)Main.projectile[freeShots[i]]=oldShots[i];
            Main.player[0]=original;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);
        }
        check(ReferenceEquals(Main.player[0],original)&&freeNpcs.Select(i=>Main.npc[i]).SequenceEqual(oldNpcs)&&freeShots.Select(i=>Main.projectile[i]).SequenceEqual(oldShots),"native trial terminal audit restores all borrowed entity slots");
    }
}
