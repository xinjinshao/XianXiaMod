using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.HandGenerated;

public class ArtifactQuenchingCrystal : InscriptionToolItem
{
    // Reuse the existing artifact material icon until the dedicated art batch.
    public override string Texture => "XianXia/Content/Items/Materials/ArtifactBlankShard";
    public override InscriptionKind TargetKind => InscriptionKind.None;
    public override bool RefinesArtifact => true;
    public override void SetDefaults() => Configure(Item.buyPrice(silver: 50), ItemRarityID.Orange);
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient<ArtifactBlankShard>(2).AddIngredient<FurnaceSlagIron>(4)
            .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
        CreateRecipe(3).AddIngredient<FoundationSeal>().AddIngredient<FurnaceSlagIron>(4)
            .AddTile(ModContent.TileType<ArtifactForgeTile>()).Register();
    }
}
