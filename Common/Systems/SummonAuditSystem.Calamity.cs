using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditCalamityRecipes(Action<bool, string> check)
    {
        if (!ModContent.GetInstance<XianXiaConfig>().EnableSoftCompatibilityHooks || !ModLoader.TryGetMod("CalamityMod", out var calamity)) {
            check(CalamityRecipeSystem.RegisteredRecipeCount == 0, "native standalone or disabled compatibility has no registered Calamity bridge recipes");
            return;
        }
        int found = 0;
        foreach (string name in new[] { "EssenceofEleum", "EssenceofHavoc", "EssenceofSunlight" }) {
            if (!calamity.TryFind<ModItem>(name, out var item)) continue;
            found++;
            check(Main.recipe.Take(Recipe.numRecipes).Any(recipe => !recipe.Disabled
                && recipe.createItem.type == ModContent.ItemType<LowGradeSpiritStone>() && recipe.createItem.stack == CalamityRecipeSystem.StonesPerEssence
                && recipe.requiredItem.Any(ingredient => ingredient.type == item.Type && ingredient.stack == 1)
                && recipe.requiredTile.Contains(ModContent.TileType<AlchemyCauldronTile>()) && recipe.Conditions.Contains(Condition.Hardmode)),
                "native present Calamity material has the expected registered gated bridge recipe");
        }
        check(CalamityRecipeSystem.RegisteredRecipeCount == found, "native Calamity recipe status matches successfully resolved optional items");
    }
}
