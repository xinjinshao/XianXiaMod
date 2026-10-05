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

        Item.width = 28;

        Item.height = 28;

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
        if (global::XianXia.Common.Systems.CultivationItemTransactions.RequestIfMultiplayer(player, Item))
            return true;


        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), global::XianXia.Common.Systems.PillQualitySystem.Scale(Item, 60 * 90));

        cultivation.ReduceSpiritPressure(global::XianXia.Common.Systems.PillQualitySystem.Scale(Item, player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()) ? 18 : 12));

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe(2)

            .AddIngredient<global::XianXia.Content.Items.Materials.TribulationCloudDew>(3)

            .AddIngredient<global::XianXia.Content.Items.Materials.GreenwoodRoot>(2)

            .AddIngredient(ItemID.BottledWater)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.AlchemyCauldronTile>())

            .AddCondition(new Condition(Terraria.Localization.Language.GetText("Mods.XianXia.PillQuality.EmptyCursor"), () => Main.mouseItem.IsAir))
            .Register();

    }



}
