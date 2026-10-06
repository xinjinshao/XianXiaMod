namespace XianXia.Common.Systems;
public static class QiRecoveryRules
{
    public const int BaseRecovery = 40, PressureCost = 10, CooldownTicks = 1800;
    public static bool CanUse(bool awakened, int energy, int maximum, bool coolingDown) =>
        awakened && maximum > 0 && energy >= 0 && energy < maximum && !coolingDown;
}
