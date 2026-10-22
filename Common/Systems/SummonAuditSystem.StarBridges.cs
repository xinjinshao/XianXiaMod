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
    private static void AuditStarBridges(Action<bool,string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2 + 80,30,45,27);
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        bool dry = true;
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0 && tile.WallType == WallID.None && !tile.HasActuator
                && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native star bridge fixture dry unwired sky context");
        var system = ModContent.GetInstance<StarAbyssBridgeSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; system.ClearWorld();
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = true; tile.TileType = TileID.Stone; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; }
            config.EnableWorldGeneration = false;
            check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()),"native disabled generation skips star bridges"); config.EnableWorldGeneration = true;
            Main.netMode = NetmodeID.MultiplayerClient;
            check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()),"native client cannot build star bridges"); Main.netMode = NetmodeID.Server;
            var protectedMap = new StructureMap(); protectedMap.AddProtectedStructure(area,2);
            check(!StarAbyssBridgeSystem.TryBuild(area,protectedMap),"native star bridge preserves protected terrain");
            Tile obstacle = Main.tile[area.X + 5,area.Y + 5]; obstacle.TileType = TileID.Containers;
            try { check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()) && obstacle.TileType == TileID.Containers,"native star bridge preserves existing objects"); }
            finally { obstacle.TileType = TileID.Stone; }
            obstacle.LiquidAmount = 1;
            try { check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()),"native star bridge refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.YellowWire = true;
            try { check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()),"native star bridge refuses wiring"); } finally { obstacle.YellowWire = false; }
            obstacle.HasActuator = true;
            try { check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()),"native star bridge refuses actuators"); } finally { obstacle.HasActuator = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!StarAbyssBridgeSystem.TryBuild(area,new StructureMap()),"native star bridge preserves housing walls"); } finally { obstacle.WallType = WallID.None; }
            check(!StarAbyssBridgeSystem.TryBuild(new Rectangle(-1,30,45,27),new StructureMap()),"native star bridge rejects invalid bounds");
            check(!StarAbyssBridgeSystem.TryBuild(new Rectangle(area.X,30,45,26),new StructureMap()),"native star bridge rejects incorrect dimensions");
            var map = new StructureMap(); check(StarAbyssBridgeSystem.TryBuild(area,map),"native star bridge builds in approved natural terrain");
            int crystal = ModContent.TileType<StarAbyssCrystalTile>(), count = 0; bool shape = true;
            for (int dx = 0; dx < 45; dx++) for (int dy = 0; dy < 27; dy++) {
                Tile tile = Main.tile[area.X + dx,area.Y + dy];
                bool crossing = (dy == 15 || dy == 16) && !(dx >= 8 && dx <= 10) && !(dx >= 34 && dx <= 36);
                bool side = (dx < 2 || dx >= 43) && !(dy >= 11 && dy <= 14) && !(dy >= 20 && dy <= 24);
                bool expected = dy >= 25 || (dy < 2 && (dx < 9 || dx >= 36)) || crossing || side;
                shape &= tile.HasTile == expected && (!expected || (tile.TileType == crystal && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
                if (tile.HasTile && tile.TileType == crystal) count++;
            }
            check(shape && count == 252,"native star bridge has 252 crystals broken upper crossing and complete lower landing");
            foreach (int gap in new[] { 8,34 })
                check(!Collision.SolidCollision(new Vector2((area.X + gap) * 16,(area.Y + 15) * 16),48,32,true),"native engine confirms three-column bridge gap through both deck rows");
            foreach (int row in new[] { 11,20 }) foreach (int side in new[] { 0,43 })
                check(!Collision.SolidCollision(new Vector2((area.X + side) * 16,(area.Y + row) * 16),20,42,true),"native engine fits a player-sized body through each upper and lower side entrance");
            check(Collision.SolidCollision(new Vector2((area.X + 21) * 16,(area.Y + 15) * 16),32,32,true)
                && Collision.SolidCollision(new Vector2(area.X * 16,(area.Y + 25) * 16),45 * 16,32,true),"native engine confirms center bridge support and lower crystal landing");
            check(!map.CanPlace(area,2),"native star bridge reserves protected footprint");
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + 22) * 16,(area.Y + 12) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.StarAbyssRiftBiome>().IsBiomeActive(visitor),"native star bridge alone activates actual server rift biome scan");
            int membrane = ModContent.TileType<RiftMembraneTile>(), stoneX = area.X + 21, floor = area.Y + 15;
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(membrane,crystal,2,stoneX,floor,map),"native bridge membrane requires registered ownership");
            system.Bridges.Add(area);
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(membrane,crystal,2,stoneX + 3,floor,map),"native bridge exception rejects a different protected position");
            check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(membrane,crystal,2,stoneX,floor,map) && StarAbyssBridgeSystem.MembraneIntact(area),
                "native bridge places actual two-by-two rift membrane with complete frames");
            Tile frame = Main.tile[stoneX,floor - 2]; short before = frame.TileFrameX; frame.TileFrameX = 16;
            try { check(!StarAbyssBridgeSystem.MembraneIntact(area),"native membrane audit rejects wrong frames without relying on counts"); } finally { frame.TileFrameX = before; }
            var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Bridges.Count == 1 && system.Bridges[0] == area,"native bridge bounds survive world data roundtrip");
            frame.HasTile = false; Tile lower = Main.tile[area.X,area.Y + 25]; lower.HasTile = false; system.LoadWorldData(tag);
            check(!frame.HasTile && !lower.HasTile,"native bridge loading does not restore removed membrane or landing");
            tag["starAbyssBridges"] = new List<TagCompound> {
                new() { ["x"] = area.X, ["y"] = area.Y }, new() { ["x"] = area.X + 1, ["y"] = area.Y },
                new() { ["x"] = -1, ["y"] = area.Y }, new() { ["x"] = area.X, ["y"] = area.Y }
            };
            system.LoadWorldData(tag);
            check(system.Bridges.Count == 1 && system.Bridges[0] == area,"native bridge load rejects overlapping duplicate and invalid records");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            system.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
