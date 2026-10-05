using System;
using Terraria.Localization;

namespace XianXia.Common.Players;

/// <summary>Read-only presentation of the player's current replicated cultivation state.</summary>
public static class CultivationStatusText
{
    public static string StageName(CultivationStage stage) => Language.GetTextValue(
        "Mods.XianXia.CultivationStatus.Realms." + (stage >= CultivationStage.None && stage <= CultivationStage.DaoSevering ? stage : CultivationStage.None));

    public static string Summary(XianXiaPlayer player)
    {
        string resource = Language.GetTextValue("Mods.XianXia.CultivationStatus.Resources",
            StageName(player.cultivationStage), player.spiritualEnergy, player.maxSpiritualEnergy, player.spiritPressure);
        string trial;
        if (player.tribulationTimer > 0)
            trial = Language.GetTextValue("Mods.XianXia.CultivationStatus.ActiveTrial", Seconds(player.tribulationTimer));
        else if (player.CanRetryTribulation())
            trial = Language.GetTextValue("Mods.XianXia.CultivationStatus.RetryTrial");
        else
            trial = Language.GetTextValue("Mods.XianXia.CultivationStatus.NoActiveTrial");
        return resource + "\n" + trial + "\n" + Language.GetTextValue(
            "Mods.XianXia.CultivationStatus.Weakness", Seconds(player.tribulationWeakness));
    }

    private static int Seconds(int ticks) => (int)Math.Ceiling(Math.Max(0, ticks) / 60d);
}
