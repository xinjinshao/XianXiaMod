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
            WorldGen.PlaceObject(area.X + 20, floor - 1, ModContent.TileType<SwordTabletTile>());
            bool tablet = true;
            for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 3; dy++) {
                Tile tile = Main.tile[area.X + 20 + dx,floor - 3 + dy];
                tablet &= tile.HasTile && tile.TileType == ModContent.TileType<SwordTabletTile>() && tile.TileFrameX == dx * 16 && tile.TileFrameY == dy * 16;
            }
            check(tablet, "native hall floor accepts a complete framed sword tablet");
            halls.Halls.Clear(); halls.Halls.Add(area); var tag = new TagCompound(); halls.SaveWorldData(tag); halls.ClearWorld(); halls.LoadWorldData(tag);
            check(halls.Halls.Count == 1 && halls.Halls[0] == area, "native hall bounds persist without regenerating terrain");
            check(Main.tile[area.X + 20,floor - 3].TileType == ModContent.TileType<SwordTabletTile>(), "native hall loading preserves existing decorations");
        } finally {
            foreach (var cell in saved) {
                Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half;
            }
            halls.LoadWorldData(original);
        }
    }
}
