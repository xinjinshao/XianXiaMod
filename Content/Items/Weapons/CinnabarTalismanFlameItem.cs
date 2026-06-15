using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class CinnabarTalismanFlameItem : ModItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()

    {

        Item.width = 48;

        Item.height = 48;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.White;



        Item.damage = 24;

        Item.knockBack = 2.0f;

        Item.crit = 4;

        Item.DamageType = DamageClass.Generic;

        Item.useStyle = ItemUseStyleID.HoldUp;

        Item.useTime = 24;

        Item.useAnimation = 24;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.CinnabarTalismanFlame>();

        Item.shootSpeed = 7f;

    }



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .TryConsumeSpiritualEnergy(HasArtifactAwakening(player) ? 4 : 5);

    }



    private static bool HasArtifactAwakening(Player player)

    {

        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        return cultivation.cultivationStage >= global::XianXia.Common.Players.CultivationStage.Foundation

            && global::XianXia.Common.Systems.DownedBossSystem.SectReputation >= 24;

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

            Terraria.Localization.Language.GetTextValue(key, "Foundation", 24, 4, 10)));

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(2)

            .AddIngredient<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(6)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())

            .Register();

    }



}
