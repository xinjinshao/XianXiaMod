using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Players;

// Each player scans their own nearby area; effects never multiply by tile count.
public class BiomeObjectEffectsPlayer : ModPlayer
{
    public const int RadiusX = 20, RadiusY = 15;
    private Point lastCenter;
    private ulong nextScan;
    private bool scanned, sword, thunder, rift;

    public override void PostUpdateEquips()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !Player.active || Player.dead)
        {
            scanned = false;
            return;
        }
        Point center = Player.Center.ToTileCoordinates();
        if (!scanned || center != lastCenter || Main.GameUpdateCount >= nextScan)
        {
            scanned = true;
            lastCenter = center;
            nextScan = Main.GameUpdateCount + 15;
            sword = thunder = rift = false;
            int swordType = ModContent.TileType<SwordTabletTile>();
            int thunderType = ModContent.TileType<SingingThunderStoneTile>();
            int riftType = ModContent.TileType<RiftMembraneTile>();
            for (int x = Math.Max(0, center.X - RadiusX); x <= Math.Min(Main.maxTilesX - 1, center.X + RadiusX); x++)
                for (int y = Math.Max(0, center.Y - RadiusY); y <= Math.Min(Main.maxTilesY - 1, center.Y + RadiusY); y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile) continue;
                    sword |= tile.TileType == swordType;
                    thunder |= tile.TileType == thunderType;
                    rift |= tile.TileType == riftType;
                }
        }
        XianXiaPlayer state = Player.GetModPlayer<XianXiaPlayer>();
        if (sword) Player.AddBuff(ModContent.BuffType<Content.Buffs.TribulationResistanceBuff>(), 2);
        if (thunder) state.spiritualEnergyRegenBonus += 1;
        if (rift && Main.GameUpdateCount % 120 == 0) state.spiritPressure = Math.Min(100, state.spiritPressure + 1);
    }
}
