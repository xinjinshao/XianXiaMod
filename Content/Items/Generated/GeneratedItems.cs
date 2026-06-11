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

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.GoldenCore);
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
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpringReturnBuff>(), 60 * 60);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe(3)
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(2)
            .AddIngredient(ItemID.BottledWater)
            .AddTile(TileID.Bottles)
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

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.QiCondensation);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(3)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(5)
            .AddTile(TileID.Bottles)
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

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.Foundation);
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<global::XianXia.Content.Items.Generated.GreenwoodRoot>(4)
            .AddIngredient<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(4)
            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(10)
            .AddTile(TileID.Bottles)
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
        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), 60 * 90);
        return true;
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
        return true;
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

}


public class OldHeavenDaoScroll : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Red;

        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3;
        Item.consumable = true;
        Item.damage = 110;
        Item.knockBack = 3.5f;
        Item.DamageType = DamageClass.Generic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.SpiritBolt>();
        Item.shootSpeed = 11f;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryConsumeSpiritualEnergy(34);
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
        player.endurance += 0.04f;
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
        player.GetDamage(DamageClass.Generic) += 0.14f;
    }

}
