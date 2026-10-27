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
    private static void AuditTalismanCrossbowDao(Action<bool,string> check)
    {
        var previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); int mode = Main.netMode, local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        int x = Main.maxTilesX / 2, y = 12; Tile tile = Main.tile[x,y]; bool has = tile.HasTile; ushort oldType = tile.TileType;
        var priorShots = Main.projectile.Where(p => p.active).ToHashSet();
        try {
            Item[] refinable = ModLoader.GetMod("XianXia").GetContent<ModItem>().Select(sample => new Item(sample.Type)).Where(RefinedArtifact.SupportsRefinement).ToArray();
            check(refinable.Length == 19 && refinable.All(RefinedArtifact.SupportsAwakening) && refinable.All(RefinedArtifact.SupportsDaoTransformation),
                "native all nineteen registered refinable weapons support explicit awakening and Dao transformation");
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
                actor.inventory[1] = new Item(ModContent.ItemType<TalismanCrossbow>());
                actor.inventory[2] = new Item(ModContent.ItemType<LowGradeSpiritStone>(),38);
                var item = actor.inventory[1]; item.Prefix(PrefixID.Demonic); int prefix = item.prefix;
                var growth = item.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); item.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
                void Submit(byte before) => DaoArtifactTransactions.Handle(actor,0,actor.inventory[0].type,1,item.type,item.prefix,1,3,true,before,(byte)route);
                Submit(0);
                check(RefinedArtifact.GetDaoRoute(item) == (DownedBossSystem.EndgameRoute)route && actor.inventory[0].stack == 1 && actor.inventory[2].stack == 2,
                    "native talisman crossbow Dao consumes one seal and thirty-six stones for each actual route");
                actor.GetModPlayer<InscriptionPlayer>().Initialize(); Submit((byte)route);
                check(actor.inventory[0].stack == 1 && actor.inventory[2].stack == 2 && ReferenceEquals(item,actor.inventory[1]),
                    "native repeated talisman crossbow Dao never repeats cost or replaces equipment");
                foreach (Item copy in new[] { item.Clone(),ItemIO.Load(ItemIO.Save(item)) })
                    check(copy.prefix == prefix && RefinedArtifact.GetDaoRoute(copy) == (DownedBossSystem.EndgameRoute)route
                        && RefinedArtifact.GetLevel(copy) == 3 && RefinedArtifact.IsAwakened(copy) && InscribedEquipment.GetKind(copy) == InscriptionKind.Greenwood,
                        "native talisman crossbow full save and clone keep Dao awakening refinement inscription and prefix");
                var weapon = (TalismanCrossbow)item.ModItem;
                foreach (int otherRoute in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                    World(otherRoute,core); bool active = otherRoute == route && core;
                    check(RefinedArtifact.ActiveDaoRoute(item) == (active ? (DownedBossSystem.EndgameRoute)route : DownedBossSystem.EndgameRoute.None),
                        "native talisman crossbow Dao activates only for matching world and defeated core");
                    var damage = StatModifier.Default; weapon.ModifyWeaponDamage(actor,ref damage); growth.ModifyWeaponDamage(item,actor,ref damage);
                    float expected = item.damage * (active ? 5f : 1f) * (1.22f + (active && route == 2 ? .15f : 0));
                    check(Math.Abs(damage.ApplyTo(item.damage) - expected) < .02f && RefinedArtifact.GetDaoRoute(item) == (DownedBossSystem.EndgameRoute)route,
                        "native talisman crossbow cross-world Dao damage reverts without losing record or awakening");
                    actor.selectedItem = 1; state.spiritualEnergy = 0; state.activeSkillCooldown = 1200;
                    check(item.DamageType == DamageClass.Ranged && item.useAmmo == AmmoID.Arrow && item.mana == 0
                        && Math.Abs(weapon.UseSpeedMultiplier(actor) - 1.1f) < .001f,
                        "native talisman crossbow Dao keeps native arrow identity zero mana and awakened use speed");
                    int ammoMode = Main.netMode, ammoLocal = Main.myPlayer;
                    try {
                        Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
                        foreach (int ammoType in new[] { ItemID.WoodenArrow,ModContent.ItemType<CinnabarTalismanArrow>() }) {
                            actor.inventory[54] = new Item(ammoType,5); int expectedProjectile = actor.inventory[54].shoot;
                            bool picked = actor.PickAmmo(item,out int projectile,out float speed,out int shotDamage,out float knockback,out int usedAmmo,false);
                            check(picked && usedAmmo == ammoType && projectile == expectedProjectile && actor.inventory[54].stack == 4
                                && shotDamage >= actor.GetWeaponDamage(item) && state.spiritualEnergy == 0 && state.activeSkillCooldown == 1200
                                && RefinedArtifact.GetDaoRoute(item) == (DownedBossSystem.EndgameRoute)route,
                                "native Dao or fallback talisman crossbow consumes one wood or cinnabar arrow with current weapon damage and no spirit cost");
                        }
                        actor.inventory[54].TurnToAir();
                        check(!actor.PickAmmo(item,out _,out _,out _,out _,out _,false) && state.spiritualEnergy == 0,
                            "native transformed talisman crossbow cannot substitute spirit energy for missing arrows");
                    } finally { Main.netMode = ammoMode; Main.myPlayer = ammoLocal; }
                    var tips = new List<TooltipLine>(); weapon.ModifyTooltips(tips); growth.ModifyTooltips(item,tips);
                    check(tips.Count == 3 && tips.All(tip => !tip.Text.Contains("Mods.XianXia.") && !tip.Text.Contains("{0}"))
                        && ArtifactSkillTransactions.HeldSkill(item) == null && !weapon.AltFunctionUse(actor),
                        "native talisman crossbow Dao tooltip resolves recorded route without inventing an active skill");
                }

            }
        } finally {
            foreach (var shot in Main.projectile.Where(p => p.active && !priorShots.Contains(p))) shot.active = false;
            Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Main.netMode = mode; Main.myPlayer = local;
            tile.HasTile = has; tile.TileType = oldType; world.LoadWorldData(saved);
        }
    }
}
