using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class GreenwoodMedicineCauldron : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/MedicineKingCauldronDecoration";
    public override bool DeploysArray => true;
    public override void SetDefaults()
    {
        Item.width = 40; Item.height = 40; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Lime;
        Item.damage = 82; Item.knockBack = 2f; Item.crit = 6;
        Item.DamageType = DamageClass.Magic;
        Item.useStyle = ItemUseStyleID.HoldUp; Item.useTime = Item.useAnimation = 40;
        Item.UseSound = SoundID.Item20; Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.MedicineCauldronField>();
        Item.shootSpeed = 0f;
    }
    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 26 : 32;
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
    }
    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player, Item);
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        return state.cultivationStage >= CultivationStage.NascentSoul
            && CultivationRules.GetWorldFailure(CultivationStage.NascentSoul, Main.hardMode,
                NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0
            && state.CanDeployArray(Item.shoot, GetSpiritCost(player));
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<MedicineKingWoodHeart>()
        .AddIngredient<GreenwoodRoot>(16).AddIngredient<LowGradeSpiritStone>(24)
        .AddTile(ModContent.TileType<SectTrialAltarTile>()).AddCondition(Condition.DownedPlantera).Register();
}
