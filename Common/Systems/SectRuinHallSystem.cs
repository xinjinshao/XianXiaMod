using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

// New-world geometry only. Loading a saved hall never regenerates or repairs it.
public class SectRuinHallSystem : ModSystem
{
    internal const int Width = 41, Height = 19;
    internal readonly List<Rectangle> Halls = new();
    public override void ClearWorld() => Halls.Clear();
    public override void SaveWorldData(TagCompound tag) => tag["sectRuinHalls"] = Halls.ConvertAll(area => new TagCompound { ["x"] = area.X, ["y"] = area.Y });
    public override void LoadWorldData(TagCompound tag)
    {
        Halls.Clear();
        foreach (var entry in tag.GetList<TagCompound>("sectRuinHalls")) {
            var area = new Rectangle(entry.GetInt("x"), entry.GetInt("y"), Width, Height);
            if (Halls.Count < 2 && WorldGen.InWorld(area.X, area.Y, 10)
                && area.X <= Main.maxTilesX - Width - 10 && area.Y <= Main.maxTilesY - Height - 10
                && !Halls.Contains(area)) Halls.Add(area);
        }
    }
    internal List<Rectangle> Generate(List<Rectangle> regions)
    {
        Halls.Clear();
        ModContent.GetInstance<SectRuinPathSystem>().Paths.Clear();
        var refusals = new Dictionary<string, int>();
        foreach (Rectangle source in regions) {
            // Nearby dry terrain can be safer than the original ore patch after vanilla structures finish.
            var region = Rectangle.Intersect(new Rectangle(source.X - 60, source.Y - 30, source.Width + 120, source.Height + 60),
                new Rectangle(12, (int)Main.rockLayer - 80, Main.maxTilesX - 24, Main.maxTilesY - 200 - ((int)Main.rockLayer - 80)));
            bool built = false;
            for (int y = region.Top; y <= region.Bottom - Height && !built; y += 3)
                for (int x = region.Left; x <= region.Right - Width && !built; x += 3) {
                    var area = new Rectangle(x, y, Width, Height);
                    if (TryBuild(area, GenVars.structures, out string reason, registerProtection: false)) {
                        Halls.Add(area); built = true;
                        if (SectRuinPathSystem.TryConnect(area, GenVars.structures, out Rectangle path))
                            ModContent.GetInstance<SectRuinPathSystem>().Paths.Add(path);
                        GenVars.structures.AddProtectedStructure(area, 2);
                        if (TryPlaceSupplyChest(area, WorldGen.genRand) < 0) Mod.Logger.Warn("Sect ruin hall supply chest could not be placed; terrain retained.");
                    }
                    else refusals[reason] = refusals.GetValueOrDefault(reason) + 1;
                }
        }
        Mod.Logger.Info($"Sect ruin halls: {Halls.Count}/{regions.Count} safely placed.");
        foreach (var refusal in refusals) Mod.Logger.Debug($"Sect hall rejected {refusal.Key}: {refusal.Value}");
        return Halls.Count > 0 ? new List<Rectangle>(Halls) : regions;
    }
    // Called only for newly generated halls; occupied/looted chest positions are never refilled.
    internal static int TryPlaceSupplyChest(Rectangle area, Terraria.Utilities.UnifiedRandom random)
    {
        if (random == null || Main.netMode == NetmodeID.MultiplayerClient
            || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration
            || area.Width != Width || area.Height != Height || !WorldGen.InWorld(area.Left, area.Top, 12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return -1;
        int x = area.X + 19, floor = area.Bottom - 3;
        if (Chest.FindChest(x, floor - 2) >= 0) return -1;
        for (int dx = 0; dx < 2; dx++) {
            Tile support = Main.tile[x + dx, floor];
            if (!support.HasTile || support.TileType != ModContent.TileType<SectRuinBrickTile>() || support.IsHalfBlock || support.Slope != SlopeType.Solid) return -1;
            for (int dy = 1; dy <= 2; dy++) {
                Tile tile = Main.tile[x + dx, floor - dy];
                if (tile.HasTile || tile.LiquidAmount > 0) return -1;
            }
        }
        int index = WorldGen.PlaceChest(x, floor - 1, TileID.Containers, false, 0);
        if (index < 0) return -1;
        Chest chest = Main.chest[index];
        void Add(int slot, int type, int count) { chest.item[slot].SetDefaults(type); chest.item[slot].stack = count; }
        Add(0, ModContent.ItemType<LowGradeSpiritStone>(), random.Next(6, 11));
        Add(1, ModContent.ItemType<ArtifactBlankShard>(), random.Next(2, 5));
        Add(2, ItemID.RecallPotion, random.Next(1, 3));
        Add(3, ItemID.Torch, 10);
        return index;
    }
    internal static bool IsBrick(int x, int y) => y < 3 || y >= Height - 3
        || ((x < 2 || x >= Width - 2) && y < Height - 8);
    internal static bool TryBuild(Rectangle area, StructureMap structures) => TryBuild(area, structures, out _);
    private static bool TryBuild(Rectangle area, StructureMap structures, out string reason, bool registerProtection = true)
    {
        reason = "bounds";
        if (structures == null || area.Width != Width || area.Height != Height
            || !WorldGen.InWorld(area.Left, area.Top, 12)
            || area.X > Main.maxTilesX - Width - 12 || area.Y > Main.maxTilesY - Height - 12) return false;
        int brick = ModContent.TileType<SectRuinBrickTile>();
        var allowed = NaturalTiles();
        reason = "protected structure or non-natural tile";
        if (!structures.CanPlace(area, allowed, 2)) {
            var any = new bool[allowed.Length]; System.Array.Fill(any, true);
            reason = structures.CanPlace(area, any, 2) ? "non-natural tile" : "protected structure";
            if (reason == "non-natural tile")
                for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
                    Tile candidate = Main.tile[x,y];
                    if (candidate.TileType < allowed.Length && !allowed[candidate.TileType]) {
                        reason = $"tile {candidate.TileType} active={candidate.HasTile}";
                        return false;
                    }
                }
            return false;
        }
        // Check the buffer too, so framing cannot affect a nearby object or occupied building.
        for (int x = area.Left - 2; x < area.Right + 2; x++) for (int y = area.Top - 2; y < area.Bottom + 2; y++) {
            Tile tile = Main.tile[x, y];
            reason = tile.LiquidAmount > 0 ? "liquid" : "occupied terrain wall or wiring";
            if (tile.LiquidAmount > 0 || (tile.HasTile && (tile.TileType >= allowed.Length || !allowed[tile.TileType]))
                || Main.wallHouse[tile.WallType] || tile.HasActuator || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire) return false;
        }
        for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) {
            Tile tile = Main.tile[area.X + x, area.Y + y];
            tile.HasTile = IsBrick(x, y);
            if (tile.HasTile) tile.TileType = (ushort)brick;
            tile.IsHalfBlock = false; tile.Slope = SlopeType.Solid;
            tile.TileFrameX = tile.TileFrameY = 0;
        }
        for (int x = area.Left; x < area.Right; x++) for (int y = area.Top; y < area.Bottom; y++) WorldGen.SquareTileFrame(x, y);
        if (registerProtection) structures.AddProtectedStructure(area, 2);
        return true;
    }
    internal static bool[] NaturalTiles()
    {
        int brick = ModContent.TileType<SectRuinBrickTile>();
        var allowed = new bool[TileLoader.TileCount];
        foreach (int type in new[] { TileID.Dirt, TileID.Stone, TileID.ClayBlock, TileID.Mud,
            TileID.SnowBlock, TileID.IceBlock, TileID.Sand, TileID.Silt, TileID.Slush,
            TileID.HardenedSand, TileID.Sandstone, TileID.DesertFossil,
            TileID.Cobweb, TileID.Stalactite, TileID.SmallPiles, TileID.Plants, TileID.Plants2,
            TileID.JunglePlants, TileID.JunglePlants2, TileID.MushroomPlants, TileID.MushroomVines,
            TileID.WaterDrip, TileID.LavaDrip, TileID.SandDrip, TileID.ExposedGems,
            TileID.Sapphire, TileID.Ruby, TileID.Emerald, TileID.Topaz, TileID.Amethyst, TileID.Diamond,
            TileID.GreenMoss, TileID.BrownMoss, TileID.RedMoss, TileID.BlueMoss, TileID.PurpleMoss, TileID.LongMoss,
            TileID.Grass, TileID.JungleGrass, TileID.MushroomGrass, TileID.Ash, TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, brick }) allowed[type] = true;
        for (int type = 0; type < allowed.Length; type++) if (TileID.Sets.Ore[type]) allowed[type] = true;
        return allowed;
    }

}
