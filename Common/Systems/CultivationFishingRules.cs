namespace XianXia.Common.Systems;

public static class CultivationFishingRules
{
    public static string GetGuidance(bool hardmode, bool plantera, bool golem, bool moonLord)
    {
        int tier = 4;
        while (tier > 0 && !Allows(tier, hardmode, plantera, golem, moonLord)) tier--;
        return Terraria.Localization.Language.GetTextValue("Mods.XianXia.FishingGuide.Guide",
            Terraria.Localization.Language.GetTextValue("Mods.XianXia.FishingGuide.Tier" + tier),
            Items.CultivationFishingCrate.StoneMin, Items.CultivationFishingCrate.StoneMax,
            Items.CultivationFishingCrate.MaterialMin, Items.CultivationFishingCrate.MaterialMax);
    }
    public static string OpeningFailure(int tier, bool hardmode, bool plantera, bool golem, bool moonLord)
    {
        if (tier < 0 || tier > 4) return "Unavailable";
        if (tier >= 1 && !hardmode) return "RequiresHardmode";
        if (tier >= 2 && !plantera) return "RequiresPlantera";
        if (tier >= 3 && !golem) return "RequiresGolem";
        if (tier >= 4 && !moonLord) return "RequiresMoonLord";
        return "";
    }
    public static bool Allows(int tier, bool hardmode, bool plantera, bool golem, bool moonLord) =>
        OpeningFailure(tier, hardmode, plantera, golem, moonLord).Length == 0;
}
