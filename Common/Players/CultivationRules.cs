using System;

namespace XianXia.Common.Players;

/// <summary>Shared progression policy, independent of rendering and network transport.</summary>
public static class CultivationRules
{
    public static string GetFailure(CultivationStage current, CultivationStage target,
        int activeTrialTicks, bool currentTrialCleared, bool hardmode,
        bool plantera, bool golem, bool moonLord, Func<string, bool> downed)
    {
        if (activeTrialTicks < 0 || current < CultivationStage.None || current > CultivationStage.DaoSevering
            || target <= CultivationStage.None || target > CultivationStage.DaoSevering
            || (int)target != (int)current + 1)
            return "InvalidBreakthroughItem";
        if (activeTrialTicks > 0 || (current >= CultivationStage.Foundation && !currentTrialCleared))
            return "TribulationUnfinished";

        string worldFailure = GetWorldFailure(target, hardmode, plantera, golem, moonLord);
        if (worldFailure.Length > 0)
            return worldFailure;

        string boss = GetRequiredBoss(target);
        return boss.Length > 0 && !downed(boss) ? "BreakthroughRequiresTrial" : "";
    }

    public static string GetWorldFailure(CultivationStage target, bool hardmode,
        bool plantera, bool golem, bool moonLord)
    {
        if (target >= CultivationStage.GoldenCore && !hardmode)
            return "BreakthroughRequiresHardmode";
        if (target >= CultivationStage.NascentSoul && !plantera)
            return "BreakthroughRequiresPlantera";
        if (target >= CultivationStage.SpiritSevering && !golem)
            return "BreakthroughRequiresGolem";
        if (target >= CultivationStage.Tribulation && !moonLord)
            return "BreakthroughRequiresMoonLord";

        return "";
    }

    public static CultivationStage GetBossWorldStage(string boss) => boss switch
    {
        "thunder_marsh_jiao" or "abyssal_star_womb" => CultivationStage.GoldenCore,
        "formless_sword_soul" or "greenwood_medicine_king_echo" => CultivationStage.NascentSoul,
        "heaven_tablet_guardian" or "broken_heaven_inspector" => CultivationStage.SpiritSevering,
        "moonbone_immortal" or "old_heaven_dao_core" => CultivationStage.Tribulation,
        _ => CultivationStage.None
    };

    public static string GetRequiredBoss(CultivationStage target) => target switch
    {
        CultivationStage.Foundation => "tribulation_cloud_avatar",
        CultivationStage.GoldenCore => "abyssal_star_womb",
        CultivationStage.NascentSoul => "formless_sword_soul",
        CultivationStage.SpiritSevering => "heaven_tablet_guardian",
        CultivationStage.Tribulation => "broken_heaven_inspector",
        CultivationStage.DaoSevering => "old_heaven_dao_core",
        _ => ""
    };

    public static bool CanRetry(CultivationStage current, int activeTrialTicks, bool cleared) =>
        current >= CultivationStage.Foundation && current <= CultivationStage.DaoSevering
        && activeTrialTicks == 0 && !cleared;
}
