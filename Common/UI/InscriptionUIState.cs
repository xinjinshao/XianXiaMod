using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.Items;
using XianXia.Common.Systems;

namespace XianXia.Common.UI;

public class InscriptionUIState : UIState
{
    private UIPanel panel;
    private UIElement rows, content;
    private UIList viewport;
    private UIScrollbar scrollbar;
    private ResponsiveTextButton previousButton, nextButton, confirmButton, closeButton;
    private UIText navigationHint;
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
    private readonly Dictionary<int,Action> selectTargetActions = new();
    private string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Inscriptions." + key, args);
    private void Close() => ModContent.GetInstance<InscriptionUISystem>().Close();
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.Width.Set(540, 0); panel.Height.Set(500, 0); panel.HAlign = 0.75f; panel.VAlign = 0.5f;
        panel.BackgroundColor = new Color(25, 45, 55, 235); panel.SetPadding(12); Append(panel);
        viewport = new UIList(); viewport.Width.Set(-24,1); viewport.Height.Set(0,1); panel.Append(viewport);
        scrollbar = new UIScrollbar(); scrollbar.Left.Set(-20,1); scrollbar.Width.Set(20,0); scrollbar.Height.Set(0,1); panel.Append(scrollbar); viewport.SetScrollbar(scrollbar);
        content = new UIElement(); content.Width.Set(0,1); content.Height.Set(476,0); viewport.Add(content);
        title = new UIText("") { IsWrapped = true }; title.Top.Set(2, 0); title.Width.Set(0,1); content.Append(title);
        summary = new UIText("", 0.75f) { IsWrapped = true }; summary.Top.Set(32, 0);
        summary.Width.Set(0, 1); summary.Height.Set(110, 0); content.Append(summary);
        rows = new UIElement(); rows.Top.Set(150, 0); rows.Width.Set(0, 1); rows.Height.Set(210, 0); content.Append(rows);
        previousButton = Button(content, Text("Previous"), 0, 366, 72, () => { page--; Refresh(); });
        nextButton = Button(content, Text("Next"), 80, 366, 72, () => { page++; Refresh(); });
        pageLabel = new UIText("",0.75f) { IsWrapped = true }; pageLabel.Width.Set(-8,.6f); pageLabel.Left.Set(164, 0); pageLabel.Top.Set(374, 0); content.Append(pageLabel);
        confirmButton = Button(content, Text("Confirm"), 0, 405, 330, ConfirmSelection);
        closeButton = Button(content, Text("Close"), 350, 405, 160, Close);
        navigationHint = new UIText(Text("NavigationHint"),0.65f) { IsWrapped = true }; navigationHint.Top.Set(442,0); navigationHint.Width.Set(0,1); navigationHint.Height.Set(32,0); content.Append(navigationHint);
        previousButton.Width.Set(-4,.2f); nextButton.Left.Set(0,.2f); nextButton.Width.Set(-4,.2f); pageLabel.Left.Set(8,.4f);
        confirmButton.Width.Set(-8,.65f); closeButton.Left.Set(0,.65f); closeButton.Width.Set(0,.35f);
    }
    private static ResponsiveTextButton Button(UIElement parent, string text, float x, float y, float width, Action action)
    {
        var button = new ResponsiveTextButton(text); button.Left.Set(x, 0); button.Top.Set(y, 0);
        button.Width.Set(width, 0); button.Height.Set(30, 0);
        button.OnLeftClick += (_, _) => action(); parent.Append(button); return button;
    }
    public void SelectTool(int slot)
    {
        toolSlot = slot; toolType = Main.LocalPlayer.inventory[slot].type;
        targetSlot = -1; targetChanged = false; page = 0; refreshTimer = 0; Refresh(); scrollbar.ViewPosition = 0;
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
        if (targetSlot >= 0 && TargetStillMatches(player)) {
            string failure = tool.TransformsArtifact ? DaoArtifactTransactions.RequirementFailure(player, player.inventory[targetSlot])
                : InscriptionTransactions.RequirementFailure(player, tool, player.inventory[targetSlot]);
            summary.SetText(summary.Text + "\n" + (failure.Length == 0 ? Text("ReadyToConfirm") : Language.GetTextValue(failure)));
        }
        if (targetChanged) summary.SetText(Text("TargetChanged"));
        eligibleSlots.Clear(); selectTargetActions.Clear();
        for (int slot = 0; slot < Math.Min(58, player.inventory.Length); slot++)
            if (slot != toolSlot && (tool.RefinesArtifact ? RefinedArtifact.SupportsRefinement(player.inventory[slot])
                : tool.AwakensArtifact ? RefinedArtifact.SupportsAwakening(player.inventory[slot])
                : tool.TransformsArtifact ? RefinedArtifact.SupportsDaoTransformation(player.inventory[slot])
                : InscribedEquipment.IsEligible(player.inventory[slot]))) eligibleSlots.Add(slot);
        int pages = Math.Max(1, (eligibleSlots.Count + 5) / 6); page = Math.Clamp(page, 0, pages - 1);
        pageLabel.SetText(Text("Page", page + 1, pages));
        rows.RemoveAllChildren();
        if (eligibleSlots.Count == 0) { var empty = new UIText(Text("NoTargets"), 0.75f) { IsWrapped = true }; empty.Width.Set(0,1); rows.Append(empty); LayoutContent(); return; }
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
            Action choose = () => {
                Item chosen = Main.LocalPlayer.inventory[slot];
                if (!InscribedEquipment.IsEligible(chosen) || chosen.type != shownType || chosen.prefix != shownPrefix
                    || (byte)InscribedEquipment.GetKind(chosen) != shownKind || RefinedArtifact.GetLevel(chosen) != shownLevel
                    || RefinedArtifact.IsAwakened(chosen) != shownAwakened || (byte)RefinedArtifact.GetDaoRoute(chosen) != shownDaoRoute) { targetSlot = -1; targetChanged = true; Refresh(); return; }
                targetSlot = slot; targetType = shownType; targetPrefix = shownPrefix;
                previousKind = shownKind; previousLevel = shownLevel; previousAwakened = shownAwakened; previousDaoRoute = shownDaoRoute;
                targetChanged = false;
                previousWorldRoute = (byte)DownedBossSystem.ChosenRoute; targetName = shownName; Refresh();
            };
            selectTargetActions[slot] = choose;
            var button = Button(rows,label,0,row * 35,508,choose);
            button.Width.Set(0,1);
            if (slot == targetSlot) button.BorderColor = Color.Gold;
        }
        LayoutContent();
    }
    private void LayoutContent()
    {
        title.Recalculate();
        summary.Top.Set(Math.Max(32,title.GetOuterDimensions().Height + 8),0); summary.Recalculate();
        rows.Top.Set(summary.Top.Pixels + Math.Max(110,summary.GetOuterDimensions().Height) + 8,0);
        float pagingTop = rows.Top.Pixels + 216;
        previousButton.Top.Set(pagingTop,0); nextButton.Top.Set(pagingTop,0); pageLabel.Top.Set(pagingTop + 8,0); pageLabel.Recalculate();
        float confirmTop = pagingTop + Math.Max(30,pageLabel.GetOuterDimensions().Height + 8) + 9;
        confirmButton.Top.Set(confirmTop,0); closeButton.Top.Set(confirmTop,0);
        navigationHint.Top.Set(confirmTop + 37,0); navigationHint.Recalculate();
        content.Height.Set(navigationHint.Top.Pixels + Math.Max(32,navigationHint.GetOuterDimensions().Height),0);
        viewport.Recalculate();
    }
    private bool ToolStillMatches(Player player) => !Main.gameMenu && !Main.blockInput && !Main.drawingPlayerChat
        && player.active && !player.dead && Main.playerInventory && !player.noItems && !player.CCed
        && toolSlot >= 0 && toolSlot < Math.Min(58,player.inventory.Length) && Main.mouseItem.IsAir
        && player.selectedItem == toolSlot && player.inventory[toolSlot].type == toolType
        && player.inventory[toolSlot].ModItem is InscriptionToolItem;
    private void ConfirmSelection()
    {
        Player player = Main.LocalPlayer;
        if (!ToolStillMatches(player)) { Close(); return; }
        if (targetSlot < 0) return;
        if (!TargetStillMatches(player)) { targetSlot = -1; targetChanged = true; Refresh(); return; }
        var tool = (InscriptionToolItem)player.inventory[toolSlot].ModItem;
        string failure = tool.TransformsArtifact ? DaoArtifactTransactions.RequirementFailure(player, player.inventory[targetSlot])
            : InscriptionTransactions.RequirementFailure(player, tool, player.inventory[targetSlot]);
        if (failure.Length != 0) { refreshTimer = 20; return; }
        if (tool.TransformsArtifact)
            DaoArtifactTransactions.Request(player,toolSlot,toolType,targetSlot,targetType,targetPrefix,previousKind,previousLevel,previousAwakened,previousDaoRoute,previousWorldRoute);
        else InscriptionTransactions.Request(player,toolSlot,toolType,targetSlot,targetType,targetPrefix,previousKind,previousLevel,previousAwakened);
        Close();
    }
    private void NavigateTarget(bool previous)
    {
        Refresh();
        int slot = InscriptionRules.AdjacentTarget(eligibleSlots,targetSlot,previous);
        if (slot < 0) return;
        page = eligibleSlots.IndexOf(slot) / 6; Refresh();
        if (selectTargetActions.TryGetValue(slot,out var choose)) choose();
        scrollbar.ViewPosition = 0;
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
        if (!ToolStillMatches(player) || Main.keyState.IsKeyDown(Keys.Escape)
            || ArtifactKeybindSystem.ForgeCancelKey?.JustPressed == true) { Close(); return; }
        float width = InscriptionRules.PanelExtent(Main.screenWidth,Main.UIScale,540), height = InscriptionRules.PanelExtent(Main.screenHeight,Main.UIScale,500);
        if (panel.Width.Pixels != width || panel.Height.Pixels != height) {
            panel.Width.Set(width,0); panel.Height.Set(height,0); panel.Recalculate(); LayoutContent();
        }
        if (panel.ContainsPoint(Main.MouseScreen / Math.Max(0.01f, Main.UIScale))) player.mouseInterface = true;
        if (targetSlot >= 0 && !TargetStillMatches(player)) { targetSlot = -1; targetChanged = true; Refresh(); }
        bool next = ArtifactKeybindSystem.ForgeNextKey?.JustPressed == true, previous = ArtifactKeybindSystem.ForgePreviousKey?.JustPressed == true;
        if (next && previous) { Close(); return; }
        if (next || previous) { NavigateTarget(previous); return; }
        bool up = ArtifactKeybindSystem.ForgeScrollUpKey?.JustPressed == true, down = ArtifactKeybindSystem.ForgeScrollDownKey?.JustPressed == true;
        if (up || down) { scrollbar.ViewPosition = Math.Max(0,scrollbar.ViewPosition + (down && !up ? 60 : up && !down ? -60 : 0)); return; }
        if (ArtifactKeybindSystem.ForgeConfirmKey?.JustPressed == true) { ConfirmSelection(); return; }
        base.Update(gameTime);
        if (++refreshTimer >= 20) { refreshTimer = 0; Refresh(); }
    }
}
