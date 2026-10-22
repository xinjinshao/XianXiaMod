using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// Ruined exploration courts on final new-world terrain; saved descriptions never repair tiles.
public class FallenHeavenCourtSystem : ModSystem
{
    internal const int Width = 49, Height = 23, FloorDepth = 3, TabletOffset = 10, PillarOffset = 35;
    internal readonly List<Rectangle> Courts = new();
    public override void ClearWorld() => Courts.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["fallenHeavenCourts"] = Courts.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Courts.Clear();
        foreach (TagCompound entry in tag.GetList<TagCompound>("fallenHeavenCourts")) {
            var area = new Rectangle(entry.GetInt("x"),entry.GetInt("y"),Width,Height);
            if (Courts.Count < 2 && WorldGen.InWorld(area.X,area.Y,12)
                && area.X <= Main.maxTilesX - Width - 12 && area.Y <= Main.maxTilesY - Height - 12
                && !Courts.Exists(other => other.Intersects(area))) Courts.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions)
    {
        Courts.Clear();
        var refusals = new Dictionary<string,int>();
        foreach (Rectangle source in regions) {
            if (Courts.Count == 2) break;
            Rectangle search = Rectangle.Intersect(new Rectangle(source.X - 80,source.Y - 40,source.Width + 160,source.Height + 80),
                new Rectangle(12,(int)Main.rockLayer,Main.maxTilesX - 24,Main.maxTilesY - 200 - (int)Main.rockLayer));
            bool built = false;
            for (int y = search.Top; y <= search.Bottom - Height && !built; y += 3)
                for (int x = search.Left; x <= search.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x,y,Width,Height);
                    if (TryBuild(area,GenVars.structures,out string reason)) { Courts.Add(area); built = true; }
                    else refusals[reason] = refusals.GetValueOrDefault(reason) + 1;
                }
        }
        Mod.Logger.Info($"Fallen heaven courts: {Courts.Count}/{regions.Count} safely placed.");
        foreach (var refusal in refusals) Mod.Logger.Debug($"Heaven court rejected {refusal.Key}: {refusal.Value}");
    }
    internal static bool IsJade(int x,int y) => y >= Height - FloorDepth
        || (y < 2 && (x < 12 || x >= Width - 12))
        || ((x < 2 || x >= Width - 2) && y < Height - 8);

    internal static bool TryBuild(Rectangle area,StructureMap structures) => TryBuild(area,structures,out _);
    private static bool TryBuild(Rectangle area,StructureMap structures,out string reason)
    {
        reason = "configuration mode or bounds";
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.X,area.Y,12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        int jade = ModContent.TileType<FallenHeavenJadeTile>();
        var allowed = SectRuinHallSystem.NaturalTiles(); allowed[ModContent.TileType<SectRuinBrickTile>()] = false; allowed[jade] = true;
        if (!structures.CanPlace(area,allowed,2)) {
            var any = new bool[allowed.Length]; System.Array.Fill(any,true);
            reason = structures.CanPlace(area,any,2) ? "non-natural tile" : "protected structure";
            if (reason == "non-natural tile")
                for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
                    Tile tile = Main.tile[x,y];
                    if (tile.TileType >= allowed.Length || !allowed[tile.TileType]) {
                        reason = $"tile {tile.TileType} active={tile.HasTile}";
                        return false;
                    }
                }
            return false;
        }
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y];
            reason = tile.LiquidAmount > 0 ? "liquid" : "housing wall wiring or occupied tile";
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator
                || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))) return false;
        }
        for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.HasTile = IsJade(x,y);
            if (tile.HasTile) tile.TileType = (ushort)jade;
            tile.IsHalfBlock = false; tile.Slope = SlopeType.Solid; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        structures.AddProtectedStructure(area,2);
        reason = "placed";
        return true;
    }
    internal static bool IsPlannedObject(int tileType,int height,int x,int groundY)
    {
        int offset = tileType == ModContent.TileType<BrokenHeavenTabletTile>() && height == 4 ? TabletOffset
            : tileType == ModContent.TileType<ArchiveLightPillarTile>() && height == 6 ? PillarOffset : -1;
        if (offset < 0) return false;
        foreach (Rectangle area in ModContent.GetInstance<FallenHeavenCourtSystem>().Courts)
            if (x == area.X + offset && groundY == area.Bottom - FloorDepth) return true;
        return false;
    }
    internal static bool ObjectIntact(Rectangle area,bool pillar)
    {
        int height = pillar ? 6 : 4, offset = pillar ? PillarOffset : TabletOffset;
        int type = pillar ? ModContent.TileType<ArchiveLightPillarTile>() : ModContent.TileType<BrokenHeavenTabletTile>();
        int top = area.Bottom - FloorDepth - height;
        for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < height; dy++) {
            Tile tile = Main.tile[area.X + offset + dx,top + dy];
            if (!tile.HasTile || tile.TileType != type || tile.TileFrameX != dx * 16 || tile.TileFrameY != dy * 16) return false;
        }
        return true;
    }
}
