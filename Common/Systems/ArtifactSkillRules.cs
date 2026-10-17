namespace XianXia.Common.Systems;

public enum ArtifactSkill : byte { SwordBurst = 1, ArrayPulse = 2, WardGuard = 3, MoonCrescent = 4, WheelFormation = 5, HammerVolley = 6, ThunderVolley = 7 }
public static class ArtifactSkillRules
{
    public const int MaximumCooldown = 1200;
    public const float HammerDamageMultiplier = 1.75f;
    public const float ThunderDamageMultiplier = 1.5f;
    public const int ThunderRange = 800, ThunderSpacing = 96, ThunderDrop = 240;
    public static bool IsValid(ArtifactSkill skill) => skill is ArtifactSkill.SwordBurst or ArtifactSkill.ArrayPulse or ArtifactSkill.WardGuard or ArtifactSkill.MoonCrescent or ArtifactSkill.WheelFormation or ArtifactSkill.HammerVolley or ArtifactSkill.ThunderVolley;
    public static int Cost(ArtifactSkill skill) => skill == ArtifactSkill.WheelFormation ? 36 : skill is ArtifactSkill.MoonCrescent or ArtifactSkill.HammerVolley or ArtifactSkill.ThunderVolley ? 30 : skill == ArtifactSkill.SwordBurst ? 24 : 18;
    public static int Cooldown(ArtifactSkill skill) => skill == ArtifactSkill.SwordBurst ? 900 : 1200;
}
