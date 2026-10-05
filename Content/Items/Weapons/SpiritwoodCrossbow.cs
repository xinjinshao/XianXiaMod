using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Projectiles;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class SpiritwoodCrossbow : global::XianXia.Common.Items.CultivationWeaponItem
{
    private const int SpiritCost = 2;

    public override void SetDefaults()
    {
        Item.width = 56;
        Item.height = 44;
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
        return player.GetModPlayer<XianXiaPlayer>().CanConsumeSpiritualEnergy(SpiritCost);
    }

    public override int GetSpiritCost(Player player) => SpiritCost;


    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 16)
            .AddIngredient<LowGradeSpiritStone>(4)
            .AddTile(ModContent.TileType<ArtifactForgeTile>())
            .Register();
    }
}
