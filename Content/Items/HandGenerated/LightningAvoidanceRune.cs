using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Buffs;

namespace XianXia.Content.Items.HandGenerated;

public class LightningAvoidanceRune : ModItem
{
    public const int Duration = 60 * 30;
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;
    public override void SetDefaults()
    {
        Item.width = Item.height = 32; Item.maxStack = 30;
        Item.useStyle = ItemUseStyleID.DrinkLiquid; Item.useTime = Item.useAnimation = 20;
        Item.UseSound = SoundID.Item4; Item.consumable = true;
        Item.value = Item.buyPrice(silver: 30); Item.rare = ItemRarityID.White;
    }
    public override bool CanUseItem(Player player)
    {
        int buff = ModContent.BuffType<TribulationResistanceBuff>();
        int index = player.FindBuffIndex(buff);
        return player.active && !player.dead && !player.noItems && !player.CCed && !player.buffImmune[buff]
            && (index < 0 || player.buffTime[index] < Duration);
    }
    public override bool? UseItem(Player player)
    {
        if (CultivationItemTransactions.RequestIfMultiplayer(player, Item)) return true;
        if (!CanUseItem(player)) return false;
        player.AddBuff(ModContent.BuffType<TribulationResistanceBuff>(), Duration);
        return true;
    }
}
