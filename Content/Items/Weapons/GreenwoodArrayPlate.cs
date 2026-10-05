using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class GreenwoodArrayPlate : global::XianXia.Common.Items.CultivationWeaponItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()

    {

        Item.width = 56;

        Item.height = 56;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.White;



        Item.damage = 18;

        Item.knockBack = 0f;

        Item.crit = 4;

        Item.DamageType = DamageClass.Magic;

        Item.useStyle = ItemUseStyleID.HoldUp;

        Item.useTime = 36;

        Item.useAnimation = 36;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.GreenwoodArrayField>();

        Item.shootSpeed = 0f;

    }



    public override bool CanUseItem(Player player)

    {
        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player, Item);

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanDeployArray(Item.shoot, GetSpiritCost(player));

    }



    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name,route) : HasArtifactAwakening(player) ? 14 : 16;
    public override bool DeploysArray => true;


    private bool HasArtifactAwakening(Player player) => global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item);



    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)

    {

        if (HasArtifactAwakening(player))

            damage += 0.1f;

    }







    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(2)

            .AddIngredient<global::XianXia.Content.Items.Materials.GreenwoodRoot>(6)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())

            .Register();

    }



}
