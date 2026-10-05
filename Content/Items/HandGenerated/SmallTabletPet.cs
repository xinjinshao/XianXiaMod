using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace XianXia.Content.Items.HandGenerated;
public class SmallTabletPet : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.DefaultToVanitypet(ModContent.ProjectileType<global::XianXia.Content.Projectiles.SmallTabletPetProjectile>(), ModContent.BuffType<global::XianXia.Content.Buffs.SmallTabletPetBuff>());
        Item.width = Item.height = 32;
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Yellow;
    }
    public override void UseStyle(Player player, Microsoft.Xna.Framework.Rectangle heldItemFrame)
    {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            player.AddBuff(Item.buffType, 3600);
    }
}
