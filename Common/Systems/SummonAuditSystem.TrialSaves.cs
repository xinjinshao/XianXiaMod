using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditTrialSaves(Action<bool,string> check)
    {
        Player original=Main.player[0];int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;
        var clients=(RemoteClient[])Netplay.Clients.Clone();
        int[] freeNpcs=Enumerable.Range(0,Main.maxNPCs).Where(i=>!Main.npc[i].active).ToArray(),freeShots=Enumerable.Range(0,Main.maxProjectiles).Where(i=>!Main.projectile[i].active).ToArray();
        NPC[] oldNpcs=freeNpcs.Select(i=>Main.npc[i]).ToArray();Projectile[] oldShots=freeShots.Select(i=>Main.projectile[i]).ToArray();
        var begin=typeof(XianXiaPlayer).GetMethod("BeginTribulation",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var update=typeof(XianXiaPlayer).GetMethod("UpdateTribulation",BindingFlags.Instance|BindingFlags.NonPublic)!;
        try {
            for(int i=0;i<Netplay.Clients.Length;i++)Netplay.Clients[i]=new RemoteClient {Id=i,Socket=new AuditOutputSocket(),State=0};
            foreach(int netMode in new[] {NetmodeID.SinglePlayer,NetmodeID.Server})
            for(int stage=3;stage<=8;stage++)foreach(int ending in Enumerable.Range(0,3))
            foreach(bool compressed in new[] {false,true})foreach(bool samePlayer in new[] {false,true}) {
                Main.netMode=netMode;Main.myPlayer=Main.maxPlayers;Main.rand=new Terraria.Utilities.UnifiedRandom(325);
                foreach(int i in freeNpcs)Main.npc[i]=new NPC {whoAmI=i};foreach(int i in freeShots)Main.projectile[i]=new Projectile {whoAmI=i};
                var actor=new Player {whoAmI=0,active=true,statLife=100};actor.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=actor;
                var state=actor.GetModPlayer<XianXiaPlayer>();state.cultivationStage=(CultivationStage)stage;state.discoveredSpiritualEnergy=true;state.ResetEffects();state.tribulationAttempts=2;state.spiritPressure=70;
                begin.Invoke(state,new object[] {(CultivationStage)stage});state.tribulationTimer=120;long session=state.TribulationSession;
                int index=Projectile.NewProjectile(actor.GetSource_FromThis(),actor.Center,Microsoft.Xna.Framework.Vector2.Zero,ModContent.ProjectileType<BossArrayFieldProjectile>(),10,0,Main.maxPlayers);
                Projectile oldField=Main.projectile[index];oldField.timeLeft=50;
                check(oldField.ModProjectile.CanDamage()==true,"native binary save fixture has a damaging active-session field before terminal transition");
                NPC heart=null;
                if(stage==5) {
                    typeof(XianXiaPlayer).GetMethod("SpawnHeartDemonEnemy",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(state,null);
                    heart=freeNpcs.Select(i=>Main.npc[i]).Single(n=>n.active);heart.target=0;int cooldown=0;
                    check(NPCLoader.CanHitPlayer(heart,actor,ref cooldown),"native binary save fixture has actual damaging heart NPC before terminal transition");
                }
                if(ending==1)state.HandleTribulationFailure();
                if(ending==2){state.tribulationTimer=1;update.Invoke(state,null);}
                state.activeSkillCooldown=99;state.wardGuardTimer=22;state.arrayDeploymentCooldown=123;
                var expected=state.CaptureSnapshot() with {Revision=0,WardTimer=0};
                var saved=new TagCompound();state.SaveData(saved);
                check(saved.GetInt("saveVersion")==3&&!saved.ContainsKey("tribulationSession")&&!saved.ContainsKey("artifactOwnerSession")&&!saved.ContainsKey("wardGuardTimer"),"native trial save omits transient source identities and guard lease");
                using var stream=new MemoryStream();TagIO.ToStream(saved,stream,compressed);stream.Position=0;TagCompound decoded=TagIO.FromStream(stream,compressed);
                check(stream.Length>0&&decoded.GetInt("cultivationStage")==stage,"native TagIO binary roundtrip preserves trial save payload in both compression modes");
                Player receiver=samePlayer?actor:new Player {whoAmI=0,active=true,statLife=100};receiver.Center=actor.Center;Main.player[0]=receiver;
                var loaded=receiver.GetModPlayer<XianXiaPlayer>();loaded.LoadData(decoded);
                check((loaded.CaptureSnapshot() with {Revision=0}).Equals(expected)&&loaded.CaptureSnapshot().IsValid(),"native binary reload preserves canonical trial resources penalties progress and cooldowns");
                check(loaded.wardGuardTimer==0&&loaded.activeSkillCooldown==99&&loaded.arrayDeploymentCooldown==123,"native binary reload drops temporary protection while retaining cooldowns");
                if(ending==0)check(loaded.TribulationSession>session&&loaded.tribulationTimer==120&&!loaded.CanRetryTribulation(),"native active binary reload rotates session and preserves remaining trial without retry");
                else if(ending==1)check(loaded.TribulationSession==0&&loaded.tribulationWeakness==10800&&loaded.tribulationAttempts==3&&loaded.TryRetryTribulation()&&loaded.TribulationSession>session,"native failed binary reload retains weakness attempts and starts a new retry session");
                else check(loaded.TribulationSession==0&&loaded.tribulationComprehension==1&&loaded.maxSpiritualEnergy==XianXiaPlayer.GetMaxSpiritualEnergy((CultivationStage)stage)+5&&!loaded.TryRetryTribulation(),"native cleared binary reload retains exactly one comprehension and rejects repeat trial");
                check(oldField.ModProjectile.CanDamage()==false,"native binary reload cannot revive a field from the previous saved incarnation");oldField.ModProjectile.AI();
                check(oldField.ModProjectile.CanDamage()==false&&oldField.timeLeft<=15,"native prior incarnation field cancels through actual AI after binary reload");
                if(heart!=null) {int cooldown=0;check(!NPCLoader.CanHitPlayer(heart,receiver,ref cooldown)&&!NPCLoader.PreAI(heart)&&!heart.active,"native prior incarnation heart remains harmless and despawns after binary reload");}
            }
        } finally {
            for(int i=0;i<freeNpcs.Length;i++)Main.npc[freeNpcs[i]]=oldNpcs[i];for(int i=0;i<freeShots.Length;i++)Main.projectile[freeShots[i]]=oldShots[i];
            Main.player[0]=original;Main.netMode=mode;Main.myPlayer=local;Main.rand=random;clients.CopyTo(Netplay.Clients,0);
        }
        check(ReferenceEquals(Main.player[0],original)&&freeNpcs.Select(i=>Main.npc[i]).SequenceEqual(oldNpcs)&&freeShots.Select(i=>Main.projectile[i]).SequenceEqual(oldShots),"native binary trial save audit restores all borrowed entity slots");
    }
}
