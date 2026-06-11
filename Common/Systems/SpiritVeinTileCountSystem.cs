using System;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public class SpiritVeinTileCountSystem : ModSystem
{
    public int spiritVeinTileCount;

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        spiritVeinTileCount =
            tileCounts[ModContent.TileType<SpiritOreTile>()] +
            tileCounts[ModContent.TileType<SpiritMossTile>()];
    }
}
