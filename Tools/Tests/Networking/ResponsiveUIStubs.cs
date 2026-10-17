// Measurement-only boundary for the actual responsive button source. No graphics or real font coverage.
namespace Microsoft.Xna.Framework.Graphics { public class SpriteBatch { } }
namespace Terraria.GameContent {
 public class MeasurementFont { public Microsoft.Xna.Framework.Vector2 MeasureString(string text)=>new((text?.Length??0)*7,20); }
 public class MeasurementAsset { public MeasurementFont Value=new(); }
 public static class FontAssets { public static MeasurementAsset MouseText=new(); }
}
namespace Terraria.GameContent.UI.Elements {
 public class MeasurementStyle { public float Pixels,Width; public void Set(float pixels,float percent){Pixels=pixels;} }
 public class UITextPanel<T> {
  public readonly MeasurementStyle Width=new(),MinWidth=new();
  public T Text {get;private set;} public float EffectiveTextScale; public bool IsMouseHovering;
  public UITextPanel(T text,float scale=1){SetText(text,scale,false);Recalculate();}
  public void SetText(T text,float textScale=1,bool large=false){Text=text;EffectiveTextScale=textScale;MinWidth.Set((text?.ToString()?.Length??0)*7*textScale+24,0);}
  public virtual void Recalculate() { }
  public MeasurementStyle GetInnerDimensions()=>new(){Pixels=0,Width=System.Math.Max(0,System.Math.Max(Width.Pixels,MinWidth.Pixels)-24)};
  protected virtual void DrawSelf(Microsoft.Xna.Framework.Graphics.SpriteBatch batch) { }
  public void DrawMeasurementOnly()=>DrawSelf(null);
 }
}
