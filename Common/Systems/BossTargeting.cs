using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

public static class BossTargeting
{
    public const float MaximumDistance = 4000f;
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);

    private static bool Valid(NPC npc, int index) => index >= 0 && index < Main.maxPlayers
        && Main.player[index] is Player player && player.active && !player.dead
        && Finite(player.Center) && Finite(player.velocity)
        && Vector2.DistanceSquared(npc.Center, player.Center) <= MaximumDistance * MaximumDistance;

    public static bool HasLivingTarget(NPC npc) => npc.active && npc.life > 0
        && Finite(npc.Center) && Finite(npc.velocity) && Valid(npc, npc.target);

    public static bool TryGetLivingTarget(NPC npc, out Player target)
    {
        target = null;
        if (!npc.active || npc.life <= 0 || !Finite(npc.Center) || !Finite(npc.velocity)) return false;
        if (Valid(npc, npc.target)) { target = Main.player[npc.target]; return true; }
        // Clients wait for the authoritative target rather than selecting a different player.
        if (Main.netMode == NetmodeID.MultiplayerClient) return false;
        int closest = -1;
        float distance = float.PositiveInfinity;
        for (int index = 0; index < Main.maxPlayers; index++) {
            if (!Valid(npc, index)) continue;
            float candidate = Vector2.DistanceSquared(npc.Center, Main.player[index].Center);
            if (candidate >= distance) continue;
            closest = index; distance = candidate;
        }
        if (closest < 0) return false;
        npc.target = closest; npc.netUpdate = true;
        target = Main.player[closest];
        return true;
    }
}
