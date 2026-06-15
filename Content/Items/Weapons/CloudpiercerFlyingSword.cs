using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class CloudpiercerFlyingSword : ModItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()

    {

        Item.width = 72;

        Item.height = 72;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.White;



        Item.damage = 28;

        Item.knockBack = 3.5f;

        Item.crit = 4;

        Item.DamageType = DamageClass.Generic;

        Item.useStyle = ItemUseStyleID.Swing;

        Item.useTime = 25;

        Item.useAnimation = 25;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.CloudpiercerSwordProjectile>();

        Item.shootSpeed = 11f;

    }



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .TryConsumeSpiritualEnergy(HasArtifactAwakening(player) ? 5 : 6);

    }



    private static bool HasArtifactAwakening(Player player)

    {

        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        return cultivation.cultivationStage >= global::XianXia.Common.Players.CultivationStage.GoldenCore

            && global::XianXia.Common.Systems.DownedBossSystem.SectReputation >= 32;

    }



    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)

    {

        if (HasArtifactAwakening(player))

            damage += 0.1f;

    }



    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)

    {

        Player player = Main.LocalPlayer;

        string key = HasArtifactAwakening(player)

            ? "Mods.XianXia.Progression.ArtifactAwakeningReady"

            : "Mods.XianXia.Progression.ArtifactAwakeningLocked";

        tooltips.Add(new TooltipLine(

            Mod,

            "XianXiaArtifactAwakening",

            Terraria.Localization.Language.GetTextValue(key, "GoldenCore", 32, 5, 10)));

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
