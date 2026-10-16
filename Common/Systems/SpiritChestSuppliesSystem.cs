using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

// Supplement existing exploration loot without replacing vanilla or other-mod items.
public class SpiritChestSuppliesSystem : ModSystem
{
    private bool completed;
    public override void ClearWorld() => completed = false;
    public override void PostWorldGen() => GenerateSupplies(WorldGen.genRand);
    internal void GenerateSupplies(Terraria.Utilities.UnifiedRandom random)
    {
        if (completed || Main.netMode == NetmodeID.MultiplayerClient || Main.chest == null
            || !ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration) return;
        completed = true;
        int added = 0, limit = Math.Max(1, Main.maxTilesX / 420);
        int stone = ModContent.ItemType<LowGradeSpiritStone>();
        foreach (Chest chest in Main.chest) {
            if (added >= limit) break;
            if (chest == null || chest.item == null || chest.x < 0 || chest.x >= Main.maxTilesX - 1
                || chest.y < Main.worldSurface || chest.y >= Main.maxTilesY - 200) continue;
            // Only intact, unlocked vanilla gold chests (style 1), including all four frames.
            bool gold = true;
            for (int column = 0; column < 2 && gold; column++)
                for (int row = 0; row < 2; row++) {
                    Tile tile = Main.tile[chest.x + column, chest.y + row];
                    if (!tile.HasTile || tile.TileType != TileID.Containers
                        || tile.TileFrameX != 36 + column * 18 || tile.TileFrameY != row * 18) { gold = false; break; }
                }
            if (!gold) continue;
            int free = -1; bool supplied = false;
            for (int slot = 0; slot < chest.item.Length; slot++) {
                Item item = chest.item[slot];
                if (item != null && item.type == stone && !item.IsAir) supplied = true;
                if (free < 0 && (item == null || item.IsAir)) free = slot;
            }
            if (supplied || free < 0 || !random.NextBool(4)) continue;
            var supply = new Item(); supply.SetDefaults(stone); supply.stack = random.Next(4, 9);
            chest.item[free] = supply; added++;
        }
    }
}
