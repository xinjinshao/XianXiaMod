namespace XianXia.Common.Systems;

public static class EnemySpawnRules
{
    public static int RequiredTier(string enemy) => enemy switch {
        "WanderingSpiritSlime" or "ShatteredJadeWorm" or "TalismanBat" or "HerbGardenVineSpirit"
            or "MiasmaFlowerMoth" or "IronShardSpirit" or "FurnaceAshGolem" => 0,
        "ThunderPatternHawk" or "TribulationCloudling" or "StarAbyssLarva" or "StarEclipsedCultivator" => 1,
        "ObsessedSwordCultivator" or "ScriptureArchiveEcho" => 2,
        "HeavenTabletGuard" or "CelestialPuppet" => 3,
        "MoonboneCultivator" or "ArchivedImmortalSoul" => 4,
        _ => -1,
    };
    public static bool Allows(string enemy,bool hardmode,bool plantera,bool golem,bool moonLord)
    {
        int tier = RequiredTier(enemy);
        return tier >= 0 && (tier < 1 || hardmode) && (tier < 2 || plantera)
            && (tier < 3 || golem) && (tier < 4 || moonLord);
    }
}
