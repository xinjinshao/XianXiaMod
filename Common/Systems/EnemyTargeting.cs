using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

public static class EnemyTargeting
{
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y)
        && float.IsFinite(value.LengthSquared());
    private static bool Valid(NPC npc, int index) => Main.player != null && index >= 0
        && index < Main.maxPlayers && index < Main.player.Length
        && Main.player[index] is Player player && player.active && !player.dead
        && Finite(player.Center) && Finite(player.velocity)
        && float.IsFinite(Vector2.DistanceSquared(npc.Center, player.Center));

    public static bool TryGetLivingTarget(NPC npc, out Player target)
    {
        target = null;
        if (npc == null || !npc.active || npc.life <= 0 || !Finite(npc.Center) || !Finite(npc.velocity)) return false;
        if (Valid(npc, npc.target)) { target = Main.player[npc.target]; return true; }
        if (Main.netMode == NetmodeID.MultiplayerClient || Main.player == null) return false;
        // Keep vanilla targeting behavior when its player table is safe to traverse.
        bool nativeSafe = Main.player.Length >= Main.maxPlayers;
        for (int i = 0; nativeSafe && i < Main.maxPlayers; i++)
            nativeSafe = Main.player[i] is Player player
                && (!player.active || Finite(player.Center) && Finite(player.velocity));
        if (nativeSafe) {
            npc.TargetClosest(false);
            if (Valid(npc, npc.target)) {
                target = Main.player[npc.target]; npc.netUpdate = true; return true;
            }
        }
        // Loading/disconnect or malformed slots must not enter the vanilla traversal.
        int closest = -1; float distance = float.PositiveInfinity;
        for (int i = 0; i < Math.Min(Main.maxPlayers, Main.player.Length); i++) {
            if (!Valid(npc, i)) continue;
            float candidate = Vector2.DistanceSquared(npc.Center, Main.player[i].Center);
            if (candidate >= distance) continue;
            closest = i; distance = candidate;
        }
        if (closest < 0) return false;
        npc.target = closest; npc.netUpdate = true; target = Main.player[closest]; return true;
    }
}
