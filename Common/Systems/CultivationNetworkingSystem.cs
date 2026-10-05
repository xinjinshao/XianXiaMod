using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public class CultivationNetworkingSystem : ModSystem
{
    public override void PostUpdatePlayers()
    {
        if (Main.netMode != NetmodeID.Server) return;
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            XianXiaPlayer state = Main.player[i].GetModPlayer<XianXiaPlayer>();
            if (Main.player[i].active) state.NetworkWasActive = true;
            else if (state.NetworkWasActive) state.ResetNetworkSession();
        }
    }
}
