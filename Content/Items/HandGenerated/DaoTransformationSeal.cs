using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.HandGenerated;

public class DaoTransformationSeal : InscriptionToolItem
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/RouteMaterial";
    public override InscriptionKind TargetKind => InscriptionKind.None;
    public override bool TransformsArtifact => true;
    public override void SetDefaults() => Configure(Item.buyPrice(gold: 3), ItemRarityID.Purple);
    public override bool CanUseItem(Player player) => EndgameRouteTransactions.NearAltar(player)
        && (Main.netMode == NetmodeID.Server || Main.mouseItem.IsAir);
    public override void AddRecipes() => CreateRecipe().AddIngredient<RouteMaterial>()
        .AddIngredient<HeavenDaoFragment>(8).AddIngredient<Moonbone>(8)
        .AddTile(ModContent.TileType<DaoSeveringAltarTile>()).Register();
}
