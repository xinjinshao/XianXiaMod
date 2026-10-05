using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Common.Players;

// SceneMetrics/TileCountsAvailable describe the local client's scene. Dedicated
// servers instead cache an independent, vanilla-sized scan for each player.
public class ServerBiomePlayer : ModPlayer
{
    private int[] counts;
    private Point lastCenter;
    private ulong nextScan;

    public int Count(int tileType)
    {
        if (Main.netMode != NetmodeID.Server || !Player.active) return 0;
        Point center = Player.Center.ToTileCoordinates();
        if (counts == null || Main.GameUpdateCount >= nextScan
            || Math.Abs(center.X - lastCenter.X) > 8 || Math.Abs(center.Y - lastCenter.Y) > 8)
        {
            counts ??= new int[TileLoader.TileCount];
            Array.Clear(counts);
            lastCenter = center;
            nextScan = Main.GameUpdateCount + 60;
            int left = Math.Max(0, center.X - Main.buffScanAreaWidth / 2);
            int top = Math.Max(0, center.Y - Main.buffScanAreaHeight / 2);
            int right = Math.Min(Main.maxTilesX, center.X - Main.buffScanAreaWidth / 2 + Main.buffScanAreaWidth);
            int bottom = Math.Min(Main.maxTilesY, center.Y - Main.buffScanAreaHeight / 2 + Main.buffScanAreaHeight);
            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && tile.TileType < counts.Length) counts[tile.TileType]++;
                }
        }
        return tileType >= 0 && tileType < counts.Length ? counts[tileType] : 0;
    }
}
