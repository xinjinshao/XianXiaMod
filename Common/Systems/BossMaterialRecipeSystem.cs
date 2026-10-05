using Terraria.ModLoader;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

// Boss trophies provide material-efficient alternatives to the normal recipes.
// Register separately so regenerating the base weapons preserves these recipes.
public class BossMaterialRecipeSystem : ModSystem
{
    public override void AddRecipes()
    {
        Terraria.Recipe.Create(ModContent.ItemType<MoonboneDharmaSword>())
            .AddIngredient<StarCalamityCore>().AddIngredient<Moonbone>(6)
            .AddIngredient<LowGradeSpiritStone>(6)
            .AddTile(ModContent.TileType<DaoSeveringAltarTile>()).Register();

        Terraria.Recipe.Create(ModContent.ItemType<BrokenHeavenDecree>())
            .AddIngredient<HeavenTabletSeal>().AddIngredient<HeavenDaoFragment>(4)
            .AddIngredient<LowGradeSpiritStone>(8)
            .AddTile(ModContent.TileType<HeavenFireFurnaceTile>()).Register();

        Terraria.Recipe.Create(ModContent.ItemType<BrokenHeavenDecree>())
            .AddIngredient<ImperialDecreeItem>().AddIngredient<HeavenDaoFragment>(2)
            .AddIngredient<LowGradeSpiritStone>(6)
            .AddTile(ModContent.TileType<HeavenFireFurnaceTile>()).Register();
    }
}
