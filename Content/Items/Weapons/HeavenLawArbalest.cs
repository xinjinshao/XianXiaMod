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
    public override int GetSpiritCost(Player player) => 28;
    public override bool CanUseItem(Player player)
    {
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
