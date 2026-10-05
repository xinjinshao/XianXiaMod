using System;

namespace XianXia.Common.Systems;

public enum PillQuality : byte { Coarse = 1, Standard = 2, Fine = 3, Spirit = 4 }

public static class PillQualityRules
{
    public static PillQuality Normalize(int value) => value >= 1 && value <= 4 ? (PillQuality)value : PillQuality.Standard;
    public static PillQuality Roll(float value)
    {
        if (!float.IsFinite(value) || value < 0 || value >= 1) return PillQuality.Standard;
        return value < 0.05f ? PillQuality.Spirit : value < 0.20f ? PillQuality.Fine
            : value < 0.50f ? PillQuality.Standard : PillQuality.Coarse;
    }
    public static float Multiplier(PillQuality quality) => Normalize((int)quality) switch
    { PillQuality.Coarse => 0.75f, PillQuality.Fine => 1.25f, PillQuality.Spirit => 1.5f, _ => 1f };
    public static int Scale(int amount, PillQuality quality) => amount <= 0 ? 0 : (int)Math.Ceiling(amount * Multiplier(quality));
    public static int BonusEnergy(PillQuality quality) => quality switch { PillQuality.Fine => 14, PillQuality.Spirit => 16, _ => 0 };
    public static int RegenerationTicks(PillQuality quality) => quality switch { PillQuality.Fine => 900, PillQuality.Spirit => 1200, _ => 0 };
}
