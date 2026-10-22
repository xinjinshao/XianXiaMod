using System;
using System.Collections.Generic;
using System.Linq;
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
    private static void AuditGreenwoodGardens(Action<bool,string> check)
    {
        var area = new Rectangle(Main.maxTilesX / 2 + 280,30,GreenwoodGardenSystem.Width,GreenwoodGardenSystem.Height);
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>(); bool dry = true;
        for (int x = area.Left - 4; x < area.Right + 4; x++) for (int y = area.Top - 4; y < area.Bottom + 4; y++) {
            Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0 && tile.WallType == 0 && !tile.HasActuator && !tile.RedWire && !tile.BlueWire && !tile.GreenWire && !tile.YellowWire;
            saved.Add((x,y,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        check(dry,"native garden fixture dry unoccupied sky");
        var system = ModContent.GetInstance<GreenwoodGardenSystem>(); var original = new TagCompound(); system.SaveWorldData(original);
        var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration; int mode = Main.netMode; var random = Main.rand;
        void Reset(bool solid = true) { foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = solid; tile.TileType = TileID.Stone; tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; } }
        try {
            Main.netMode = NetmodeID.Server; config.EnableWorldGeneration = true; Main.rand = new Terraria.Utilities.UnifiedRandom(20261020);
            Reset(false); check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native garden does not float in unsupported empty sky");
            Reset(); var protectedMap = new StructureMap(); protectedMap.AddProtectedStructure(area,2);
            check(!GreenwoodGardenSystem.TryBuild(area,protectedMap) && Main.tile[area.X,area.Y].TileType == TileID.Stone,"native garden preserves protected terrain");
            config.EnableWorldGeneration = false; check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native disabled generation skips garden");
            config.EnableWorldGeneration = true; Main.netMode = NetmodeID.MultiplayerClient;
            check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native client does not generate garden"); Main.netMode = NetmodeID.Server;
            Tile obstacle = Main.tile[area.X + 4,area.Y + 4]; obstacle.TileType = TileID.Containers;
            try { check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()) && obstacle.TileType == TileID.Containers,"native garden preserves existing chest tile"); } finally { obstacle.TileType = TileID.Stone; }
            obstacle.LiquidAmount = 1;
            try { check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native garden refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            Tile nearbyWater = Main.tile[area.Left - 3,area.Top]; nearbyWater.LiquidAmount = 1;
            try { check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native garden refuses nearby water outside immediate framing buffer"); } finally { nearbyWater.LiquidAmount = 0; }
            obstacle.RedWire = true;
            try { check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native garden refuses wiring"); } finally { obstacle.RedWire = false; }
            obstacle.WallType = WallID.Wood;
            try { check(!GreenwoodGardenSystem.TryBuild(area,new StructureMap()),"native garden refuses housing walls"); } finally { obstacle.WallType = 0; }
            check(!GreenwoodGardenSystem.TryBuild(new Rectangle(-1,30,31,13),new StructureMap()),"native garden rejects invalid bounds");
            check(!GreenwoodGardenSystem.TryBuild(new Rectangle(20,30,31,13),new StructureMap()),"native garden keeps dry cultivation clearings out of the coastal margin");
            var map = new StructureMap(); check(GreenwoodGardenSystem.TryBuild(area,map),"native garden builds approved terrain");
            int soil = ModContent.TileType<GreenwoodSoilTile>(), crop = ModContent.TileType<CultivatedSpiritHerbTile>(), soilCount = 0, plantCount = 0;
            int floor = area.Bottom - GreenwoodGardenSystem.SoilDepth; bool geometry = true;
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile tile = Main.tile[x,y];
                if (y >= floor) geometry &= tile.HasTile && tile.TileType == soil && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock;
                else {
                    int dx = x - area.X, dy = y - area.Y;
                    bool roof = dy == 1 && (dx == 13 || dx == 14 || dx == 15 || dx == 17) && tile.TileType == TileID.WoodBlock;
                    bool post = (dx == 13 || dx == 17) && dy >= 2 && dy <= 8 && tile.TileType == TileID.WoodenBeam;
                    bool bench = (dx == 14 || dx == 15) && dy == 8 && tile.TileType == TileID.WorkBenches;
                    bool bottle = dx == 14 && dy == 7 && tile.TileType == TileID.Bottles;
                    geometry &= !tile.HasTile || tile.TileType == crop || roof || post || bench || bottle;
                }
                if (tile.HasTile && tile.TileType == soil) soilCount++;
                if (tile.HasTile && tile.TileType == crop) plantCount++;
            }
            check(geometry && soilCount == 124 && plantCount == 6,"native garden flat four-row bed clear walking space and six actual crops");
            check(GreenwoodGardenSystem.ShelterIntact(area),"native garden has a broken wood roof non-solid posts and complete vanilla bench/bottle frames");
            check(!Main.tileSolid[TileID.WoodenBeam] && !Main.tile[area.X, floor - 2].HasTile && !Main.tile[area.Right - 1, floor - 2].HasTile,
                "native garden shelter posts do not block either walking approach");
            Tile bottleTile = Main.tile[area.X + 14,floor - 2]; short bottleFrame = bottleTile.TileFrameX;
            bottleTile.TileFrameX = 18;
            try { check(!GreenwoodGardenSystem.ShelterIntact(area),"native garden shelter audit rejects a wrong bottle frame"); }
            finally { bottleTile.TileFrameX = bottleFrame; }
            for (int i = 0; i < GreenwoodGardenSystem.CropOffsets.Length; i++) {
                int x = area.X + GreenwoodGardenSystem.CropOffsets[i]; Tile plant = Main.tile[x,floor - 1];
                check(plant.HasTile && plant.TileType == crop && plant.TileFrameX == (i % 3) * 18 && plant.TileFrameY == 0
                    && ModContent.GetInstance<CultivatedSpiritHerbTile>().GetMapOption(x,floor - 1) == i % 3,"native garden plant frames match seedling growing and mature map options");
            }
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + area.Width / 2) * 16,(floor - 3) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>().IsBiomeActive(visitor),"native garden activates actual server herb biome scan");
            var drops = ModContent.GetInstance<CultivatedSpiritHerbTile>().GetItemDrops(area.X + 11,floor - 1).ToArray();
            check(drops.Length == 2 && drops.Any(item => item.type == ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>() && item.stack == 1)
                && drops.Any(item => item.type == ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritHerbSeeds>() && item.stack >= 1 && item.stack <= 3),"native mature garden crop uses existing root and seed harvest hooks");
            check(!map.CanPlace(area,2),"native garden reserves protected structure area");
            system.Gardens.Clear(); system.Gardens.Add(area); var tag = new TagCompound(); system.SaveWorldData(tag); system.ClearWorld(); system.LoadWorldData(tag);
            check(system.Gardens.Count == 1 && system.Gardens[0] == area,"native garden bounds persist");
            Tile removed = Main.tile[area.X + 11,floor - 1]; removed.HasTile = false; system.LoadWorldData(tag);
            check(!removed.HasTile,"native garden loading does not replant harvested crops");
            Player originalVisitor = Main.player[0]; int originalLocal = Main.myPlayer;
            try {
                Main.player[0] = new Player { whoAmI = 0, active = true };
                Main.player[0].Center = new Vector2((area.X + 15) * 16,(floor - 2) * 16);
                Main.myPlayer = 0; Main.netMode = NetmodeID.SinglePlayer;
                Main.player[0].AdjTiles();
                check(Main.player[0].adjTile[TileID.WorkBenches] && Main.player[0].adjTile[TileID.Bottles],
                    "native player adjacency recognizes both garden workbench and bottle crafting stations");
                bottleTile.HasTile = false; Main.player[0].AdjTiles();
                check(Main.player[0].adjTile[TileID.WorkBenches] && !Main.player[0].adjTile[TileID.Bottles],
                    "native player adjacency loses bottle recipes when the garden bottle is removed");
            } finally { Main.player[0] = originalVisitor; Main.myPlayer = originalLocal; Main.netMode = NetmodeID.Server; }
            system.LoadWorldData(tag);
            check(!bottleTile.HasTile && !GreenwoodGardenSystem.ShelterIntact(area),"native garden load never rebuilds a removed alchemy bottle");
        } finally {
            foreach (var cell in saved) { Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type; tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half; }
            system.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled; Main.rand = random;
        }
    }
}
