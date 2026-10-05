using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Materials;

public class QiCondensingPill : ModItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()

    {

        Item.width = 24;

        Item.height = 24;

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
        if (global::XianXia.Common.Systems.CultivationItemTransactions.RequestIfMultiplayer(player, Item))
            return true;


        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        if (cultivation.TryAdvanceCultivation(global::XianXia.Common.Players.CultivationStage.QiCondensation,
                global::XianXia.Common.Systems.PillQualitySystem.BenefitMultiplier(Item))

            && player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()))

            cultivation.ReduceSpiritPressure(global::XianXia.Common.Systems.PillQualitySystem.Scale(Item, 6));

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.GreenwoodRoot>(3)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(5)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.EarthClayFurnaceTile>())

            .AddCondition(new Condition(Terraria.Localization.Language.GetText("Mods.XianXia.PillQuality.EmptyCursor"), () => Main.mouseItem.IsAir))
            .Register();

    }



}
