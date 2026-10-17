using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class ThunderPatternSwordCase : global::XianXia.Common.Items.CultivationWeaponItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()

    {

        Item.width = 80;

        Item.height = 80;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.LightRed;



        Item.damage = 54;

        Item.knockBack = 3.0f;

        Item.crit = 6;

        Item.DamageType = DamageClass.Melee;

        Item.useStyle = ItemUseStyleID.HoldUp;

        Item.useTime = 22;

        Item.useAnimation = 22;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.ThunderSwordProjectile>();

        Item.shootSpeed = 13f;

    }



    public override bool CanUseItem(Player player)

    {

        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player,Item);
        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanConsumeSpiritualEnergy(GetSpiritCost(player));

    }



    public const int AwakenedSpiritCost = 6;
    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name,route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)
        ? AwakenedSpiritCost : HasSectInheritance(player) ? 7 : 9;
    public override bool AltFunctionUse(Player player) => global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item);


    private static bool HasSectInheritance(Player player)

    {

        if (player == null) return false;
        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        return cultivation.cultivationStage >= global::XianXia.Common.Players.CultivationStage.GoldenCore

            && global::XianXia.Common.Systems.DownedBossSystem.SectReputation >= 40;

    }



    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)

    {

        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
        if (HasSectInheritance(player))

            damage += 0.12f;

    }



    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)

    {

        Player player = Main.LocalPlayer;

        string key = HasSectInheritance(player)

            ? "Mods.XianXia.Refinement.ThunderCaseHeritageReady"

            : "Mods.XianXia.Refinement.ThunderCaseHeritageLocked";

        tooltips.Add(new TooltipLine(

            Mod,

            "XianXiaThunderHeritage",

            Terraria.Localization.Language.GetTextValue(key, global::XianXia.Common.Players.CultivationStatusText.StageName(global::XianXia.Common.Players.CultivationStage.GoldenCore), 40, GetSpiritCost(player), 12)));

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(2)

            .AddIngredient<global::XianXia.Content.Items.HandGenerated.ThunderPatternFeather>(12)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ThunderPatternForgeTile>())

            .Register();

    }



}
