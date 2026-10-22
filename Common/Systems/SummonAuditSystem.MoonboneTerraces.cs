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
    private static void AuditMoonboneTerraces(Action<bool,string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2 + 80,30,47,23);
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        bool dry = true;
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0 && tile.WallType == WallID.None && !tile.HasActuator
                && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native moonbone terrace fixture dry unwired sky context");
        var system = ModContent.GetInstance<MoonboneTerraceSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; system.ClearWorld();
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = true; tile.TileType = TileID.Stone; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; }
            config.EnableWorldGeneration = false;
            check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()),"native disabled generation skips moonbone terraces"); config.EnableWorldGeneration = true;
            Main.netMode = NetmodeID.MultiplayerClient;
            check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()),"native client cannot build moonbone terraces"); Main.netMode = NetmodeID.Server;
            var reserved = new StructureMap(); reserved.AddProtectedStructure(area,2);
            check(!MoonboneTerraceSystem.TryBuild(area,reserved),"native moonbone terrace preserves protected terrain");
            Tile obstacle = Main.tile[area.X + 5,area.Y + 5]; obstacle.TileType = TileID.Containers;
            try { check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()) && obstacle.TileType == TileID.Containers,"native moonbone terrace preserves existing objects"); }
            finally { obstacle.TileType = TileID.Stone; }
            obstacle.LiquidAmount = 1;
            try { check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()),"native moonbone terrace refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.BlueWire = true;
            try { check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()),"native moonbone terrace refuses wiring"); } finally { obstacle.BlueWire = false; }
            obstacle.HasActuator = true;
            try { check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()),"native moonbone terrace refuses actuators"); } finally { obstacle.HasActuator = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!MoonboneTerraceSystem.TryBuild(area,new StructureMap()),"native moonbone terrace preserves housing walls"); } finally { obstacle.WallType = WallID.None; }
            check(!MoonboneTerraceSystem.TryBuild(new Rectangle(-1,30,47,23),new StructureMap()),"native moonbone terrace rejects invalid bounds");
            check(!MoonboneTerraceSystem.TryBuild(new Rectangle(area.X,30,47,22),new StructureMap()),"native moonbone terrace rejects incorrect dimensions");
            var map = new StructureMap(); check(MoonboneTerraceSystem.TryBuild(area,map),"native moonbone terrace builds in approved natural terrain");
            int bone = ModContent.TileType<MoonboneTile>(), count = 0; bool shape = true;
            for (int dx = 0; dx < 47; dx++) for (int dy = 0; dy < 23; dy++) {
                Tile tile = Main.tile[area.X + dx,area.Y + dy];
                bool expected = dy >= 19 || (dx >= 17 && dx <= 29 && dy >= 15) || (dx >= 13 && dx <= 33 && dy >= 17)
                    || (dy < 2 && (dx < 10 || dx >= 37)) || (dy >= 2 && dy <= 6 && (dx == 2 || dx == 3 || dx == 43 || dx == 44));
                shape &= tile.HasTile == expected && (!expected || (tile.TileType == bone && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
                if (tile.HasTile && tile.TileType == bone) count++;
            }
            check(shape && count == 316,"native moonbone terrace has 316 bone tiles raised center two-step shoulders and broken ribs");
            foreach (var stand in new[] { (X:0,Y:19),(X:8,Y:19),(X:14,Y:17),(X:23,Y:15),(X:31,Y:17),(X:38,Y:19),(X:45,Y:19) }) {
                var feet = new Vector2((area.X + stand.X) * 16,(area.Y + stand.Y) * 16);
                check(!Collision.SolidCollision(feet - new Vector2(0,42),20,42,true) && Collision.SolidCollision(feet,20,16,true),
                    "native moonbone terrace fits a standing player with physical support on each level and approach");
            }
            check(!map.CanPlace(area,2),"native moonbone terrace reserves protected footprint");
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + 23) * 16,(area.Y + 12) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.MoonboneAbyssBiome>().IsBiomeActive(visitor),"native moonbone terrace alone activates actual server lunar biome scan");
            system.Terraces.Add(area); var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Terraces.Count == 1 && system.Terraces[0] == area,"native moonbone terrace bounds survive world data roundtrip");
            Tile removed = Main.tile[area.X + 23,area.Y + 15]; removed.HasTile = false; system.LoadWorldData(tag);
            check(!removed.HasTile,"native moonbone terrace load never repairs player-removed platform");
            tag["moonboneTerraces"] = new List<TagCompound> {
                new() { ["x"] = area.X, ["y"] = area.Y }, new() { ["x"] = area.X + 1, ["y"] = area.Y },
                new() { ["x"] = -1, ["y"] = area.Y }, new() { ["x"] = area.X, ["y"] = area.Y }
            };
            system.LoadWorldData(tag);
            check(system.Terraces.Count == 1 && system.Terraces[0] == area,"native moonbone terrace load rejects overlapping duplicate and invalid records");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            system.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
