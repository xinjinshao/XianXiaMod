using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using XianXia.Common.Players;

namespace XianXia.Common.UI;

public class SpiritualEnergyUIState : UIState
{
    private Asset<Texture2D> frameTexture = null!;
    private Asset<Texture2D> fillTexture = null!;

    public override void OnInitialize()
    {
        frameTexture = ModContent.Request<Texture2D>("XianXia/Common/UI/SpiritualEnergyBarFrame");
        fillTexture = ModContent.Request<Texture2D>("XianXia/Common/UI/SpiritualEnergyBarFill");
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Player player = Main.LocalPlayer;
        XianXiaPlayer modPlayer = player.GetModPlayer<XianXiaPlayer>();
        if (!modPlayer.discoveredSpiritualEnergy)
        {
            return;
        }

        Texture2D frame = frameTexture.Value;
        Texture2D fill = fillTexture.Value;
        Vector2 position = new(28f, 84f);
        float ratio = modPlayer.maxSpiritualEnergy <= 0 ? 0f : modPlayer.spiritualEnergy / (float)modPlayer.maxSpiritualEnergy;
        ratio = MathHelper.Clamp(ratio, 0f, 1f);

        Rectangle source = new(0, 0, (int)(fill.Width * ratio), fill.Height);
        spriteBatch.Draw(fill, position + new Vector2(2f, 2f), source, Color.White);
        spriteBatch.Draw(frame, position, Color.White);

        string text = $"{modPlayer.spiritualEnergy}/{modPlayer.maxSpiritualEnergy}";
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.ItemStack.Value,
            text,
            position.X + 52f,
            position.Y + 18f,
            new Color(115, 255, 230),
            Color.Black,
            Vector2.Zero,
            0.8f
        );
    }
}
