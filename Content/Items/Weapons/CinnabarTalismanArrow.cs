using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class CinnabarTalismanArrow : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FlamingArrow}";
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 99;
    public override void SetDefaults()
    {
        Item.width = 14; Item.height = 32; Item.maxStack = 9999;
        Item.value = 5; Item.rare = ItemRarityID.Green;
        Item.damage = 8; Item.knockBack = 2f; Item.DamageType = DamageClass.Ranged;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.CinnabarArrowProjectile>();
        Item.shootSpeed = 4f; Item.ammo = AmmoID.Arrow; Item.consumable = true;
    }
    public override void AddRecipes() => CreateRecipe(50).AddIngredient(ItemID.WoodenArrow, 50)
        .AddIngredient<TornTalismanPaper>(2).AddIngredient<CinnabarPowder>()
        .AddTile(ModContent.TileType<SimpleTalismanTableTile>()).Register();
}
