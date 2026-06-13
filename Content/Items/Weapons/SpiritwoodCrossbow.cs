using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Projectiles;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class SpiritwoodCrossbow : ModItem
{
    private const int SpiritCost = 2;

    public override void SetDefaults()
    {
        Item.width = 48;
        Item.height = 48;
        Item.damage = 12;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 2f;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<SpiritBoltProjectile>();
        Item.shootSpeed = 10f;
        Item.UseSound = SoundID.Item5;
        Item.value = Item.buyPrice(silver: 18);
        Item.rare = ItemRarityID.Blue;
        Item.autoReuse = true;
    }

    public override bool CanUseItem(Player player)
    {
        return player.GetModPlayer<XianXiaPlayer>().spiritualEnergy >= SpiritCost;
    }

    public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback)
    {
        if (!player.GetModPlayer<XianXiaPlayer>().TryConsumeSpiritualEnergy(SpiritCost))
        {
            return false;
        }
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 16)
            .AddIngredient<LowGradeSpiritStone>(4)
            .AddTile(ModContent.TileType<ArtifactForgeTile>())
            .Register();
    }
}
