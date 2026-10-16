using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public class CultivationNetworkingSystem : ModSystem
{
    public override void PostUpdatePlayers()
    {
        if (Main.netMode != NetmodeID.Server || Main.player == null) return;
        for (int i = 0; i < System.Math.Min(Main.maxPlayers, Main.player.Length); i++)
        {
            Player player = Main.player[i];
            if (player == null || !player.TryGetModPlayer<XianXiaPlayer>(out var state)) continue;
            if (player.active) state.NetworkWasActive = true;
            else if (state.NetworkWasActive) state.ResetNetworkSession();
        }
    }
}
