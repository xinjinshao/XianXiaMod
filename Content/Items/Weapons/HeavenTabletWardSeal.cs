using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class HeavenTabletWardSeal : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/HeavenTabletSeal";
    public override void SetDefaults()
    {
        Item.width = Item.height = 32; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 7); Item.rare = ItemRarityID.Yellow;
        Item.damage = 156; Item.knockBack = 7f; Item.crit = 6;
        Item.DamageType = DamageClass.Melee;
        Item.useStyle = ItemUseStyleID.Swing; Item.useTime = Item.useAnimation = 48;
        Item.UseSound = SoundID.Item20; Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.HeavenTabletWardProjectile>();
        Item.shootSpeed = 7f;
    }
    public override int GetSpiritCost(Player player) => 28;
    public override bool CanUseItem(Player player)
    {
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        return state.cultivationStage >= CultivationStage.SpiritSevering
            && CultivationRules.GetWorldFailure(CultivationStage.SpiritSevering, Main.hardMode,
                NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0
            && state.CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<HeavenTabletSeal>()
        .AddIngredient<HeavenDaoFragment>(12).AddIngredient<HeavenTabletRubbing>(8).AddIngredient<LowGradeSpiritStone>(24)
        .AddTile(ModContent.TileType<HeavenFireFurnaceTile>()).AddCondition(Condition.DownedGolem).Register();
}
