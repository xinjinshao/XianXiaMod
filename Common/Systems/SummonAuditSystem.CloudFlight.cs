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
    private static void AuditBoundFriendlyFlights(Action<bool,string> check,int variant)
    {
        Player original = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone();
        var text = Main.combatText; var random = Main.rand; var before = Main.projectile.Where(p => p.active).ToHashSet(); NPC target = null;
        bool parentFlight = variant == 0, moonbone = variant == 2, thunder = variant == 3, split = variant == 7;
        DamageClass expectedClass = variant == 6 ? DamageClass.Summon : variant >= 4 ? DamageClass.Ranged : DamageClass.Melee;
        int type = split ? ModContent.ProjectileType<StarEclipseSplitBolt>() : variant == 4 ? ModContent.ProjectileType<SpiritBolt>() : variant == 5 ? ModContent.ProjectileType<SpiritBoltProjectile>() : variant == 6 ? ModContent.ProjectileType<ContractSpiritBolt>() : thunder ? ModContent.ProjectileType<ThunderSwordProjectile>() : moonbone ? ModContent.ProjectileType<MoonboneShardProjectile>() : parentFlight ? ModContent.ProjectileType<CloudpiercerSwordProjectile>() : ModContent.ProjectileType<CloudWispProjectile>();
        int lifetime = split ? 160 : thunder ? 110 : moonbone ? 80 : parentFlight ? 105 : 180, childType = ModContent.ProjectileType<CloudWispProjectile>();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Player Fixture(int netMode) {
            Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
            var actor = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
            var state = actor.GetModPlayer<XianXiaPlayer>(); state.discoveredSpiritualEnergy = true; state.cultivationStage = CultivationStage.NascentSoul;
            check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native bound friendly flight fixture imports owner session"); return actor;
        }
        Projectile Spawn(Player actor,DamageClass damageClass,Vector2 center,Vector2 velocity) {
            var parent = new Projectile { owner = 0, DamageType = damageClass, active = false };
            int index = Projectile.NewProjectile(new EntitySource_Parent(parent),center,velocity,type,60,3,0);
            check(index >= 0 && index < Main.maxProjectiles,"native bound friendly flight registered parent-source creation succeeds"); return Main.projectile[index];
        }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(274); Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient,NetmodeID.Server }) foreach (int bad in Enumerable.Range(0,12)) {
                var actor = Fixture(NetmodeID.SinglePlayer); var p = Spawn(actor,DamageClass.Melee,actor.Center,Vector2.UnitX * 4); Main.netMode = netMode;
                switch (bad) {
                    case 0: p.owner = -1; break; case 1: p.owner = Main.maxPlayers; break; case 2: Main.player[0] = null; break;
                    case 3: actor.active = false; break; case 4: actor.dead = true; break; case 5: p.Center = new Vector2(float.NaN,0); break;
                    case 6: p.velocity = new Vector2(float.MaxValue,0); break; case 7: p.rotation = float.NaN; break;
                    case 8: p.width++; break; case 9: p.timeLeft = lifetime + 1; break; case 10: p.timeLeft = 0; break; case 11: actor.velocity = new Vector2(float.NaN,0); break;
                }
                check(p.ModProjectile.CanDamage() == false,"native invalid bound friendly flight is harmless before AI"); p.ModProjectile.AI();
                if (netMode != NetmodeID.MultiplayerClient || bad >= 5 && bad <= 10)
                    check(!p.friendly && p.hide && p.timeLeft <= 6,"native invalid cloud geometry or authority owner latches bounded cancellation");
                if (parentFlight) { p.ModProjectile.OnKill(p.timeLeft); check(!Main.projectile.Any(child => child.active && !before.Contains(child) && child.type == childType),"native invalid cloud parent never releases wisp"); }
            }
            {
                var actor = Fixture(NetmodeID.SinglePlayer); var p = Spawn(actor,DamageClass.Melee,actor.Center,Vector2.UnitX * 4); var logic = p.ModProjectile;
                Main.netMode = NetmodeID.MultiplayerClient;
                try { logic.ReceiveExtraAI(new BinaryReader(new MemoryStream())); check(false,"empty cloud metadata must throw"); } catch (EndOfStreamException) { check(logic.CanDamage() == null,"empty cloud metadata leaves healthy state unchanged"); }
                actor.active = false; logic.AI(); check(logic.CanDamage() == false && p.friendly && p.timeLeft == lifetime,"client cloud waits harmlessly for missing owner without shortening life");
                actor.active = true; check(logic.CanDamage() == null,"client cloud owner arrival resumes an uncancelled projectile");
                logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 1 }))); check(logic.CanDamage() == false,"client cloud received cancellation immediately stops damage");
                logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 }))); logic.AI(); check(!p.friendly && p.hide && p.timeLeft <= 6,"client cloud stale metadata cannot undo cancellation");
                if (parentFlight) { Main.netMode = NetmodeID.SinglePlayer; logic.OnKill(0); check(!Main.projectile.Any(child => child.active && !before.Contains(child) && child.type == childType),"received parent cancellation prevents later child release"); }
            }
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (var damageClass in new[] { expectedClass }) foreach (int lifecycle in Enumerable.Range(0,6)) {
                var actor = Fixture(netMode); var state = actor.GetModPlayer<XianXiaPlayer>(); var snapshot = state.CaptureSnapshot(); var p = Spawn(actor,damageClass,actor.Center,Vector2.UnitY * 12); var logic = p.ModProjectile;
                check(logic.CanDamage() == null && p.DamageType == damageClass,"native bound friendly flight retains configured damage class with parent-source creation");
                switch (lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native bound friendly flight owner reconnects using same Player"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true }; break;
                }
                if (thunder || split) {
                    int count = Main.projectile.Count(child => child.active && !before.Contains(child)); var rng = Main.rand; Main.rand = new Terraria.Utilities.UnifiedRandom(276);
                    logic.OnHitNPC(new NPC { Center = actor.Center },default,1);
                    check(Main.projectile.Count(child => child.active && !before.Contains(child)) == count && Main.rand.Next() == new Terraria.Utilities.UnifiedRandom(276).Next(),"native old-session derived shot cannot roll or create a lightning child before cancellation AI"); Main.rand = rng;
                }
                check(logic.CanDamage() == false,"native released bound friendly flight stops damage before invalid-session AI"); logic.AI();
                using var bytes = new MemoryStream(); logic.SendExtraAI(new BinaryWriter(bytes));
                check(bytes.ToArray().AsSpan().SequenceEqual(new byte[] { (byte)(parentFlight ? 3 : 1) }) && !p.friendly && p.timeLeft <= 6,"native bound friendly flight encodes sticky cancellation and parent release guard");
                logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 })));
                check(logic.CanDamage() == false && p.DamageType == damageClass,"native old bound friendly flight metadata cannot revive cancelled damage");
                for (int tick = 0; tick < 6 && p.active; tick++) p.Update(p.whoAmI);
                check(!p.active && !Main.projectile.Any(child => child.active && !before.Contains(child) && child.type == childType),"native bound friendly flight cancels within six real updates without releasing children");
            }
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (var damageClass in new[] { expectedClass }) {
                var actor = Fixture(netMode); Vector2 corridor = Vector2.Zero; bool found = false;
                for (int offset = 2000; offset <= 6000; offset += 800) {
                    Vector2 candidate = actor.Center + new Vector2(offset,0); var bounds = new Rectangle((int)candidate.X - 12,(int)candidate.Y - 40,900,80);
                    if (bounds.Right >= Main.maxTilesX * 16 - 200 || Collision.SolidCollision(new Vector2(bounds.X,bounds.Y),bounds.Width,bounds.Height)
                        || Main.npc.Any(npc => npc != null && npc.active && npc.Hitbox.Intersects(bounds))) continue;
                    corridor = candidate; found = true; break;
                }
                check(found,"native bound friendly flight full-life fixture finds clear real corridor"); var p = Spawn(actor,damageClass,corridor,Vector2.UnitX * 4);
                for (int tick = 1; tick <= lifetime; tick++) { p.Update(p.whoAmI); check(p.active == (tick < lifetime) && p.timeLeft == lifetime - tick,"native bound friendly flight keeps full natural flight and expiry"); }
                if (parentFlight) {
                    var children = Main.projectile.Where(child => child.active && !before.Contains(child) && child.type == childType).ToArray();
                    check(children.Length == 1 && children[0].damage == 20 && children[0].owner == 0 && children[0].DamageType == DamageClass.Melee && Math.Abs(children[0].velocity.Length() - 6) < .01f,"native cloud sword natural expiry releases one third-damage melee wisp at speed six");
                    int identity = children[0].identity; p.ModProjectile.OnKill(0);
                    check(Main.projectile.Count(child => child.active && !before.Contains(child) && child.type == childType) == 1 && children[0].identity == identity,"native duplicate cloud sword kill never duplicates wisp");
                    check(children[0].ModProjectile.CanDamage() == null,"native cloud wisp remains valid after parent expires");
                }
            }
            if (moonbone) foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient,NetmodeID.Server }) {
                var actor = Fixture(NetmodeID.SinglePlayer); var p = Spawn(actor,DamageClass.Melee,actor.Center,Vector2.UnitX * 5); Main.netMode = netMode;
                p.ModProjectile.OnHitNPC(new NPC(),default,1);
                check(p.velocity == Vector2.UnitX && p.timeLeft == 24 && p.penetrate == 2,"native moonbone hit callback preserves one-fifth speed and 24-frame cap without consuming penetration itself");
                p.ModProjectile.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 1 }))); var velocity = p.velocity; int remaining = p.timeLeft;
                p.ModProjectile.OnHitNPC(new NPC(),default,1);
                check(p.velocity == velocity && p.timeLeft == remaining,"native cancelled moonbone ignores stale hit callback");
            }
            var caster = Fixture(NetmodeID.SinglePlayer);
            int npc = NPC.NewNPC(new EntitySource_Misc("CloudFlightTarget"),(int)caster.Center.X + 100,400,NPCID.BlueSlime);
            check(npc >= 0 && npc < Main.maxNPCs,"native bound friendly flight creates living vanilla contact target"); target = Main.npc[npc]; target.Center = caster.Center + new Vector2(100,0); target.lifeMax = 1000; target.damage = target.defense = 0; target.noGravity = true;
            foreach (var damageClass in new[] { expectedClass }) {
                Clear(); target.life = 1000; target.velocity = Vector2.Zero; Array.Clear(target.immune); var p = Spawn(caster,damageClass,target.Center,moonbone ? Vector2.UnitX * 4 : Vector2.Zero); p.Update(p.whoAmI);
                check(target.life < 1000 && (thunder ? p.active && p.penetrate == 2 : parentFlight || moonbone || split ? p.active && p.penetrate == 1 : !p.active) && p.DamageType == damageClass,"native friendly flight contact damages NPC and consumes one penetration");
                if (moonbone) check(Math.Abs(p.velocity.X - .8f) < .001f && p.timeLeft == 23,"native moonbone actual NPC collision applies slow and 24-frame cap before update decrement");
                if (moonbone) {
                    int health = target.life; Array.Clear(target.immune); p.Center = target.Center; p.Update(p.whoAmI);
                    check(target.life < health && !p.active,"native moonbone second NPC collision consumes final penetration and removes shard");
                }
            }
        } finally { Clear(); if (target != null) { target.damage = 0; target.active = false; } Main.player[0] = original; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; clients.CopyTo(Netplay.Clients,0); }
        check(ReferenceEquals(Main.player[0],original) && Main.combatText == text && Main.rand == random,"native bound friendly flight fixture restores authority rendering and random context");
    }
}
