using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Items;

public abstract class CultivationFishingCrate : ModItem
{
    public abstract int ProgressionTier { get; }
    protected abstract int MaterialType { get; }
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
        ItemID.Sets.IsFishingCrate[Type] = true;
        ItemID.Sets.IsFishingCrateHardmode[Type] = ProgressionTier > 0;
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ProgressionTier == 0 ? ItemRarityID.Green : ItemRarityID.LightRed;
    }
    // Recheck the current world on opening, including crates brought from another world.
    public override bool CanRightClick() => CultivationFishingRules.Allows(ProgressionTier,
        Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord);
    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 1, 4, 8));
        itemLoot.Add(ItemDropRule.Common(MaterialType, 1, 2, 4));
    }
}
