using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// A broken crystal crossing above a dry lower landing, not a new damage source.
public class StarAbyssBridgeSystem : ModSystem
{
    internal const int Width = 45, Height = 27, BridgeRow = 15, MembraneOffset = 21;
    internal readonly List<Rectangle> Bridges = new();
    public override void ClearWorld() => Bridges.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["starAbyssBridges"] = Bridges.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Bridges.Clear();
        foreach (TagCompound entry in tag.GetList<TagCompound>("starAbyssBridges")) {
            var area = new Rectangle(entry.GetInt("x"),entry.GetInt("y"),Width,Height);
            if (Bridges.Count < 3 && WorldGen.InWorld(area.X,area.Y,12)
                && area.X <= Main.maxTilesX - Width - 12 && area.Y <= Main.maxTilesY - Height - 12
                && !Bridges.Exists(other => other.Intersects(area))) Bridges.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions)
    {
        Bridges.Clear();
        var refusals = new Dictionary<string,int>();
        foreach (Rectangle source in regions) {
            if (Bridges.Count == 3) break;
            Rectangle search = Rectangle.Intersect(new Rectangle(source.X - 80,source.Y - 40,source.Width + 160,source.Height + 80),
                new Rectangle(12,(int)Main.rockLayer,Main.maxTilesX - 24,Main.maxTilesY - 180 - (int)Main.rockLayer));
            bool built = false;
            for (int y = search.Top; y <= search.Bottom - Height && !built; y += 3)
                for (int x = search.Left; x <= search.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x,y,Width,Height);
                    if (TryBuild(area,GenVars.structures,out string reason)) { Bridges.Add(area); built = true; }
                    else refusals[reason] = refusals.GetValueOrDefault(reason) + 1;
                }
        }
        Mod.Logger.Info($"Star abyss bridges: {Bridges.Count}/{regions.Count} safely placed.");
        foreach (var refusal in refusals) Mod.Logger.Debug($"Star bridge rejected {refusal.Key}: {refusal.Value}");
    }
    internal static bool IsCrystal(int x,int y)
    {
        bool upperDoor = y >= 11 && y < 15, lowerDoor = y >= 20 && y < 25;
        bool gap = (x >= 8 && x <= 10) || (x >= 34 && x <= 36);
        return y >= 25 || (y < 2 && (x < 9 || x >= 36))
            || ((y == 15 || y == 16) && !gap)
            || ((x < 2 || x >= Width - 2) && !upperDoor && !lowerDoor);
    }
    internal static bool TryBuild(Rectangle area,StructureMap structures) => TryBuild(area,structures,out _);
    private static bool TryBuild(Rectangle area,StructureMap structures,out string reason)
    {
        reason = "configuration mode or bounds";
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.X,area.Y,12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        int crystal = ModContent.TileType<StarAbyssCrystalTile>();
        var allowed = SectRuinHallSystem.NaturalTiles(); allowed[ModContent.TileType<SectRuinBrickTile>()] = false; allowed[crystal] = true;
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
            Tile tile = Main.tile[x,y];
            reason = tile.LiquidAmount > 0 ? "liquid" : "housing wall wiring or occupied tile";
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator
                || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))) return false;
        }
        for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.HasTile = IsCrystal(x,y);
            if (tile.HasTile) tile.TileType = (ushort)crystal;
            tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        structures.AddProtectedStructure(area,2);
        reason = "placed";
        return true;
    }
    internal static bool IsPlannedMembrane(int x,int groundY)
    {
        foreach (Rectangle area in ModContent.GetInstance<StarAbyssBridgeSystem>().Bridges)
            if (x == area.X + MembraneOffset && groundY == area.Y + BridgeRow) return true;
        return false;
    }
    internal static bool MembraneIntact(Rectangle area)
    {
        int type = ModContent.TileType<RiftMembraneTile>();
        for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 2; dy++) {
            Tile tile = Main.tile[area.X + MembraneOffset + dx,area.Y + BridgeRow - 2 + dy];
            if (!tile.HasTile || tile.TileType != type || tile.TileFrameX != dx * 16 || tile.TileFrameY != dy * 16) return false;
        }
        return true;
    }
}
