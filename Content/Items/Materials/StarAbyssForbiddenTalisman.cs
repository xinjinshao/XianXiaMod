using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Materials;

public class StarAbyssForbiddenTalisman : ModItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()

    {

        Item.width = 36;

        Item.height = 36;

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

            .AddIngredient<global::XianXia.Content.Items.Materials.StarEclipseCrystal>(6)

            .AddIngredient<global::XianXia.Content.Items.Materials.StarAbyssMembrane>(2)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(TileID.DemonAltar)

            .Register();

    }



}
