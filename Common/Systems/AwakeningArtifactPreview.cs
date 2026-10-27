using Terraria;
using Terraria.Localization;
using XianXia.Common.Items;

namespace XianXia.Common.Systems;

public static class AwakeningArtifactPreview
{
    public static string Describe(Item target)
    {
        if (!RefinedArtifact.SupportsAwakening(target)) return "";
        return Language.GetTextValue("Mods.XianXia.Refinement.Preview",
            Language.GetTextValue(RefinedArtifact.AwakeningTooltipKey(target, true)));
    }
}
