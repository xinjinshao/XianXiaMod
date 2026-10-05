using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class SectMechanismCrossbow : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/Weapons/StarEclipseArbalest";
    public override void SetDefaults()
    {
        Item.width = 72; Item.height = 52; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 4); Item.rare = ItemRarityID.Lime;
        Item.damage = 110; Item.knockBack = 3f; Item.crit = 6;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Shoot; Item.useTime = Item.useAnimation = 26;
        Item.UseSound = SoundID.Item20; Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.SectMechanismBolt>();
        Item.shootSpeed = 14f;
    }
    public override int GetSpiritCost(Player player) => 20;
    public override bool CanUseItem(Player player)
    {
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        return state.cultivationStage >= CultivationStage.NascentSoul
            && CultivationRules.GetWorldFailure(CultivationStage.NascentSoul, Main.hardMode,
                NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0
            && state.CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<ArtifactBlankShard>(12)
        .AddIngredient<TornScrollPage>(8).AddIngredient<LowGradeSpiritStone>(16)
        .AddTile(ModContent.TileType<SectTrialAltarTile>()).AddCondition(Condition.DownedPlantera).Register();
}
