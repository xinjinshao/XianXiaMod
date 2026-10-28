using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

// These materials must be obtainable before their corresponding bosses.
public class BossSummonMaterialRecipes : ModSystem
{
    public override void AddRecipes()
    {
        Register<GardenBrokenKey>(ModContent.ItemType<GreenwoodRoot>(), 4);
        Register<OldFurnaceEmber>(ModContent.ItemType<FurnaceSlagIron>(), 4);
        Recipe.Create(ModContent.ItemType<ThunderCallingJade>())
            .AddIngredient(ItemID.Cloud, 8).AddIngredient<SpiritGel>(3)
            .AddIngredient<LowGradeSpiritStone>(3).AddTile(TileID.WorkBenches).Register();
        Register<StarAbyssMembrane>(ModContent.ItemType<StarEclipseCrystal>(), 4);
        Register<HeavenTabletRubbing>(ModContent.ItemType<HeavenDaoFragment>(), 4);
        Register<MoonboneRitualTalisman>(ModContent.ItemType<Moonbone>(), 4);
    }

    private static void Register<T>(int material, int count) where T : ModItem =>
        Recipe.Create(ModContent.ItemType<T>()).AddIngredient(material, count)
            .AddIngredient<LowGradeSpiritStone>(3).AddTile(TileID.WorkBenches).Register();
}
