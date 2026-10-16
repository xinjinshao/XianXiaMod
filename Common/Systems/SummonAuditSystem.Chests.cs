using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
        int seed = Enumerable.Range(0, 100).First(value => new UnifiedRandom(value).NextBool(4));
        int stone = ModContent.ItemType<LowGradeSpiritStone>();
        UnifiedRandom lootRandom = new UnifiedRandom(seed);
        Chest MakeChest() {
            for (int c = 0; c < 2; c++) for (int r = 0; r < 2; r++) {
                Tile tile = Main.tile[x + c, y + r]; tile.HasTile = true; tile.TileType = TileID.Containers;
                tile.TileFrameX = (short)(36 + c * 18); tile.TileFrameY = (short)(r * 18);
            }
            var chest = new Chest { x = x, y = y };
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
