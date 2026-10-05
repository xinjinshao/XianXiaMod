using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace XianXia.Content.Items.HandGenerated;
public class BlackFurnaceIronGolemPet : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.DefaultToVanitypet(ModContent.ProjectileType<global::XianXia.Content.Projectiles.BlackFurnaceIronGolemPetProjectile>(), ModContent.BuffType<global::XianXia.Content.Buffs.BlackFurnaceIronGolemPetBuff>());
        Item.width = Item.height = 32;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.Orange;
    }
    public override void UseStyle(Player player, Microsoft.Xna.Framework.Rectangle heldItemFrame)
    {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            player.AddBuff(Item.buffType, 3600);
    }
}
