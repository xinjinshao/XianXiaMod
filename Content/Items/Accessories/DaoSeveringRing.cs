using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Accessories;

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

            .AddIngredient<global::XianXia.Content.Items.Materials.DaoSeveringDust>(5)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(8)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())

            .Register();

    }



}
