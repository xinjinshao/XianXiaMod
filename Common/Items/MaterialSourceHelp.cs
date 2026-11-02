using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

public class MaterialSourceHelp : GlobalItem
{
    public override bool AppliesToEntity(Item entity,bool lateInstantiation) => entity.ModItem?.Mod?.Name == "XianXia";
    public override void ModifyTooltips(Item item,List<TooltipLine> tooltips)
    {
        var catalog = ModContent.GetInstance<MaterialDropSources>(); var sources = catalog.ForItem(item.type);
        if (sources.Count == 0) return;
        tooltips.Add(new TooltipLine(Mod,"EnemySourceScope",Language.GetTextValue("Mods.XianXia.MaterialSources.Scope")));
        foreach (var source in sources) tooltips.Add(new TooltipLine(Mod,"EnemySource" + source.NpcType,catalog.Description(source)));
    }
}
