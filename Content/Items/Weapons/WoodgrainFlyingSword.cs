using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Projectiles;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class WoodgrainFlyingSword : global::XianXia.Common.Items.CultivationWeaponItem
{
    private const int SpiritCost = 4;

    public override void SetDefaults()
    {
        Item.width = 56;
        Item.height = 56;
        Item.damage = 14;
        Item.DamageType = DamageClass.Melee;
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
        return player.GetModPlayer<XianXiaPlayer>().CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }

    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 3 : SpiritCost;
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
    }


    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.Wood, 12)
            .AddIngredient<LowGradeSpiritStone>(6)
            .AddIngredient<SpiritGel>(10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
