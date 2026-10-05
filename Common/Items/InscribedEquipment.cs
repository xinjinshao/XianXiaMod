using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

public class InscribedEquipment : GlobalItem
{
    public override bool InstancePerEntity => true;
    public InscriptionKind Kind { get; private set; }
    public static bool IsEligible(Item item) => item.ModItem?.Mod is global::XianXia.XianXia
        && !item.IsAir && item.maxStack == 1 && !item.consumable && !item.vanity && item.ammo == 0
        && (item.accessory || item.damage > 0);
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => IsEligible(entity);
    public static InscriptionKind GetKind(Item item) => IsEligible(item) ? item.GetGlobalItem<InscribedEquipment>().Kind : InscriptionKind.None;
    public void SetKind(InscriptionKind kind) => Kind = InscriptionRules.Normalize((int)kind);
    public override void SaveData(Item item, TagCompound tag)
    {
        if (Kind == InscriptionKind.None) return;
        tag["inscriptionVersion"] = 1;
        tag["inscription"] = (int)Kind;
    }
    public override void LoadData(Item item, TagCompound tag) => Kind = InscriptionRules.Normalize(tag.GetInt("inscription"));
    public override void NetSend(Item item, BinaryWriter writer) => writer.Write((byte)Kind);
    public override void NetReceive(Item item, BinaryReader reader) => Kind = InscriptionRules.Normalize(reader.ReadByte());
    public override void UpdateAccessory(Item item, Player player, bool hideVisual) =>
        player.GetModPlayer<InscriptionPlayer>().RegisterAccessory(Kind);
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (!item.accessory) damage += InscriptionRules.WeaponDamageBonus(Kind);
    }
    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        if (!item.accessory) crit += InscriptionRules.WeaponCritBonus(Kind);
    }
    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        if (!item.accessory) knockback += InscriptionRules.WeaponKnockbackBonus(Kind);
    }
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (Kind == InscriptionKind.None) return;
        tooltips.Add(new TooltipLine(Mod, "Inscription", Language.GetTextValue("Mods.XianXia.Inscriptions.Applied",
            Language.GetTextValue("Mods.XianXia.Inscriptions." + Kind))) { OverrideColor = new Color(120, 230, 210) });
        string effect = "Mods.XianXia.Inscriptions." + Kind + (item.accessory ? "Accessory" : "Weapon");
        tooltips.Add(new TooltipLine(Mod, "InscriptionEffect", Language.GetTextValue(effect)));
        tooltips.Add(new TooltipLine(Mod, "InscriptionStacking", Language.GetTextValue("Mods.XianXia.Inscriptions.Stacking")));
    }
}
