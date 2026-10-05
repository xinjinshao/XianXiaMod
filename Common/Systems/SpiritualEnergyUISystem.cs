using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.UI;

namespace XianXia.Common.Systems;

#nullable enable

public class SpiritualEnergyUISystem : ModSystem
{
    private UserInterface? userInterface;
    private SpiritualEnergyUIState? state;
    private UserInterface? skillInterface;

    public override void Load()
    {
        if (Main.dedServ)
        {
            return;
        }

        state = new SpiritualEnergyUIState();
        state.Activate();
        userInterface = new UserInterface();
        userInterface.SetState(state);
        var skills = new ArtifactSkillUIState(); skills.Activate(); skillInterface = new UserInterface(); skillInterface.SetState(skills);
    }

    public override void Unload()
    {
        state = null;
        userInterface = null;
        skillInterface = null;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        userInterface?.Update(gameTime);
        if (!Main.gameMenu && Main.LocalPlayer.GetModPlayer<Common.Players.XianXiaPlayer>().discoveredSpiritualEnergy) skillInterface?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (index == -1)
        {
            return;
        }

        layers.Insert(index + 1, new LegacyGameInterfaceLayer(
            "XianXia: Spiritual Energy",
            delegate
            {
                userInterface?.Draw(Main.spriteBatch, new GameTime());
                if (!Main.gameMenu && Main.LocalPlayer.GetModPlayer<Common.Players.XianXiaPlayer>().discoveredSpiritualEnergy) skillInterface?.Draw(Main.spriteBatch, new GameTime());
                return true;
            },
            InterfaceScaleType.UI)
        );
    }
}
