using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

public class BossRewardSourceHelp : GlobalItem
{
    public override bool AppliesToEntity(Item entity,bool lateInstantiation) => entity.ModItem?.Mod?.Name == "XianXia";
    public override void ModifyTooltips(Item item,List<TooltipLine> tooltips)
    {
        var catalog = ModContent.GetInstance<BossRewardSources>(); var sources = catalog.ForItem(item.type);
        if (sources.Count == 0) return;
        tooltips.Add(new TooltipLine(Mod,"BossSourceScope",Language.GetTextValue("Mods.XianXia.BossSources.Scope")));
        bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
        int count = expanded ? sources.Count : Math.Min(4,sources.Count);
        for (int i = 0; i < count; i++) tooltips.Add(new TooltipLine(Mod,"BossSource" + i,catalog.Description(sources[i])));
        if (count < sources.Count) tooltips.Add(new TooltipLine(Mod,"BossSourceMore",Language.GetTextValue("Mods.XianXia.BossSources.More",sources.Count - count)));
        tooltips.Add(new TooltipLine(Mod,"BossSourceCaution",Language.GetTextValue("Mods.XianXia.BossSources.Caution")));
    }
}
