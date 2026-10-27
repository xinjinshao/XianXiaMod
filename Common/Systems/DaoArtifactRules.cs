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
    public static float DamageScale(string name) => name switch {
        "SectMechanismCrossbow" => 220f / 110f,
        "ThunderTalismanArrayPlate" => 120f / 46f,
        "HeavenLawArbalest" => 260f / 170f,
        "StarCalamityMechanismCase" => 340f / 260f,
        "ArchiveStarCodex" => 300f / 230f,
        "BrokenHeavenDecree" => 240f / 165f,
        "StarEclipseArbalest" => 180f / 68f,
        "SpiritwoodCrossbow" => 120f / 12f,
        "WoodgrainFlyingSword" => 140f / 14f,
        "CinnabarTalismanFlameItem" => 120f / 24f,
        "CloudpiercerFlyingSword" => 150f / 28f,
        "GreenwoodArrayPlate" => 80f / 18f,
        "MoonboneDharmaSword" => 280f / 220f,
        "FormlessSwordWheel" => 130f / 92f,
        "BlackFurnaceWarhammer" => 84f / 54f,
        "ThunderPatternSwordCase" => 90f / 54f,
        "HeavenTabletWardSeal" => 210f / 156f,
        _ => 1f
    };
    public static int WeaponCost(string name, DownedBossSystem.EndgameRoute route) => name switch {
        "SectMechanismCrossbow" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 18 : 24,
        "ThunderTalismanArrayPlate" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 24 : 30,
        "HeavenLawArbalest" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 24 : 30,
        "StarCalamityMechanismCase" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 28 : 36,
        "ArchiveStarCodex" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 28 : 36,
        "BrokenHeavenDecree" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 22 : 28,
        "StarEclipseArbalest" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 14 : 18,
        "SpiritwoodCrossbow" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 9 : 12,
        "WoodgrainFlyingSword" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 12 : 16,
        "CinnabarTalismanFlameItem" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 9 : 12,
        "CloudpiercerFlyingSword" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 16 : 20,
        "MoonboneDharmaSword" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 14 : 18,
        "FormlessSwordWheel" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 11 : 14,
        "HeavenTabletWardSeal" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 24 : 30,
        "ThunderPatternSwordCase" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 8 : 10,
        "BlackFurnaceWarhammer" => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 12 : 16,
        _ => route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 22 : 28
    };
    public static int SkillCost(ArtifactSkill skill, DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.None
        ? ArtifactSkillRules.Cost(skill)
        : skill == ArtifactSkill.WardGuard ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 20 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 30 : 24
        : skill == ArtifactSkill.ThunderVolley ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 30 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 48 : 36
        : skill == ArtifactSkill.HammerVolley ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 34 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 54 : 40
        : skill == ArtifactSkill.WheelFormation ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 40 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 60 : 48
        : skill == ArtifactSkill.MoonCrescent ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 30 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 48 : 36
        : skill == ArtifactSkill.SwordBurst ? route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 40 : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 60 : 48
        : route == DownedBossSystem.EndgameRoute.AcceptStarAbyss ? 26 : 32;
    public static float MoonBurstMultiplier(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.None
        ? 1.5f : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 2.5f : 2f;
    public static float ThunderBurstMultiplier(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.None
        ? ArtifactSkillRules.ThunderDamageMultiplier : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 2.5f : 2f;
    public static int PulseHeal(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.RebuildHeaven ? 40 : 20;
    public static float HammerBurstMultiplier(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.None
        ? ArtifactSkillRules.HammerDamageMultiplier : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 2.75f : 2.25f;
    public static float WheelBurstMultiplier(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.None
        ? 1.5f : route == DownedBossSystem.EndgameRoute.SeverHeaven ? 2.5f : 2f;
    public static int BurstMultiplier(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.SeverHeaven ? 3 : 2;
    public static float ExtraDamage(DownedBossSystem.EndgameRoute route) => route == DownedBossSystem.EndgameRoute.SeverHeaven ? 0.15f : 0;
}
