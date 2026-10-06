using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
namespace XianXia.Content.Items.Accessories;
public class SpiritTidePearl : ModItem
{
    public override string Texture => "XianXia/Content/Items/Materials/StarEclipseCrystal";
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width=Item.height=24;Item.accessory=true;Item.value=Item.buyPrice(gold:2);Item.rare=ItemRarityID.Pink;
    }
    public override void UpdateAccessory(Player player,bool hideVisual)
    {
        player.statManaMax2+=20;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus+=1;
    }
}
