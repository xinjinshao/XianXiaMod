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
        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player, Item);
        return player.GetModPlayer<XianXiaPlayer>().CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }

    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 1 : SpiritCost;
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
    }


    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.Wood, 16)
            .AddIngredient<LowGradeSpiritStone>(4)
            .AddTile(ModContent.TileType<ArtifactForgeTile>())
            .Register();
    }
}
