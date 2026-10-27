using Terraria;
using Terraria.Localization;
using XianXia.Common.Items;

namespace XianXia.Common.Systems;

public static class DaoArtifactPreview
{
    // Reuse the actual weapon description, without crafting a temporary item or changing progress.
    public static string Describe(Item target, DownedBossSystem.EndgameRoute route)
    {
        if (!RefinedArtifact.SupportsDaoTransformation(target) || DaoArtifactRules.Normalize((int)route) == DownedBossSystem.EndgameRoute.None)
            return "";
        string description = Language.GetTextValue(RefinedArtifact.DaoTooltipKey(target), Language.GetTextValue(EndgameRouteTransactions.NameKey(route)));
        int newline = description.IndexOf('\n');
        return Language.GetTextValue("Mods.XianXia.DaoArtifacts.Preview") + "\n" + (newline >= 0 ? description.Substring(newline + 1) : description);
    }
}
