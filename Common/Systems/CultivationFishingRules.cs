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
    public static bool Allows(int tier, bool hardmode, bool plantera, bool golem, bool moonLord) =>
        tier >= 0 && tier <= 4 && (tier < 1 || hardmode) && (tier < 2 || plantera)
        && (tier < 3 || golem) && (tier < 4 || moonLord);
}
