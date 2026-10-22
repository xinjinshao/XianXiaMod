using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

// Expose a small seam from existing ore; never create additional spirit ore.
public class ShallowSpiritChamberSystem : ModSystem
{
    internal const int Width = 29, Height = 15, FloorRow = 12;
    internal static readonly int[] OreOffsets = { 3,7,11,17,21,25 };
    internal readonly List<Rectangle> Chambers = new();
    public override void ClearWorld() => Chambers.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["shallowSpiritChambers"] = Chambers.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Chambers.Clear();
        foreach (TagCompound entry in tag.GetList<TagCompound>("shallowSpiritChambers")) {
            var area = new Rectangle(entry.GetInt("x"),entry.GetInt("y"),Width,Height);
            if (Chambers.Count < 3 && WorldGen.InWorld(area.X,area.Y,12)
                && area.X <= Main.maxTilesX - Width - 12 && area.Y <= Main.maxTilesY - Height - 12
                && !Chambers.Exists(other => other.Intersects(area))) Chambers.Add(area);
        }
    }
    internal void Generate(List<Rectangle> regions)
    {
        Chambers.Clear();
        foreach (Rectangle source in regions) {
            if (Chambers.Count == 3) break;
            int minY = (int)Main.worldSurface + 20, maxY = System.Math.Min((int)Main.rockLayer + 80,Main.maxTilesY - 200);
            Rectangle search = Rectangle.Intersect(new Rectangle(source.X - 24,source.Y - 12,source.Width + 48,source.Height + 24),
                new Rectangle(12,minY,Main.maxTilesX - 24,System.Math.Max(0,maxY - minY)));
            bool built = false;
            for (int y = search.Top; y <= search.Bottom - Height && !built; y += 2)
                for (int x = search.Left; x <= search.Right - Width && !built; x += 2) {
                    var area = new Rectangle(x,y,Width,Height);
                    if (TryBuild(area,GenVars.structures)) { Chambers.Add(area); built = true; }
                }
        }
        Mod.Logger.Info($"Shallow spirit chambers: {Chambers.Count}/{regions.Count} safely placed.");
    }
    internal static bool IsSolid(int x,int y) => y >= FloorRow || (y < 2 && (x < 5 || x >= Width - 5))
        || ((x == 0 || x == Width - 1) && y >= 2 && y < 8);

    internal static bool TryBuild(Rectangle area,StructureMap structures)
    {
        if (structures == null || Main.netMode == NetmodeID.MultiplayerClient || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.X,area.Y,12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        int moss = ModContent.TileType<SpiritMossTile>(), ore = ModContent.TileType<SpiritOreTile>();
        var allowed = SectRuinHallSystem.NaturalTiles(); allowed[ModContent.TileType<SectRuinBrickTile>()] = false; allowed[moss] = allowed[ore] = true;
        if (!structures.CanPlace(area,allowed,2)) return false;
        int originalOre = 0, originalSpirit = 0;
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x,y];
            if (tile.LiquidAmount > 0 || Main.wallHouse[tile.WallType] || tile.HasActuator
                || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))) return false;
            if (area.Contains(x,y) && tile.HasTile) {
                if (tile.TileType == ore) originalOre++;
                if (tile.TileType == ore || tile.TileType == moss) originalSpirit++;
            }
        }
        // Six exposed cells plus two complete buried rows: retain every original ore cell or skip.
        if (originalSpirit < 40 || originalOre < 1 || originalOre > OreOffsets.Length + Width * 2) return false;
        for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.HasTile = IsSolid(x,y);
            if (tile.HasTile) tile.TileType = (ushort)moss;
            tile.Slope = SlopeType.Solid; tile.IsHalfBlock = false; tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int i = 0; i < System.Math.Min(originalOre,OreOffsets.Length); i++) {
            Tile tile = Main.tile[area.X + OreOffsets[i],area.Y + FloorRow]; tile.TileType = (ushort)ore;
        }
        int buried = originalOre - System.Math.Min(originalOre,OreOffsets.Length);
        for (int y = FloorRow + 1; y < Height && buried > 0; y++) for (int x = 0; x < Width && buried > 0; x++,buried--) {
            Tile tile = Main.tile[area.X + x,area.Y + y]; tile.TileType = (ushort)ore;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x,y);
        structures.AddProtectedStructure(area,2);
        return true;
    }
    internal static bool OreSeamIntact(Rectangle area)
    {
        int ore = ModContent.TileType<SpiritOreTile>(), moss = ModContent.TileType<SpiritMossTile>(), found = 0; bool end = false;
        foreach (int offset in OreOffsets) {
            Tile tile = Main.tile[area.X + offset,area.Y + FloorRow];
            if (!tile.HasTile || tile.Slope != SlopeType.Solid || tile.IsHalfBlock) return false;
            if (tile.TileType == ore && !end) found++;
            else if (tile.TileType == moss) end = true;
            else return false;
        }
        return found > 0;
    }
}
