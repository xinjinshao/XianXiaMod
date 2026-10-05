using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.Weapons;

public class StarCalamityMechanismCase : global::XianXia.Common.Items.CultivationWeaponItem
{
    public override string Texture => "XianXia/Content/Items/Weapons/ThunderPatternSwordCase";
    public override void SetDefaults()
    {
        Item.width = 72; Item.height = 52; Item.maxStack = 1;
        Item.value = Item.buyPrice(gold: 10); Item.rare = ItemRarityID.Red;
        Item.damage = 260; Item.knockBack = 3f; Item.crit = 6;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Shoot; Item.useTime = Item.useAnimation = 44;
        Item.UseSound = SoundID.Item20; Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<global::XianXia.Content.Projectiles.StarCalamityMechanismBolt>();
        Item.shootSpeed = 18f;
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
    public override void AddRecipes() => CreateRecipe().AddIngredient<StarCalamityCore>(1)
        .AddIngredient<Moonbone>(16).AddIngredient<LowGradeSpiritStone>(32)
        .AddTile(ModContent.TileType<DaoSeveringAltarTile>()).AddCondition(Condition.DownedMoonLord).Register();
}
