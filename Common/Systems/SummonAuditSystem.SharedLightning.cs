using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditSharedLightning(Action<bool,string> check)
    {
        Player original = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone();
        var text = Main.combatText; var random = Main.rand; var before = Main.projectile.Where(p => p.active).ToHashSet(); NPC target = null;
        int type = ModContent.ProjectileType<MinorThunderboltProjectile>();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Player Fixture(int netMode) {
            Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
            var actor = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
            var state = actor.GetModPlayer<XianXiaPlayer>(); state.discoveredSpiritualEnergy = true; state.cultivationStage = CultivationStage.NascentSoul;
            check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native shared lightning fixture imports owner session"); return actor;
        }
        Projectile Spawn(Player actor,DamageClass damageClass,Vector2 center,Vector2 velocity) {
            var parent = new Projectile { owner = 0, DamageType = damageClass, active = false };
            int index = Projectile.NewProjectile(new EntitySource_Parent(parent),center,velocity,type,60,3,0);
            check(index >= 0 && index < Main.maxProjectiles,"native shared lightning registered parent-source creation succeeds"); return Main.projectile[index];
        }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(272); Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (var damageClass in new[] { DamageClass.Melee,DamageClass.Magic }) foreach (int lifecycle in Enumerable.Range(0,6)) {
                var actor = Fixture(netMode); var state = actor.GetModPlayer<XianXiaPlayer>(); var snapshot = state.CaptureSnapshot(); var p = Spawn(actor,damageClass,actor.Center,Vector2.UnitY * 12); var logic = (MinorThunderboltProjectile)p.ModProjectile;
                check(logic.CanDamage() == null && p.DamageType == damageClass,"native shared lightning keeps parent class without requiring active parent");
                switch (lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native shared lightning owner reconnects using same Player"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true }; break;
                }
                check(logic.CanDamage() == false,"native released shared lightning stops damage before invalid-session AI"); logic.AI();
                using var bytes = new MemoryStream(); logic.SendExtraAI(new BinaryWriter(bytes));
                check(bytes.ToArray().AsSpan().SequenceEqual(new byte[] { (byte)(damageClass == DamageClass.Melee ? 1 : 0),1 }) && !p.friendly && p.timeLeft <= 6,"native shared lightning encodes class plus sticky cancellation");
                logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { (byte)(damageClass == DamageClass.Melee ? 1 : 0),0 })));
                check(logic.CanDamage() == false && p.DamageType == damageClass,"native old shared lightning metadata cannot revive cancelled damage");
                for (int tick = 0; tick < 6 && p.active; tick++) p.Update(p.whoAmI);
                check(!p.active,"native shared lightning cancels within six real updates");
            }
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (var damageClass in new[] { DamageClass.Melee,DamageClass.Magic }) {
                var actor = Fixture(netMode); Vector2 corridor = Vector2.Zero; bool found = false;
                for (int offset = 2000; offset <= 6000; offset += 800) {
                    Vector2 candidate = actor.Center + new Vector2(offset,0); var bounds = new Rectangle((int)candidate.X - 12,(int)candidate.Y - 40,744,80);
                    if (bounds.Right >= Main.maxTilesX * 16 - 200 || Collision.SolidCollision(new Vector2(bounds.X,bounds.Y),bounds.Width,bounds.Height)
                        || Main.npc.Any(npc => npc != null && npc.active && npc.Hitbox.Intersects(bounds))) continue;
                    corridor = candidate; found = true; break;
                }
                check(found,"native shared lightning full-life fixture finds clear real corridor"); var p = Spawn(actor,damageClass,corridor,Vector2.UnitX * 4);
                for (int tick = 1; tick <= 180; tick++) { p.Update(p.whoAmI); check(p.active == (tick < 180) && p.timeLeft == 180 - tick,"native shared lightning keeps full 180-frame natural flight and expiry"); }
            }
            var caster = Fixture(NetmodeID.SinglePlayer);
            int npc = NPC.NewNPC(new EntitySource_Misc("SharedLightningTarget"),(int)caster.Center.X + 100,400,NPCID.BlueSlime);
            check(npc >= 0 && npc < Main.maxNPCs,"native shared lightning creates living vanilla contact target"); target = Main.npc[npc]; target.Center = caster.Center + new Vector2(100,0); target.lifeMax = 1000; target.damage = target.defense = 0; target.noGravity = true;
            foreach (var damageClass in new[] { DamageClass.Melee,DamageClass.Magic }) {
                Clear(); target.life = 1000; target.velocity = Vector2.Zero; Array.Clear(target.immune); var p = Spawn(caster,damageClass,target.Center,Vector2.Zero); p.Update(p.whoAmI);
                check(target.life < 1000 && !p.active && p.DamageType == damageClass,"native melee and magic lightning contact updates damage NPC and consume sole penetration");
            }
        } finally { Clear(); if (target != null) { target.damage = 0; target.active = false; } Main.player[0] = original; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; clients.CopyTo(Netplay.Clients,0); }
        check(ReferenceEquals(Main.player[0],original) && Main.combatText == text && Main.rand == random,"native shared lightning fixture restores authority rendering and random context");
    }
}
