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
    private static void AuditHeavenCourts(Action<bool,string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2 + 80,30,49,23);
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        bool dry = true;
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0 && tile.WallType == WallID.None && !tile.HasActuator
                && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native heaven court fixture dry unwired sky context");
        var system = ModContent.GetInstance<FallenHeavenCourtSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; system.ClearWorld();
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = true; tile.TileType = TileID.Stone; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; }
            config.EnableWorldGeneration = false;
            check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()),"native disabled generation skips heaven courts"); config.EnableWorldGeneration = true;
            Main.netMode = NetmodeID.MultiplayerClient;
            check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()),"native client cannot build heaven courts"); Main.netMode = NetmodeID.Server;
            var reserved = new StructureMap(); reserved.AddProtectedStructure(area,2);
            check(!FallenHeavenCourtSystem.TryBuild(area,reserved),"native heaven court preserves protected terrain");
            Tile obstacle = Main.tile[area.X + 5,area.Y + 5]; obstacle.TileType = TileID.Containers;
            try { check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()) && obstacle.TileType == TileID.Containers,"native heaven court preserves existing objects"); }
            finally { obstacle.TileType = TileID.Stone; }
            obstacle.LiquidAmount = 1;
            try { check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()),"native heaven court refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.BlueWire = true;
            try { check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()),"native heaven court refuses wiring"); } finally { obstacle.BlueWire = false; }
            obstacle.HasActuator = true;
            try { check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()),"native heaven court refuses actuators"); } finally { obstacle.HasActuator = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!FallenHeavenCourtSystem.TryBuild(area,new StructureMap()),"native heaven court preserves housing walls"); } finally { obstacle.WallType = WallID.None; }
            check(!FallenHeavenCourtSystem.TryBuild(new Rectangle(-1,30,49,23),new StructureMap()),"native heaven court rejects invalid bounds");
            check(!FallenHeavenCourtSystem.TryBuild(new Rectangle(area.X,30,48,23),new StructureMap()),"native heaven court rejects incorrect dimensions");
            var map = new StructureMap(); check(FallenHeavenCourtSystem.TryBuild(area,map),"native heaven court builds in approved natural terrain");
            int jade = ModContent.TileType<FallenHeavenJadeTile>(), count = 0; bool shape = true;
            for (int dx = 0; dx < 49; dx++) for (int dy = 0; dy < 23; dy++) {
                Tile tile = Main.tile[area.X + dx,area.Y + dy];
                bool shell = dy >= 20 || (dy < 2 && (dx < 12 || dx >= 37)) || ((dx < 2 || dx >= 47) && dy < 15);
                shape &= tile.HasTile == shell && (!shell || (tile.TileType == jade && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
                if (tile.HasTile && tile.TileType == jade) count++;
            }
            check(shape && count == 247,"native heaven court has 247 jade tiles broken roof five-high side entrances and flat floor");
            check(!map.CanPlace(area,2),"native heaven court reserves protected footprint");
            int floor = area.Bottom - 3, tablet = ModContent.TileType<BrokenHeavenTabletTile>(), pillar = ModContent.TileType<ArchiveLightPillarTile>();
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + 24) * 16,(floor - 3) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.FallenHeavenPalaceBiome>().IsBiomeActive(visitor),"native heaven court alone activates actual server palace biome scan");
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(tablet,jade,4,area.X + 10,floor,map),"native court object requires registered court ownership");
            system.Courts.Add(area);
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(tablet,jade,4,area.X + 35,floor,map)
                && !GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(pillar,jade,6,area.X + 10,floor,map),"native court object exception rejects swapped reserved positions");
            check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(tablet,jade,4,area.X + 10,floor,map)
                && FallenHeavenCourtSystem.ObjectIntact(area,false),"native court places actual two-by-four heaven tablet with complete frames");
            check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(pillar,jade,6,area.X + 35,floor,map)
                && FallenHeavenCourtSystem.ObjectIntact(area,true),"native court places actual two-by-six archive pillar with complete frames");
            Tile frame = Main.tile[area.X + 35,floor - 6]; short before = frame.TileFrameY; frame.TileFrameY = 16;
            try { check(!FallenHeavenCourtSystem.ObjectIntact(area,true),"native court object audit rejects incorrect pillar frames without relying on counts"); }
            finally { frame.TileFrameY = before; }
            var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Courts.Count == 1 && system.Courts[0] == area,"native court bounds survive world data roundtrip");
            frame.HasTile = false; Tile removedFloor = Main.tile[area.X,floor]; removedFloor.HasTile = false; system.LoadWorldData(tag);
            check(!frame.HasTile && !removedFloor.HasTile,"native court loading does not restore removed objects or floor");
            tag["fallenHeavenCourts"] = new List<TagCompound> {
                new() { ["x"] = area.X, ["y"] = area.Y }, new() { ["x"] = area.X + 1, ["y"] = area.Y },
                new() { ["x"] = -1, ["y"] = area.Y }, new() { ["x"] = area.X, ["y"] = area.Y }
            };
            system.LoadWorldData(tag);
            check(system.Courts.Count == 1 && system.Courts[0] == area,"native court load rejects overlapping duplicate and invalid records");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            system.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
