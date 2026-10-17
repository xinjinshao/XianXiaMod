namespace XianXia.Common.Systems;

public static class CultivationPanelLayoutRules
{
    public static float FitExtent(int pixels,float scale,int maximum)
    {
        float safeScale = float.IsFinite(scale) && scale > 0 ? scale : 1f;
        float viewport = System.Math.Max(1,pixels / safeScale);
        return System.Math.Min(maximum,System.Math.Max(1,viewport - 20));
    }
}
