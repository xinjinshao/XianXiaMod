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
        // World loading also visits vanilla slots whose mod-player arrays are not installed yet.
        if (player.TryGetModPlayer<ServerBiomePlayer>(out var biome)) biome.ResetScan();
        if (player.TryGetModPlayer<BiomeObjectEffectsPlayer>(out var objects)) objects.ResetScan();
    }
    private static void ResetAll()
    {
        if (Main.player == null) return;
        for (int i = 0; i < System.Math.Min(Main.maxPlayers, Main.player.Length); i++) Reset(Main.player[i]);
    }
    public override void OnWorldLoad() => ResetAll();
    public override void OnWorldUnload() => ResetAll();
    public override void PostUpdatePlayers()
    {
        if (Main.player == null) return;
        for (int i = 0; i < System.Math.Min(Main.maxPlayers, Main.player.Length); i++)
            if (Main.player[i] != null && !Main.player[i].active) Reset(Main.player[i]);
    }
}
