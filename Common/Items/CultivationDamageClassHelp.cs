using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Items;

public class CultivationDamageClassHelp : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.ModItem is CultivationWeaponItem;
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.DamageType == DamageClass.Magic)
            tooltips.Add(new TooltipLine(Mod,"SpiritMagic",Language.GetTextValue("Mods.XianXia.DamageClasses.SpiritMagic")));
    }
}
