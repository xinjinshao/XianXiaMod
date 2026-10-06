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
    private UIText summary;
    private UIList description;
    private UITextPanel<string> confirm;
    private int slot;
    private Item selectedItem;
    private DownedBossSystem.EndgameRoute selected;
    private static string Text(string key, params object[] args) => Language.GetTextValue("Mods.XianXia.Routes." + key, args);
    public void SelectMaterial(int inventorySlot)
    {
        slot = inventorySlot; selectedItem = Main.LocalPlayer.inventory[slot]; selected = DownedBossSystem.EndgameRoute.None;
        summary.SetText(Text("Warning")); confirm.SetText(Text("ChooseFirst"));
    }
    public override void OnInitialize()
    {
        panel = new UIPanel(); panel.HAlign = panel.VAlign = 0.5f;
        panel.Width.Set(680,0); panel.Height.Set(410,0); Append(panel);
        var title = new UIText(Text("Title")); panel.Append(title);
        summary = new UIText(Text("Warning"),0.85f) { IsWrapped = true };
        description = new UIList(); description.Top.Set(46,0); description.Width.Set(-28,1); description.Height.Set(-190,1); panel.Append(description);
        var scrollbar = new UIScrollbar(); scrollbar.Top.Set(46,0); scrollbar.Left.Set(-20,1); scrollbar.Width.Set(20,0); scrollbar.Height.Set(-190,1);
        panel.Append(scrollbar); description.SetScrollbar(scrollbar);
        summary.Width.Set(0,1); description.Add(summary);
        for (int i = 1; i <= 3; i++) {
            var route = (DownedBossSystem.EndgameRoute)i;
            var button = new UITextPanel<string>(Text(route.ToString()),0.85f);
            button.Top.Set(-136,1); button.Left.Set(0,(i-1)/3f); button.Width.Set(-8,1/3f); button.Height.Set(38,0);
            button.OnLeftClick += (_,_) => {
                selected = route; summary.SetText(Text("Warning") + "\n\n" + Text(route + "Description"));
                confirm.SetText(Text("Confirm", Text(route.ToString())));
            };
            panel.Append(button);
        }
        confirm = new UITextPanel<string>(Text("ChooseFirst"),0.8f); confirm.Top.Set(-84,1); confirm.Width.Set(-158,1); confirm.Height.Set(40,0);
        confirm.OnLeftClick += (_,_) => {
            if (selected == DownedBossSystem.EndgameRoute.None || !ValidSelection()) return;
            EndgameRouteTransactions.Request(Main.LocalPlayer,slot,selected);
            ModContent.GetInstance<EndgameRouteUISystem>().Close();
        }; panel.Append(confirm);
        var cancel = new UITextPanel<string>(Text("Cancel"),0.85f); cancel.Top.Set(-84,1); cancel.Left.Set(-138,1); cancel.Width.Set(138,0); cancel.Height.Set(40,0);
        cancel.OnLeftClick += (_,_) => ModContent.GetInstance<EndgameRouteUISystem>().Close(); panel.Append(cancel);
    }
    private bool ValidSelection()
    {
        Player player = Main.LocalPlayer;
        return player.active && !player.dead && Main.playerInventory && Main.mouseItem.IsAir
            && slot >= 0 && slot < 58 && slot == player.selectedItem
            && ReferenceEquals(player.inventory[slot],selectedItem)
            && selectedItem.type == ModContent.ItemType<RouteMaterial>() && selectedItem.stack > 0;
    }
    public override void Update(GameTime gameTime)
    {
        if (!ValidSelection() || Main.gameMenu || Main.keyState.IsKeyDown(Keys.Escape)) { ModContent.GetInstance<EndgameRouteUISystem>().Close(); return; }
        float scale = Math.Max(0.01f, Main.UIScale);
        float width = Math.Max(300f, Math.Min(680f, Main.screenWidth / scale - 20f));
        float height = Math.Max(260f, Math.Min(410f, Main.screenHeight / scale - 20f));
        if (panel.Width.Pixels != width || panel.Height.Pixels != height)
        {
            panel.Width.Set(width,0); panel.Height.Set(height,0); panel.Recalculate();
        }
        base.Update(gameTime);
        if (panel.ContainsPoint(Main.MouseScreen / scale)) Main.LocalPlayer.mouseInterface = true;
        if (DownedBossSystem.ChosenRoute != DownedBossSystem.EndgameRoute.None) {
            summary.SetText(Text("AlreadyChosen") + "\n" + Text(DownedBossSystem.ChosenRoute.ToString()));
            selected = DownedBossSystem.EndgameRoute.None; confirm.SetText(Text("AlreadyChosen"));
        }
    }
}
