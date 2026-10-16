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

    internal void ResetScan() { counts = null; lastCenter = default; nextScan = 0; }
    public override void Initialize() => ResetScan();
    public override void OnEnterWorld() => ResetScan();

    // Validate in world space before Terraria converts coordinates to tile-sized integers.
    // Non-finite/huge inputs can otherwise wrap, overflow distance checks or scan unrelated tiles.
    internal static bool TryScanCenter(Player player, out Point center)
    {
        center = default;
        if (player == null || !player.active || Main.maxTilesX <= 0 || Main.maxTilesY <= 0) return false;
        Vector2 position = player.Center;
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y)
            || position.X < 0 || position.Y < 0
            || position.X >= Main.maxTilesX * 16d || position.Y >= Main.maxTilesY * 16d) return false;
        center = position.ToTileCoordinates();
        return true;
    }

    public int Count(int tileType)
    {
        if (Main.netMode != NetmodeID.Server || !TryScanCenter(Player, out Point center))
        {
            ResetScan();
            return 0;
        }
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
