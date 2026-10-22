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
    private static void AuditThunderShrines(Action<bool,string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2 + 500,120,51,13);
        int floor = area.Bottom - 3, cloud = ModContent.TileType<ThunderCloudTile>();
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        bool dry = true;
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y];
            dry &= tile.LiquidAmount == 0 && tile.WallType == WallID.None && !tile.HasActuator
                && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native thunder shrine fixture dry unwired sky context");
        var system = ModContent.GetInstance<ThunderCloudShrineSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        void Reset(int clouds = 20) {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = false; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; }
            for (int dx = 0; dx < clouds; dx++) { Tile tile = Main.tile[area.X + dx,area.Bottom - 1]; tile.HasTile = true; tile.TileType = (ushort)cloud; }
        }
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; system.ClearWorld();
            Reset(0); check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native shrine refuses empty sky without existing thunder clouds");
            Reset(19); check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native shrine requires twenty pre-existing cloud tiles");
            Reset(); config.EnableWorldGeneration = false;
            check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native disabled generation skips thunder shrines");
            config.EnableWorldGeneration = true; Main.netMode = NetmodeID.MultiplayerClient;
            check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native client never builds thunder shrines"); Main.netMode = NetmodeID.Server;
            var reserved = new StructureMap(); reserved.AddProtectedStructure(area,2);
            check(!ThunderCloudShrineSystem.TryBuild(area,reserved),"native shrine respects protected sky structure");
            Tile obstacle = Main.tile[area.Left + 4,area.Top + 4];
            foreach (int type in new[] { TileID.Dirt, TileID.Cloud, TileID.Containers }) {
                obstacle.HasTile = true; obstacle.TileType = (ushort)type;
                check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()) && obstacle.HasTile && obstacle.TileType == type,
                    "native shrine preserves vanilla terrain clouds and chest objects even without reservations");
                obstacle.HasTile = false;
            }
            obstacle.LiquidAmount = 1;
            try { check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native shrine refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.RedWire = true;
            try { check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native shrine refuses wires"); } finally { obstacle.RedWire = false; }
            obstacle.HasActuator = true;
            try { check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native shrine refuses actuators"); } finally { obstacle.HasActuator = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!ThunderCloudShrineSystem.TryBuild(area,new StructureMap()),"native shrine preserves walls"); } finally { obstacle.WallType = WallID.None; }
            check(!ThunderCloudShrineSystem.TryBuild(new Rectangle(area.X,79,51,13),new StructureMap()),"native shrine refuses low sky-world margin");
            check(!ThunderCloudShrineSystem.TryBuild(new Rectangle(area.X,(int)Main.worldSurface - 25,51,13),new StructureMap()),"native shrine stays above the surface band");
            check(!ThunderCloudShrineSystem.TryBuild(new Rectangle(-1,120,51,13),new StructureMap()),"native shrine rejects invalid world bounds");
            var map = new StructureMap(); check(ThunderCloudShrineSystem.TryBuild(area,map),"native shrine builds a landing platform in its own clouds");
            bool shape = true; int cloudsBuilt = 0;
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile tile = Main.tile[x,y]; bool platform = y >= floor;
                shape &= tile.HasTile == platform && (!platform || (tile.TileType == cloud && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
                if (tile.HasTile && tile.TileType == cloud) cloudsBuilt++;
            }
            check(shape && cloudsBuilt == 153,"native shrine has fifty-one flat columns three foundation rows and ten clear rows");
            check(!map.CanPlace(area,2),"native shrine registers its protected footprint");
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + 25) * 16,(floor - 3) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>().IsBiomeActive(visitor),"native shrine alone activates actual server thunder biome scan");
            int stone = ModContent.TileType<SingingThunderStoneTile>(), stoneX = area.X + 24;
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(stone,cloud,2,stoneX,floor,map),"native reservation refuses stone placement without a registered shrine");
            system.Shrines.Add(area);
            check(!GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(stone,cloud,2,stoneX + 3,floor,map),"native owned shrine exception applies only to its exact planned stone position");
            check(GeneratedBiomeWorldGenSystem.TryPlaceProtectedObject(stone,cloud,2,stoneX,floor,map) && ThunderCloudShrineSystem.StoneIntact(area),
                "native shrine accepts its real two-by-two thunder stone with complete engine frames");
            var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Shrines.Count == 1 && system.Shrines[0] == area,"native shrine bounds survive world data roundtrip");
            Tile removed = Main.tile[stoneX,floor - 2]; removed.HasTile = false;
            Tile floorTile = Main.tile[area.X,floor]; floorTile.HasTile = false; system.LoadWorldData(tag);
            check(!removed.HasTile && !floorTile.HasTile && !ThunderCloudShrineSystem.StoneIntact(area),"native shrine load never restores removed stone or platform");
            tag["thunderCloudShrines"] = new List<TagCompound> {
                new() { ["x"] = area.X, ["y"] = area.Y }, new() { ["x"] = area.X + 1, ["y"] = area.Y },
                new() { ["x"] = -1, ["y"] = area.Y }, new() { ["x"] = area.X, ["y"] = area.Y }
            };
            system.LoadWorldData(tag);
            check(system.Shrines.Count == 1 && system.Shrines[0] == area,"native shrine load rejects overlapping duplicate and invalid bounds");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            system.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
