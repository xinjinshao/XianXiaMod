using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.Systems;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.UI;

public class EndgameRouteUIState : UIState
{
    private UIPanel panel;
    private UIText summary, title;
    private UIElement content;
    private ResponsiveTextButton cancel;
    private float lastLayoutWidth = -1;
    private bool worldLocked;
    private UIList description;
    private UIScrollbar scrollbar;
    private readonly System.Collections.Generic.Dictionary<DownedBossSystem.EndgameRoute,ResponsiveTextButton> routeButtons = new();
    private ResponsiveTextButton confirm;
    private int slot;
    private Item selectedItem;
    private DownedBossSystem.EndgameRoute selected;
    private static string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Routes." + key, args);
    public void SelectMaterial(int inventorySlot)
    {
        slot = inventorySlot; selectedItem = Main.LocalPlayer.inventory[slot]; selected = DownedBossSystem.EndgameRoute.None; worldLocked = false;
        summary.SetText(Text("Warning") + "\n\n" + Text("NavigationHint")); confirm.SetFullText(Text("ChooseFirst"));
        foreach (var button in routeButtons.Values) button.BorderColor = Color.Black;
        LayoutContent(); scrollbar.ViewPosition = 0;
    }
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.HAlign = panel.VAlign = 0.5f;
        panel.Width.Set(680,0); panel.Height.Set(410,0); Append(panel);
        panel.SetPadding(12);
        description = new UIList(); description.Width.Set(-24,1); description.Height.Set(0,1); panel.Append(description);
        scrollbar = new UIScrollbar(); scrollbar.Left.Set(-20,1); scrollbar.Width.Set(20,0); scrollbar.Height.Set(0,1); panel.Append(scrollbar); description.SetScrollbar(scrollbar);
        content = new UIElement(); content.Width.Set(0,1); content.Height.Set(386,0); description.Add(content);
        title = new UIText(Text("Title")) { IsWrapped = true }; title.Width.Set(0,1); content.Append(title);
        summary = new UIText(Text("Warning"),.85f) { IsWrapped = true }; summary.Top.Set(36,0); summary.Width.Set(0,1); content.Append(summary);
        for (int i = 1; i <= 3; i++) {
            var route = (DownedBossSystem.EndgameRoute)i;
            var button = new ResponsiveTextButton(Text(route.ToString()),.85f);
            button.Width.Set(0,1); button.Height.Set(38,0);
            button.OnLeftClick += (_,_) => SelectRoute(route);
            routeButtons[route] = button;
            content.Append(button);
        }
        confirm = new ResponsiveTextButton(Text("ChooseFirst"),.8f); confirm.Width.Set(0,1); confirm.Height.Set(40,0);
        confirm.OnLeftClick += (_,_) => ConfirmSelection(); content.Append(confirm);
        cancel = new ResponsiveTextButton(Text("Cancel"),.85f); cancel.Width.Set(0,1); cancel.Height.Set(40,0);
        cancel.OnLeftClick += (_,_) => ModContent.GetInstance<EndgameRouteUISystem>().Close(); content.Append(cancel);
    }
    private void SelectRoute(DownedBossSystem.EndgameRoute route)
    {
        if (!ValidSelection() || DownedBossSystem.ChosenRoute != DownedBossSystem.EndgameRoute.None || DaoArtifactRules.Normalize((int)route) == DownedBossSystem.EndgameRoute.None) return;
        selected = route; summary.SetText(Text("Warning") + "\n\n" + Text(route + "Description") + "\n\n" + Text("NavigationHint"));
        confirm.SetFullText(Text("Confirm",Text(route.ToString())));
        foreach (var entry in routeButtons) entry.Value.BorderColor = entry.Key == route ? Color.Gold : Color.Black;
        worldLocked = false; LayoutContent(); scrollbar.ViewPosition = 0;
    }
    private void LayoutContent()
    {
        title.Recalculate(); summary.Top.Set(Math.Max(36,title.GetOuterDimensions().Height + 8),0); summary.Recalculate();
        float top = summary.Top.Pixels + summary.GetOuterDimensions().Height + 12;
        for (int i = 1; i <= 3; i++) { routeButtons[(DownedBossSystem.EndgameRoute)i].Top.Set(top,0); top += 44; }
        confirm.Top.Set(top + 8,0); cancel.Top.Set(top + 54,0); content.Height.Set(top + 98,0);
        description.Recalculate(); lastLayoutWidth = content.GetInnerDimensions().Width;
    }
    private void ConfirmSelection()
    {
        if (DaoArtifactRules.Normalize((int)selected) == DownedBossSystem.EndgameRoute.None || !ValidSelection() || DownedBossSystem.ChosenRoute != DownedBossSystem.EndgameRoute.None) return;
        EndgameRouteTransactions.Request(Main.LocalPlayer,slot,selected);
        ModContent.GetInstance<EndgameRouteUISystem>().Close();
    }
    private bool ValidSelection()
    {
        Player player = Main.LocalPlayer;
        return !Main.gameMenu && !Main.blockInput && !Main.drawingPlayerChat && player.active && !player.dead && !player.noItems && !player.CCed && Main.playerInventory && Main.mouseItem.IsAir
            && slot >= 0 && slot < Math.Min(58,player.inventory.Length) && slot == player.selectedItem
            && ReferenceEquals(player.inventory[slot],selectedItem)
            && selectedItem != null && selectedItem.type == ModContent.ItemType<RouteMaterial>() && selectedItem.stack > 0;
    }
    public override void Update(GameTime gameTime)
    {
        if (!ValidSelection() || Main.keyState.IsKeyDown(Keys.Escape) || ArtifactKeybindSystem.RouteCancelKey?.JustPressed == true) { ModContent.GetInstance<EndgameRouteUISystem>().Close(); return; }
        float scale = Math.Max(0.01f, Main.UIScale);
        float width = CultivationPanelLayoutRules.FitExtent(Main.screenWidth,Main.UIScale,680);
        float height = CultivationPanelLayoutRules.FitExtent(Main.screenHeight,Main.UIScale,410);
        if (panel.Width.Pixels != width || panel.Height.Pixels != height)
        {
            panel.Width.Set(width,0); panel.Height.Set(height,0); panel.Recalculate();
        }
        if (Math.Abs(content.GetInnerDimensions().Width - lastLayoutWidth) > .01f) LayoutContent();
        if (panel.ContainsPoint(Main.MouseScreen / scale)) Main.LocalPlayer.mouseInterface = true;
        if (DownedBossSystem.ChosenRoute != DownedBossSystem.EndgameRoute.None) {
            if (!worldLocked) {
                summary.SetText(Text("AlreadyChosen") + "\n" + Text(DownedBossSystem.ChosenRoute.ToString()));
                selected = DownedBossSystem.EndgameRoute.None; confirm.SetFullText(Text("AlreadyChosen"));
                foreach (var button in routeButtons.Values) button.BorderColor = Color.Black;
                worldLocked = true; LayoutContent();
            }
            base.Update(gameTime); return;
        }
        bool next = ArtifactKeybindSystem.RouteNextKey?.JustPressed == true, previous = ArtifactKeybindSystem.RoutePreviousKey?.JustPressed == true;
        if (next && previous) { ModContent.GetInstance<EndgameRouteUISystem>().Close(); return; }
        if (next || previous) { SelectRoute(EndgameRouteTransactions.AdjacentPreview(selected,previous)); return; }
        bool up = ArtifactKeybindSystem.RouteScrollUpKey?.JustPressed == true, down = ArtifactKeybindSystem.RouteScrollDownKey?.JustPressed == true;
        if (up || down) { scrollbar.ViewPosition = Math.Max(0,scrollbar.ViewPosition + (down && !up ? 60 : up && !down ? -60 : 0)); return; }
        if (ArtifactKeybindSystem.RouteConfirmKey?.JustPressed == true) { ConfirmSelection(); return; }
        base.Update(gameTime);
    }
}
