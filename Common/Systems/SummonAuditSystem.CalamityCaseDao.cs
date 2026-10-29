using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditCalamityCaseDao(Action<bool,string> check)
    {
        var previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode, local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        int x = Main.maxTilesX / 2, y = 12; Tile tile = Main.tile[x,y]; bool has = tile.HasTile; ushort oldType = tile.TileType;
        bool oldHard = Main.hardMode, oldPlant = NPC.downedPlantBoss, oldGolem = NPC.downedGolemBoss, oldMoon = NPC.downedMoonlord;
        var priorShots = Main.projectile.Where(p => p.active).ToHashSet();
        try {
            Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = NPC.downedMoonlord = true;
            var output = new AuditOutputSocket(true);
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            tile.HasTile = true; tile.TileType = (ushort)ModContent.TileType<DaoSeveringAltarTile>();
            void World(int route,bool core) {
                world.LoadWorldData(new TagCompound { ["chosenRoute"] = route });
                if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                typeof(DownedBossSystem).GetProperty(nameof(DownedBossSystem.SectReputation))!.SetValue(null,24);
            }
            foreach (int authority in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (int route in new[] { 1,2,3 }) {
                Main.netMode = authority; Main.myPlayer = authority == NetmodeID.Server ? Main.maxPlayers : 0; World(route,true);
                var actor = new Player { active = true, whoAmI = 0, selectedItem = 0 }; Main.player[0] = actor; actor.Center = new Vector2(x * 16 + 8,y * 16 + 8);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.discoveredSpiritualEnergy = true; state.TryInitializeNetwork(state.CaptureSnapshot());
                actor.inventory[0] = new Item(ModContent.ItemType<DaoTransformationSeal>(),2);
                actor.inventory[1] = new Item(ModContent.ItemType<StarCalamityMechanismCase>());
                actor.inventory[2] = new Item(ModContent.ItemType<LowGradeSpiritStone>(),38);
                var item = actor.inventory[1]; item.Prefix(PrefixID.Demonic); int prefix = item.prefix;
                var growth = item.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); item.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
                void Submit(byte before) => DaoArtifactTransactions.Handle(actor,0,actor.inventory[0].type,1,item.type,item.prefix,1,3,true,before,(byte)route);
                Submit(0);
                check(RefinedArtifact.GetDaoRoute(item) == (DownedBossSystem.EndgameRoute)route && actor.inventory[0].stack == 1 && actor.inventory[2].stack == 2,
                    "native star calamity case Dao consumes one seal and thirty-six stones for each actual route");
                actor.GetModPlayer<InscriptionPlayer>().Initialize(); Submit((byte)route);
                check(actor.inventory[0].stack == 1 && actor.inventory[2].stack == 2 && ReferenceEquals(item,actor.inventory[1]),
                    "native repeated star calamity case Dao never repeats cost or replaces equipment");
                foreach (Item copy in new[] { item.Clone(),ItemIO.Load(ItemIO.Save(item)) })
                    check(copy.prefix == prefix && RefinedArtifact.GetDaoRoute(copy) == (DownedBossSystem.EndgameRoute)route
                        && RefinedArtifact.GetLevel(copy) == 3 && RefinedArtifact.IsAwakened(copy) && InscribedEquipment.GetKind(copy) == InscriptionKind.Greenwood,
                        "native star calamity case full save and clone keep Dao awakening refinement inscription and prefix");
                var weapon = (StarCalamityMechanismCase)item.ModItem;
                actor.selectedItem = 1; state.spiritualEnergy = 100; state.activeSkillCooldown = 1200;
                for (int missing = 0; missing < 5; missing++) {
                    Main.hardMode = missing != 0; NPC.downedPlantBoss = missing != 1; NPC.downedGolemBoss = missing != 2; NPC.downedMoonlord = missing != 3;
                    state.cultivationStage = missing == 4 ? CultivationStage.NascentSoul : CultivationStage.DaoSevering;
                    bool rejected = !Terraria.ModLoader.CombinedHooks.CanUseItem(actor,item);
                    if (authority == NetmodeID.Server) {
                        state.WeaponShotCooldown = 0;
                        WeaponShotTransactions.HandleRequest(actor,1,item.type,actor.Center + Vector2.UnitX * 100);
                        rejected &= state.WeaponShotCooldown > 0;
                    }
                    check(rejected && state.spiritualEnergy == 100 && state.activeSkillCooldown == 1200
                        && !Main.projectile.Any(p => p.active && !priorShots.Contains(p)),
                        "native transformed star calamity case keeps each world progression and character realm use gate without spending or projectiles");
                }
                Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = NPC.downedMoonlord = true; state.cultivationStage = CultivationStage.DaoSevering;
                foreach (int otherRoute in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                    World(otherRoute,core); bool active = otherRoute == route && core;
                    int cost = active ? route == 3 ? 28 : 36 : 28;
                    check(weapon.GetSpiritCost(actor) == cost && RefinedArtifact.ActiveDaoRoute(item) == (active ? (DownedBossSystem.EndgameRoute)route : DownedBossSystem.EndgameRoute.None),
                        "native star calamity case Dao costs activate only for matching world and defeated core");
                    var damage = StatModifier.Default; weapon.ModifyWeaponDamage(actor,ref damage); growth.ModifyWeaponDamage(item,actor,ref damage);
                    float expected = item.damage * (active ? 340f / 260f : 1f) * (1.22f + (active && route == 2 ? .15f : 0));
                    check(Math.Abs(damage.ApplyTo(item.damage) - expected) < .02f && RefinedArtifact.GetDaoRoute(item) == (DownedBossSystem.EndgameRoute)route,
                        "native star calamity case cross-world Dao damage reverts without losing record or awakening");
                    state.spiritualEnergy = cost - 1; check(!weapon.CanUseItem(actor), "native star calamity case Dao rejects insufficient current-route energy");
                    state.spiritualEnergy = cost; check(weapon.CanUseItem(actor), "native star calamity case Dao accepts exact current-route energy");
                    actor.selectedItem = 1; state.activeSkillCooldown = 1200;
                    int actualDamage = actor.GetWeaponDamage(item);
                    bool Fire() => WeaponShotTransactions.FirePrepared(actor,weapon,new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,item,0),actor.Center,Vector2.UnitX * item.shootSpeed,item.shoot,actualDamage,2,authority == NetmodeID.Server);
                    state.spiritualEnergy = cost - 1;
                    check(!Fire() && state.spiritualEnergy == cost - 1 && state.activeSkillCooldown == 1200
                        && !Main.projectile.Any(p => p.active && !priorShots.Contains(p)),
                        "native star calamity case every world state rejects unaffordable actual shot without energy cooldown or projectile changes");
                    state.spiritualEnergy = cost;
                    check(Fire() && state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200
                        && Main.projectile.Count(p => p.active && !priorShots.Contains(p) && p.type == item.shoot && p.owner == 0 && p.damage == actualDamage) == 1
                        && RefinedArtifact.GetDaoRoute(item) == (DownedBossSystem.EndgameRoute)route,
                        "native star calamity case every world state fires exactly one bolt at active or fallback cost and retains Dao record");
                    var bolt = Main.projectile.Single(p => p.active && !priorShots.Contains(p) && p.type == item.shoot);
                    bool collisions = bolt.DamageType == DamageClass.Ranged && bolt.penetrate == 4 && bolt.tileCollide && bolt.timeLeft == 150 && item.useAmmo == 0 && item.mana == 0;
                    for (int bounce = 0; bounce < 3; bounce++) {
                        Vector2 oldVelocity = new Vector2(18,2); bolt.velocity = new Vector2(0,2); bolt.netUpdate = false;
                        bool destroy = bolt.ModProjectile.OnTileCollide(oldVelocity);
                        collisions &= destroy == (bounce == 2) && bolt.ai[0] == Math.Min(2,bounce + 1)
                            && (bounce == 2 || bolt.velocity == new Vector2(-18,2) && bolt.netUpdate) && bolt.damage == actualDamage;
                    }
                    check(collisions,"native Dao and fallback star calamity bolts keep four-hit ranged identity two bounces and destruction on third collision");
                    foreach (var entity in Main.projectile.Where(p => p.active && !priorShots.Contains(p))) entity.active = false;
                    var tips = new List<TooltipLine>(); weapon.ModifyTooltips(tips); growth.ModifyTooltips(item,tips);
                    check(tips.Count == 3 && tips.All(tip => !tip.Text.Contains("Mods.XianXia.") && !tip.Text.Contains("{0}"))
                        && ArtifactSkillTransactions.HeldSkill(item) == ArtifactSkill.CalamityVolley && weapon.AltFunctionUse(actor),
                        "native star calamity case Dao tooltip resolves recorded route with its own active skill");
                }
                World(route,true); actor.selectedItem = 1; int firingCost = route == 3 ? 28 : 36; state.spiritualEnergy = firingCost; state.activeSkillCooldown = 1200;
                int hitDamage = actor.GetWeaponDamage(item);
                bool shot = WeaponShotTransactions.FirePrepared(actor,weapon,new Terraria.DataStructures.EntitySource_ItemUse_WithAmmo(actor,item,0),actor.Center,Vector2.UnitX * item.shootSpeed,item.shoot,hitDamage,2,authority == NetmodeID.Server);
                check(shot && state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200
                    && Main.projectile.Any(p => p.active && !priorShots.Contains(p) && p.type == item.shoot && p.owner == 0 && p.damage == hitDamage),
                    "native transformed star calamity case actually fires its ranged mechanism bolt at route cost and computed weapon damage");
                foreach (var shotEntity in Main.projectile.Where(p => p.active && !priorShots.Contains(p))) shotEntity.active = false;
            }
        } finally {
            foreach (var shot in Main.projectile.Where(p => p.active && !priorShots.Contains(p))) shot.active = false;
            Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local;
            Main.hardMode = oldHard; NPC.downedPlantBoss = oldPlant; NPC.downedGolemBoss = oldGolem; NPC.downedMoonlord = oldMoon;
            tile.HasTile = has; tile.TileType = oldType; world.LoadWorldData(saved);
        }
    }
}
