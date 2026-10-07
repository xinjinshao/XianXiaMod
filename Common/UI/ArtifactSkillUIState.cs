using System;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.ModLoader;
using XianXia.Common.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using XianXia.Common.Players;
using XianXia.Common.Systems;

namespace XianXia.Common.UI;

public class ArtifactSkillUIState : UIState
{
    private UIPanel panel;
    private UIText status;
    private readonly List<(UIElement Button, ArtifactSkill Skill)> skillButtons = new();
    private string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Skills." + key, args);
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.Left.Set(28,0); panel.Top.Set(140,0); panel.Width.Set(300,0); panel.Height.Set(120,0); Append(panel);
        status = new UIText("",0.75f); panel.Append(status);
        AddButton("Sword",0,ArtifactSkill.SwordBurst); AddButton("Array",92,ArtifactSkill.ArrayPulse); AddButton("Ward",184,ArtifactSkill.WardGuard);
        AddButton("Moon",0,ArtifactSkill.MoonCrescent,64);
    }
    private void AddButton(string key,float left,ArtifactSkill skill,float top=28)
    {
        var button = new UITextPanel<string>(Text(key),0.75f); button.Left.Set(left,0); button.Top.Set(top,0);
        button.Width.Set(86,0); button.Height.Set(30,0);
        skillButtons.Add((button, skill));
        button.OnLeftClick += (_,_) => ArtifactSkillTransactions.Request(Main.LocalPlayer,skill); panel.Append(button);
    }
    public override void Update(GameTime gameTime)
    {
        if (Main.gameMenu || Main.dedServ || !Main.LocalPlayer.active || Main.LocalPlayer.dead) return;
        var config = Terraria.ModLoader.ModContent.GetInstance<XianXiaClientConfig>();
        float scale = Math.Max(0.01f, Main.UIScale);
        float x = MathHelper.Clamp(config.SkillPanelX, 0f, Math.Max(0f, Main.screenWidth / scale - 300f));
        float y = MathHelper.Clamp(config.SkillPanelY, 0f, Math.Max(0f, Main.screenHeight / scale - 120f));
        if (panel.Left.Pixels != x || panel.Top.Pixels != y)
        {
            panel.Left.Set(x, 0); panel.Top.Set(y, 0); panel.Recalculate();
        }
        base.Update(gameTime);
        var state = Main.LocalPlayer.GetModPlayer<XianXiaPlayer>();
        status.SetText(state.wardGuardTimer > 0 ? Text("WardStatus",(state.activeSkillCooldown+59)/60,(state.wardGuardTimer+59)/60)
            : state.activeSkillCooldown > 0 ? Text("Cooldown", (state.activeSkillCooldown + 59)/60) : Text("Ready"));
        if (panel.ContainsPoint(Main.MouseScreen / scale)) Main.LocalPlayer.mouseInterface = true;
        foreach (var entry in skillButtons)
            if (entry.Button.ContainsPoint(Main.MouseScreen / scale))
                Main.instance.MouseText(SkillTooltip(entry.Skill, state));
    }
    private string SkillTooltip(ArtifactSkill skill, XianXiaPlayer state)
    {
        ModKeybind binding = skill == ArtifactSkill.WardGuard
            ? ArtifactKeybindSystem.WardSkillKey : ArtifactKeybindSystem.ArtifactSkillKey;
        string Keys(InputMode mode)
        {
            var assigned = binding?.GetAssignedKeys(mode);
            return assigned == null || assigned.Count == 0 ? Text("Unbound") : string.Join(", ", assigned);
        }
        var route = ArtifactSkillTransactions.HeldSkill(Main.LocalPlayer.HeldItem) == skill
            ? RefinedArtifact.ActiveDaoRoute(Main.LocalPlayer.HeldItem) : DownedBossSystem.EndgameRoute.None;
        int cost = state.GetSpiritualEnergyCost(DaoArtifactRules.SkillCost(skill, route));
        string requirement = skill switch
        {
            ArtifactSkill.SwordBurst => Text("SwordRequirement"),
            ArtifactSkill.ArrayPulse => Text("ArrayRequirement"),
            ArtifactSkill.MoonCrescent => Text(route == DownedBossSystem.EndgameRoute.None ? "MoonRequirement" : "MoonDaoRequirement"),
            _ => Text("WardRequirement")
        };
        return requirement + "\n" + Text("SkillNumbers", cost, ArtifactSkillRules.Cooldown(skill) / 60)
            + "\n" + Text("AssignedControls", Keys(InputMode.Keyboard), Keys(InputMode.XBoxGamepad));
    }
}
