using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Items;

public class CultivationDamageClassHelp : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.ModItem is CultivationWeaponItem;
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.DamageType == DamageClass.Magic)
            tooltips.Add(new TooltipLine(Mod,"SpiritMagic",Language.GetTextValue("Mods.XianXia.DamageClasses.SpiritMagic")));
        if (item.ModItem is not CultivationWeaponItem weapon || Main.player == null
            || Main.myPlayer < 0 || Main.myPlayer >= Main.maxPlayers || Main.myPlayer >= Main.player.Length) return;
        Player player = Main.player[Main.myPlayer];
        if (player == null || !player.active || player.dead || !player.TryGetModPlayer<XianXiaPlayer>(out var state)) return;
        int cost = state.GetSpiritualEnergyCost(weapon.GetSpiritCost(player));
        tooltips.Add(new TooltipLine(Mod, "CurrentSpiritCost", Language.GetTextValue(
            "Mods.XianXia.DamageClasses." + (weapon.DeploysArray ? "CurrentArrayCost" : "CurrentCastCost"), cost)));
    }
}
