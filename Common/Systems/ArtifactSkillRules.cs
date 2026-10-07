namespace XianXia.Common.Systems;

public enum ArtifactSkill : byte { SwordBurst = 1, ArrayPulse = 2, WardGuard = 3, MoonCrescent = 4 }
public static class ArtifactSkillRules
{
    public const int MaximumCooldown = 1200;
    public static bool IsValid(ArtifactSkill skill) => skill is ArtifactSkill.SwordBurst or ArtifactSkill.ArrayPulse or ArtifactSkill.WardGuard or ArtifactSkill.MoonCrescent;
    public static int Cost(ArtifactSkill skill) => skill == ArtifactSkill.MoonCrescent ? 30 : skill == ArtifactSkill.SwordBurst ? 24 : 18;
    public static int Cooldown(ArtifactSkill skill) => skill == ArtifactSkill.SwordBurst ? 900 : 1200;
}
