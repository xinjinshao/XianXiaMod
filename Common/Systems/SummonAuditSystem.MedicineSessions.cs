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
    private static void AuditMedicineSessions(Action<bool,string> check)
    {
        Player original = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone();
        var text = Main.combatText; var random = Main.rand; var before = Main.projectile.Where(p => p.active).ToHashSet(); NPC target = null;
        int boltType = ModContent.ProjectileType<MedicineSpiritBolt>(), fieldType = ModContent.ProjectileType<MedicineCauldronField>();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Player Fixture(int netMode) {
            Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
            var actor = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
            var state = actor.GetModPlayer<XianXiaPlayer>(); state.discoveredSpiritualEnergy = true; state.cultivationStage = CultivationStage.NascentSoul;
            check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native medicine bolt session fixture imports valid owner"); return actor;
        }
        Projectile Spawn(Player actor,Vector2 center,Vector2 velocity) {
            int index = Projectile.NewProjectile(new EntitySource_Misc("MedicineSpiritSessionAudit"),center,velocity,boltType,60,3,0);
            check(index >= 0 && index < Main.maxProjectiles,"native registered medicine spirit creation succeeds"); return Main.projectile[index];
        }
        try {
            Main.rand = new Terraria.Utilities.UnifiedRandom(271); Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (int lifecycle in Enumerable.Range(0,6)) {
                var actor = Fixture(netMode); var state = actor.GetModPlayer<XianXiaPlayer>(); var snapshot = state.CaptureSnapshot(); Projectile p = Spawn(actor,actor.Center,Vector2.UnitX * 10); var logic = (MedicineSpiritBolt)p.ModProjectile;
                check(logic.CanDamage() == null && p.DamageType == DamageClass.Magic && p.tileCollide && p.timeLeft == 90,"native fresh medicine spirit retains class collision and lifetime");
                switch (lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native released medicine spirit owner reconnects on same Player"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true }; break;
                }
                check(logic.CanDamage() == false,"native old medicine spirit owner session is harmless before AI"); logic.AI();
                using var bytes = new MemoryStream(); logic.SendExtraAI(new BinaryWriter(bytes));
                check(bytes.ToArray().AsSpan().SequenceEqual(new byte[] { 1 }) && !p.friendly && p.timeLeft <= 6 && p.velocity == Vector2.Zero,"native medicine cancellation serializes one byte and stops damage/movement");
                logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 })));
                check(logic.CanDamage() == false && p.DamageType == DamageClass.Magic,"native stale medicine flags cannot restore cancelled damage or change class");
                for (int tick = 0; tick < 6 && p.active; tick++) p.Update(p.whoAmI);
                check(!p.active,"native stale medicine spirit exits within six actual projectile updates");
            }
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                var actor = Fixture(netMode); Vector2 corridor = Vector2.Zero; bool found = false;
                for (int offset = 2000; offset <= 6000; offset += 400) {
                    Vector2 candidate = actor.Center + new Vector2(offset,0); var bounds = new Rectangle((int)candidate.X - 8,(int)candidate.Y - 8,384,16);
                    if (bounds.Right >= Main.maxTilesX * 16 - 200 || Collision.SolidCollision(new Vector2(bounds.X,bounds.Y),bounds.Width,bounds.Height)
                        || Main.npc.Any(npc => npc != null && npc.active && npc.Hitbox.Intersects(bounds))) continue;
                    corridor = candidate; found = true; break;
                }
                check(found,"native medicine expiry uses a clear real terrain corridor without NPC overlap");
                Projectile p = Spawn(actor,corridor,Vector2.UnitX * 4);
                for (int tick = 1; tick <= 90; tick++) {
                    p.Update(p.whoAmI);
                    check(p.active == (tick < 90) && p.timeLeft == 90 - tick,"native healthy medicine spirit ninety-frame expiry: mode=" + netMode + "; tick=" + tick + "; active=" + p.active + "; life=" + p.timeLeft + "; center=" + p.Center + "; owner=" + p.owner);
                }
            }
            var caster = Fixture(NetmodeID.SinglePlayer);
            int npc = NPC.NewNPC(new EntitySource_Misc("MedicineSpiritTarget"),(int)caster.Center.X + 100,400,NPCID.BlueSlime);
            check(npc >= 0 && npc < Main.maxNPCs,"native medicine spirit fixture creates living vanilla target"); target = Main.npc[npc]; target.Center = caster.Center + new Vector2(100,0); target.life = target.lifeMax = 1000; target.defense = target.damage = 0; target.noGravity = true;
            Projectile hit = Spawn(caster,target.Center,Vector2.Zero); Array.Clear(target.immune); target.velocity = Vector2.Zero; int life = target.life; hit.Update(hit.whoAmI);
            check(target.life < life && !hit.active,"native medicine collision update damages NPC and consumes sole penetration: life=" + target.life + "; active=" + hit.active + "; penetration=" + hit.penetrate);

            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                caster = Fixture(netMode);
                int index = Projectile.NewProjectile(new EntitySource_Misc("MedicineParentExpiryAudit"),caster.Center,Vector2.Zero,fieldType,60,3,0);
                check(index >= 0 && index < Main.maxProjectiles,"native medicine parent creates for independent bolt lifetime"); var parent = Main.projectile[index]; parent.ai[0] = 59; parent.ModProjectile.AI();
                var children = Main.projectile.Where(p => p.active && !before.Contains(p) && p.type == boltType).ToArray(); parent.Kill();
                check(children.Length == 2 && children.All(p => p.active && p.ModProjectile.CanDamage() == null),"native emitted medicine spirits remain valid after ordinary parent removal");
            }
        } finally { Clear(); if (target != null) { target.damage = 0; target.active = false; } Main.player[0] = original; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; clients.CopyTo(Netplay.Clients,0); }
        check(ReferenceEquals(Main.player[0],original) && Main.combatText == text && Main.rand == random,"native medicine session audit restores player rendering random and authority context");
    }
}
