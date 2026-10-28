using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using Terraria.Utilities;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditChestSupplies(Action<bool, string> check)
    {
        var oldChests = Main.chest; var oldRandom = WorldGen.genRand; int oldMode = Main.netMode;
        var config = ModContent.GetInstance<XianXiaConfig>(); bool oldEnabled = config.EnableWorldGeneration;
        int x = Main.maxTilesX / 2, y = (int)Math.Ceiling(Main.worldSurface) + 20;
        check(y + 1 < Main.maxTilesY - 200, "native chest fixture has ordinary underground depth");
        var tiles = new List<(int X, int Y, bool HasTile, ushort Type, short FrameX, short FrameY)>();
        for (int c = 0; c < 2; c++) for (int r = 0; r < 2; r++) {
            Tile tile = Main.tile[x + c, y + r]; tiles.Add((x + c, y + r, tile.HasTile, tile.TileType, tile.TileFrameX, tile.TileFrameY));
        }
        var captured = new HashSet<(int, int)>(tiles.Select(saved => (saved.X, saved.Y)));
        int seed = Enumerable.Range(0, 100).First(value => new UnifiedRandom(value).NextBool(4));
        int stone = ModContent.ItemType<LowGradeSpiritStone>();
        UnifiedRandom lootRandom = new UnifiedRandom(seed);
        Chest MakeChest(int offset = 0) {
            for (int c = 0; c < 2; c++) for (int r = 0; r < 2; r++) {
                Tile tile = Main.tile[x + offset + c, y + r];
                if (captured.Add((x + offset + c, y + r))) tiles.Add((x + offset + c, y + r, tile.HasTile, tile.TileType, tile.TileFrameX, tile.TileFrameY));
                tile.HasTile = true; tile.TileType = TileID.Containers;
                tile.TileFrameX = (short)(36 + c * 18); tile.TileFrameY = (short)(r * 18);
            }
            var chest = new Chest { x = x + offset, y = y };
            var original = new Item(); original.SetDefaults(ItemID.IronBroadsword); original.Prefix(PrefixID.Godly); chest.item[0] = original;
            Main.chest = new[] { chest }; lootRandom = new UnifiedRandom(seed); return chest;
        }
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true;
            Chest chest = MakeChest(); Item original = chest.item[0]; int damage = original.damage; int prefix = original.prefix;
            var system = new SpiritChestSuppliesSystem(); system.GenerateSupplies(lootRandom);
            check(ReferenceEquals(chest.item[0], original) && original.type == ItemID.IronBroadsword && original.damage == damage && original.prefix == prefix,
                "native supply generation preserves existing chest loot reference damage and prefix");
            check(chest.item[1].type == stone && chest.item[1].stack >= 4 && chest.item[1].stack <= 8,
                "native gold chest receives registered spirit stones only in empty slot");
            check(chest.item.Count(item => item != null && !item.IsAir) == 2, "native gold chest retains original loot and exactly one supply stack");
            int stack = chest.item[1].stack; system.GenerateSupplies(lootRandom);
            check(chest.item[1].stack == stack && chest.item.Count(item => item != null && item.type == stone) == 1,
                "native repeated generation hook cannot duplicate supply loot");
            foreach (int gate in new[] { 0, 1, 2, 3, 4 }) {
                chest = MakeChest(); Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true;
                if (gate == 0) config.EnableWorldGeneration = false;
                if (gate == 1) Main.netMode = NetmodeID.MultiplayerClient;
                if (gate == 2) { Tile tile = Main.tile[x, y]; tile.TileFrameX = 72; }
                if (gate == 3) for (int i = 0; i < chest.item.Length; i++) { chest.item[i] = new Item(); chest.item[i].SetDefaults(ItemID.Torch); }
                if (gate == 4) { chest.item[1] = new Item(); chest.item[1].SetDefaults(stone); chest.item[1].stack = 5; }
                new SpiritChestSuppliesSystem().GenerateSupplies(lootRandom);
                check(chest.item.Count(item => item != null && item.type == stone) == (gate == 4 ? 1 : 0)
                    && (gate != 4 || chest.item[1].stack == 5), "native supply guards disabled client locked full and pre-supplied chests");
            }
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true;
            int limit = Math.Max(1, Main.maxTilesX / 420);
            var pool = new Chest[limit * 20];
            for (int i = 0; i < pool.Length; i++) pool[i] = MakeChest(i * 3);
            var originals = pool.Select(entry => entry.item[0]).ToArray(); Main.chest = pool;
            new SpiritChestSuppliesSystem().GenerateSupplies(lootRandom);
            check(pool.Count(entry => entry.item.Any(item => item != null && item.type == stone)) == limit
                && pool.Select(entry => entry.item[0]).SequenceEqual(originals),
                "native capped reservoir preserves original loot across a large real chest candidate pool");
            check(pool.Skip(limit).Any(entry => entry.item.Any(item => item != null && item.type == stone)),
                "native chest reservoir includes candidates beyond the initial array prefix");
            chest = MakeChest(); Main.chest = new[] { chest, chest };
            new SpiritChestSuppliesSystem().GenerateSupplies(lootRandom);
            check(chest.item.Count(item => item != null && item.type == stone) == 1,
                "native duplicate chest references cannot allocate duplicate supply stacks");
            Item carriedStone = chest.item.First(item => item != null && item.type == stone);
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.MultiplayerClient, NetmodeID.Server }) {
                Main.netMode = mode;
                var actor = new Player { whoAmI = 0, active = true, dead = false };
                actor.inventory[0] = carriedStone.Clone(); Item heldStone = actor.inventory[0]; int originalStack = heldStone.stack;
                var state = actor.GetModPlayer<XianXiaPlayer>(); var before = state.CaptureSnapshot();
                ItemLoader.UpdateInventory(heldStone, actor); var after = state.CaptureSnapshot();
                bool authority = mode != NetmodeID.MultiplayerClient;
                string summary = CultivationStatusText.Summary(state);
                string expectedHint = authority
                    ? Terraria.Localization.Language.GetTextValue("Mods.XianXia.CultivationStatus.DiscoveryFound", CultivationStatusText.StageName(CultivationStage.QiAwakening),
                        global::XianXia.Content.Items.Consumables.QiDrawingTalisman.SpiritStoneCost, global::XianXia.Content.Items.Consumables.QiDrawingTalisman.SpiritGelCost)
                    : Terraria.Localization.Language.GetTextValue("Mods.XianXia.CultivationStatus.DiscoverySeek");
                check(summary.EndsWith(expectedHint) && state.CaptureSnapshot() == after,
                    "native starter status follows replicated discovery state and remains read-only");
                check(after == (before with { Discovered = authority, Revision = before.Revision + (authority ? 1u : 0u) }),
                    "native registered inventory dispatch discovers awareness only on authority without realm energy or pressure grants");
                check(ReferenceEquals(actor.inventory[0], heldStone) && heldStone.stack == originalStack,
                    "native carried supply discovery neither consumes nor replaces chest loot");
                ItemLoader.UpdateInventory(heldStone, actor);
                check(state.CaptureSnapshot() == after, "native repeated inventory frames do not repeat awareness revision");
                if (authority) {
                    var tag = new TagCompound(); state.SaveData(tag);
                    var restored = new Player().GetModPlayer<XianXiaPlayer>(); restored.LoadData(tag);
                    check(restored.discoveredSpiritualEnergy && restored.cultivationStage == CultivationStage.None && restored.spiritualEnergy == before.Energy,
                        "native discovered awareness persists through actual character save/load hooks without advancing realm");
                }
            }
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.MultiplayerClient, NetmodeID.Server }) {
                Main.netMode = mode; var actor = new Player { active = true, dead = false };
                Item pickupStone = carriedStone.Clone(); int amount = pickupStone.stack;
                var state = actor.GetModPlayer<XianXiaPlayer>(); var before = state.CaptureSnapshot();
                check(ItemLoader.OnPickup(pickupStone, actor) && state.CaptureSnapshot() == (before with { Discovered = true, Revision = before.Revision + (mode == NetmodeID.MultiplayerClient ? 0u : 1u) }),
                    "native registered pickup keeps local awareness and authority-only discovery revision");
                var afterPickup = state.CaptureSnapshot(); ItemLoader.UpdateInventory(pickupStone, actor); ItemLoader.OnPickup(pickupStone, actor);
                check(state.CaptureSnapshot() == afterPickup && pickupStone.stack == amount,
                    "native pickup followed by inventory dispatch cannot repeat discovery revision or consume stone");
            }
            int qiTalisman = ModContent.ItemType<global::XianXia.Content.Items.Consumables.QiDrawingTalisman>();
            check(Main.recipe.Take(Recipe.numRecipes).Any(recipe => !recipe.Disabled && recipe.createItem.type == qiTalisman
                && recipe.requiredTile.Contains(TileID.WorkBenches)
                && recipe.requiredItem.Any(item => item.type == stone && item.stack == global::XianXia.Content.Items.Consumables.QiDrawingTalisman.SpiritStoneCost)
                && recipe.requiredItem.Any(item => item.type == ModContent.ItemType<SpiritGel>() && item.stack == global::XianXia.Content.Items.Consumables.QiDrawingTalisman.SpiritGelCost)),
                "native starter hint costs match registered Qi talisman recipe and workbench station");
            var sourceRecipes = new[] {
                (CultivationStage.QiAwakening, qiTalisman, TileID.WorkBenches, new[] {
                    (stone, global::XianXia.Content.Items.Consumables.QiDrawingTalisman.SpiritStoneCost),
                    (ModContent.ItemType<SpiritGel>(), global::XianXia.Content.Items.Consumables.QiDrawingTalisman.SpiritGelCost) }),
                (CultivationStage.QiCondensation, ModContent.ItemType<QiCondensingPill>(), ModContent.TileType<global::XianXia.Content.Tiles.Stations.EarthClayFurnaceTile>(), new[] {
                    (ModContent.ItemType<GreenwoodRoot>(), QiCondensingPill.GreenwoodRootCost), (stone, QiCondensingPill.SpiritStoneCost) }),
                (CultivationStage.Foundation, ModContent.ItemType<FoundationPill>(), ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>(), new[] {
                    (ModContent.ItemType<GreenwoodRoot>(), FoundationPill.GreenwoodRootCost), (ModContent.ItemType<FurnaceSlagIron>(), FoundationPill.FurnaceSlagCost),
                    (stone, FoundationPill.SpiritStoneCost) })
            };
            foreach (var entry in sourceRecipes) {
                check(Main.recipe.Take(Recipe.numRecipes).Any(recipe => !recipe.Disabled && recipe.createItem.type == entry.Item2
                    && recipe.requiredTile.Contains(entry.Item3) && entry.Item4.All(ingredient => recipe.requiredItem.Any(item => item.type == ingredient.Item1 && item.stack == ingredient.Item2))),
                    "native breakthrough source quantities and station match registered early-realm recipe");
                var reader = new Player(); var state = reader.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = entry.Item1 - 1; state.discoveredSpiritualEnergy = true;
                var beforeGuide = state.CaptureSnapshot(); string source = CultivationStatusText.BreakthroughSource(entry.Item1);
                string preparation = global::XianXia.Content.Items.Guides.SectLedger.GetPreparation(reader);
                check(preparation.EndsWith(source) && !source.Contains("{0}") && !source.Contains("{1}") && !source.Contains("{2}") && state.CaptureSnapshot() == beforeGuide,
                    "native ledger preparation uses formatted recipe sources without mutating player state");
            }
            foreach (var entry in new[] {
                (CultivationStage.GoldenCore, new[] { "AbyssalStarWomb" }), (CultivationStage.NascentSoul, new[] { "ArchiveScrollSpirit" }),
                (CultivationStage.SpiritSevering, new[] { "HeavenTabletGuardian", "BrokenHeavenInspector" }),
                (CultivationStage.Tribulation, new[] { "MoonboneCultivator" }), (CultivationStage.DaoSevering, new[] { "MoonboneImmortal", "OldHeavenDaoCore" }) }) {
                string source = CultivationStatusText.BreakthroughSource(entry.Item1);
                foreach (string npc in entry.Item2) {
                    string displayName = ModLoader.GetMod("XianXia").Find<ModNPC>(npc).DisplayName.Value;
                    check(!string.IsNullOrWhiteSpace(displayName) && source.Contains(displayName) && !source.Contains("{0}") && !source.Contains("{1}"),
                        "native breakthrough source names match actual registered NPC display names");
                }
            }
            Main.netMode = NetmodeID.Server;
            foreach (int invalid in new[] { 0, 1, 2 }) {
                var actor = new Player { active = invalid != 0, dead = invalid == 1 }; Item heldStone = carriedStone.Clone();
                if (invalid == 2) heldStone.stack = 0;
                ItemLoader.UpdateInventory(heldStone, actor);
                check(!actor.GetModPlayer<XianXiaPlayer>().discoveredSpiritualEnergy,
                    "native inactive dead or empty inventory cannot discover awareness");
                check(ItemLoader.OnPickup(heldStone, actor) && !actor.GetModPlayer<XianXiaPlayer>().discoveredSpiritualEnergy,
                    "native pickup dispatch skips inactive dead or empty recipients without cancelling pickup permission");
            }
            Player unready = Main.player.First(player => player != null && !player.TryGetModPlayer<XianXiaPlayer>(out _));
            bool unreadyWasActive = unready.active;
            try {
                unready.active = true; ItemLoader.UpdateInventory(carriedStone, unready); check(true, "native inventory dispatch skips missing player components safely");
                check(ItemLoader.OnPickup(carriedStone, unready), "native pickup safely skips actual missing player components");
            }
            finally { unready.active = unreadyWasActive; }

        } finally {
            foreach (var saved in tiles) { Tile tile = Main.tile[saved.X, saved.Y]; tile.HasTile = saved.HasTile; tile.TileType = saved.Type; tile.TileFrameX = saved.FrameX; tile.TileFrameY = saved.FrameY; }
            Main.chest = oldChests; Main.netMode = oldMode; config.EnableWorldGeneration = oldEnabled;
        }
        check(ReferenceEquals(Main.chest, oldChests) && ReferenceEquals(WorldGen.genRand, oldRandom) && Main.netMode == oldMode && config.EnableWorldGeneration == oldEnabled
            && tiles.All(saved => Main.tile[saved.X, saved.Y].HasTile == saved.HasTile && Main.tile[saved.X, saved.Y].TileType == saved.Type
                && Main.tile[saved.X, saved.Y].TileFrameX == saved.FrameX && Main.tile[saved.X, saved.Y].TileFrameY == saved.FrameY),
            "native supply audit restores original chest table tile frames generator RNG and configuration");
    }
}
