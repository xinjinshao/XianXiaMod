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
    private static void AuditSectRuinPaths(Action<bool,string> check)
    {
        var hall = new Rectangle(Main.maxTilesX / 2 + 80,30,41,19); int floor = hall.Bottom - 3;
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        bool dryFixture = true;
        for (int x = hall.Left - 56; x < hall.Right + 56; x++) for (int y = hall.Top - 4; y < hall.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y];
            dryFixture &= tile.LiquidAmount == 0 && tile.WallType == 0 && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire && !tile.HasActuator;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dryFixture, "native path fixture dry empty unmodified sky context");
        var system = ModContent.GetInstance<SectRuinPathSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        void Reset() {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = true; tile.TileType = TileID.Stone; tile.IsHalfBlock = false; tile.Slope = SlopeType.Solid; }
            if (!SectRuinHallSystem.TryBuild(hall,new StructureMap())) throw new InvalidOperationException("path parent fixture failed");
        }
        void Cavity(int direction,int start,int width,int height) {
            for (int d = start; d < start + width; d++) for (int y = floor - height; y < floor; y++) {
                Tile tile = Main.tile[direction < 0 ? hall.Left - d : hall.Right + d - 1,y]; tile.HasTile = false;
            }
        }
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true;
            Reset(); check(!SectRuinPathSystem.TryConnect(hall,new StructureMap(),out _), "native sealed terrain never creates a path to an invented cave");
            Cavity(1,49,5,10); check(!SectRuinPathSystem.TryConnect(hall,new StructureMap(),out _), "native cave outside bounded search is not excavated");
            Reset(); Cavity(1,6,5,5); check(!SectRuinPathSystem.TryConnect(hall,new StructureMap(),out _), "native tiny void fails minimum connected natural cavity size");
            Reset(); Cavity(-1,10,8,10); Cavity(1,6,8,10);
            config.EnableWorldGeneration = false; check(!SectRuinPathSystem.TryConnect(hall,new StructureMap(),out _), "native disabled generation skips paths");
            config.EnableWorldGeneration = true; Main.netMode = NetmodeID.MultiplayerClient;
            check(!SectRuinPathSystem.TryConnect(hall,new StructureMap(),out _), "native client does not excavate paths"); Main.netMode = NetmodeID.Server;
            var blocked = new StructureMap(); blocked.AddProtectedStructure(new Rectangle(hall.Left - 1,floor - 6,1,7),2); blocked.AddProtectedStructure(new Rectangle(hall.Right,floor - 6,1,7),2);
            check(!SectRuinPathSystem.TryConnect(hall,blocked,out _) && Main.tile[hall.Right,floor - 3].HasTile, "native protected approaches refuse paths without carving");
            var leftBlocked = new StructureMap(); leftBlocked.AddProtectedStructure(new Rectangle(hall.Left - 1,floor - 6,1,7),2);
            Tile obstacle = Main.tile[hall.Right,floor - 3]; obstacle.LiquidAmount = 1;
            try { check(!SectRuinPathSystem.TryConnect(hall,leftBlocked,out _), "native path refuses wet approach"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.RedWire = true;
            try { check(!SectRuinPathSystem.TryConnect(hall,leftBlocked,out _), "native path refuses wired approach"); } finally { obstacle.RedWire = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!SectRuinPathSystem.TryConnect(hall,leftBlocked,out _), "native path refuses housing wall approach"); } finally { obstacle.WallType = 0; }
            obstacle.TileType = TileID.Containers;
            try { check(!SectRuinPathSystem.TryConnect(hall,leftBlocked,out _) && obstacle.TileType == TileID.Containers, "native path refuses and preserves existing object"); } finally { obstacle.TileType = TileID.Stone; }
            var map = new StructureMap();
            check(SectRuinPathSystem.TryConnect(hall,map,out Rectangle path) && path == new Rectangle(hall.Right,floor - 6,10,7), "native path chooses shortest safe approach to actual pre-existing cave");
            bool geometry = true;
            for (int x = path.Left; x < path.Right; x++) for (int y = path.Top; y < path.Bottom; y++) {
                Tile tile = Main.tile[x,y]; bool shell = y == path.Top || y == path.Bottom - 1;
                geometry &= tile.HasTile == shell && (!shell || (tile.TileType == ModContent.TileType<SectRuinBrickTile>() && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
            }
            check(geometry, "native connector has flat floor roof and five clear interior rows");
            check(!map.CanPlace(path,2), "native connector reserves its protected structure area");
            check(!Main.tile[hall.Right - 1,floor - 3].HasTile && !Main.tile[hall.Right,floor - 3].HasTile, "native connector joins open hall doorway directly");
            system.Paths.Clear(); system.Paths.Add(path); var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Paths.Count == 1 && system.Paths[0] == path, "native connector bounds survive world data roundtrip");
            Tile changed = Main.tile[path.X + 1,path.Y + 2]; changed.HasTile = true;
            system.LoadWorldData(tag); check(changed.HasTile, "native connector loading does not repair or excavate player changes");
            Reset(); Cavity(-1,10,8,10); Cavity(1,6,8,10); Main.tile[hall.Right,floor - 3].TileType = TileID.Containers;
            check(SectRuinPathSystem.TryConnect(hall,new StructureMap(),out path) && path.Right == hall.Left && path.Width == 14
                && Main.tile[hall.Right,floor - 3].TileType == TileID.Containers, "native blocked short approach falls back to safe other-side cave without touching object");
        } finally {
            foreach (var cell in saved) {
                Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half;
            }
            system.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
