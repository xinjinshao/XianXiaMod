using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Commands;

// Console-only, explicit read-only scan. No work is done during normal play.
public class WorldAuditCommand : ModCommand
{
    public override string Command => "xianxia-world-audit";
    public override CommandType Type => CommandType.Console;
    public override string Description => Language.GetTextValue("Mods.XianXia.WorldAudit.Description");
    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var objects = new Dictionary<int, ObjectCount>();
        void Add<T>(int height) where T : ModTile => objects.Add(ModContent.TileType<T>(), new ObjectCount { Name = typeof(T).Name, Height = height });
        Add<SwordTabletTile>(3);
        Add<SingingThunderStoneTile>(2);
        Add<RiftMembraneTile>(2);
        Add<BrokenHeavenTabletTile>(4);
        Add<ArchiveLightPillarTile>(6);
        for (int x = 0; x < Main.maxTilesX; x++)
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (!tile.HasTile || !objects.TryGetValue(tile.TileType, out ObjectCount count)) continue;
                count.Cells++;
                if (tile.TileFrameX != 0 || tile.TileFrameY != 0) continue;
                count.Origins++;
                bool complete = x + 1 < Main.maxTilesX && y + count.Height <= Main.maxTilesY;
                if (complete)
                    for (int column = 0; column < 2; column++)
                        for (int row = 0; row < count.Height; row++)
                        {
                            Tile part = Main.tile[x + column, y + row];
                            if (!part.HasTile || part.TileType != tile.TileType
                                || part.TileFrameX != column * 16 || part.TileFrameY != row * 16) complete = false;
                        }
                if (complete) count.Complete++;
            }
        int supplyChests = 0, spiritStones = 0;
        int stone = ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>();
        foreach (Chest chest in Main.chest) {
            if (chest?.item == null) continue;
            bool found = false;
            foreach (Item item in chest.item)
                if (item != null && item.type == stone && !item.IsAir) { found = true; spiritStones += item.stack; }
            if (found) supplyChests++;
        }
        var hallReports = new List<object>();
        foreach (var area in ModContent.GetInstance<global::XianXia.Common.Systems.SectRuinHallSystem>().Halls) {
            int bricks = 0, blockedInterior = 0, tablets = 0; bool entrances = true, dry = true;
            for (int x = 0; x < area.Width; x++) for (int y = 0; y < area.Height; y++) {
                Tile tile = Main.tile[area.X + x,area.Y + y];
                dry &= tile.LiquidAmount == 0;
                if (tile.HasTile && tile.TileType == ModContent.TileType<SectRuinBrickTile>()) bricks++;
                bool shell = global::XianXia.Common.Systems.SectRuinHallSystem.IsBrick(x,y);
                if (!shell && tile.HasTile && Main.tileSolid[tile.TileType]) blockedInterior++;
                if (!shell && (x < 2 || x >= area.Width - 2)) entrances &= !tile.HasTile;
                if (tile.HasTile && tile.TileType == ModContent.TileType<SwordTabletTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 0) tablets++;
            }
            var hallChests = new List<object>();
            foreach (Chest chest in Main.chest) {
                if (chest == null || chest.x != area.X + 19 || chest.y != area.Bottom - 5) continue;
                bool intact = true;
                for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 2; dy++) {
                    Tile tile = Main.tile[chest.x + dx,chest.y + dy];
                    intact &= tile.HasTile && tile.TileType == Terraria.ID.TileID.Containers && tile.TileFrameX == dx * 18 && tile.TileFrameY == dy * 18;
                }
                hallChests.Add(new { chest.x, chest.y, Intact = intact, Items = chest.item.Where(item => item != null && !item.IsAir)
                    .Select(item => new { Name = item.ModItem?.Name ?? Terraria.ID.ItemID.Search.GetName(item.type), Stack = item.stack }).ToArray() });
            }
            hallReports.Add(new { area.X, area.Y, area.Width, area.Height, Bricks = bricks, BlockedInterior = blockedInterior, EntrancesOpen = entrances, Dry = dry, SwordTablets = tablets, SupplyChests = hallChests });
        }
        var pathReports = new List<object>();
        var halls = ModContent.GetInstance<global::XianXia.Common.Systems.SectRuinHallSystem>().Halls;
        foreach (var path in ModContent.GetInstance<global::XianXia.Common.Systems.SectRuinPathSystem>().Paths) {
            bool clear = true, shell = true, dry = true;
            for (int x = path.Left; x < path.Right; x++) for (int y = path.Top; y < path.Bottom; y++) {
                Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0;
                if (y == path.Top || y == path.Bottom - 1) shell &= tile.HasTile && tile.TileType == ModContent.TileType<SectRuinBrickTile>() && !tile.IsHalfBlock && tile.Slope == 0;
                else clear &= !tile.HasTile;
            }
            bool attached = halls.Any(hall => path.Y == hall.Bottom - 9 && (path.Right == hall.Left || path.Left == hall.Right));
            bool exit = halls.Any(hall => global::XianXia.Common.Systems.SectRuinPathSystem.HasOpenExit(path,hall));
            pathReports.Add(new { path.X, path.Y, path.Width, path.Height, InteriorClear = clear, ShellIntact = shell, Dry = dry, AttachedHall = attached, OpenCavityExit = exit });
        }
        var gardenReports = new List<object>();
        foreach (var garden in ModContent.GetInstance<global::XianXia.Common.Systems.GreenwoodGardenSystem>().Gardens) {
            int floor = garden.Bottom - global::XianXia.Common.Systems.GreenwoodGardenSystem.SoilDepth, soils = 0, crops = 0;
            bool bed = true, clear = true, dry = true; var frames = new List<int>();
            for (int x = garden.Left; x < garden.Right; x++) for (int y = garden.Top; y < garden.Bottom; y++) {
                Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0;
                bool soil = tile.HasTile && tile.TileType == ModContent.TileType<GreenwoodSoilTile>();
                bool crop = tile.HasTile && tile.TileType == ModContent.TileType<CultivatedSpiritHerbTile>();
                if (soil) soils++; if (crop) { crops++; frames.Add(tile.TileFrameX); }
                if (y >= floor) bed &= soil && tile.Slope == 0 && !tile.IsHalfBlock;
                else clear &= !tile.HasTile || crop || tile.TileType == global::XianXia.Common.Systems.GreenwoodGardenSystem.ShelterTile(x - garden.Left,y - garden.Top);
            }
            gardenReports.Add(new { garden.X, garden.Y, garden.Width, garden.Height, SoilTiles = soils, Crops = crops, CropFrames = frames, BedIntact = bed, WalkwayClear = clear, Dry = dry, ShelterIntact = global::XianXia.Common.Systems.GreenwoodGardenSystem.ShelterIntact(garden) });
        }
        var workshopReports = new List<object>();
        foreach (var room in ModContent.GetInstance<global::XianXia.Common.Systems.FurnaceWorkshopSystem>().Workshops) {
            int slag = 0, furnaces = 0, anvils = 0, blocked = 0; bool dry = true, shell = true;
            for (int x = 0; x < room.Width; x++) for (int y = 0; y < room.Height; y++) {
                Tile tile = Main.tile[room.X + x,room.Y + y]; dry &= tile.LiquidAmount == 0;
                bool expected = global::XianXia.Common.Systems.FurnaceWorkshopSystem.IsSlag(x,y);
                bool isSlag = tile.HasTile && tile.TileType == ModContent.TileType<FurnaceSlagTile>();
                if (isSlag) slag++; if (expected) shell &= isSlag && tile.Slope == 0 && !tile.IsHalfBlock;
                else if (tile.HasTile) { if (tile.TileType == TileID.Furnaces) furnaces++; else if (tile.TileType == TileID.Anvils) anvils++; else blocked++; }
            }
            workshopReports.Add(new { room.X,room.Y,room.Width,room.Height,SlagTiles = slag,FurnaceCells = furnaces,AnvilCells = anvils,BlockedInterior = blocked,ShellIntact = shell,Dry = dry,StationsIntact = global::XianXia.Common.Systems.FurnaceWorkshopSystem.StationsIntact(room) });
        }
        var workshopPaths = new List<object>();
        foreach (var path in ModContent.GetInstance<global::XianXia.Common.Systems.FurnaceWorkshopPathSystem>().Paths) {
            bool clear = true,shell = true,dry = true;
            for (int x = path.Left; x < path.Right; x++) for (int y = path.Top; y < path.Bottom; y++) {
                Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0;
                if (y == path.Top || y == path.Bottom - 1) shell &= tile.HasTile && tile.TileType == ModContent.TileType<FurnaceSlagTile>() && tile.Slope == 0 && !tile.IsHalfBlock;
                else clear &= !tile.HasTile;
            }
            bool exit = ModContent.GetInstance<global::XianXia.Common.Systems.FurnaceWorkshopSystem>().Workshops.Any(room => global::XianXia.Common.Systems.FurnaceWorkshopPathSystem.HasOpenExit(path,room));
            workshopPaths.Add(new { path.X,path.Y,path.Width,path.Height,InteriorClear = clear,ShellIntact = shell,Dry = dry,OpenCavityExit = exit });
        }
        var shrineReports = new List<object>();
        foreach (var area in ModContent.GetInstance<global::XianXia.Common.Systems.ThunderCloudShrineSystem>().Shrines) {
            int floor = area.Bottom - global::XianXia.Common.Systems.ThunderCloudShrineSystem.FloorDepth, clouds = 0;
            bool platform = true, clear = true, dry = true;
            for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
                Tile tile = Main.tile[x,y]; dry &= tile.LiquidAmount == 0;
                bool cloud = tile.HasTile && tile.TileType == ModContent.TileType<ThunderCloudTile>();
                if (cloud) clouds++;
                if (y >= floor) platform &= cloud && tile.Slope == 0 && !tile.IsHalfBlock;
                else clear &= !tile.HasTile || (tile.TileType == ModContent.TileType<SingingThunderStoneTile>()
                    && x >= area.X + 24 && x < area.X + 26 && y >= floor - 2);
            }
            shrineReports.Add(new { area.X,area.Y,area.Width,area.Height,CloudTiles = clouds,PlatformIntact = platform,AirspaceClear = clear,Dry = dry,
                StoneIntact = global::XianXia.Common.Systems.ThunderCloudShrineSystem.StoneIntact(area) });
        }
        string report = JsonSerializer.Serialize(new { Width = Main.maxTilesX, Height = Main.maxTilesY, Objects = objects.Values,
            ThunderCloudShrines = shrineReports,FurnaceWorkshops = workshopReports,FurnaceWorkshopPaths = workshopPaths, SectRuinHalls = hallReports, SectRuinPaths = pathReports, GreenwoodGardens = gardenReports, ChestsWithSpiritStones = supplyChests, ChestSpiritStoneTotal = spiritStones });
        Mod.Logger.Info("SavedWorldAudit: " + report);
        caller.Reply(report);
    }

    private sealed class ObjectCount
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Cells { get; set; }
        public int Origins { get; set; }
        public int Complete { get; set; }
    }
}
