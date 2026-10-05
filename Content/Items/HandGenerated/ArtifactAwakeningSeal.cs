using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.HandGenerated;

public class ArtifactAwakeningSeal : InscriptionToolItem
{
    public override string Texture => "XianXia/Content/Items/Materials/ThunderCallingJade";
    public override InscriptionKind TargetKind => InscriptionKind.None;
    public override bool AwakensArtifact => true;
    public override void SetDefaults() => Configure(Item.buyPrice(gold: 1), ItemRarityID.Lime);
    public override void AddRecipes() => CreateRecipe().AddIngredient<ArtifactBlankShard>(2)
        .AddIngredient<TribulationCloudDew>(6).AddIngredient<GreenwoodRoot>(4).AddIngredient<MedicineKingWoodHeart>()
        .AddTile(ModContent.TileType<StarPatternCauldronTile>()).Register();
}
