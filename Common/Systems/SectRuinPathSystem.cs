using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// Connect one doorway to a pre-existing five-column dry cavity, never to an invented endpoint.
public class SectRuinPathSystem : ModSystem
{
    internal const int Limit = 48, Height = 7;
    internal readonly List<Rectangle> Paths = new();
    public override void ClearWorld() => Paths.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["sectRuinPaths"] = Paths.ConvertAll(path => new TagCompound { ["x"] = path.X, ["y"] = path.Y, ["width"] = path.Width });
    public override void LoadWorldData(TagCompound tag)
    {
        Paths.Clear();
        foreach (var entry in tag.GetList<TagCompound>("sectRuinPaths")) {
            var path = new Rectangle(entry.GetInt("x"), entry.GetInt("y"), entry.GetInt("width"), Height);
            if (Paths.Count < 2 && path.Width >= 5 && path.Width <= Limit && WorldGen.InWorld(path.X,path.Y,12)
                && path.X <= Main.maxTilesX - path.Width - 12 && path.Y <= Main.maxTilesY - Height - 12 && !Paths.Contains(path)) Paths.Add(path);
        }
    }
    internal static bool TryConnect(Rectangle hall, StructureMap structures, out Rectangle path)
    {
        path = default;
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || hall.Width != SectRuinHallSystem.Width || hall.Height != SectRuinHallSystem.Height
            || !WorldGen.InWorld(hall.X,hall.Y,12) || hall.X > Main.maxTilesX - hall.Width - 12 || hall.Y > Main.maxTilesY - hall.Height - 12) return false;
        int floor = hall.Bottom - 3; var allowed = SectRuinHallSystem.NaturalTiles();
        Rectangle best = default;
        foreach (int direction in new[] { -1, 1 }) {
            int open = 0;
            for (int length = 1; length <= Limit; length++) {
                int x = direction < 0 ? hall.Left - length : hall.Right + length - 1;
                if (!WorldGen.InWorld(x,floor - 6,12) || !WorldGen.InWorld(x,floor,12)) break;
                bool clear = true;
                for (int y = floor - 5; y < floor; y++) clear &= !Main.tile[x,y].HasTile && Main.tile[x,y].LiquidAmount == 0;
                open = clear ? open + 1 : 0;
                if (open < 5) continue;
                var candidate = new Rectangle(direction < 0 ? x : hall.Right, floor - 6, length, Height);
                if (!HasCavity(x, floor - 3, hall) || !Safe(candidate, structures, allowed)) continue;
                if (best.Width == 0 || candidate.Width < best.Width) best = candidate;
                break;
            }
        }
        if (best.Width == 0) return false;
        int brick = ModContent.TileType<SectRuinBrickTile>();
        for (int x = best.Left; x < best.Right; x++) for (int y = best.Top; y < best.Bottom; y++) {
            Tile tile = Main.tile[x,y]; tile.HasTile = y == best.Top || y == best.Bottom - 1;
            if (tile.HasTile) tile.TileType = (ushort)brick;
            tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = best.Left; x < best.Right; x++) for (int y = best.Top; y < best.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        structures.AddProtectedStructure(best,2); path = best;
        return true;
    }
    private static bool HasCavity(int x, int y, Rectangle hall)
    {
        var pending = new Queue<Point>(); var visited = new HashSet<Point>(); pending.Enqueue(new Point(x,y));
        while (pending.Count > 0 && visited.Count < 80) {
            Point point = pending.Dequeue();
            if (System.Math.Abs(point.X - x) > 24 || System.Math.Abs(point.Y - y) > 24 || hall.Contains(point)
                || !WorldGen.InWorld(point.X,point.Y,12) || Main.tile[point.X,point.Y].HasTile || Main.tile[point.X,point.Y].LiquidAmount > 0
                || !visited.Add(point)) continue;
            pending.Enqueue(new Point(point.X - 1,point.Y)); pending.Enqueue(new Point(point.X + 1,point.Y));
            pending.Enqueue(new Point(point.X,point.Y - 1)); pending.Enqueue(new Point(point.X,point.Y + 1));
        }
        return visited.Count >= 80;
    }
    private static bool Safe(Rectangle area, StructureMap structures, bool[] allowed)
    {
        if (!structures.CanPlace(area,allowed,2)) return false;
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y];
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))) return false;
        }
        return true;
    }
}
