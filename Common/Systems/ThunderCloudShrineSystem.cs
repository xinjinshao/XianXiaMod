using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// New-world landing space inside existing thunder clouds. Loading never rebuilds it.
public class ThunderCloudShrineSystem : ModSystem
{
    internal const int Width = 51, Height = 13, FloorDepth = 3, StoneOffset = 24;
    internal readonly List<Rectangle> Shrines = new();
    public override void ClearWorld() => Shrines.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["thunderCloudShrines"] = Shrines.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Shrines.Clear();
        foreach (TagCompound entry in tag.GetList<TagCompound>("thunderCloudShrines")) {
            var area = new Rectangle(entry.GetInt("x"), entry.GetInt("y"), Width, Height);
            if (Shrines.Count < 3 && WorldGen.InWorld(area.X, area.Y, 12)
                && area.X <= Main.maxTilesX - Width - 12 && area.Y <= Main.maxTilesY - Height - 12
                && !Shrines.Exists(other => other.Intersects(area))) Shrines.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions)
    {
        Shrines.Clear();
        foreach (Rectangle source in regions) {
            if (Shrines.Count == 3) break;
            Rectangle search = Rectangle.Intersect(new Rectangle(source.X - 24, source.Y - 12, source.Width + 48, source.Height + 24),
                new Rectangle(24, 80, Main.maxTilesX - 48, System.Math.Max(0, (int)Main.worldSurface - 104)));
            bool built = false;
            for (int y = search.Top; y <= search.Bottom - Height && !built; y += 2)
                for (int x = search.Left; x <= search.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x, y, Width, Height);
                    if (TryBuild(area, GenVars.structures)) { Shrines.Add(area); built = true; }
                }
        }
        Mod.Logger.Info($"Thunder cloud shrines: {Shrines.Count}/{regions.Count} safely placed.");
    }
    internal static bool TryBuild(Rectangle area, StructureMap structures)
    {
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || area.Y < 80 || area.Bottom > Main.worldSurface - 24
            || !WorldGen.InWorld(area.X, area.Y, 12) || area.X > Main.maxTilesX - Width - 12
            || area.Y > Main.maxTilesY - Height - 12) return false;
        int cloud = ModContent.TileType<ThunderCloudTile>();
        var allowed = new bool[TileLoader.TileCount]; allowed[cloud] = true;
        if (!structures.CanPlace(area, allowed, 2)) return false;
        int existingClouds = 0;
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y];
            // Only our own clouds may be excavated, even if a native sky structure was not reserved.
            if (tile.LiquidAmount > 0 || tile.WallType != WallID.None || tile.HasActuator
                || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire || (tile.HasTile && tile.TileType != cloud)) return false;
            if (area.Contains(x,y) && tile.HasTile) existingClouds++;
        }
        if (existingClouds < 20) return false;
        int floor = area.Bottom - FloorDepth;
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) {
            Tile tile = Main.tile[x,y]; tile.HasTile = y >= floor;
            if (tile.HasTile) tile.TileType = (ushort)cloud;
            tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        structures.AddProtectedStructure(area, 2);
        return true;
    }
    internal static bool IsPlannedStone(int x, int groundY)
    {
        foreach (Rectangle area in ModContent.GetInstance<ThunderCloudShrineSystem>().Shrines)
            if (x == area.Left + StoneOffset && groundY == area.Bottom - FloorDepth) return true;
        return false;
    }
    internal static bool StoneIntact(Rectangle area)
    {
        int stone = ModContent.TileType<SingingThunderStoneTile>(), floor = area.Bottom - FloorDepth;
        for (int dx = 0; dx < 2; dx++) for (int dy = 0; dy < 2; dy++) {
            Tile tile = Main.tile[area.X + StoneOffset + dx, floor - 2 + dy];
            if (!tile.HasTile || tile.TileType != stone || tile.TileFrameX != dx * 16 || tile.TileFrameY != dy * 16) return false;
        }
        return true;
    }
}
