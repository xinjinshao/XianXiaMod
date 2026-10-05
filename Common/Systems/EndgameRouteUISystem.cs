using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.UI;

namespace XianXia.Common.Systems;

public class EndgameRouteUISystem : ModSystem
{
    private UserInterface userInterface;
    private EndgameRouteUIState state;
    public void Open(int slot)
    {
        if (Main.dedServ || userInterface == null || Main.gameMenu) return;
        state.SelectMaterial(slot); userInterface.SetState(state); Main.playerInventory = true;
    }
    public void Close() => userInterface?.SetState(null);
    public override void Load()
    {
        if (Main.dedServ) return;
        state = new EndgameRouteUIState(); state.Activate(); userInterface = new UserInterface();
    }
    public override void Unload() { state = null; userInterface = null; }
    public override void OnWorldUnload() => Close();
    public override void UpdateUI(GameTime gameTime) => userInterface?.Update(gameTime);
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index < 0) return;
        layers.Insert(index, new LegacyGameInterfaceLayer("XianXia: Endgame Route", () => {
            userInterface?.Draw(Main.spriteBatch, new GameTime()); return true;
        }, InterfaceScaleType.UI));
    }
}
