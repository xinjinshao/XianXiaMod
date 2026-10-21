using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;
namespace XianXia.Common.Systems;
// New-world exploration space; persisted bounds are descriptive and never refill or repair terrain.
public class FurnaceWorkshopSystem : ModSystem {
    internal const int Width = 35, Height = 17;
    internal readonly List<Rectangle> Workshops = new();
    public override void ClearWorld() => Workshops.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["furnaceWorkshops"] = Workshops.ConvertAll(r => new TagCompound { ["x"] = r.X, ["y"] = r.Y });
    public override void LoadWorldData(TagCompound tag) {
        Workshops.Clear();
        foreach (var entry in tag.GetList<TagCompound>("furnaceWorkshops")) {
            var area = new Rectangle(entry.GetInt("x"),entry.GetInt("y"),Width,Height);
            if (Workshops.Count < 3 && WorldGen.InWorld(area.X,area.Y,12) && area.X <= Main.maxTilesX - Width - 12 && area.Y <= Main.maxTilesY - Height - 12 && !Workshops.Exists(existing => existing.Intersects(area))) Workshops.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions) {
        Workshops.Clear();
        if (Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration) return;
        foreach (var source in regions) {
            if (Workshops.Count == 3) break;
            var region = Rectangle.Intersect(new Rectangle(source.X - 40,source.Y - 20,source.Width + 80,source.Height + 40),
                new Rectangle(12,(int)Main.rockLayer,Main.maxTilesX - 24,Main.maxTilesY - 260 - (int)Main.rockLayer));
            bool built = false;
            for (int y = region.Top; y <= region.Bottom - Height && !built; y += 3)
                for (int x = region.Left; x <= region.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x,y,Width,Height);
                    if (TryBuild(area,GenVars.structures)) { Workshops.Add(area); built = true; }
                }
        }
        Mod.Logger.Info($"Furnace workshops: {Workshops.Count}/{regions.Count} safely placed.");
    }
    internal static bool StationsIntact(Rectangle area) {
        int floor = area.Bottom - 4;
        for (int dx = 0; dx < 3; dx++) for (int dy = 0; dy < 2; dy++) {
            Tile tile = Main.tile[area.Left + 8 + dx,floor - 2 + dy];
            if (!tile.HasTile || tile.TileType != TileID.Furnaces || tile.TileFrameX != dx * 18 || tile.TileFrameY != dy * 18) return false;
        }
        for (int dx = 0; dx < 2; dx++) {
            Tile tile = Main.tile[area.Left + 24 + dx,floor - 1];
            if (!tile.HasTile || tile.TileType != TileID.Anvils || tile.TileFrameX != dx * 18 || tile.TileFrameY != 0) return false;
        }
        return true;
    }
    internal static bool IsSlag(int x,int y) => y < 2 || y >= Height - 4 || ((x < 2 || x >= Width - 2) && y < Height - 8);
    internal static bool TryBuild(Rectangle area,StructureMap structures) {
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.X,area.Y,12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        var allowed = SectRuinHallSystem.NaturalTiles(); allowed[ModContent.TileType<SectRuinBrickTile>()] = false;
        int slag = ModContent.TileType<FurnaceSlagTile>(); allowed[slag] = true;
        if (!structures.CanPlace(area,allowed,2)) return false;
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y];
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType])) return false;
        }
        for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.HasTile = IsSlag(x,y);
            if (tile.HasTile) tile.TileType = (ushort)slag;
            tile.IsHalfBlock = false; tile.Slope = SlopeType.Solid; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        int floor = area.Bottom - 4;
        WorldGen.PlaceObject(area.Left + 9,floor - 1,TileID.Furnaces,mute:true);
        WorldGen.PlaceObject(area.Left + 24,floor - 1,TileID.Anvils,mute:true);
        structures.AddProtectedStructure(area,2);
        return true;
    }
}
