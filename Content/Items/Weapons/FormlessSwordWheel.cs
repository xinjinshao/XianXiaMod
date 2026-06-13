using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class FormlessSwordWheel : ModItem

{

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 25;

    public override void SetDefaults()

    {

        Item.width = 32;

        Item.height = 32;

        Item.maxStack = 1;

        Item.value = Item.buyPrice(silver: 10);

        Item.rare = ItemRarityID.White;



        Item.damage = 92;

        Item.knockBack = 4.0f;

        Item.crit = 8;

        Item.DamageType = DamageClass.Generic;

        Item.useStyle = ItemUseStyleID.Swing;

        Item.useTime = 20;

        Item.useAnimation = 20;

        Item.UseSound = SoundID.Item20;

        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.Generated.FormlessSwordWheelProjectile>();

        Item.shootSpeed = 8f;

    }



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .TryConsumeSpiritualEnergy(HasArtifactAwakening(player) ? 11 : 14);

    }



    private static bool HasArtifactAwakening(Player player)

    {

        global::XianXia.Common.Players.XianXiaPlayer cultivation = player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();

        return cultivation.cultivationStage >= global::XianXia.Common.Players.CultivationStage.NascentSoul

            && global::XianXia.Common.Systems.DownedBossSystem.SectReputation >= 56;

    }



    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)

    {

        if (HasArtifactAwakening(player))

            damage += 0.14f;

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

            Terraria.Localization.Language.GetTextValue(key, "NascentSoul", 56, 11, 14)));

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(2)

            .AddIngredient<global::XianXia.Content.Items.HandGenerated.BrokenSwordIntent>(12)

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(12)

            .AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.ArtifactForgeTile>())

            .Register();

    }



}
