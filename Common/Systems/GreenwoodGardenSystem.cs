using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// A traversable herb clearing in new worlds; saved bounds never rebuild terrain or crops.
public class GreenwoodGardenSystem : ModSystem
{
    internal const int Width = 31, Height = 13, SoilDepth = 4, CoastBuffer = 360;
    internal static readonly int[] CropOffsets = { 3, 7, 11, 19, 23, 27 };
    internal readonly List<Rectangle> Gardens = new();
    // A broken five-column roof and non-solid posts leave both approaches and crops open.
    internal static int ShelterTile(int x, int y)
    {
        if (y == 1 && x >= 13 && x <= 17 && x != 16) return TileID.WoodBlock;
        if ((x == 13 || x == 17) && y >= 2 && y < Height - SoilDepth) return TileID.WoodenBeam;
        if ((x == 14 || x == 15) && y == Height - SoilDepth - 1) return TileID.WorkBenches;
        if (x == 14 && y == Height - SoilDepth - 2) return TileID.Bottles;
        return -1;
    }
    internal static bool ShelterIntact(Rectangle area)
    {
        if (area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.X,area.Y,12)
            || !WorldGen.InWorld(area.Right,area.Bottom,12)) return false;
        for (int x = 13; x <= 17; x++) for (int y = 1; y < Height - SoilDepth; y++) {
            int type = ShelterTile(x,y); if (type < 0) continue;
            Tile tile = Main.tile[area.X + x,area.Y + y];
            if (!tile.HasTile || tile.TileType != type || tile.IsHalfBlock || tile.Slope != SlopeType.Solid) return false;
            if (type == TileID.WorkBenches && (tile.TileFrameX != (x - 14) * 18 || tile.TileFrameY != 0)) return false;
            if (type == TileID.Bottles && (tile.TileFrameX != 0 || tile.TileFrameY != 0)) return false;
        }
        return !Main.tile[area.X + 16,area.Y + 1].HasTile;
    }
    public override void ClearWorld() => Gardens.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["greenwoodGardens"] = Gardens.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Gardens.Clear();
        foreach (var entry in tag.GetList<TagCompound>("greenwoodGardens")) {
            var area = new Rectangle(entry.GetInt("x"),entry.GetInt("y"),Width,Height);
            if (Gardens.Count < 3 && WorldGen.InWorld(area.X,area.Y,12) && area.X <= Main.maxTilesX - Width - 12
                && area.Y <= Main.maxTilesY - Height - 12 && !Gardens.Contains(area)) Gardens.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions)
    {
        Gardens.Clear();
        foreach (Rectangle source in regions) {
            var region = Rectangle.Intersect(new Rectangle(source.X - 40,source.Y - 20,source.Width + 80,source.Height + 40), new Rectangle(12,60,Main.maxTilesX - 24,Main.maxTilesY - 260));
            bool built = false;
            for (int y = region.Top; y <= region.Bottom - Height && !built; y += 3)
                for (int x = region.Left; x <= region.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x,y,Width,Height);
                    if (TryBuild(area,GenVars.structures)) { Gardens.Add(area); built = true; }
                }
        }
        Mod.Logger.Info($"Greenwood garden clearings: {Gardens.Count}/{regions.Count} safely placed.");
    }
    internal static bool TryBuild(Rectangle area, StructureMap structures)
    {
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || area.Left < CoastBuffer || area.Right > Main.maxTilesX - CoastBuffer || !WorldGen.InWorld(area.X,area.Y,12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        // Final liquid settling can drain a nearby pool into newly excavated space.
        // Keep clear of coasts and water sources anywhere above the bed, including tall cave shafts.
        for (int x = System.Math.Max(10,area.Left - 24); x < System.Math.Min(Main.maxTilesX - 10,area.Right + 24); x++)
            for (int y = 10; y < area.Bottom; y++)
                if (Main.tile[x,y].LiquidAmount > 0) return false;
        var allowed = SectRuinHallSystem.NaturalTiles();
        allowed[ModContent.TileType<SectRuinBrickTile>()] = false;
        int soil = ModContent.TileType<GreenwoodSoilTile>(), crop = ModContent.TileType<CultivatedSpiritHerbTile>();
        allowed[soil] = allowed[ModContent.TileType<SpiritHerbTile>()] = true;
        if (!structures.CanPlace(area,allowed,2)) return false;
        int ground = 0;
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y];
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))) return false;
            if (x >= area.Left && x < area.Right && y == area.Bottom - 1 && tile.HasTile && Main.tileSolid[tile.TileType]) ground++;
        }
        // Do not create floating gardens merely because the search encounters empty sky.
        if (ground < Width / 2 + 1) return false;
        int floor = area.Bottom - SoilDepth;
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
            Tile tile = Main.tile[x,y]; tile.HasTile = y >= floor;
            if (tile.HasTile) tile.TileType = (ushort)soil;
            tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = 13; x <= 17; x++) for (int y = 1; y < Height - SoilDepth; y++) {
            int type = ShelterTile(x,y);
            if (type != TileID.WoodBlock && type != TileID.WoodenBeam) continue;
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.HasTile = true; tile.TileType = (ushort)type;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        WorldGen.PlaceObject(area.X + 14,floor - 1,TileID.WorkBenches,mute:true);
        WorldGen.PlaceObject(area.X + 14,floor - 2,TileID.Bottles,mute:true);
        if (!ShelterIntact(area)) ModContent.GetInstance<GreenwoodGardenSystem>().Mod.Logger.Warn("Greenwood garden alchemy shelter could not be fully placed; clearing retained.");
        int planted = 0;
        for (int i = 0; i < CropOffsets.Length; i++) {
            int x = area.X + CropOffsets[i]; WorldGen.PlaceObject(x,floor - 1,crop);
            Tile plant = Main.tile[x,floor - 1];
            if (plant.HasTile && plant.TileType == crop) { plant.TileFrameX = (short)((i % 3) * 18); planted++; }
        }
        if (planted != CropOffsets.Length) ModContent.GetInstance<GreenwoodGardenSystem>().Mod.Logger.Warn($"Greenwood garden crops: {planted}/{CropOffsets.Length} placed.");
        structures.AddProtectedStructure(area,2);
        return true;
    }
}
