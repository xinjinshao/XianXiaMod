using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

public static class EnemyTargeting
{
    public static bool TryGetLivingTarget(NPC npc, out Player target)
    {
        target = null;
        bool Valid() => npc.target >= 0 && npc.target < Main.maxPlayers
            && Main.player[npc.target].active && !Main.player[npc.target].dead;
        if (!Valid() && Main.netMode != NetmodeID.MultiplayerClient) npc.TargetClosest(false);
        if (!Valid()) return false;
        target = Main.player[npc.target];
        return true;
    }
}
