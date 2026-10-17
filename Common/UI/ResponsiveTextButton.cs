using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
namespace XianXia.Common.UI;

public class ResponsiveTextButton : UITextPanel<string>
{
    private string fullText;
    private readonly float textScale;
    private float lastTextWidth = -1;
    public ResponsiveTextButton(string text,float scale = .75f) : base(text,scale) { fullText = text; textScale = scale; }
    public void SetFullText(string text)
    {
        fullText = text; lastTextWidth = -1;
        SetText(text,textScale,false); Recalculate();
    }
    public override void Recalculate()
    {
        MinWidth.Set(0,0); base.Recalculate();
        if (fullText == null) return;
        float width = Math.Max(0,GetInnerDimensions().Width);
        if (Math.Abs(width - lastTextWidth) < .01f) return;
        lastTextWidth = width;
        string shown = fullText;
        while (shown.Length > 0 && FontAssets.MouseText.Value.MeasureString(shown).X * textScale > width) shown = shown[..^1];
        if (shown.Length < fullText.Length) {
            const string suffix = "…";
            while (shown.Length > 0 && FontAssets.MouseText.Value.MeasureString(shown + suffix).X * textScale > width) shown = shown[..^1];
            shown = FontAssets.MouseText.Value.MeasureString(suffix).X * textScale <= width ? shown + suffix : "";
        }
        if (shown != Text) SetText(shown,textScale,false);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        if (IsMouseHovering) Main.hoverItemName = fullText;
    }
}
