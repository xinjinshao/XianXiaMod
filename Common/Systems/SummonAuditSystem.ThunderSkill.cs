using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Projectiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditThunderSkill(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone();
        int mode = Main.netMode, local = Main.myPlayer;
        var before = Main.projectile.Where(p => p.active).ToHashSet();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            Player Fixture(int netMode) {
                Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { active = true, whoAmI = 0, selectedItem = 0, statLife = 100, statLifeMax2 = 100 }; Main.player[0] = actor;
                actor.Center = new Vector2(Main.maxTilesX * 8,400);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.NascentSoul; state.discoveredSpiritualEnergy = true;
                check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native Thunderfall fixture authority initialized"); state.spiritualEnergy = 100;
                actor.inventory[0].SetDefaults(ModContent.ItemType<ThunderPatternSwordCase>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken();
                return actor;
            }
            void Cast(Player actor,Vector2 aim) => ArtifactSkillTransactions.HandleRequest(actor,ArtifactSkill.ThunderVolley,0,actor.HeldItem.type,aim);
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) {
                var actor = Fixture(netMode); var state = actor.GetModPlayer<XianXiaPlayer>(); Vector2 aim = actor.Center + new Vector2(200,0);
                Cast(actor,aim); var strikes = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.Center.X).ToArray();
                check(strikes.Length == 3,"native Thunderfall creates exactly three registered projectiles");
                for (int i = 0; i < strikes.Length; i++) {
                    var p = strikes[i];
                    check(p.type == ModContent.ProjectileType<ThunderCaseStrikeProjectile>() && p.ModProjectile is ThunderCaseStrikeProjectile
                        && p.damage == Math.Max(1,(int)(actor.GetWeaponDamage(actor.HeldItem) * 1.5f)) && p.owner == 0
                        && p.DamageType == DamageClass.Melee && p.tileCollide && p.netImportant && p.timeLeft == 180
                        && Vector2.DistanceSquared(p.Center,aim + new Vector2((i - 1) * 96,-240)) < .01f && p.velocity == Vector2.UnitY * 12,
                        "native Thunderfall canonical damage class owner geometry and lifetime");
                }
                check(state.spiritualEnergy == 70 && state.activeSkillCooldown == 1200,"native Thunderfall exact cost and shared cooldown");
                state.skillRequestCooldown = 0; Cast(actor,aim);
                check(state.spiritualEnergy == 70 && Main.projectile.Count(p => p.active && !before.Contains(p)) == 3,"native Thunderfall shared cooldown refuses repeat");
                foreach (var p in strikes) {
                    var logic = (ThunderCaseStrikeProjectile)p.ModProjectile; actor.dead = true;
                    check(logic.CanDamage() == false,"native Thunderfall owner death immediately suppresses damage"); logic.AI(); actor.dead = false; logic.AI();
                    check(logic.CanDamage() == false && !p.friendly && p.timeLeft <= 6 && p.velocity == Vector2.Zero,"native Thunderfall cancellation remains sticky after revival");
                    using var bytes = new MemoryStream(); logic.SendExtraAI(new BinaryWriter(bytes));
                    check(bytes.ToArray().AsSpan().SequenceEqual(new byte[] { 1,1 }),"native Thunderfall two-byte melee and cancellation extra AI");
                    logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0,0 })));
                    check(logic.CanDamage() == false && p.DamageType == DamageClass.Melee,"native stale flags cannot revive Thunderfall or change its class");
                    for (int tick = 0; tick < 6 && p.active; tick++) p.Update(p.whoAmI);
                    check(!p.active,"native Thunderfall cancellation expires within six native projectile updates");
                }
                for (int invalid = 0; invalid < 4; invalid++) {
                    actor = Fixture(netMode); state = actor.GetModPlayer<XianXiaPlayer>(); aim = actor.Center + new Vector2(200,0);
                    if (invalid == 0) state.spiritualEnergy = 29;
                    if (invalid == 1) actor.HeldItem.GetGlobalItem<RefinedArtifact>().SetLevel(2);
                    if (invalid == 2) aim.X = float.NaN;
                    if (invalid == 3) aim.X = 1e20f;
                    int energy = state.spiritualEnergy; Cast(actor,aim);
                    check(state.spiritualEnergy == energy && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),"native Thunderfall insufficient energy dormant and malformed aim refuse without charge");
                }
                actor = Fixture(netMode); state = actor.GetModPlayer<XianXiaPlayer>(); var weapon = (ThunderPatternSwordCase)actor.HeldItem.ModItem;
                actor.altFunctionUse = 2; state.spiritualEnergy = 29; check(!weapon.CanUseItem(actor),"native Thunderfall alternative input rejects twenty-nine energy");
                state.spiritualEnergy = 30; check(weapon.CanUseItem(actor) && weapon.AltFunctionUse(actor),"native Thunderfall alternative input accepts thirty energy");
                check(!WeaponShotTransactions.FirePrepared(actor,weapon,new EntitySource_ItemUse_WithAmmo(actor,actor.HeldItem,0),actor.Center,Vector2.UnitX * 13,actor.HeldItem.shoot,54,3,netMode == NetmodeID.Server)
                    && state.spiritualEnergy == 30,"native Thunderfall alternative input cannot also fire ordinary shot");
                Cast(actor,actor.Center + new Vector2(2000,0));
                var ranged = Main.projectile.Where(p => p.active && !before.Contains(p)).OrderBy(p => p.Center.X).ToArray();
                check(ranged.Length == 3 && Vector2.DistanceSquared(ranged[1].Center + new Vector2(0,240),actor.MountedCenter) <= 800 * 800 + .1f,"native Thunderfall far aim clamps focus to eight hundred pixels");
                actor = Fixture(netMode); Cast(actor,actor.Center + new Vector2(200,0));
                var healthy = Main.projectile.First(p => p.active && !before.Contains(p)); var healthyLogic = (ThunderCaseStrikeProjectile)healthy.ModProjectile;
                using (var truncated = new BinaryReader(new MemoryStream(new byte[] { 1 }))) {
                    bool rejected = false; try { healthyLogic.ReceiveExtraAI(truncated); } catch (EndOfStreamException) { rejected = true; }
                    check(rejected && healthyLogic.CanDamage() == null,"native truncated Thunderfall extra AI does not partially mutate valid state");
                }
                Main.player[0] = new Player { active = true, whoAmI = 0 }; healthyLogic.AI();
                check(healthyLogic.CanDamage() == false && healthy.timeLeft <= 6,"native Thunderfall refuses replacement owner in reused slot");
                actor = Fixture(netMode); state = actor.GetModPlayer<XianXiaPlayer>(); aim = actor.Center + new Vector2(200,0);
                int tileX = (int)(aim.X / 16), tileY = (int)((aim.Y - 240) / 16); Tile obstruction = Main.tile[tileX,tileY];
                bool oldHas = obstruction.HasTile; ushort oldType = obstruction.TileType;
                try {
                    obstruction.HasTile = true; obstruction.TileType = TileID.Stone;
                    Cast(actor,aim);
                    check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && !Main.projectile.Any(p => p.active && !before.Contains(p)),"native Thunderfall blocked spawn refunds energy without cooldown");
                } finally { obstruction.HasTile = oldHas; obstruction.TileType = oldType; }
                actor = Fixture(netMode); state = actor.GetModPlayer<XianXiaPlayer>();
                var free = Main.projectile.Take(Main.maxProjectiles).Where(p => !p.active).ToArray();
                try {
                    foreach (var p in free.Take(free.Length - 2)) p.active = true;
                    Cast(actor,actor.Center + new Vector2(200,0));
                    check(state.spiritualEnergy == 100 && state.activeSkillCooldown == 0 && free.Count(p => !p.active) == 2,"native Thunderfall requires three allocatable slots before charging");
                } finally { foreach (var p in free) p.active = false; }
                for (int invalid = 0; invalid < 10; invalid++) {
                    actor = Fixture(netMode); Cast(actor,actor.Center + new Vector2(200,0));
                    var malformed = Main.projectile.First(p => p.active && !before.Contains(p)); var logic = (ThunderCaseStrikeProjectile)malformed.ModProjectile;
                    switch (invalid) {
                        case 0: malformed.Center = new Vector2(float.NaN,0); break;
                        case 1: malformed.Center = new Vector2(1e30f,0); break;
                        case 2: malformed.velocity = new Vector2(float.PositiveInfinity,0); break;
                        case 3: malformed.velocity = new Vector2(1e30f,0); break;
                        case 4: malformed.rotation = float.NaN; break;
                        case 5: malformed.width = 25; break;
                        case 6: malformed.height = 81; break;
                        case 7: malformed.timeLeft = 181; break;
                        case 8: malformed.timeLeft = 0; break;
                        case 9: malformed.timeLeft = -1; break;
                    }
                    check(logic.CanDamage() == false,"native malformed Thunderfall suppresses damage before AI"); logic.AI();
                    check(malformed.hide && !malformed.friendly && malformed.width == 24 && malformed.height == 80 && malformed.velocity == Vector2.Zero
                        && float.IsFinite(malformed.Center.LengthSquared()) && float.IsFinite(malformed.rotation) && malformed.timeLeft >= 0 && malformed.timeLeft <= 6,
                        "native malformed Thunderfall sanitizes position rotation dimensions and lifetime");
                    for (int tick = 0; tick < 6 && malformed.active; tick++) malformed.Update(malformed.whoAmI);
                    check(!malformed.active,"native malformed Thunderfall completes bounded native cleanup without unsafe geometry");
                }
            }
        }
        finally { Clear(); Main.player[0] = previous; Array.Copy(clients,Netplay.Clients,clients.Length); Main.netMode = mode; Main.myPlayer = local; }
        check(ReferenceEquals(Main.player[0],previous) && Main.netMode == mode && Main.myPlayer == local,"native Thunderfall fixture restores authority context");
    }
}
