using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public static class RefinementRules
{
    public const int MaximumLevel = 3;
    public const int AwakeningStoneCost = 24;
    public static bool CanAwaken(int level, bool inscribed, bool awakened, CultivationStage stage, bool bossDefeated) =>
        level == MaximumLevel && inscribed && !awakened && stage >= CultivationStage.NascentSoul && stage <= CultivationStage.DaoSevering && bossDefeated;
    public static byte Normalize(int level) => level >= 0 && level <= MaximumLevel ? (byte)level : (byte)0;
    public static float DamageBonus(int level) => Normalize(level) * 0.04f;
    public static int StoneCost(int level) => level >= 0 && level < MaximumLevel ? 6 + level * 6 : 0;
    public static CultivationStage RequiredStage(int currentLevel) => currentLevel switch
    { 0 => CultivationStage.Foundation, 1 => CultivationStage.GoldenCore, _ => CultivationStage.NascentSoul };
    public static string RequiredBoss(int currentLevel) => currentLevel switch
    { 0 => "black_furnace_iron_golem", 1 => "thunder_marsh_jiao", _ => "formless_sword_soul" };
    public static bool CanAdvance(int level, bool inscribed, CultivationStage stage, bool bossDefeated) =>
        level >= 0 && level < MaximumLevel && inscribed && stage >= RequiredStage(level) && stage <= CultivationStage.DaoSevering && bossDefeated;
}
