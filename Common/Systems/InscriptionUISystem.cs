using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.UI;

namespace XianXia.Common.Systems;

public class InscriptionUISystem : ModSystem
{
    private UserInterface userInterface;
    private InscriptionUIState state;
    public void Open(int toolSlot)
    {
        if (Main.dedServ || userInterface == null || Main.gameMenu || !Main.LocalPlayer.active
            || Main.LocalPlayer.dead || Main.LocalPlayer.noItems || Main.LocalPlayer.CCed
            || toolSlot < 0 || toolSlot >= System.Math.Min(58, Main.LocalPlayer.inventory.Length)
            || toolSlot != Main.LocalPlayer.selectedItem
            || Main.LocalPlayer.inventory[toolSlot].ModItem is not global::XianXia.Common.Items.InscriptionToolItem) return;
        state.SelectTool(toolSlot);
        userInterface.SetState(state);
        Main.playerInventory = true;
    }
    public void Close() => userInterface?.SetState(null);
    public override void Load()
    {
        if (Main.dedServ) return;
        state = new InscriptionUIState(); state.Activate(); userInterface = new UserInterface();
    }
    public override void Unload() { state = null; userInterface = null; }
    public override void OnWorldUnload() => Close();
    public override void UpdateUI(GameTime gameTime) => userInterface?.Update(gameTime);
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index < 0) return;
        layers.Insert(index, new LegacyGameInterfaceLayer("XianXia: Inscription", () => {
            userInterface?.Draw(Main.spriteBatch, new GameTime()); return true;
        }, InterfaceScaleType.UI));
    }
}
