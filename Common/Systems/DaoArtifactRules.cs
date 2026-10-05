using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public static class DaoArtifactRules
{
    public const int StoneCost = 36;
    public static DownedBossSystem.EndgameRoute Normalize(int route) => route >= 1 && route <= 3
        ? (DownedBossSystem.EndgameRoute)route : DownedBossSystem.EndgameRoute.None;
    public static bool CanTransform(byte level, bool awakened, bool inscribed, int previousRoute,
        CultivationStage stage, int worldRoute, bool defeatedCore) => level == 3 && awakened && inscribed
        && previousRoute == 0 && stage == CultivationStage.DaoSevering && Normalize(worldRoute) != DownedBossSystem.EndgameRoute.None && defeatedCore;
    public static float DamageScale(string name) => name == "CloudpiercerFlyingSword" ? 150f / 28f : 80f / 18f;
    public static int WeaponCost(string name, DownedBossSystem.EndgameRoute route) => name == "CloudpiercerFlyingSword"
        ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 16 : 20
        : route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 22 : 28;
    public static int SkillCost(ArtifactSkill skill, DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.None
        || skill == ArtifactSkill.WardGuard ? ArtifactSkillRules.Cost(skill)
        : skill == ArtifactSkill.SwordBurst ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 40 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 60 : 48
        : route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 26 : 32;
    public static int PulseHeal(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.RebuildHeaven ? 40 : 20;
    public static int BurstMultiplier(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.SeverHeaven ? 3 : 2;
    public static float ExtraDamage(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.SeverHeaven ? 0.15f : 0;
}
