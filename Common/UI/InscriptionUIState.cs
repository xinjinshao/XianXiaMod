using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.Items;
using XianXia.Common.Systems;

namespace XianXia.Common.UI;

public class InscriptionUIState : UIState
{
    private UIPanel panel;
    private UIElement rows;
    private UIText title, summary, pageLabel;
    private int toolSlot, toolType, page, targetSlot = -1, targetType;
    private int targetPrefix;
    private byte previousKind;
    private byte previousLevel;
    private bool previousAwakened;
    private byte previousDaoRoute, previousWorldRoute;
    private int refreshTimer;
    private bool targetChanged;
    private string targetName;
    private readonly List<int> eligibleSlots = new();
    private string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Inscriptions." + key, args);
    private void Close() => ModContent.GetInstance<InscriptionUISystem>().Close();
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.Width.Set(540, 0); panel.Height.Set(470, 0); panel.HAlign = 0.75f; panel.VAlign = 0.5f;
        panel.BackgroundColor = new Color(25, 45, 55, 235); Append(panel);
        title = new UIText(""); title.Top.Set(2, 0); panel.Append(title);
        summary = new UIText("", 0.75f) { IsWrapped = true }; summary.Top.Set(32, 0);
        summary.Width.Set(0, 1); summary.Height.Set(110, 0); panel.Append(summary);
        rows = new UIElement(); rows.Top.Set(150, 0); rows.Width.Set(0, 1); rows.Height.Set(210, 0); panel.Append(rows);
        Button(panel, Text("Previous"), 0, 366, 72, () => { page--; Refresh(); });
        Button(panel, Text("Next"), 80, 366, 72, () => { page++; Refresh(); });
        pageLabel = new UIText(""); pageLabel.Left.Set(164, 0); pageLabel.Top.Set(374, 0); panel.Append(pageLabel);
        Button(panel, Text("Confirm"), 0, 405, 330, () => {
            if (targetSlot < 0) return;
            if (!TargetStillMatches(Main.LocalPlayer)) { targetSlot = -1; targetChanged = true; Refresh(); return; }
            if (Main.LocalPlayer.inventory[toolSlot].ModItem is InscriptionToolItem tool && tool.TransformsArtifact)
                DaoArtifactTransactions.Request(Main.LocalPlayer,toolSlot,toolType,targetSlot,targetType,targetPrefix,previousKind,previousLevel,previousAwakened,previousDaoRoute,previousWorldRoute);
            else InscriptionTransactions.Request(Main.LocalPlayer, toolSlot, toolType, targetSlot, targetType, targetPrefix, previousKind, previousLevel, previousAwakened);
            Close();
        });
        Button(panel, Text("Close"), 350, 405, 160, Close);
    }
    private static void Button(UIElement parent, string text, float x, float y, float width, Action action)
    {
        string shown = text;
        while (shown.Length > 1 && FontAssets.MouseText.Value.MeasureString(shown).X * 0.75f > width - 24)
            shown = shown[..^1];
        if (shown.Length < text.Length && shown.Length > 2) shown = shown[..^2] + "…";
        var button = new InscriptionButton(shown, text); button.Left.Set(x, 0); button.Top.Set(y, 0);
        button.Width.Set(width, 0); button.Height.Set(30, 0);
        button.OnLeftClick += (_, _) => action(); parent.Append(button);
    }
    private sealed class InscriptionButton : UITextPanel<string>
    {
        private readonly string fullText;
        public InscriptionButton(string shown, string full) : base(shown, 0.75f) { fullText = full; }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            if (IsMouseHovering) Main.hoverItemName = fullText;
        }
    }
    public void SelectTool(int slot)
    {
        toolSlot = slot; toolType = Main.LocalPlayer.inventory[slot].type;
        targetSlot = -1; targetChanged = false; page = 0; refreshTimer = 0; Refresh();
    }
    private void Refresh()
    {
        Player player = Main.LocalPlayer;
        if (player.inventory[toolSlot].ModItem is not InscriptionToolItem tool) { Close(); return; }
        title.SetText(Text("Title", player.inventory[toolSlot].AffixName()));
        string cost = tool.TargetKind == InscriptionKind.None ? Text("RemovalCost") : Text("NeedleCost", InscriptionRules.SpiritStoneCost);
        if (tool.RefinesArtifact) cost = Language.GetTextValue("Mods.XianXia.Refinement.Cost", RefinementRules.StoneCost(targetSlot < 0 ? 0 : previousLevel));
        if (tool.AwakensArtifact) cost = Language.GetTextValue("Mods.XianXia.Refinement.AwakeningCost");
        summary.SetText(targetSlot < 0 ? Text("ChooseTarget") + "\n" + cost
            : Text("Selection", targetName, Text(((InscriptionKind)previousKind).ToString()), Text(tool.TargetKind.ToString())) + "\n" + cost);
        if (tool.RefinesArtifact) summary.SetText(targetSlot < 0 ? Language.GetTextValue("Mods.XianXia.Refinement.ChooseTarget")
            : Language.GetTextValue("Mods.XianXia.Refinement.Selection", targetName, previousLevel, Math.Min(3, previousLevel + 1)) + "\n" + cost);
        if (tool.AwakensArtifact) summary.SetText((targetSlot < 0 ? Language.GetTextValue("Mods.XianXia.Refinement.AwakeningChooseTarget")
            : Language.GetTextValue("Mods.XianXia.Refinement.AwakeningSelection", targetName)) + "\n" + cost);
        if (tool.TransformsArtifact) summary.SetText(Language.GetTextValue("Mods.XianXia.DaoArtifacts.Selection",targetSlot < 0 ? Text("ChooseTarget") : targetName,
            Language.GetTextValue(EndgameRouteTransactions.NameKey(DownedBossSystem.ChosenRoute))));
        if (targetChanged) summary.SetText(Text("TargetChanged"));
        eligibleSlots.Clear();
        for (int slot = 0; slot < Math.Min(58, player.inventory.Length); slot++)
            if (slot != toolSlot && (tool.RefinesArtifact ? RefinedArtifact.SupportsRefinement(player.inventory[slot])
                : tool.AwakensArtifact || tool.TransformsArtifact ? RefinedArtifact.IsSample(player.inventory[slot])
                : InscribedEquipment.IsEligible(player.inventory[slot]))) eligibleSlots.Add(slot);
        int pages = Math.Max(1, (eligibleSlots.Count + 5) / 6); page = Math.Clamp(page, 0, pages - 1);
        pageLabel.SetText(Text("Page", page + 1, pages));
        rows.RemoveAllChildren();
        if (eligibleSlots.Count == 0) { rows.Append(new UIText(Text("NoTargets"), 0.75f)); return; }
        for (int row = 0; row < 6 && page * 6 + row < eligibleSlots.Count; row++) {
            int slot = eligibleSlots[page * 6 + row]; Item item = player.inventory[slot];
            int shownType = item.type, shownPrefix = item.prefix;
            byte shownKind = (byte)InscribedEquipment.GetKind(item);
            byte shownLevel = RefinedArtifact.GetLevel(item);
            bool shownAwakened = RefinedArtifact.IsAwakened(item);
            byte shownDaoRoute = (byte)RefinedArtifact.GetDaoRoute(item);
            string shownName = item.AffixName();
            string label = Text("TargetRow", slot + 1, item.AffixName(), Text(InscribedEquipment.GetKind(item).ToString()));
            if (tool.RefinesArtifact) label = Language.GetTextValue("Mods.XianXia.Refinement.Row", slot + 1, shownName, shownLevel);
            if (tool.AwakensArtifact) label = Language.GetTextValue("Mods.XianXia.Refinement.AwakeningRow", slot + 1, shownName,
                Language.GetTextValue(shownAwakened ? "Mods.XianXia.Refinement.AwakeState" : "Mods.XianXia.Refinement.DormantState"));
            if (tool.TransformsArtifact) label = Language.GetTextValue("Mods.XianXia.DaoArtifacts.Row",slot + 1,shownName,Language.GetTextValue(EndgameRouteTransactions.NameKey((DownedBossSystem.EndgameRoute)shownDaoRoute)));
            Button(rows, label, 0, row * 35, 508, () => {
                Item chosen = Main.LocalPlayer.inventory[slot];
                if (!InscribedEquipment.IsEligible(chosen) || chosen.type != shownType || chosen.prefix != shownPrefix
                    || (byte)InscribedEquipment.GetKind(chosen) != shownKind || RefinedArtifact.GetLevel(chosen) != shownLevel
                    || RefinedArtifact.IsAwakened(chosen) != shownAwakened || (byte)RefinedArtifact.GetDaoRoute(chosen) != shownDaoRoute) { targetSlot = -1; targetChanged = true; Refresh(); return; }
                targetSlot = slot; targetType = shownType; targetPrefix = shownPrefix;
                previousKind = shownKind; previousLevel = shownLevel; previousAwakened = shownAwakened; previousDaoRoute = shownDaoRoute;
                targetChanged = false;
                previousWorldRoute = (byte)DownedBossSystem.ChosenRoute; targetName = shownName; Refresh();
            });
        }
    }
    private bool TargetStillMatches(Player player)
    {
        if (targetSlot < 0 || targetSlot >= Math.Min(58, player.inventory.Length)) return false;
        Item item = player.inventory[targetSlot];
        return !item.IsAir && item.type == targetType && item.prefix == targetPrefix
            && (byte)InscribedEquipment.GetKind(item) == previousKind
            && RefinedArtifact.GetLevel(item) == previousLevel
            && RefinedArtifact.IsAwakened(item) == previousAwakened
            && (byte)RefinedArtifact.GetDaoRoute(item) == previousDaoRoute
            && (byte)DownedBossSystem.ChosenRoute == previousWorldRoute;
    }

    public override void Update(GameTime gameTime)
    {
        Player player = Main.LocalPlayer;
        if (Main.gameMenu || player.dead || !Main.playerInventory || !player.active || player.noItems || player.CCed
            || toolSlot < 0 || toolSlot >= Math.Min(58, player.inventory.Length)
            || !Main.mouseItem.IsAir
            || player.selectedItem != toolSlot || player.inventory[toolSlot].type != toolType
            || Main.keyState.IsKeyDown(Keys.Escape)) { Close(); return; }
        if (targetSlot >= 0 && !TargetStillMatches(player)) { targetSlot = -1; targetChanged = true; Refresh(); }
        base.Update(gameTime);
        if (panel.ContainsPoint(Main.MouseScreen / Math.Max(0.01f, Main.UIScale))) player.mouseInterface = true;
        if (++refreshTimer >= 20) { refreshTimer = 0; Refresh(); }
    }
}
