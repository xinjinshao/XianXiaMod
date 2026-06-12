// TODO: ART_PLACEHOLDER - all items in this file use placeholder sprites. See Docs/ART_TODO.md
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Generated;

// ---- Inscription Needles ----
public class GreenwoodInscriptionNeedle : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(silver: 50); Item.rare = ItemRarityID.Green;
    }
}

public class FurnaceInscriptionNeedle : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(silver: 50); Item.rare = ItemRarityID.Orange;
    }
}

public class ThunderInscriptionNeedle : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(gold: 4); Item.rare = ItemRarityID.LightRed;
    }
}

public class StarAbyssInscriptionNeedle : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(gold: 4); Item.rare = ItemRarityID.Pink;
    }
}

public class BrokenHeavenInscriptionNeedle : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(gold: 10); Item.rare = ItemRarityID.Yellow;
    }
}

public class InscriptionRemovalStone : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(gold: 1); Item.rare = ItemRarityID.Green;
    }
}

// ---- Summon / Minion Equipment ----
public class SmallArtifactPendant : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 1); Item.rare = ItemRarityID.Blue;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 1;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<LowGradeSpiritStone>(8).AddIngredient(ItemID.FallenStar, 3)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>()).Register();
    }
}

public class FurnaceAshSpiritContract : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 2); Item.rare = ItemRarityID.Orange;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage(DamageClass.Summon) += 0.08f;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<FurnaceSlagIron>(10).AddIngredient<ArtifactBlankShard>(5)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>()).Register();
    }
}

public class StarAbyssLarvaContract : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 3); Item.rare = ItemRarityID.LightRed;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage(DamageClass.Summon) += 0.10f;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1.05f;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<StarEclipseCrystal>(8).AddIngredient<StarAbyssMembrane>(4)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.StarPatternCauldronTile>()).Register();
    }
}

public class NascentSoulCloneTalisman : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Lime;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.maxMinions += 1;
        player.GetDamage(DamageClass.Summon) += 0.12f;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<SectTrialToken>(8).AddIngredient<OldHeavenDaoScroll>(1)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.SectTrialAltarTile>()).Register();
    }
}

public class CelestialPuppetToken : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 8); Item.rare = ItemRarityID.Yellow;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.maxMinions += 1;
        player.GetDamage(DamageClass.Summon) += 0.14f;
    }
}

public class ArchivedImmortalSoulContract : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 12); Item.rare = ItemRarityID.Red;
        Item.accessory = true;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.maxMinions += 2;
        player.GetDamage(DamageClass.Summon) += 0.18f;
    }
}

// ---- Smoke / Utility Items ----
public class LightningAvoidanceRune : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 30;
        Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.useTime = 20; Item.useAnimation = 20;
        Item.UseSound = SoundID.Item4; Item.consumable = true;
        Item.value = Item.buyPrice(silver: 30); Item.rare = ItemRarityID.White;
    }
    public override bool? UseItem(Player player)
    {
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), 60 * 30);
        return true;
    }
}

public class TribulationTrainingToken : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.HoldUp; Item.useTime = 30; Item.useAnimation = 30;
        Item.UseSound = SoundID.Item4;
        Item.value = Item.buyPrice(gold: 1); Item.rare = ItemRarityID.Green;
    }
    public override bool? UseItem(Player player)
    {
        XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
        cultivation.spiritPressure = System.Math.Min(100, cultivation.spiritPressure + 10);
        cultivation.spiritualEnergy = System.Math.Min(cultivation.maxSpiritualEnergy, cultivation.spiritualEnergy + 10);
        if (Main.myPlayer == player.whoAmI)
            Main.NewText("Tribulation training initiated. Spirit pressure increased to test your resistance.", 160, 210, 255);
        return true;
    }
}

public class BlankSectScroll : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 20); Item.rare = ItemRarityID.White;
    }
}

public class SpiritHerbSeeds : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 5); Item.rare = ItemRarityID.White;
    }
}

public class HeavenDaoRouteHint : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Yellow;
    }
}

public class EndgameRouteFrame : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32; Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 15); Item.rare = ItemRarityID.Red;
    }
}
