using System;

namespace XianXia.Common.Systems;

public static class BossStatRules
{
    public static (int Life, int Damage) Get(string boss) => boss switch
    {
        "SpiritVeinWyrm" => (1200, 22),
        "GardenWarden" => (2800, 28),
        "BlackFurnaceIronGolem" => (3200, 34),
        "TribulationCloudAvatar" => (4200, 30),
        "ThunderMarshJiao" => (18000, 58),
        "AbyssalStarWomb" => (21000, 54),
        "FormlessSwordSoul" => (48000, 72),
        "GreenwoodMedicineKingEcho" => (52000, 66),
        "HeavenTabletGuardian" => (86000, 82),
        "BrokenHeavenInspector" => (96000, 92),
        "MoonboneImmortal" => (420000, 180),
        "OldHeavenDaoCore" => (650000, 220),
        _ => throw new ArgumentException("Unknown cultivation boss", nameof(boss))
    };

    // Native expert/master/seed scaling already ran before this hook.
    // Apply only the supplied player balance and boss adjustment, once.
    public static int ScaleLife(int engineLife, float balance, float bossAdjustment)
    {
        if (!float.IsFinite(balance) || balance <= 0f) balance = 1f;
        if (!float.IsFinite(bossAdjustment) || bossAdjustment <= 0f) bossAdjustment = 1f;
        double value = (double)Math.Max(1, engineLife) * balance * bossAdjustment;
        return (int)Math.Clamp(value, 1d, int.MaxValue);
    }
}
