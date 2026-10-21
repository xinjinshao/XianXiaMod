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
public partial class SummonAuditSystem {
    private static void AuditFurnaceWorkshops(Action<bool,string> check) {
        var area = new Rectangle(Main.maxTilesX / 2,100,FurnaceWorkshopSystem.Width,FurnaceWorkshopSystem.Height);
        var saved = new List<(int X,int Y,bool Has,ushort Type,short FX,short FY,SlopeType Slope,bool Half)>();
        for (int x = area.Left - 3; x < area.Right + 3; x++) for (int y = area.Top - 3; y < area.Bottom + 3; y++) {
            var t = Main.tile[x,y]; saved.Add((x,y,t.HasTile,t.TileType,t.TileFrameX,t.TileFrameY,t.Slope,t.IsHalfBlock));
        }
        var rooms = ModContent.GetInstance<FurnaceWorkshopSystem>(); var original = new TagCompound(); rooms.SaveWorldData(original);
        int mode = Main.netMode; var config = ModContent.GetInstance<XianXiaConfig>(); bool enabled = config.EnableWorldGeneration;
        try {
            check(saved.TrueForAll(c => Main.tile[c.X,c.Y].LiquidAmount == 0 && Main.tile[c.X,c.Y].WallType == 0 && !Main.tile[c.X,c.Y].HasActuator && !Main.tile[c.X,c.Y].RedWire && !Main.tile[c.X,c.Y].BlueWire && !Main.tile[c.X,c.Y].GreenWire && !Main.tile[c.X,c.Y].YellowWire),"native workshop fixture is dry unwired sky");
            foreach (var c in saved) { var t = Main.tile[c.X,c.Y]; t.HasTile = false; }
            config.EnableWorldGeneration = false; check(!FurnaceWorkshopSystem.TryBuild(area,new StructureMap()),"native disabled generation never builds workshops"); config.EnableWorldGeneration = true;
            Main.netMode = NetmodeID.MultiplayerClient; check(!FurnaceWorkshopSystem.TryBuild(area,new StructureMap()),"native client cannot build workshop"); Main.netMode = NetmodeID.Server;
            var blocked = new StructureMap(); blocked.AddProtectedStructure(area,2); check(!FurnaceWorkshopSystem.TryBuild(area,blocked),"native workshop preserves reserved structures");
            Tile obstacle = Main.tile[area.X + 5,area.Y + 5]; obstacle.HasTile = true; obstacle.TileType = TileID.Containers;
            check(!FurnaceWorkshopSystem.TryBuild(area,new StructureMap()) && obstacle.TileType == TileID.Containers,"native workshop preserves artificial objects"); obstacle.HasTile = false;
            obstacle.LiquidAmount = 1; try { check(!FurnaceWorkshopSystem.TryBuild(area,new StructureMap()),"native workshop refuses liquid"); } finally { obstacle.LiquidAmount = 0; }
            obstacle.RedWire = true; try { check(!FurnaceWorkshopSystem.TryBuild(area,new StructureMap()),"native workshop refuses wire"); } finally { obstacle.RedWire = false; }
            obstacle.WallType = WallID.Wood; try { check(!FurnaceWorkshopSystem.TryBuild(area,new StructureMap()),"native workshop preserves housing walls"); } finally { obstacle.WallType = 0; }
            check(!FurnaceWorkshopSystem.TryBuild(new Rectangle(-1,100,35,17),new StructureMap()),"native workshop rejects malformed bounds");
            var map = new StructureMap(); check(FurnaceWorkshopSystem.TryBuild(area,map),"native workshop builds dry natural room"); int slag = 0, furnace = 0, anvil = 0; bool clear = true;
            for (int x = 0; x < area.Width; x++) for (int y = 0; y < area.Height; y++) {
                Tile t = Main.tile[area.X + x,area.Y + y]; bool shell = FurnaceWorkshopSystem.IsSlag(x,y);
                if (shell) { check(t.HasTile && t.TileType == ModContent.TileType<FurnaceSlagTile>() && t.Slope == SlopeType.Solid && !t.IsHalfBlock,"native workshop shell is complete flat slag"); slag++; }
                else if(t.HasTile) { if(t.TileType == TileID.Furnaces) furnace++; else if(t.TileType == TileID.Anvils) anvil++; else clear = false; }
            }
            check(clear && slag == 238 && furnace == 6 && anvil == 2,"native workshop keeps two entrances and complete furnace/anvil footprints");
            check(FurnaceWorkshopSystem.StationsIntact(area),"native furnace and anvil exact vanilla multi-tile frames intact");
            Tile badFrame = Main.tile[area.X + 8,area.Bottom - 6]; short frame = badFrame.TileFrameX; badFrame.TileFrameX += 18;
            try { check(!FurnaceWorkshopSystem.StationsIntact(area),"native station audit rejects corrupt frame despite unchanged cell counts"); } finally { badFrame.TileFrameX = frame; }
            var visitor = new Player { active = true }; visitor.Center = new Vector2((area.X + area.Width / 2) * 16,(area.Y + area.Height / 2) * 16);
            check(ModContent.GetInstance<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>().IsBiomeActive(visitor),"native workshop alone activates real furnace biome threshold");
            check(!map.CanPlace(area,2),"native workshop reserves built footprint");
            rooms.Workshops.Clear(); rooms.Workshops.Add(area); var tag = new TagCompound(); rooms.SaveWorldData(tag); rooms.ClearWorld(); rooms.LoadWorldData(tag);
            check(rooms.Workshops.Count == 1 && rooms.Workshops[0] == area,"native workshop bounds persist through actual TagCompound");
            var invalid = new TagCompound { ["furnaceWorkshops"] = new List<TagCompound> {
                new() { ["x"] = area.X,["y"] = area.Y }, new() { ["x"] = area.X,["y"] = area.Y }, new() { ["x"] = area.X + 1,["y"] = area.Y },
                new() { ["x"] = -1,["y"] = area.Y }, new() { ["x"] = Main.maxTilesX - 10,["y"] = area.Y },
                new() { ["x"] = area.X + 100,["y"] = area.Y }, new() { ["x"] = area.X + 200,["y"] = area.Y }, new() { ["x"] = area.X + 300,["y"] = area.Y } } };
            rooms.LoadWorldData(invalid); check(rooms.Workshops.Count == 3 && rooms.Workshops[0] == area && rooms.Workshops[1].X == area.X + 100 && rooms.Workshops[2].X == area.X + 200,"native workshop load rejects overlap duplicates bad bounds and excess records");
            Tile removed = Main.tile[area.X + 8,area.Bottom - 6]; removed.HasTile = false; rooms.LoadWorldData(tag); check(!removed.HasTile,"native workshop load never repairs removed station tiles");
        } finally {
            foreach(var c in saved) { Tile t = Main.tile[c.X,c.Y]; t.HasTile = c.Has; t.TileType = c.Type; t.TileFrameX = c.FX; t.TileFrameY = c.FY; t.Slope = c.Slope; t.IsHalfBlock = c.Half; }
            rooms.LoadWorldData(original); Main.netMode = mode; config.EnableWorldGeneration = enabled;
        }
    }
}
