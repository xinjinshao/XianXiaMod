using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles.Generated;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public class GeneratedBiomeWorldGenSystem : ModSystem
{
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
    }

    private void GenerateCultivationBiomes(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Carving cultivation domains";

        GenerateGroundPatches(ModContent.TileType<GreenwoodSoilTile>(), ModContent.TileType<SpiritHerbTile>(), 3, (int)Main.worldSurface - 80, (int)Main.worldSurface + 40, 34, 16);
        GenerateGroundPatches(ModContent.TileType<FurnaceSlagTile>(), ModContent.WallType<BlackFurnaceWall>(), 3, (int)Main.rockLayer, Main.maxTilesY - 260, 42, 22);
        GenerateCloudFields(ModContent.TileType<ThunderCloudTile>(), 4, 160, (int)Main.worldSurface - 160, 46, 12);
        GenerateGroundPatches(ModContent.TileType<StarAbyssCrystalTile>(), ModContent.TileType<StarAbyssCrystalTile>(), 3, Main.maxTilesY - 420, Main.maxTilesY - 180, 38, 24);
        GenerateGroundPatches(ModContent.TileType<SectRuinBrickTile>(), ModContent.TileType<SectRuinBrickTile>(), 2, (int)Main.rockLayer - 80, Main.maxTilesY - 360, 50, 18);
        GenerateGroundPatches(ModContent.TileType<FallenHeavenJadeTile>(), ModContent.TileType<FallenHeavenJadeTile>(), 2, Main.maxTilesY - 520, Main.maxTilesY - 260, 44, 24);
        GenerateGroundPatches(ModContent.TileType<MoonboneTile>(), ModContent.TileType<MoonboneTile>(), 2, Main.maxTilesY - 360, Main.maxTilesY - 140, 54, 28);
        PlaceObjects(ModContent.TileType<SwordTabletTile>(), 4, (int)Main.rockLayer - 80, Main.maxTilesY - 360);
        PlaceObjects(ModContent.TileType<SingingThunderStoneTile>(), 3, 160, (int)Main.worldSurface - 160);
        PlaceObjects(ModContent.TileType<RiftMembraneTile>(), 3, Main.maxTilesY - 420, Main.maxTilesY - 180);
        PlaceObjects(ModContent.TileType<BrokenHeavenTabletTile>(), 2, Main.maxTilesY - 520, Main.maxTilesY - 260);
        PlaceObjects(ModContent.TileType<ArchiveLightPillarTile>(), 2, Main.maxTilesY - 520, Main.maxTilesY - 260);
    }

    private static void PlaceObjects(int tileType, int count, int minY, int maxY)
    {
        for (int i = 0; i < count; i++)
        {
            int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
            int y = NextWorldY(minY, maxY);
            for (int attempt = 0; attempt < 60; attempt++)
            {
                if (WorldGen.InWorld(x, y, 10) && Main.tile[x, y].HasTile && Main.tile[x, y].TileType != tileType)
                {
                    WorldGen.PlaceTile(x, y - 1, tileType, true, true);
                    break;
                }
                x += WorldGen.genRand.Next(-3, 4);
                y += WorldGen.genRand.Next(-2, 3);
            }
        }
    }

    private static void GenerateGroundPatches(int primaryTile, int accentTileOrWall, int patches, int minY, int maxY, int radiusX, int radiusY)
    {
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(160, Main.maxTilesX - 160);
            int y = NextWorldY(minY, maxY);
            PaintEllipse(x, y, radiusX + WorldGen.genRand.Next(-8, 9), radiusY + WorldGen.genRand.Next(-4, 5), primaryTile, accentTileOrWall);
        }
    }

    private static void GenerateCloudFields(int cloudTile, int patches, int minY, int maxY, int radiusX, int radiusY)
    {
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(180, Main.maxTilesX - 180);
            int y = NextWorldY(minY, maxY);
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
                    tile.HasTile = true;
                    tile.TileType = (ushort)cloudTile;
                    WorldGen.SquareTileFrame(tx, ty);
                }
            }
        }
    }

    private static void PaintEllipse(int x, int y, int radiusX, int radiusY, int primaryTile, int accentTileOrWall)
    {
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

                tile.TileType = WorldGen.genRand.NextBool(5) ? (ushort)accentTileOrWall : (ushort)primaryTile;
                if (accentTileOrWall == ModContent.WallType<BlackFurnaceWall>())
                {
                    tile.TileType = (ushort)primaryTile;
                    tile.WallType = (ushort)accentTileOrWall;
                }
                WorldGen.SquareTileFrame(tx, ty);
            }
        }
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
