namespace XianXia.Common.Systems;

public static class CultivationFishingRules
{
    public static bool Allows(int tier, bool hardmode, bool plantera, bool golem, bool moonLord) =>
        tier >= 0 && tier <= 4 && (tier < 1 || hardmode) && (tier < 2 || plantera)
        && (tier < 3 || golem) && (tier < 4 || moonLord);
}
