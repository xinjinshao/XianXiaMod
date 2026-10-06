using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Buffs;
using XianXia.Content.Tiles.Stations;
namespace XianXia.Content.Items.Materials;
public class ThunderBurstPill : ModItem
{
    public const int BaseDuration = 1200;
    public override string Texture => "XianXia/Content/Items/Materials/TribulationResistingPill";
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = Item.height = 24; Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 15); Item.rare = ItemRarityID.Blue;
        Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.useTime = Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3; Item.consumable = true;
    }
    public override bool CanUseItem(Player player) => player.active && !player.dead
        && player.GetModPlayer<XianXiaPlayer>().cultivationStage >= CultivationStage.Foundation
        && !player.HasBuff(ModContent.BuffType<ThunderBurstBuff>())
        && !player.HasBuff(ModContent.BuffType<WindStepBuff>())
        && !player.HasBuff(ModContent.BuffType<FurnaceGuardBuff>())
        && !player.HasBuff(ModContent.BuffType<QiRecoveryCooldownBuff>());
    public override bool? UseItem(Player player)
    {
        if (CultivationItemTransactions.RequestIfMultiplayer(player, Item)) return true;
        if (!CanUseItem(player)) return false;
        player.AddBuff(ModContent.BuffType<ThunderBurstBuff>(), PillQualitySystem.Scale(Item, BaseDuration));
        player.AddBuff(ModContent.BuffType<QiRecoveryCooldownBuff>(), QiRecoveryRules.CooldownTicks);
        return true;
    }
    public override void AddRecipes() => CreateRecipe(2).AddIngredient<TribulationCloudDew>(2)
        .AddIngredient<FurnaceSlagIron>(2).AddIngredient<LowGradeSpiritStone>(8).AddIngredient(ItemID.BottledWater)
        .AddTile(ModContent.TileType<AlchemyCauldronTile>())
        .AddCondition(new Condition(Terraria.Localization.Language.GetText("Mods.XianXia.PillQuality.EmptyCursor"), () => Main.mouseItem.IsAir)).Register();
}
