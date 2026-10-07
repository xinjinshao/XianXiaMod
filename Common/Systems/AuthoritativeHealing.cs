using System;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;

public static class AuthoritativeHealing
{
    public static int Apply(Player target, int amount)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || target == null || !target.active || target.dead
            || target.statLife <= 0 || target.statLife >= target.statLifeMax2 || amount <= 0 || target.whoAmI < 0 || target.whoAmI >= Main.maxPlayers
            || !ReferenceEquals(Main.player[target.whoAmI], target)) return 0;
        // SpiritHeal carries a signed 16-bit amount. Keep authority and receivers identical.
        int healed = Math.Min(short.MaxValue, Math.Min(amount, Math.Max(0, target.statLifeMax2 - target.statLife)));
        if (healed == 0) return 0;
        if (Main.netMode == NetmodeID.Server)
        {
            target.statLife += healed;
            NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, target.whoAmI, healed);
        }
        else target.Heal(healed);
        return healed;
    }
}
