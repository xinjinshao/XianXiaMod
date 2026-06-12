using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.Generated;

public class GreenwoodRoot : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

    }

}


public class FurnaceSlagIron : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

    }

}


public class ArtifactBlankShard : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

    }

}


public class TribulationCloudDew : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

    }

}


public class StarEclipseCrystal : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.GoldenCore);
    }

    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        if (cultivation.TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.GoldenCore)
            && player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()))
            cultivation.ReduceSpiritPressure(10);
        return true;
    }

}


public class SectTrialToken : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

    }

}


public class HeavenDaoFragment : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.SpiritSevering);
    }

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.SpiritSevering);
        return true;
    }

}


public class Moonbone : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.Tribulation);
    }

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.Tribulation);
        return true;
    }

}


public class DaoSeveringDust : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.DaoSevering);
    }

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.DaoSevering);
        return true;
    }

}


public class SpringReturnPill : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpringReturnBuff>(), 60 * 60);
        cultivation.ReduceSpiritPressure(player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()) ? 8 : 4);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe(3)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(2)
            .AddIngredient(ItemID.BottledWater)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }

}


public class QiCondensingPill : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.QiCondensation);
    }

    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        if (cultivation.TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.QiCondensation)
            && player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()))
            cultivation.ReduceSpiritPressure(6);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(3)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(5)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }

}


public class FoundationPill : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.Foundation);
    }

    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        if (cultivation.TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.Foundation)
            && player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()))
            cultivation.ReduceSpiritPressure(8);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(4)
            .AddIngredient<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(4)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(10)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }

}


public class TribulationResistingPill : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), 60 * 90);
        cultivation.ReduceSpiritPressure(player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()) ? 18 : 12);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.TribulationCloudDew>(3)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(2)
            .AddIngredient(ItemID.BottledWater)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())
            .Register();
    }

}


public class StarAbyssForbiddenTalisman : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool? UseItem(Player player)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
        cultivation.RestoreSpiritualEnergy(80);
        cultivation.spiritPressure = Math.Clamp(cultivation.spiritPressure + 25, 0, 100);
        if (cultivation.spiritPressure >= 80)
            player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60 * 8);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(6)
            .AddIngredient<global::XianXia.Content.Items.Generated.StarAbyssMembrane>(2)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(TileID.DemonAltar)
            .Register();
    }

}


public class GardenBrokenKey : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Yellow;

    }

}


public class OldFurnaceEmber : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

    }

}


public class ThunderCallingJade : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

    }

}


public class StarAbyssMembrane : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

    }

}


public class HeavenTabletRubbing : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Yellow;

    }

}


public class MoonboneRitualTalisman : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

    }

}


public class CloudpiercerFlyingSword : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.damage = 42;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.CloudpiercerSwordProjectile>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(12);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class ThunderPatternSwordCase : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.damage = 56;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.ThunderSwordProjectile>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(18);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.TribulationCloudDew>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class FormlessSwordWheel : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.damage = 88;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.FormlessSwordWheelProjectile>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(28);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.SectTrialToken>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class MoonboneDharmaSword : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.damage = 145;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.MoonboneShardProjectile>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(36);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.Moonbone>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class CinnabarTalismanFlameItem : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.damage = 38;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.CinnabarTalismanFlame>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(14);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class GreenwoodArrayPlate : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.damage = 30;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.GreenwoodArrayField>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(20);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class ThunderTalismanArrayPlate : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.damage = 62;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.ThunderTalismanArray>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(22);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.TribulationCloudDew>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class BrokenHeavenDecree : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Yellow;

        Item.damage = 128;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.DecreeJudgementBeam>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(40);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class OldHeavenDaoScroll : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()
            .CanUseBreakthroughItem(global::XianXia.Common.Players.CultivationStage.NascentSoul);
    }

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.NascentSoul);
        return true;
    }

}


public class StarEclipseArbalest : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.damage = 74;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.StarEclipseSplitBolt>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(20);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(2)
            .AddIngredient<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(6)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class QiGatheringPendant : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.QiGatheringBuff>(), 2);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class SpiritwoodCharm : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.lifeRegen += 2;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class FurnaceHeartRing : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.statDefense += 3;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class LightningWardJade : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.endurance += 0.04f; if (Main.GameUpdateCount % 120 == 0) player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().ReduceSpiritPressure(1);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.TribulationCloudDew>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class StarAbyssEye : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage(DamageClass.Generic) += 0.06f;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class NascentSoulJadeBox : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().maxSpiritualEnergy += 30;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.SectTrialToken>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class BrokenHeavenCrownSeal : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Yellow;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage(DamageClass.Generic) += 0.1f; player.statDefense -= 4;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}


public class DaoSeveringRing : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>(); player.GetDamage(DamageClass.Generic) += 0.14f; cultivation.spiritualEnergyCostMultiplier *= 1.08f;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.DaoSeveringDust>(5)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())
            .Register();
    }

}
