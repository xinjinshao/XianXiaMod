using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// Raised lunar landing with two-step shoulders and broken overhead ribs in new worlds.
public class MoonboneTerraceSystem : ModSystem
{
    internal const int Width = 47, Height = 23;
    internal readonly List<Rectangle> Terraces = new();
    public override void ClearWorld() => Terraces.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["moonboneTerraces"] = Terraces.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Terraces.Clear();
        foreach (TagCompound entry in tag.GetList<TagCompound>("moonboneTerraces")) {
            var area = new Rectangle(entry.GetInt("x"),entry.GetInt("y"),Width,Height);
            if (Terraces.Count < 2 && WorldGen.InWorld(area.X,area.Y,12)
                && area.X <= Main.maxTilesX - Width - 12 && area.Y <= Main.maxTilesY - Height - 12
                && !Terraces.Exists(other => other.Intersects(area))) Terraces.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions)
    {
        Terraces.Clear();
        var refusals = new Dictionary<string,int>();
        foreach (Rectangle source in regions) {
            if (Terraces.Count == 2) break;
            Rectangle search = Rectangle.Intersect(new Rectangle(source.X - 80,source.Y - 80,source.Width + 160,source.Height + 160),
                new Rectangle(12,(int)Main.rockLayer,Main.maxTilesX - 24,Main.maxTilesY - 180 - (int)Main.rockLayer));
            bool built = false;
            for (int y = search.Top; y <= search.Bottom - Height && !built; y += 3)
                for (int x = search.Left; x <= search.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x,y,Width,Height);
                    if (TryBuild(area,GenVars.structures,out string reason)) { Terraces.Add(area); built = true; }
                    else refusals[reason] = refusals.GetValueOrDefault(reason) + 1;
                }
        }
        Mod.Logger.Info($"Moonbone terraces: {Terraces.Count}/{regions.Count} safely placed.");
        foreach (var refusal in refusals) Mod.Logger.Debug($"Moonbone terrace rejected {refusal.Key}: {refusal.Value}");
    }
    internal static bool IsBone(int x,int y) => y >= 19
        || (x >= 17 && x <= 29 && y >= 15)
        || (x >= 13 && x <= 33 && y >= 17)
        || (y < 2 && (x < 10 || x >= 37))
        || (y >= 2 && y <= 6 && ((x >= 2 && x <= 3) || (x >= 43 && x <= 44)));

    internal static bool TryBuild(Rectangle area,StructureMap structures) => TryBuild(area,structures,out _);
    private static bool TryBuild(Rectangle area,StructureMap structures,out string reason)
    {
        reason = "configuration mode or bounds";
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.X,area.Y,12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        int bone = ModContent.TileType<MoonboneTile>();
        var allowed = SectRuinHallSystem.NaturalTiles(); allowed[ModContent.TileType<SectRuinBrickTile>()] = false; allowed[bone] = true;
        if (!structures.CanPlace(area,allowed,2)) {
            var any = new bool[allowed.Length]; System.Array.Fill(any,true);
            reason = structures.CanPlace(area,any,2) ? "non-natural tile" : "protected structure";
            if (reason == "non-natural tile")
                for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
                    Tile tile = Main.tile[x,y];
                    if (tile.TileType >= allowed.Length || !allowed[tile.TileType]) { reason = $"tile {tile.TileType} active={tile.HasTile}"; return false; }
                }
            return false;
        }
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y]; reason = tile.LiquidAmount > 0 ? "liquid" : "housing wall wiring or occupied tile";
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator
                || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))) return false;
        }
        for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.HasTile = IsBone(x,y);
            if (tile.HasTile) tile.TileType = (ushort)bone;
            tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        structures.AddProtectedStructure(area,2); reason = "placed";
        return true;
    }
}
