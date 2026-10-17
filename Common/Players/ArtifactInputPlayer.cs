using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Players;

public class ArtifactInputPlayer : ModPlayer
{
    public ArtifactSkill? SelectedPanelSkill { get; private set; }
    public override void Initialize() => SelectedPanelSkill = null;
    public override void OnEnterWorld() => SelectedPanelSkill = null;
    public override void UpdateDead() => SelectedPanelSkill = null;
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Main.dedServ || Player.whoAmI != Main.myPlayer || Main.gameMenu || Main.blockInput || Main.drawingPlayerChat || !Player.active || Player.dead || Player.noItems || Player.CCed) { SelectedPanelSkill = null; return; }
        if (Main.playerInventory || !Player.GetModPlayer<XianXiaPlayer>().discoveredSpiritualEnergy) SelectedPanelSkill = null;
        else {
            bool next = ArtifactKeybindSystem.PanelNextKey?.JustPressed == true, previous = ArtifactKeybindSystem.PanelPreviousKey?.JustPressed == true;
            if (ArtifactKeybindSystem.PanelCancelKey?.JustPressed == true || next && previous) { SelectedPanelSkill = null; return; }
            if (next || previous) {
                SelectedPanelSkill = SelectedPanelSkill is ArtifactSkill selected
                    ? ArtifactSkillRules.StepPanelSkill(selected,previous)
                    : ArtifactSkillTransactions.HeldSkill(Player.HeldItem) ?? (previous ? ArtifactSkill.ThunderVolley : ArtifactSkill.SwordBurst);
                return;
            }
            if (ArtifactKeybindSystem.PanelCastKey?.JustPressed == true && SelectedPanelSkill is ArtifactSkill panelSkill) {
                if (ArtifactSkillTransactions.UnavailableReason(Player,panelSkill) == null) ArtifactSkillTransactions.Request(Player,panelSkill);
                return;
            }
        }
        if (ArtifactKeybindSystem.WardSkillKey?.JustPressed == true) ArtifactSkillTransactions.Request(Player, ArtifactSkill.WardGuard);
        else if (ArtifactKeybindSystem.ArtifactSkillKey?.JustPressed == true && ArtifactSkillTransactions.HeldSkill(Player.HeldItem) is ArtifactSkill skill)
            ArtifactSkillTransactions.Request(Player, skill);
    }
}
