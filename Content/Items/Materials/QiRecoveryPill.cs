using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using XianXia.Content.Buffs;
using XianXia.Content.Tiles.Stations;
namespace XianXia.Content.Items.Materials;
public class QiRecoveryPill : ModItem
{
    public override string Texture => "XianXia/Content/Items/Materials/SpringReturnPill";
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = Item.height = 24; Item.maxStack = 30;
        Item.value = Item.buyPrice(silver: 8); Item.rare = ItemRarityID.Blue;
        Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.useTime = Item.useAnimation = 20;
        Item.UseSound = SoundID.Item3; Item.consumable = true;
    }
    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) =>
        tooltips.Add(new TooltipLine(Mod, "QiRecoveryAmounts", Terraria.Localization.Language.GetTextValue(
            "Mods.XianXia.PillQuality.RecoveryAmounts", PillQualitySystem.Scale(Item, QiRecoveryRules.BaseRecovery),
            PillQualityRules.BonusEnergy(PillQualitySystem.GetQuality(Item)), QiRecoveryRules.PressureCost,
            QiRecoveryRules.CooldownTicks / 60)));
    public override bool CanUseItem(Player player)
    {
        var state = player.GetModPlayer<XianXiaPlayer>();
        return player.active && !player.dead && QiRecoveryRules.CanUse(state.discoveredSpiritualEnergy,
            state.spiritualEnergy, state.maxSpiritualEnergy, player.HasBuff(ModContent.BuffType<QiRecoveryCooldownBuff>()));
    }
    public override bool? UseItem(Player player)
    {
        if (CultivationItemTransactions.RequestIfMultiplayer(player, Item)) return true;
        if (!CanUseItem(player)) return false;
        var state = player.GetModPlayer<XianXiaPlayer>();
        state.RestoreSpiritualEnergy(PillQualitySystem.Scale(Item, QiRecoveryRules.BaseRecovery));
        state.spiritPressure = Math.Min(100, state.spiritPressure + QiRecoveryRules.PressureCost);
        player.AddBuff(ModContent.BuffType<QiRecoveryCooldownBuff>(), QiRecoveryRules.CooldownTicks);
        global::XianXia.Common.Systems.PillQualitySystem.ApplySinglePlayerBonus(Item, player);
        return true;
    }
    public override void AddRecipes() => CreateRecipe(2).AddIngredient<GreenwoodRoot>(2)
        .AddIngredient<LowGradeSpiritStone>(4).AddIngredient(ItemID.BottledWater)
        .AddTile(ModContent.TileType<AlchemyCauldronTile>())
        .AddCondition(new Condition(Terraria.Localization.Language.GetText("Mods.XianXia.PillQuality.EmptyCursor"), () => Main.mouseItem.IsAir)).Register();
}
