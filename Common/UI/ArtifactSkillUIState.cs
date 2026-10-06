using System;
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
    private string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Skills." + key, args);
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.Left.Set(28,0); panel.Top.Set(140,0); panel.Width.Set(300,0); panel.Height.Set(84,0); Append(panel);
        status = new UIText("",0.75f); panel.Append(status);
        AddButton("Sword",0,ArtifactSkill.SwordBurst); AddButton("Array",92,ArtifactSkill.ArrayPulse); AddButton("Ward",184,ArtifactSkill.WardGuard);
    }
    private void AddButton(string key,float left,ArtifactSkill skill)
    {
        var button = new UITextPanel<string>(Text(key),0.75f); button.Left.Set(left,0); button.Top.Set(28,0);
        button.Width.Set(86,0); button.Height.Set(30,0);
        button.OnLeftClick += (_,_) => ArtifactSkillTransactions.Request(Main.LocalPlayer,skill); panel.Append(button);
    }
    public override void Update(GameTime gameTime)
    {
        if (Main.gameMenu || Main.dedServ || !Main.LocalPlayer.active || Main.LocalPlayer.dead) return;
        var config = Terraria.ModLoader.ModContent.GetInstance<XianXiaClientConfig>();
        float scale = Math.Max(0.01f, Main.UIScale);
        float x = MathHelper.Clamp(config.SkillPanelX, 0f, Math.Max(0f, Main.screenWidth / scale - 300f));
        float y = MathHelper.Clamp(config.SkillPanelY, 0f, Math.Max(0f, Main.screenHeight / scale - 84f));
        if (panel.Left.Pixels != x || panel.Top.Pixels != y)
        {
            panel.Left.Set(x, 0); panel.Top.Set(y, 0); panel.Recalculate();
        }
        base.Update(gameTime);
        var state = Main.LocalPlayer.GetModPlayer<XianXiaPlayer>();
        status.SetText(state.wardGuardTimer > 0 ? Text("WardStatus",(state.activeSkillCooldown+59)/60,(state.wardGuardTimer+59)/60)
            : state.activeSkillCooldown > 0 ? Text("Cooldown", (state.activeSkillCooldown + 59)/60) : Text("Ready"));
        if (panel.ContainsPoint(Main.MouseScreen / scale)) Main.LocalPlayer.mouseInterface = true;
    }
}
