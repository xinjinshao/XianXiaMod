using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public static class SpiritSummonRequirements
{
    public static string GetFailure(Player player, int projectileType, float slots, bool worldReady = true)
    {
        if (player == null || !player.active || player.dead || player.noItems || player.CCed) return "Unavailable";
        if (!worldReady) return "WorldLocked";
        if (player.slotsMinions + slots > player.maxMinions) return "SlotsFull";
        foreach (Projectile projectile in Main.ActiveProjectiles)
            if (projectile.owner == player.whoAmI && projectile.type == projectileType) return "AlreadySummoned";
        for (int i = 0; i < Main.maxProjectiles; i++) if (!Main.projectile[i].active) return "";
        return "ProjectilesFull";
    }

    public static void AddTooltip(ModItem item, List<TooltipLine> tooltips, float slots, bool worldReady = true)
    {
        if (Main.myPlayer < 0 || Main.myPlayer >= Main.maxPlayers) return;
        string failure = GetFailure(Main.LocalPlayer, item.Item.shoot, slots, worldReady);
        tooltips.Add(new TooltipLine(item.Mod, "SpiritSummonRequirement",
            Language.GetTextValue("Mods.XianXia.SpiritSummon." + (failure.Length == 0 ? "Ready" : failure))));
    }
}
