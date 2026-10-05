using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public class GeneratedBiomeWorldGenSystem : ModSystem
{
    private List<Rectangle> thunder = new(), star = new(), sect = new(), heaven = new();
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        if (!ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration)
        {
            return;
        }

        int index = tasks.FindIndex(pass => pass.Name == "Shinies");
        if (index == -1)
        {
            return;
        }

        tasks.Insert(index + 2, new PassLegacy("XianXia Cultivation Biomes", GenerateCultivationBiomes));
        // Place on final terrain after native cleanup and structure passes.
        tasks.Add(new PassLegacy("XianXia Cultivation Objects", PlaceCultivationObjects));
    }

    private void GenerateCultivationBiomes(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.XianXia.WorldGeneration.CultivationDomains");

        GenerateGroundPatches(ModContent.TileType<GreenwoodSoilTile>(), ModContent.TileType<SpiritHerbTile>(), 3, (int)Main.worldSurface - 80, (int)Main.worldSurface + 40, 34, 16);
        GenerateGroundPatches(ModContent.TileType<FurnaceSlagTile>(), ModContent.TileType<FurnaceSlagTile>(), 3, (int)Main.rockLayer, Main.maxTilesY - 260, 42, 22, ModContent.WallType<BlackFurnaceWall>());
        thunder = GenerateCloudFields(ModContent.TileType<ThunderCloudTile>(), 4, 160, (int)Main.worldSurface - 160, 46, 12);
        star = GenerateGroundPatches(ModContent.TileType<StarAbyssCrystalTile>(), ModContent.TileType<StarAbyssCrystalTile>(), 3, Main.maxTilesY - 420, Main.maxTilesY - 180, 38, 24);
        sect = GenerateGroundPatches(ModContent.TileType<SectRuinBrickTile>(), ModContent.TileType<SectRuinBrickTile>(), 2, (int)Main.rockLayer - 80, Main.maxTilesY - 360, 50, 18);
        heaven = GenerateGroundPatches(ModContent.TileType<FallenHeavenJadeTile>(), ModContent.TileType<FallenHeavenJadeTile>(), 2, Main.maxTilesY - 520, Main.maxTilesY - 260, 44, 24);
        GenerateGroundPatches(ModContent.TileType<MoonboneTile>(), ModContent.TileType<MoonboneTile>(), 2, Main.maxTilesY - 360, Main.maxTilesY - 140, 54, 28);
    }

    private void PlaceCultivationObjects(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.XianXia.WorldGeneration.CultivationDomains");
        PlaceObjects(ModContent.TileType<SwordTabletTile>(), 4, sect, ModContent.TileType<SectRuinBrickTile>(), 3);
        PlaceObjects(ModContent.TileType<SingingThunderStoneTile>(), 3, thunder, ModContent.TileType<ThunderCloudTile>(), 2);
        PlaceObjects(ModContent.TileType<RiftMembraneTile>(), 3, star, ModContent.TileType<StarAbyssCrystalTile>(), 2);
        PlaceObjects(ModContent.TileType<BrokenHeavenTabletTile>(), 2, heaven, ModContent.TileType<FallenHeavenJadeTile>(), 4);
        PlaceObjects(ModContent.TileType<ArchiveLightPillarTile>(), 2, heaven, ModContent.TileType<FallenHeavenJadeTile>(), 6);
    }

    private void PlaceObjects(int tileType, int count, List<Rectangle> regions, int groundType, int height)
    {
        int placed = 0;
        for (int attempt = 0; attempt < count * 600 && placed < count && regions.Count > 0; attempt++)
        {
            Rectangle region = regions[WorldGen.genRand.Next(regions.Count)];
            int x = WorldGen.genRand.Next(region.Left, region.Right - 1);
            int groundY = WorldGen.genRand.Next(region.Top + height, region.Bottom);
            if (TryPlaceObject(tileType, groundType, height, x, groundY)) placed++;
        }
        // Random sampling can miss narrow legal ledges. Exhaust all region-local
        // positions before reporting shortage; this fallback changes no terrain.
        foreach (Rectangle region in regions)
        {
            for (int groundY = region.Top + height; groundY < region.Bottom && placed < count; groundY++)
                for (int x = region.Left; x < region.Right - 1 && placed < count; x++)
                    if (TryPlaceObject(tileType, groundType, height, x, groundY)) placed++;
        }
        // Final terrain may lack flat ledges. Add a small matching pedestal
        // only in completely empty, dry space; never excavate existing tiles.
        foreach (Rectangle region in regions)
            for (int groundY = region.Top + height; groundY < region.Bottom && placed < count; groundY++)
                for (int x = region.Left; x < region.Right - 1 && placed < count; x++)
                    if (TryPlacePedestal(tileType, groundType, height, x, groundY)) placed++;
        Mod.Logger.Info($"Cultivation object {tileType}: {placed}/{count} placed in {regions.Count} matching regions.");
        if (placed < count) Mod.Logger.Warn($"Cultivation object {tileType} placement incomplete; no suitable space in generated regions.");
    }

    private static bool TryPlacePedestal(int tileType, int groundType, int height, int x, int groundY)
    {
        if (!WorldGen.InWorld(x, groundY, 10) || !WorldGen.InWorld(x + 1, groundY - height, 10)) return false;
        for (int column = 0; column < 2; column++)
            for (int row = 0; row <= height; row++)
            {
                Tile space = Main.tile[x + column, groundY - row];
                if (space.HasTile || space.LiquidAmount > 0) return false;
            }
        for (int column = 0; column < 2; column++)
        {
            Tile support = Main.tile[x + column, groundY];
            support.HasTile = true;
            support.TileType = (ushort)groundType;
            support.IsHalfBlock = false;
            support.Slope = 0;
            WorldGen.SquareTileFrame(x + column, groundY);
        }
        if (TryPlaceObject(tileType, groundType, height, x, groundY)) return true;
        for (int column = 0; column < 2; column++)
        {
            Tile support = Main.tile[x + column, groundY];
            support.HasTile = false;
        }
        return false;
    }

    private static bool TryPlaceObject(int tileType, int groundType, int height, int x, int groundY)
    {
            if (!WorldGen.InWorld(x, groundY, 10) || !WorldGen.InWorld(x + 1, groundY - height, 10)) return false;
            bool valid = true;
            for (int column = 0; column < 2; column++)
            {
                Tile support = Main.tile[x + column, groundY];
                if (!support.HasTile || support.TileType != groundType || support.IsHalfBlock || support.Slope != 0) valid = false;
                for (int row = 1; row <= height; row++)
                {
                    Tile space = Main.tile[x + column, groundY - row];
                    if (space.HasTile || space.LiquidAmount > 0) valid = false;
                }
            }
            if (!valid) return false;
            WorldGen.PlaceObject(x, groundY - 1, tileType);
            // Do not count an attempted placement: inspect the full resulting object.
            bool complete = true;
            for (int column = 0; column < 2; column++)
                for (int row = 1; row <= height; row++)
                    if (!Main.tile[x + column, groundY - row].HasTile || Main.tile[x + column, groundY - row].TileType != tileType) complete = false;
            return complete;
    }

    private static List<Rectangle> GenerateGroundPatches(int primaryTile, int accentTile, int patches, int minY, int maxY, int radiusX, int radiusY, int wallType = -1)
    {
        var regions = new List<Rectangle>();
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(160, Main.maxTilesX - 160);
            int y = NextWorldY(minY, maxY);
            int rx = radiusX + WorldGen.genRand.Next(-8, 9);
            int ry = radiusY + WorldGen.genRand.Next(-4, 5);
            if (PaintEllipse(x, y, rx, ry, primaryTile, accentTile, wallType) > 0)
                regions.Add(new Rectangle(x - rx, y - ry, rx * 2 + 1, ry * 2 + 1));
        }
        return regions;
    }

    private static List<Rectangle> GenerateCloudFields(int cloudTile, int patches, int minY, int maxY, int radiusX, int radiusY)
    {
        var regions = new List<Rectangle>();
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(180, Main.maxTilesX - 180);
            int y = NextWorldY(minY, maxY);
            regions.Add(new Rectangle(x - radiusX, y - radiusY, radiusX * 2 + 1, radiusY * 2 + 1));
            for (int tx = x - radiusX; tx <= x + radiusX; tx++)
            {
                for (int ty = y - radiusY; ty <= y + radiusY; ty++)
                {
                    if (!WorldGen.InWorld(tx, ty, 10))
                    {
                        continue;
                    }

                    float nx = (tx - x) / (float)radiusX;
                    float ny = (ty - y) / (float)radiusY;
                    if (nx * nx + ny * ny > 1f || !WorldGen.genRand.NextBool(2))
                    {
                        continue;
                    }

                    Tile tile = Main.tile[tx, ty];
                    if (tile.HasTile || tile.LiquidAmount > 0) continue;
                    tile.HasTile = true;
                    tile.TileType = (ushort)cloudTile;
                    WorldGen.SquareTileFrame(tx, ty);
                }
            }
        }
        return regions;
    }

    private static int PaintEllipse(int x, int y, int radiusX, int radiusY, int primaryTile, int accentTile, int wallType)
    {
        int painted = 0;
        for (int tx = x - radiusX; tx <= x + radiusX; tx++)
        {
            for (int ty = y - radiusY; ty <= y + radiusY; ty++)
            {
                if (!WorldGen.InWorld(tx, ty, 10))
                {
                    continue;
                }

                float nx = (tx - x) / (float)radiusX;
                float ny = (ty - y) / (float)radiusY;
                if (nx * nx + ny * ny > 1f || !Main.tile[tx, ty].HasTile)
                {
                    continue;
                }

                Tile tile = Main.tile[tx, ty];
                if (!CanReplace(tile.TileType))
                {
                    continue;
                }

                tile.TileType = WorldGen.genRand.NextBool(5) ? (ushort)accentTile : (ushort)primaryTile;
                if (wallType >= 0) tile.WallType = (ushort)wallType;
                painted++;
                WorldGen.SquareTileFrame(tx, ty);
            }
        }
        return painted;
    }

    private static bool CanReplace(ushort tileType)
    {
        return tileType == TileID.Dirt
            || tileType == TileID.Stone
            || tileType == TileID.ClayBlock
            || tileType == TileID.Mud
            || tileType == TileID.SnowBlock
            || tileType == TileID.IceBlock
            || tileType == TileID.Ash
            || tileType == TileID.Ebonstone
            || tileType == TileID.Crimstone
            || tileType == TileID.Pearlstone;
    }

    private static int NextWorldY(int minY, int maxY)
    {
        int lower = Utils.Clamp(Math.Min(minY, maxY), 80, Main.maxTilesY - 200);
        int upper = Utils.Clamp(Math.Max(minY, maxY), lower + 1, Main.maxTilesY - 120);
        return WorldGen.genRand.Next(lower, upper);
    }
}
