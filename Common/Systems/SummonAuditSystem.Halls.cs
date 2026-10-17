using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditSectRuinHalls(Action<bool, string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2, 30, SectRuinHallSystem.Width, SectRuinHallSystem.Height);
        var saved = new List<(int X, int Y, bool Has, ushort Type, short FX, short FY, SlopeType Slope, bool Half)>();
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x, y];
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        var halls = ModContent.GetInstance<SectRuinHallSystem>(); var original = new TagCompound(); halls.SaveWorldData(original);
        try {
            // Use dry natural sky space without replacing the loaded world's Tilemap.
            check(saved.TrueForAll(c => Main.tile[c.X,c.Y].LiquidAmount == 0 && Main.tile[c.X,c.Y].WallType == 0
                && !Main.tile[c.X,c.Y].RedWire && !Main.tile[c.X,c.Y].BlueWire && !Main.tile[c.X,c.Y].GreenWire && !Main.tile[c.X,c.Y].YellowWire
                && !Main.tile[c.X,c.Y].HasActuator), "native hall fixture dry unoccupied sky");
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = false; }
            var protectedMap = new StructureMap(); protectedMap.AddProtectedStructure(area, 2);
            check(!SectRuinHallSystem.TryBuild(area, protectedMap) && !Main.tile[area.X,area.Y].HasTile, "native hall refuses protected area without terrain writes");
            Tile obstacle = Main.tile[area.X + 4,area.Y + 4];
            obstacle.HasTile = true; obstacle.TileType = TileID.Containers;
            check(!SectRuinHallSystem.TryBuild(area, new StructureMap()) && obstacle.TileType == TileID.Containers, "native hall refuses occupied objects");
            obstacle.HasTile = false; obstacle.LiquidAmount = 1;
            try { check(!SectRuinHallSystem.TryBuild(area, new StructureMap()), "native hall refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.RedWire = true;
            try { check(!SectRuinHallSystem.TryBuild(area, new StructureMap()), "native hall refuses wiring"); } finally { obstacle.RedWire = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!SectRuinHallSystem.TryBuild(area, new StructureMap()), "native hall refuses housing walls"); } finally { obstacle.WallType = 0; }
            check(!SectRuinHallSystem.TryBuild(new Rectangle(-1,30,41,19), new StructureMap()), "native hall rejects out of world geometry");
            obstacle.HasTile = true; obstacle.TileType = TileID.Copper;
            var map = new StructureMap();
            check(SectRuinHallSystem.TryBuild(area, map), "native hall builds in approved dry natural space");
            int bricks = 0, brick = ModContent.TileType<SectRuinBrickTile>();
            bool geometry = true;
            for (int x = 0; x < area.Width; x++) for (int y = 0; y < area.Height; y++) {
                Tile tile = Main.tile[area.X + x,area.Y + y]; bool expected = SectRuinHallSystem.IsBrick(x,y);
                geometry &= tile.HasTile == expected && (!expected || (tile.TileType == brick && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
                if (tile.HasTile && tile.TileType == brick) bricks++;
            }
            check(geometry && bricks >= 180, "native hall has complete shell flat floor open interior and both entrances with biome tile threshold");
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + area.Width / 2) * 16, (area.Y + area.Height / 2) * 16);
            int oldMode = Main.netMode;
            try {
                Main.netMode = NetmodeID.Server;
                check(ModContent.GetInstance<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>().IsBiomeActive(visitor), "native hall visitor activates actual server sect biome scan");
            } finally { Main.netMode = oldMode; }
            check(!map.CanPlace(area, 2), "native hall registers protection for subsequent generation");
            int floor = area.Bottom - 3;
            WorldGen.PlaceObject(area.X + 10, floor - 1, ModContent.TileType<SwordTabletTile>());
            bool tablet = true;
            for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++) {
                Tile tile = Main.tile[area.X + 10 + dx,floor - 3 + dy];
                tablet &= tile.HasTile && tile.TileType == ModContent.TileType<SwordTabletTile>() && tile.TileFrameX == dx * 16 && tile.TileFrameY == dy * 16;
            }
            check(tablet, "native hall floor accepts a complete framed sword tablet");
            AuditHallSupplyChest(area, check);
            halls.Halls.Clear(); halls.Halls.Add(area); var tag = new TagCompound(); halls.SaveWorldData(tag); halls.ClearWorld(); halls.LoadWorldData(tag);
            check(halls.Halls.Count == 1 && halls.Halls[0] == area, "native hall bounds persist without regenerating terrain");
            check(Main.tile[area.X + 10,floor - 3].TileType == ModContent.TileType<SwordTabletTile>(), "native hall loading preserves existing decorations");
        } finally {
            foreach (var cell in saved) {
                Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half;
            }
            halls.LoadWorldData(original);
        }
    }
    private static void AuditHallSupplyChest(Rectangle area, Action<bool, string> check)
    {
        var previous = (Chest[])Main.chest.Clone(); int mode = Main.netMode;
        var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        int x = area.X + 19, floor = area.Bottom - 3;
        var random = new Terraria.Utilities.UnifiedRandom(20261019);
        try {
            config.EnableWorldGeneration = false;
            check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1, "native disabled generation does not create hall loot");
            config.EnableWorldGeneration = true; Main.netMode = NetmodeID.MultiplayerClient;
            check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1, "native clients cannot create hall loot");
            Main.netMode = NetmodeID.Server;
            Tile obstacle = Main.tile[x,floor - 2]; obstacle.HasTile = true; obstacle.TileType = TileID.Stone;
            try { check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1 && obstacle.HasTile && obstacle.TileType == TileID.Stone, "native chest placement preserves obstructing tile"); }
            finally { obstacle.HasTile = false; }
            obstacle.LiquidAmount = 1;
            try { check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1, "native chest placement refuses liquid"); }
            finally { obstacle.LiquidAmount = 0; }
            for (int i = 0; i < Main.chest.Length; i++) Main.chest[i] ??= new Chest { x = -100, y = -100 };
            try { check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1 && !obstacle.HasTile, "native full chest table fails without creating loot tiles"); }
            finally { previous.CopyTo(Main.chest, 0); }
            int index = SectRuinHallSystem.TryPlaceSupplyChest(area, random);
            check(index >= 0, "native hall supply chest uses actual engine creation");
            Chest chest = Main.chest[index];
            check(chest.x == x && chest.y == floor - 2, "native chest stored top-left matches planned hall location");
            bool intact = true;
            for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 2; dy++) {
                Tile tile = Main.tile[x + dx,floor - 2 + dy];
                intact &= tile.HasTile && tile.TileType == TileID.Containers && tile.TileFrameX == dx * 18 && tile.TileFrameY == dy * 18;
            }
            check(intact, "native hall chest all four vanilla frames intact");
            check(chest.item[0].type == ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>() && chest.item[0].stack >= 6 && chest.item[0].stack <= 10
                && chest.item[1].type == ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>() && chest.item[1].stack >= 2 && chest.item[1].stack <= 4
                && chest.item[2].type == ItemID.RecallPotion && chest.item[2].stack >= 1 && chest.item[2].stack <= 2
                && chest.item[3].type == ItemID.Torch && chest.item[3].stack == 10, "native hall chest contains four advertised bounded supply stacks");
            for (int i = 4; i < chest.item.Length; i++) check(chest.item[i].IsAir, "native hall chest contains no unadvertised additional loot");
            int count = 0; foreach (Chest entry in Main.chest) if (entry != null) count++;
            check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1 && Main.chest[index] == chest, "native repeated placement preserves existing chest and loot");
            foreach (Item item in chest.item) item.TurnToAir();
            check(SectRuinHallSystem.TryPlaceSupplyChest(area, random) == -1 && Array.TrueForAll(chest.item, item => item.IsAir), "native looted chest cannot be refilled through placement");
            int after = 0; foreach (Chest entry in Main.chest) if (entry != null) after++;
            check(count == after, "native repeated chest placement does not allocate duplicate entries");
        } finally { previous.CopyTo(Main.chest, 0); config.EnableWorldGeneration = enabled; Main.netMode = mode; }
    }

}
