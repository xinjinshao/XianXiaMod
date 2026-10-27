namespace XianXia.Common.Systems;

public enum ArtifactSkill : byte { SwordBurst = 1, ArrayPulse = 2, WardGuard = 3, MoonCrescent = 4, WheelFormation = 5, HammerVolley = 6, ThunderVolley = 7, FlameFan = 8, WoodgrainRecall = 9 }
public static class ArtifactSkillRules
{
    public const ArtifactSkill LastPanelSkill = ArtifactSkill.WoodgrainRecall;
    public const int MaximumCooldown = 1200;
    public static ArtifactSkill StepPanelSkill(ArtifactSkill current, bool previous) => !IsValid(current)
        ? previous ? LastPanelSkill : ArtifactSkill.SwordBurst
        : (ArtifactSkill)(previous ? current == ArtifactSkill.SwordBurst ? (int)LastPanelSkill : (int)current - 1 : current == LastPanelSkill ? 1 : (int)current + 1);
    public const float HammerDamageMultiplier = 1.75f;
    public const float ThunderDamageMultiplier = 1.5f;
    public const int ThunderRange = 800, ThunderSpacing = 96, ThunderDrop = 240;
    public static bool IsValid(ArtifactSkill skill) => skill is ArtifactSkill.SwordBurst or ArtifactSkill.ArrayPulse or ArtifactSkill.WardGuard or ArtifactSkill.MoonCrescent or ArtifactSkill.WheelFormation or ArtifactSkill.HammerVolley or ArtifactSkill.ThunderVolley or ArtifactSkill.FlameFan or ArtifactSkill.WoodgrainRecall;
    public static int Cost(ArtifactSkill skill) => skill == ArtifactSkill.WoodgrainRecall ? 16 : skill == ArtifactSkill.WheelFormation ? 36 : skill is ArtifactSkill.MoonCrescent or ArtifactSkill.HammerVolley or ArtifactSkill.ThunderVolley ? 30 : skill == ArtifactSkill.SwordBurst ? 24 : 18;
    public static int Cooldown(ArtifactSkill skill) => skill == ArtifactSkill.SwordBurst ? 900 : 1200;
}
