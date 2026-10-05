using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

// Native arrow selection, ammo hooks and consumption are handled by Terraria.
public class TalismanCrossbow : ModItem
{
    public override string Texture => "XianXia/Content/Items/Weapons/SpiritwoodCrossbow";
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 56; Item.height = 44; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 1); Item.rare = ItemRarityID.Green;
        Item.damage = 32; Item.knockBack = 3f; Item.crit = 4;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Shoot; Item.useTime = Item.useAnimation = 28;
        Item.UseSound = SoundID.Item5; Item.noMelee = true; Item.autoReuse = true;
        Item.shoot = ProjectileID.WoodenArrowFriendly; Item.shootSpeed = 9f;
        Item.useAmmo = AmmoID.Arrow;
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<ArtifactBlankShard>(4)
        .AddIngredient<FurnaceSlagIron>(8).AddIngredient<TornTalismanPaper>(6)
        .AddIngredient<LowGradeSpiritStone>(8).AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
}
