using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Common.Items;

public abstract class MasterBossMonument : ModItem
{
    protected abstract int MonumentTile { get; }
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 48;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 10;
        Item.useAnimation = 15;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.createTile = MonumentTile;
        Item.master = true;
        Item.rare = ItemRarityID.Master;
        Item.value = Item.buyPrice(gold: 5);
    }
}
