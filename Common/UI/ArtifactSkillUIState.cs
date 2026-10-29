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
    private const float PanelHeight = 228f;
    private UIPanel panel;
    private UIText status;
    private readonly List<(UITextPanel<string> Button, ArtifactSkill Skill)> skillButtons = new();
    private string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Skills." + key, args);
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.Left.Set(28,0); panel.Top.Set(140,0); panel.Width.Set(300,0); panel.Height.Set(PanelHeight,0); Append(panel);
        status = new UIText("",0.75f); panel.Append(status);
        AddButton("Sword",0,ArtifactSkill.SwordBurst); AddButton("Array",92,ArtifactSkill.ArrayPulse); AddButton("Ward",184,ArtifactSkill.WardGuard);
        AddButton("Moon",0,ArtifactSkill.MoonCrescent,64);
        AddButton("Wheel",92,ArtifactSkill.WheelFormation,64);
        AddButton("Hammer",184,ArtifactSkill.HammerVolley,64);
        AddButton("Thunder",0,ArtifactSkill.ThunderVolley,100);
        AddButton("Flame",92,ArtifactSkill.FlameFan,100);
        AddButton("Woodgrain",184,ArtifactSkill.WoodgrainRecall,100);
        AddButton("Spiritwood",0,ArtifactSkill.SpiritwoodVolley,136);
        AddButton("StarSplit",92,ArtifactSkill.StarSplitVolley,136);
        AddButton("Sect",184,ArtifactSkill.SectVolley,136);
        AddButton("HeavenLaw",0,ArtifactSkill.HeavenLawVolley,172);
        AddButton("Calamity",92,ArtifactSkill.CalamityVolley,172);
    }
    private void AddButton(string key,float left,ArtifactSkill skill,float top=28)
    {
        var button = new UITextPanel<string>(Text(key),0.75f); button.Left.Set(left,0); button.Top.Set(top,0);
        button.Width.Set(86,0); button.Height.Set(30,0);
        skillButtons.Add((button, skill));
        button.OnLeftClick += (_,_) => {
            if (ArtifactSkillTransactions.UnavailableReason(Main.LocalPlayer,skill) == null) ArtifactSkillTransactions.Request(Main.LocalPlayer,skill);
        }; panel.Append(button);
    }
    public override void Update(GameTime gameTime)
    {
        if (Main.gameMenu || Main.dedServ || !Main.LocalPlayer.active || Main.LocalPlayer.dead) return;
        var config = Terraria.ModLoader.ModContent.GetInstance<XianXiaClientConfig>();
        float scale = Math.Max(0.01f, Main.UIScale);
        float x = MathHelper.Clamp(config.SkillPanelX, 0f, Math.Max(0f, Main.screenWidth / scale - 300f));
        float y = MathHelper.Clamp(config.SkillPanelY, 0f, Math.Max(0f, Main.screenHeight / scale - PanelHeight));
        if (panel.Left.Pixels != x || panel.Top.Pixels != y)
        {
            panel.Left.Set(x, 0); panel.Top.Set(y, 0); panel.Recalculate();
        }
        base.Update(gameTime);
        var state = Main.LocalPlayer.GetModPlayer<XianXiaPlayer>();
        status.SetText(state.wardGuardTimer > 0 ? Text("WardStatus",(state.activeSkillCooldown+59)/60,(state.wardGuardTimer+59)/60)
            : state.activeSkillCooldown > 0 ? Text("Cooldown", (state.activeSkillCooldown + 59)/60) : Text("Ready"));
        if (panel.ContainsPoint(Main.MouseScreen / scale)) Main.LocalPlayer.mouseInterface = true;
        var selected = Main.LocalPlayer.GetModPlayer<ArtifactInputPlayer>().SelectedPanelSkill;
        bool hovered = false;
        foreach (var entry in skillButtons) {
            bool available = ArtifactSkillTransactions.UnavailableReason(Main.LocalPlayer,entry.Skill) == null;
            entry.Button.BackgroundColor = available ? new Color(45,75,105) : new Color(45,45,50);
            entry.Button.BorderColor = selected == entry.Skill ? Color.Gold : available ? new Color(130,190,215) : new Color(80,80,85);
            if (entry.Button.ContainsPoint(Main.MouseScreen / scale))
            { hovered = true; Main.instance.MouseText(SkillTooltip(entry.Skill, state)); }
        }
        if (!hovered && selected is ArtifactSkill selectedSkill) Main.instance.MouseText(Text("PanelControls") + "\n" + SkillTooltip(selectedSkill,state));
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
        var route = ArtifactSkillTransactions.SkillDaoRoute(Main.LocalPlayer,skill);
        int cost = state.GetSpiritualEnergyCost(DaoArtifactRules.SkillCost(skill, route));
        string requirement = skill switch
        {
            ArtifactSkill.SwordBurst => Text(route == DownedBossSystem.EndgameRoute.None ? "SwordRequirement" : "SwordDaoRequirement"),
            ArtifactSkill.ArrayPulse => Text(route == DownedBossSystem.EndgameRoute.None ? "ArrayRequirement" : "ArrayDaoRequirement"),
            ArtifactSkill.MoonCrescent => Text(route == DownedBossSystem.EndgameRoute.None ? "MoonRequirement" : "MoonDaoRequirement"),
            ArtifactSkill.WheelFormation => Text(route == DownedBossSystem.EndgameRoute.None ? "WheelRequirement" : "WheelDaoRequirement"),
            ArtifactSkill.HammerVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "HammerRequirement" : "HammerDaoRequirement"),
            ArtifactSkill.WoodgrainRecall => Text(route == DownedBossSystem.EndgameRoute.None ? "WoodgrainRequirement" : "WoodgrainDaoRequirement"),
            ArtifactSkill.SpiritwoodVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "SpiritwoodRequirement" : "SpiritwoodDaoRequirement"),
            ArtifactSkill.StarSplitVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "StarSplitRequirement" : "StarSplitDaoRequirement"),
            ArtifactSkill.SectVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "SectRequirement" : "SectDaoRequirement"),
            ArtifactSkill.HeavenLawVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "HeavenLawRequirement" : "HeavenLawDaoRequirement"),
            ArtifactSkill.CalamityVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "CalamityRequirement" : "CalamityDaoRequirement"),
            ArtifactSkill.FlameFan => Text(route == DownedBossSystem.EndgameRoute.None ? "FlameRequirement" : "FlameDaoRequirement"),
            ArtifactSkill.ThunderVolley => Text(route == DownedBossSystem.EndgameRoute.None ? "ThunderRequirement" : "ThunderDaoRequirement"),
            _ => Text(route == DownedBossSystem.EndgameRoute.None ? "WardRequirement" : "WardDaoRequirement")
        };
        string reason = ArtifactSkillTransactions.UnavailableReason(Main.LocalPlayer,skill);
        return (reason == null ? Text("CanRequest") : Language.GetTextValue(reason)) + "\n" + requirement + "\n" + Text("SkillNumbers", cost, ArtifactSkillRules.Cooldown(skill) / 60)
            + "\n" + Text("AssignedControls", Keys(InputMode.Keyboard), Keys(InputMode.XBoxGamepad));
    }
}
