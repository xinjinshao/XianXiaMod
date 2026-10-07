using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class MoonboneDharmaSword : global::XianXia.Common.Items.CultivationWeaponItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()

    {

        Item.width = 104;

        Item.height = 104;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.Red;



        Item.damage = 220;

        Item.knockBack = 4.5f;

        Item.crit = 10;

        Item.DamageType = DamageClass.Melee;

        Item.useStyle = ItemUseStyleID.Swing;

        Item.useTime = 18;

        Item.useAnimation = 18;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.MoonboneShardProjectile>();

        Item.shootSpeed = 14f;

    }



    public override bool CanUseItem(Player player)

    {

        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player, Item);
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanConsumeSpiritualEnergy(GetSpiritCost(player));

    }



    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 14 : HasArtifactAwakening(player) ? 17 : 22;


    private static bool HasArtifactAwakening(Player player)

    {

        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        return cultivation.cultivationStage >= global::XianXia.Common.Players.CultivationStage.Tribulation

            && global::XianXia.Common.Systems.DownedBossSystem.SectReputation >= 96;

    }



    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)

    {

        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
        if (HasArtifactAwakening(player))

            damage += 0.18f;

    }



    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)

    {

        Player player = Main.LocalPlayer;

        string key = HasArtifactAwakening(player)

            ? "Mods.XianXia.Refinement.MoonHeritageReady"

            : "Mods.XianXia.Refinement.MoonHeritageLocked";

        tooltips.Add(new TooltipLine(

            Mod,

            "XianXiaMoonHeritage",

            Terraria.Localization.Language.GetTextValue(key, global::XianXia.Common.Players.CultivationStatusText.StageName(global::XianXia.Common.Players.CultivationStage.Tribulation), 96, GetSpiritCost(player), 18)));

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(2)

            .AddIngredient<global::XianXia.Content.Items.Materials.Moonbone>(20)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.DaoSeveringAltarTile>())

            .Register();

    }



}
