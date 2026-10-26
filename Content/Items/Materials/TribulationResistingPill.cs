using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Materials;

public class TribulationResistingPill : ModItem

{
    public const int DurationTicks = 5400;
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) =>
        global::XianXia.Common.Systems.PillQualitySystem.AddDurationTooltip(Item, tooltips, DurationTicks);


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



    public override bool CanUseItem(Player player) =>
        global::XianXia.Common.Systems.PillQualitySystem.CanBenefitFromBuffPill(Item, player,
            ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), DurationTicks);

    public override bool? UseItem(Player player)

    {
        if (global::XianXia.Common.Systems.CultivationItemTransactions.RequestIfMultiplayer(player, Item))
            return true;
        if (!CanUseItem(player)) return false;

        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), global::XianXia.Common.Systems.PillQualitySystem.Scale(Item, DurationTicks));

        cultivation.ReduceSpiritPressure(global::XianXia.Common.Systems.PillQualitySystem.Scale(Item, player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.AlchemyInsightBuff>()) ? 18 : 12));

        global::XianXia.Common.Systems.PillQualitySystem.ApplySinglePlayerBonus(Item, player);
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
