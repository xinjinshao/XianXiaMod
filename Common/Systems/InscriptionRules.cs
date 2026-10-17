namespace XianXia.Common.Systems;

public enum InscriptionKind : byte { None, Greenwood, Furnace, Thunder, StarAbyss, BrokenHeaven }

public static class InscriptionRules
{
    public const int SpiritStoneCost = 3;
    public static float PanelExtent(int pixels, float scale, int maximum)
    {
        float safeScale = float.IsFinite(scale) && scale > 0 ? scale : 1f;
        float viewport = System.Math.Max(1,pixels / safeScale);
        return System.Math.Min(maximum,System.Math.Max(1,viewport - 20));
    }
    public static int AdjacentTarget(System.Collections.Generic.IReadOnlyList<int> slots, int current, bool previous)
    {
        if (slots == null || slots.Count == 0) return -1;
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] == current) return slots[(i + (previous ? slots.Count - 1 : 1)) % slots.Count];
        return slots[previous ? slots.Count - 1 : 0];
    }
    public static InscriptionKind Normalize(int value) => value >= 0 && value <= 5 ? (InscriptionKind)value : InscriptionKind.None;
    public static bool CanChange(InscriptionKind current, InscriptionKind next) =>
        Normalize((int)current) == current && Normalize((int)next) == next && current != next;
    public static float WeaponDamageBonus(InscriptionKind kind) => kind switch
    { InscriptionKind.Furnace => 0.08f, InscriptionKind.StarAbyss => 0.15f, _ => 0f };
    public static int WeaponCritBonus(InscriptionKind kind) => kind == InscriptionKind.BrokenHeaven ? 8 : 0;
    public static float WeaponKnockbackBonus(InscriptionKind kind) => kind switch
    { InscriptionKind.Furnace => 0.2f, InscriptionKind.BrokenHeaven => 0.3f, _ => 0f };
}
