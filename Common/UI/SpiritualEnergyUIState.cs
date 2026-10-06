using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using XianXia.Common.Systems;
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

    private bool dragging;
    private Vector2 dragOffset, configuredPosition;
    private Vector2? sessionPosition;

    public void ResetPosition() { dragging = false; sessionPosition = null; }

    private Vector2 Position()
    {
        var config = ModContent.GetInstance<XianXiaClientConfig>();
        Vector2 configured = new(config.EnergyBarX, config.EnergyBarY);
        if (configured != configuredPosition) { configuredPosition = configured; ResetPosition(); }
        Vector2 desired = sessionPosition ?? configured;
        float scale = Math.Max(0.01f, Main.UIScale);
        return new Vector2(
            MathHelper.Clamp(desired.X, 0f, Math.Max(0f, Main.screenWidth / scale - frameTexture.Value.Width)),
            MathHelper.Clamp(desired.Y, 0f, Math.Max(0f, Main.screenHeight / scale - frameTexture.Value.Height)));
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Main.gameMenu || Main.dedServ || !Main.LocalPlayer.active || Main.LocalPlayer.dead
            || !Main.LocalPlayer.GetModPlayer<XianXiaPlayer>().discoveredSpiritualEnergy) { dragging = false; return; }
        Vector2 position = Position();
        Vector2 mouse = Main.MouseScreen / Math.Max(0.01f, Main.UIScale);
        bool shift = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
        if (!dragging && shift && Main.mouseLeft && Main.mouseLeftRelease
            && new Rectangle((int)position.X, (int)position.Y, frameTexture.Value.Width, frameTexture.Value.Height).Contains(mouse.ToPoint()))
        { dragging = true; dragOffset = mouse - position; }
        if (dragging)
        {
            Main.LocalPlayer.mouseInterface = true;
            if (!Main.mouseLeft || !shift) dragging = false;
            else sessionPosition = mouse - dragOffset;
        }
    }

    public override void OnInitialize()
    {
        frameTexture = ModContent.Request<Texture2D>("XianXia/Common/UI/SpiritualEnergyBarFrame");
        fillTexture = ModContent.Request<Texture2D>("XianXia/Common/UI/SpiritualEnergyBarFill");
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (Main.gameMenu || Main.dedServ) return;
        Player player = Main.LocalPlayer;
        if (!player.active || player.dead) return;
        XianXiaPlayer modPlayer = player.GetModPlayer<XianXiaPlayer>();
        if (!modPlayer.discoveredSpiritualEnergy)
        {
            return;
        }

        Texture2D frame = frameTexture.Value;
        Texture2D fill = fillTexture.Value;
        Vector2 position = Position();
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
        Vector2 mouse = Main.MouseScreen / Main.UIScale;
        if (new Rectangle((int)position.X, (int)position.Y, frame.Width, frame.Height).Contains(mouse.ToPoint()))
        {
            player.mouseInterface = true;
            Main.instance.MouseText(CultivationStatusText.Summary(modPlayer) + "\n" + Language.GetTextValue("Mods.XianXia.CultivationStatus.DragHint"));
        }
    }
}
