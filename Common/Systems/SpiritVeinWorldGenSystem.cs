using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public class SpiritVeinWorldGenSystem : ModSystem
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

        tasks.Insert(index + 1, new PassLegacy("XianXia Shallow Spirit Veins", GenerateShallowSpiritVeins));
    }

    private void GenerateShallowSpiritVeins(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Waking shallow spirit veins";
        int patches = Main.maxTilesX / 220;
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(120, Main.maxTilesX - 120);
            int y = WorldGen.genRand.Next((int)Main.worldSurface, (int)Main.rockLayer);
            int radiusX = WorldGen.genRand.Next(10, 24);
            int radiusY = WorldGen.genRand.Next(7, 16);

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

                    ushort type = WorldGen.genRand.NextBool(4)
                        ? (ushort)ModContent.TileType<SpiritOreTile>()
                        : (ushort)ModContent.TileType<SpiritMossTile>();
                    Tile tile = Main.tile[tx, ty];
                    if (tile.TileType == TileID.Stone || tile.TileType == TileID.Dirt || tile.TileType == TileID.ClayBlock)
                    {
                        tile.TileType = type;
                        WorldGen.SquareTileFrame(tx, ty);
                    }
                }
            }
        }
    }
}
