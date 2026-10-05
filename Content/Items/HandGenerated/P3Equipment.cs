using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.HandGenerated;

// ---- Inscription Needles ----
public class GreenwoodInscriptionNeedle : global::XianXia.Common.Items.InscriptionToolItem
{
    public override global::XianXia.Common.Systems.InscriptionKind TargetKind => global::XianXia.Common.Systems.InscriptionKind.Greenwood;
    public override void SetDefaults() => Configure(Item.buyPrice(silver: 50), ItemRarityID.Green);
}
public class FurnaceInscriptionNeedle : global::XianXia.Common.Items.InscriptionToolItem
{
    public override global::XianXia.Common.Systems.InscriptionKind TargetKind => global::XianXia.Common.Systems.InscriptionKind.Furnace;
    public override void SetDefaults() => Configure(Item.buyPrice(silver: 50), ItemRarityID.Orange);
}
public class ThunderInscriptionNeedle : global::XianXia.Common.Items.InscriptionToolItem
{
    public override global::XianXia.Common.Systems.InscriptionKind TargetKind => global::XianXia.Common.Systems.InscriptionKind.Thunder;
    public override void SetDefaults() => Configure(Item.buyPrice(gold: 4), ItemRarityID.LightRed);
}
public class StarAbyssInscriptionNeedle : global::XianXia.Common.Items.InscriptionToolItem
{
    public override global::XianXia.Common.Systems.InscriptionKind TargetKind => global::XianXia.Common.Systems.InscriptionKind.StarAbyss;
    public override void SetDefaults() => Configure(Item.buyPrice(gold: 4), ItemRarityID.Pink);
}
public class BrokenHeavenInscriptionNeedle : global::XianXia.Common.Items.InscriptionToolItem
{
    public override global::XianXia.Common.Systems.InscriptionKind TargetKind => global::XianXia.Common.Systems.InscriptionKind.BrokenHeaven;
    public override void SetDefaults() => Configure(Item.buyPrice(gold: 10), ItemRarityID.Yellow);
}
public class InscriptionRemovalStone : global::XianXia.Common.Items.InscriptionToolItem
{
    public override global::XianXia.Common.Systems.InscriptionKind TargetKind => global::XianXia.Common.Systems.InscriptionKind.None;
    public override void SetDefaults() => Configure(Item.buyPrice(gold: 1), ItemRarityID.Green);
}

// ---- Summon / Minion Equipment ----
public class FurnaceAshSpiritContract : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 2); Item.rare = ItemRarityID.Orange; Item.accessory = true; } public override void UpdateAccessory(Player player, bool hideVisual) { player.GetDamage(DamageClass.Summon) += 0.08f; } }
public class StarAbyssLarvaContract : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 3); Item.rare = ItemRarityID.LightRed; Item.accessory = true; } public override void UpdateAccessory(Player player, bool hideVisual) { player.GetDamage(DamageClass.Summon) += 0.10f; player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1.05f; } }
public class NascentSoulCloneTalisman : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Lime; Item.accessory = true; } public override void UpdateAccessory(Player player, bool hideVisual) { player.maxMinions += 1; player.GetDamage(DamageClass.Summon) += 0.12f; } }
public class CelestialPuppetToken : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 8); Item.rare = ItemRarityID.Yellow; Item.accessory = true; } public override void UpdateAccessory(Player player, bool hideVisual) { player.maxMinions += 1; player.GetDamage(DamageClass.Summon) += 0.14f; } }
public class ArchivedImmortalSoulContract : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 12); Item.rare = ItemRarityID.Red; Item.accessory = true; } public override void UpdateAccessory(Player player, bool hideVisual) { player.maxMinions += 2; player.GetDamage(DamageClass.Summon) += 0.18f; } }

// ---- Utility Items ----
public class LightningAvoidanceRune : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 30; Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.useTime = 20; Item.useAnimation = 20; Item.UseSound = SoundID.Item4; Item.consumable = true; Item.value = Item.buyPrice(silver: 30); Item.rare = ItemRarityID.White; } public override bool? UseItem(Player player) { player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), 60 * 30); return true; } }
public class BlankSectScroll : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 999; Item.value = Item.buyPrice(silver: 20); Item.rare = ItemRarityID.White; } }
public class SpiritHerbSeeds : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<global::XianXia.Content.Tiles.CultivatedSpiritHerbTile>());
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 5);
        Item.rare = ItemRarityID.White;
    }
}
public class HeavenDaoRouteHint : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Yellow; } }
public class EndgameRouteFrame : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 15); Item.rare = ItemRarityID.Red; } }
