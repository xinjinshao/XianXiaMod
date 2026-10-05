using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Items;

public abstract class ExpertBossAccessory : ModItem
{
    protected abstract int Reward { get; }
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = 1;
        Item.accessory = true;
        Item.expert = true;
        Item.rare = ItemRarityID.Expert;
        Item.value = Item.buyPrice(gold: 2);
    }

    public sealed override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (player.GetModPlayer<ExpertRewardPlayer>().TryActivate(Reward))
            ExpertRewardEffects.Apply(player, player.GetModPlayer<XianXiaPlayer>(), Reward);
    }
}
