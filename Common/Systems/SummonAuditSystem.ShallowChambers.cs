using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Common.Players;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditShallowChambers(Action<bool,string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2 + 80,30,29,15);
        int ore = ModContent.TileType<SpiritOreTile>(), moss = ModContent.TileType<SpiritMossTile>();
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>(); bool dry = true;
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0 && tile.WallType == WallID.None && !tile.HasActuator
                && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native shallow chamber fixture dry unwired sky context");
        var system = ModContent.GetInstance<ShallowSpiritChamberSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        int mode = Main.netMode, local = Main.myPlayer; Player previousPlayer = Main.player[0];
        var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        var previousItems = Main.item.Where(item => item?.active == true).ToHashSet();
        void Reset(int ores = 12,int mosses = 40) {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = true; tile.TileType = TileID.Stone; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; }
            for (int i = 0; i < mosses; i++) { Tile tile = Main.tile[area.X + i % 29,area.Bottom - 1 - i / 29]; tile.TileType = (ushort)moss; }
            for (int i = 0; i < ores; i++) { Tile tile = Main.tile[area.X + i % 29,area.Y + 3 + i / 29]; tile.TileType = (ushort)ore; }
        }
        int OreCount() { int result = 0; for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) if (Main.tile[x,y].HasTile && Main.tile[x,y].TileType == ore) result++; return result; }
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; system.ClearWorld();
            Reset(0); check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native shallow chamber never invents ore in a moss-only vein");
            Reset(1,38); check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native shallow chamber requires forty existing spirit tiles");
            Reset(65); check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()) && OreCount() == 65,"native shallow chamber refuses a vein too rich to preserve all original ore");
            Reset(); config.EnableWorldGeneration = false;
            check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native disabled generation skips shallow chambers"); config.EnableWorldGeneration = true;
            Main.netMode = NetmodeID.MultiplayerClient;
            check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native client cannot build shallow chambers"); Main.netMode = NetmodeID.Server;
            var reserved = new StructureMap(); reserved.AddProtectedStructure(area,2);
            check(!ShallowSpiritChamberSystem.TryBuild(area,reserved),"native shallow chamber preserves protected terrain");
            Tile obstacle = Main.tile[area.X + 5,area.Y + 5]; obstacle.TileType = TileID.Containers;
            try { check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()) && obstacle.TileType == TileID.Containers,"native shallow chamber preserves existing objects"); } finally { obstacle.TileType = TileID.Stone; }
            obstacle.LiquidAmount = 1;
            try { check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native shallow chamber refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.RedWire = true;
            try { check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native shallow chamber refuses wiring"); } finally { obstacle.RedWire = false; }
            obstacle.HasActuator = true;
            try { check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native shallow chamber refuses actuators"); } finally { obstacle.HasActuator = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()),"native shallow chamber preserves housing walls"); } finally { obstacle.WallType = WallID.None; }
            check(!ShallowSpiritChamberSystem.TryBuild(new Rectangle(-1,30,29,15),new StructureMap()),"native shallow chamber rejects invalid bounds");
            check(!ShallowSpiritChamberSystem.TryBuild(new Rectangle(area.X,30,29,14),new StructureMap()),"native shallow chamber rejects incorrect dimensions");
            var map = new StructureMap(); check(ShallowSpiritChamberSystem.TryBuild(area,map),"native shallow chamber builds in its actual existing spirit vein");
            int solid = 0, exposed = 0; bool shape = true;
            for (int dx = 0; dx < 29; dx++) for (int dy = 0; dy < 15; dy++) {
                Tile tile = Main.tile[area.X + dx,area.Y + dy];
                bool expected = dy >= 12 || (dy < 2 && (dx < 5 || dx >= 24)) || ((dx == 0 || dx == 28) && dy >= 2 && dy < 8);
                bool oreCell = (dy == 12 && new[] {3,7,11,17,21,25}.Contains(dx)) || (dy == 13 && dx < 6);
                shape &= tile.HasTile == expected && (!expected || (tile.TileType == (oreCell ? ore : moss) && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock));
                if (tile.HasTile) solid++; if (tile.HasTile && tile.TileType == ore && dy == 12) exposed++;
            }
            check(shape && solid == 119 && exposed == 6 && OreCount() == 12 && ShallowSpiritChamberSystem.OreSeamIntact(area),
                "native shallow chamber retains twelve original ore cells with six exposed six buried and 119 total solid cells");
            foreach (int stand in new[] { 0,14,27 }) {
                var feet = new Vector2((area.X + stand) * 16,(area.Y + 12) * 16);
                check(!Collision.SolidCollision(feet - new Vector2(0,42),20,42,true) && Collision.SolidCollision(feet,20,16,true),
                    "native shallow chamber fits a standing player with support at both entrances and center");
            }
            check(!map.CanPlace(area,2),"native shallow chamber reserves protected footprint");
            var visitor = new Player { whoAmI = 0, active = true }; visitor.Center = new Vector2((area.X + 14) * 16,(area.Y + 9) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.ShallowSpiritVeinsBiome>().IsBiomeActive(visitor),"native shallow chamber alone activates actual server starter biome scan");
            system.Chambers.Add(area); var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Chambers.Count == 1 && system.Chambers[0] == area,"native shallow chamber bounds survive world data roundtrip");
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0; Main.player[0] = visitor;
            int mineX = area.X + 3, mineY = area.Y + 12; WorldGen.KillTile(mineX,mineY);
            int stone = ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>();
            Item[] drops = Main.item.Where(item => item?.active == true && !previousItems.Contains(item) && item.type == stone).ToArray();
            check(!Main.tile[mineX,mineY].HasTile && drops.Length == 1 && drops[0].stack == 1,"native actual mining of exposed chamber ore drops exactly one low-grade spirit stone");
            var state = visitor.GetModPlayer<XianXiaPlayer>(); var beforePickup = state.CaptureSnapshot();
            check(ItemLoader.OnPickup(drops[0],visitor) && state.CaptureSnapshot() == (beforePickup with { Discovered = true, Revision = beforePickup.Revision + 1u }),
                "native mined chamber stone passes official pickup dispatch and awakens spiritual-energy discovery once");
            system.LoadWorldData(tag); check(!Main.tile[mineX,mineY].HasTile,"native shallow chamber load does not regenerate mined ore");
            Main.netMode = NetmodeID.Server;
            Reset(1); check(ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()) && OreCount() == 1 && ShallowSpiritChamberSystem.OreSeamIntact(area),
                "native one-ore source exposes exactly one ore cell without minting others");
            Reset(64); check(ShallowSpiritChamberSystem.TryBuild(area,new StructureMap()) && OreCount() == 64 && ShallowSpiritChamberSystem.OreSeamIntact(area),
                "native maximum-capacity source preserves all sixty-four ore cells");
            tag["shallowSpiritChambers"] = new List<TagCompound> {
                new() { ["x"] = area.X, ["y"] = area.Y }, new() { ["x"] = area.X + 1, ["y"] = area.Y },
                new() { ["x"] = -1, ["y"] = area.Y }, new() { ["x"] = area.X, ["y"] = area.Y }
            };
            system.LoadWorldData(tag);
            check(system.Chambers.Count == 1 && system.Chambers[0] == area,"native shallow chamber load rejects overlapping duplicate and invalid records");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
                tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            foreach (Item item in Main.item.Where(item => item?.active == true && !previousItems.Contains(item))) item.active = false;
            system.LoadWorldData(original); Main.player[0] = previousPlayer; Main.myPlayer = local; Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
