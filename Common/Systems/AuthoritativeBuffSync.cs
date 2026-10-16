using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

// Buff slots can move after removals: compare durations by buff type, not slot index.
public static class AuthoritativeBuffSync
{
    public static Dictionary<int, int> Capture(Player player)
    {
        var result = new Dictionary<int, int>();
        for (int i = 0; i < Math.Min(player.buffType.Length, player.buffTime.Length); i++)
            if (player.buffType[i] > 0 && player.buffTime[i] > 0) result[player.buffType[i]] = player.buffTime[i];
        return result;
    }
    public static void SendChanges(Player player, IReadOnlyDictionary<int, int> before)
    {
        if (Main.netMode != NetmodeID.Server) return;
        foreach (var buff in Capture(player))
            if (!before.TryGetValue(buff.Key, out int duration) || duration != buff.Value) Send(player, buff.Key, buff.Value);
    }
    public static void Send(Player player, int buff, int duration)
    {
        if (Main.netMode != NetmodeID.Server || player == null || !player.active || player.dead
            || player.whoAmI < 0 || player.whoAmI >= Main.maxPlayers || buff <= 0 || duration <= 0
            || !ReferenceEquals(Main.player[player.whoAmI], player)) return;
        NetMessage.SendData(MessageID.AddPlayerBuff, player.whoAmI, -1, null, player.whoAmI, buff, duration);
    }
}
