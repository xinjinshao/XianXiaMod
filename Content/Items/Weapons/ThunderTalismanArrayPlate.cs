using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class ThunderTalismanArrayPlate : global::XianXia.Common.Items.CultivationWeaponItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()

    {

        Item.width = 60;

        Item.height = 60;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.LightRed;



        Item.damage = 46;

        Item.knockBack = 0f;

        Item.crit = 6;

        Item.DamageType = DamageClass.Magic;

        Item.useStyle = ItemUseStyleID.HoldUp;

        Item.useTime = 34;

        Item.useAnimation = 34;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.ThunderTalismanArray>();

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
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 16 : HasArtifactAwakening(player) ? 20 : 24;
    public override bool DeploysArray => true;


    private static bool HasArtifactAwakening(Player player)

    {

        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        return cultivation.cultivationStage >= global::XianXia.Common.Players.CultivationStage.GoldenCore

            && global::XianXia.Common.Systems.DownedBossSystem.SectReputation >= 44;

    }



    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)

    {

        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
        if (HasArtifactAwakening(player))

            damage += 0.12f;

    }



    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)

    {

        Player player = Main.LocalPlayer;

        string key = HasArtifactAwakening(player)

            ? "Mods.XianXia.Refinement.ThunderArrayHeritageReady"

            : "Mods.XianXia.Refinement.ThunderArrayHeritageLocked";

        tooltips.Add(new TooltipLine(

            Mod,

            "XianXiaArtifactAwakening",

            Terraria.Localization.Language.GetTextValue(key, global::XianXia.Common.Players.CultivationStatusText.StageName(global::XianXia.Common.Players.CultivationStage.GoldenCore), 44, GetSpiritCost(player), 12)));

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(2)

            .AddIngredient<global::XianXia.Content.Items.Materials.TribulationCloudDew>(6)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ThunderPatternForgeTile>())

            .Register();

    }



}
