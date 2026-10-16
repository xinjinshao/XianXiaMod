using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

// Scan caches describe a world and an occupant, never a persistent character.
public class BiomeScanCacheSystem : ModSystem
{
    private static void Reset(Player player)
    {
        if (player == null) return;
        player.GetModPlayer<ServerBiomePlayer>().ResetScan();
        player.GetModPlayer<BiomeObjectEffectsPlayer>().ResetScan();
    }
    private static void ResetAll()
    {
        for (int i = 0; i < Main.maxPlayers; i++) Reset(Main.player[i]);
    }
    public override void OnWorldLoad() => ResetAll();
    public override void OnWorldUnload() => ResetAll();
    public override void PostUpdatePlayers()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
            if (Main.player[i] != null && !Main.player[i].active) Reset(Main.player[i]);
    }
}
