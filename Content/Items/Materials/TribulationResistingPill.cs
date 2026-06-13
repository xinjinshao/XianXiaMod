using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Materials;

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

            .AddIngredient<global::XianXia.Content.Items.Materials.TribulationCloudDew>(3)

            .AddIngredient<global::XianXia.Content.Items.Materials.GreenwoodRoot>(2)

            .AddIngredient(ItemID.BottledWater)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())

            .Register();

    }



}
