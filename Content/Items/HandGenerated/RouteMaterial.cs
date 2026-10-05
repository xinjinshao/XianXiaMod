using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.Items.HandGenerated;

public class RouteMaterial : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = Item.height = 24; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 8); Item.rare = ItemRarityID.Purple;
        Item.useStyle = ItemUseStyleID.HoldUp; Item.useTime = Item.useAnimation = 20;
        Item.consumable = false;
    }
    public override bool? UseItem(Player player)
    {
        if (!Main.dedServ && player.whoAmI == Main.myPlayer)
            ModContent.GetInstance<EndgameRouteUISystem>().Open(player.selectedItem);
        return true;
    }
}
