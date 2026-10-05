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
    public override int GetSpiritCost(Player player) => 32;
    public override bool CanUseItem(Player player)
    {
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
