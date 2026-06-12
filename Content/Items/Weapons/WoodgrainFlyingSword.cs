using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Projectiles;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class WoodgrainFlyingSword : ModItem
{
    private const int SpiritCost = 4;

    public override void SetDefaults()
    {
        Item.width = 48;
        Item.height = 48;
        Item.damage = 14;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.knockBack = 3f;
        Item.crit = 4;
        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.noUseGraphic = false;
        Item.shoot = ModContent.ProjectileType<WoodgrainSwordProjectile>();
        Item.shootSpeed = 9f;
        Item.UseSound = SoundID.Item1;
        Item.value = Item.buyPrice(silver: 20);
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
            .AddIngredient(ItemID.Wood, 12)
            .AddIngredient<LowGradeSpiritStone>(6)
            .AddIngredient<SpiritGel>(10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
