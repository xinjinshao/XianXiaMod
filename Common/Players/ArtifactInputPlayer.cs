using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Players;

public class ArtifactInputPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Main.dedServ || Player.whoAmI != Main.myPlayer || Main.gameMenu || Main.blockInput || Main.drawingPlayerChat || !Player.active || Player.dead || Player.noItems || Player.CCed) return;
        if (ArtifactKeybindSystem.WardSkillKey?.JustPressed == true) ArtifactSkillTransactions.Request(Player, ArtifactSkill.WardGuard);
        else if (ArtifactKeybindSystem.ArtifactSkillKey?.JustPressed == true && ArtifactSkillTransactions.HeldSkill(Player.HeldItem) is ArtifactSkill skill)
            ArtifactSkillTransactions.Request(Player, skill);
    }
}
