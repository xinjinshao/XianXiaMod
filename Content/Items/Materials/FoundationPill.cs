using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Materials;

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

            .AddIngredient<global::XianXia.Content.Items.Materials.GreenwoodRoot>(4)

            .AddIngredient<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(4)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(10)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())

            .Register();

    }



}
