using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class BlackFurnaceWarhammer : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/Weapons/ThunderPatternSwordCase";
    public override void SetDefaults()
    {
        Item.width = Item.height = 48; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 1); Item.rare = ItemRarityID.Green;
        Item.damage = 54; Item.knockBack = 8f; Item.crit = 4;
        Item.DamageType = DamageClass.Melee;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = Item.useAnimation = 44;
        Item.UseSound = SoundID.Item20; Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceHammerProjectile>();
        Item.shootSpeed = 9f;
    }
    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 10 : 12;
    public override bool AltFunctionUse(Player player) => global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item);
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
    }
    public override bool CanUseItem(Player player)
    {
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        if (state.cultivationStage < CultivationStage.QiCondensation || state.cultivationStage > CultivationStage.DaoSevering) return false;
        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player, Item);
        return state.CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<OldFurnaceEmber>(4)
        .AddIngredient<FurnaceSlagIron>(12).AddIngredient<LowGradeSpiritStone>(12)
        .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
}
