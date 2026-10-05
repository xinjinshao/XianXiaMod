using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Common.Items;

public abstract class CultivationBossBag : ModItem
{
    // Reuse an original material icon until individual bag artwork is ready.
    public override string Texture => "XianXia/Content/Items/Materials/ArtifactBlankShard";
    protected abstract int BossType { get; }
    protected abstract bool PreHardmode { get; }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.BossBag[Type] = true;
        ItemID.Sets.PreHardmodeLikeBossBag[Type] = PreHardmode;
        Item.ResearchUnlockCount = 3;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.rare = ItemRarityID.Purple;
        Item.expert = true;
    }

    public override bool CanRightClick() => true;
}
