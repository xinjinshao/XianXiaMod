using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditSummonMaterialRecipes(Action<bool, string> check)
    {
        (int result, int ingredient, int count)[] recipes = {
            (ModContent.ItemType<GardenBrokenKey>(), ModContent.ItemType<GreenwoodRoot>(), 4),
            (ModContent.ItemType<OldFurnaceEmber>(), ModContent.ItemType<FurnaceSlagIron>(), 4),
            (ModContent.ItemType<ThunderCallingJade>(), ItemID.Cloud, 8),
            (ModContent.ItemType<StarAbyssMembrane>(), ModContent.ItemType<StarEclipseCrystal>(), 4),
            (ModContent.ItemType<HeavenTabletRubbing>(), ModContent.ItemType<HeavenDaoFragment>(), 4),
            (ModContent.ItemType<MoonboneRitualTalisman>(), ModContent.ItemType<Moonbone>(), 4)
        };
        foreach (var spec in recipes) {
            var recipe = Main.recipe.Take(Recipe.numRecipes).Single(r => !r.Disabled && r.createItem.type == spec.result);
            check(recipe.createItem.stack == 1 && recipe.requiredTile.Contains(TileID.WorkBenches)
                && recipe.requiredItem.Any(item => item.type == spec.ingredient && item.stack == spec.count)
                && recipe.requiredItem.Any(item => item.type == ModContent.ItemType<LowGradeSpiritStone>() && item.stack == 3)
                && recipe.Conditions.Count == 0,
                "native summon material recipe registers a pre-boss workbench path with its exact ingredient and spirit cost");
            if (spec.result == ModContent.ItemType<ThunderCallingJade>())
                check(recipe.requiredItem.Any(item => item.type == ModContent.ItemType<SpiritGel>() && item.stack == 3),
                    "native thunder jade recipe includes early spirit gel rather than a Hardmode-only drop");
        }
    }
}
