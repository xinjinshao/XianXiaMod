using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

public class EndgameRouteFrame : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<global::XianXia.Content.Tiles.BossDecorations.EndgameRouteFrameTile>());
        Item.width = Item.height = 32; Item.maxStack = 99;
        Item.value = Item.buyPrice(gold: 15); Item.rare = ItemRarityID.Red;
    }
}
