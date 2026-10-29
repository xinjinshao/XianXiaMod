using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class HeavenLawArbalest : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/Weapons/SpiritwoodCrossbow";
    public override void SetDefaults()
    {
        Item.width = 72; Item.height = 52; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 7); Item.rare = ItemRarityID.Yellow;
        Item.damage = 170; Item.knockBack = 3f; Item.crit = 6;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Shoot; Item.useTime = Item.useAnimation = 36;
        Item.UseSound = SoundID.Item20; Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.HeavenLawBolt>();
        Item.shootSpeed = 16f;
    }
    public override int GetSpiritCost(Player player) => global::XianXia.Common.Items.RefinedArtifact.ActiveDaoRoute(Item) is var route
        && route != global::XianXia.Common.Systems.DownedBossSystem.EndgameRoute.None
        ? global::XianXia.Common.Systems.DaoArtifactRules.WeaponCost(Name, route)
        : global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item) ? 22 : 28;
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (global::XianXia.Common.Items.RefinedArtifact.IsAwakened(Item)) damage += 0.1f;
    }
    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2) return global::XianXia.Common.Systems.ArtifactSkillTransactions.CanUseAlternative(player, Item);
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        return state.cultivationStage >= CultivationStage.SpiritSevering
            && CultivationRules.GetWorldFailure(CultivationStage.SpiritSevering, Main.hardMode,
                NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0
            && state.CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<HeavenTabletSeal>(1)
        .AddIngredient<HeavenTabletRubbing>(8).AddIngredient<LowGradeSpiritStone>(24)
        .AddTile(ModContent.TileType<HeavenFireFurnaceTile>()).AddCondition(Condition.DownedGolem).Register();
}
