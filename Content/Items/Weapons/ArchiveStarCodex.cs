using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class ArchiveStarCodex : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/ArchiveRemnantLight";
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 10);
        Item.rare = ItemRarityID.Red;
        Item.damage = 230;
        Item.knockBack = 4f;
        Item.crit = 6;
        Item.DamageType = DamageClass.Magic;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 48;
        Item.UseSound = SoundID.Item20;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.ArchiveStarOrb>();
        Item.shootSpeed = 10f;
    }
    public override int GetSpiritCost(Player player) => 36;
    public override bool CanUseItem(Player player)
    {
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        return state.cultivationStage >= CultivationStage.Tribulation
            && CultivationRules.GetWorldFailure(CultivationStage.Tribulation, Main.hardMode,
                NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord).Length == 0
            && state.CanConsumeSpiritualEnergy(GetSpiritCost(player));
    }
    public override void AddRecipes() => CreateRecipe().AddIngredient<Moonbone>(12)
        .AddIngredient<ArchiveRemnantLight>(6).AddIngredient<ImperialDecreeItem>()
        .AddIngredient<LowGradeSpiritStone>(24).AddTile(ModContent.TileType<DaoSeveringAltarTile>())
        .AddCondition(Condition.DownedMoonLord).Register();
}
