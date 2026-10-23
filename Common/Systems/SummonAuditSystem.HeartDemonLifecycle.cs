using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditHeartDemonLifecycle(Action<bool,string> check)
    {
        int slot=Enumerable.Range(0,Main.maxNPCs).First(i=>!Main.npc[i].active);NPC original=Main.npc[slot];Player[] players=Main.player.Take(2).ToArray();
        var combatText=Main.combatText;
        int mode=Main.netMode,local=Main.myPlayer;var random=Main.rand;var shots=Main.projectile.Where(p=>p.active).ToHashSet();var items=Main.item.Where(i=>i.active).ToHashSet();int[] kills=(int[])NPC.killCount.Clone();
        try {
            Main.combatText=Enumerable.Range(0,100).Select(_=>new CombatText {active=true}).ToArray();
            foreach(int netMode in new[] {NetmodeID.SinglePlayer,NetmodeID.Server})
            foreach(int stage in new[] {3,5})
            foreach(int invalid in Enumerable.Range(0,7)) {
            Main.netMode=netMode;Main.myPlayer=0;Main.rand=new Terraria.Utilities.UnifiedRandom(321);
            var caster=new Player {whoAmI=0,active=true,statLife=100};caster.Center=new Microsoft.Xna.Framework.Vector2(Main.maxTilesX*8,400);Main.player[0]=caster;
            var observer=new Player {whoAmI=1,active=true,statLife=100};observer.Center=caster.Center;Main.player[1]=observer;
            var state=caster.GetModPlayer<XianXiaPlayer>();state.tribulationKind=XianXiaPlayer.TribulationKind.HeartDemon;state.tribulationTimer=120;state.tribulationStage=stage;
            Main.npc[slot]=new NPC {whoAmI=slot};typeof(XianXiaPlayer).GetMethod("SpawnHeartDemonEnemy",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(state,null);NPC npc=Main.npc[slot];npc.target=1;
            int shot=Projectile.NewProjectile(npc.GetSource_FromAI(),npc.Center,new Microsoft.Xna.Framework.Vector2(4,0),ModContent.ProjectileType<EnemySpiritBoltProjectile>(),10,0,Main.maxPlayers);
            int cooldown=0;check(NPCLoader.CanHitPlayer(npc,observer,ref cooldown)&&Main.projectile[shot].ModProjectile.CanDamage()==true,"native live heart demon and released bolt retain valid positive damage control");
            switch(invalid) {
                case 0: state.tribulationTimer=0; break;
                case 1: caster.dead=true; break;
                case 2: caster.active=false; break;
                case 3: state.ResetNetworkSession(); break;
                case 4:
                    var replacement=new Player {whoAmI=0,active=true,statLife=100};replacement.Center=caster.Center;Main.player[0]=replacement;
                    var next=replacement.GetModPlayer<XianXiaPlayer>();next.tribulationKind=XianXiaPlayer.TribulationKind.HeartDemon;next.tribulationTimer=120;break;
                case 5: caster.Center=npc.Center+new Microsoft.Xna.Framework.Vector2(4001,0);break;
                case 6: state.tribulationKind=XianXiaPlayer.TribulationKind.None;break;
            }
            check(!NPCLoader.CanHitPlayer(npc,observer,ref cooldown),"native ended heart trial blocks remnant contact before next NPC AI");
            check(Main.projectile[shot].ModProjectile.CanDamage()==false,"native ended heart trial blocks already released NPC bolt before cleanup");
            npc.StrikeNPC(new NPC.HitInfo {Damage=npc.life+100,HitDirection=1},noPlayerInteraction:true);
            check(!npc.active&&NPC.killCount.SequenceEqual(kills)&&Main.item.Where(i=>i.active).ToHashSet().SetEquals(items),"native forced fatal strike on invalid heart remnant produces no loot or kill progress");
            npc.active=true;npc.life=1;
            check(!NPCLoader.PreAI(npc)&&!npc.active&&npc.damage==0,"native cancelled heart remnant cannot revive on forced reactivation");
            Main.projectile[shot].ModProjectile.AI();
            check(Main.projectile[shot].timeLeft<=EnemySpiritBoltProjectile.CancellationFade&&Main.projectile[shot].velocity==Microsoft.Xna.Framework.Vector2.Zero&&Main.projectile[shot].ModProjectile.CanDamage()==false,"native orphaned heart bolt enters harmless six-tick fade through actual projectile AI");
            Main.projectile[shot].active=false;
            }
        } finally {
            foreach(var shot in Main.projectile.Where(p=>p.active&&!shots.Contains(p))){shot.damage=0;shot.active=false;}
            foreach(var item in Main.item.Where(i=>i.active&&!items.Contains(i)))item.active=false;
            Main.combatText=combatText;Main.npc[slot]=original;for(int i=0;i<2;i++)Main.player[i]=players[i];Main.netMode=mode;Main.myPlayer=local;Main.rand=random;kills.CopyTo(NPC.killCount,0);
        }
    }
}
