using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Stations;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditStationRecipes(Action<bool, string> check)
    {
        foreach (var entry in new[] {
            (ModContent.ItemType<ArtifactForge>(), StationRecipeGroups.EarlyAnvil, ItemID.IronAnvil, ItemID.LeadAnvil, TileID.WorkBenches),
            (ModContent.ItemType<ThunderPatternForge>(), StationRecipeGroups.HardmodeAnvil, ItemID.MythrilAnvil, ItemID.OrichalcumAnvil, TileID.MythrilAnvil),
            (ModContent.ItemType<HeavenFireFurnace>(), StationRecipeGroups.HardmodeForge, ItemID.AdamantiteForge, ItemID.TitaniumForge, TileID.MythrilAnvil) }) {
            check(RecipeGroup.recipeGroupIDs.TryGetValue(entry.Item2, out int groupId), "native station ore-alternative group is registered");
            var group = RecipeGroup.recipeGroups[groupId];
            check(group.ValidItems.SetEquals(new int[] { entry.Item3, entry.Item4 }), "native station group accepts exactly the two equivalent vanilla stations");
            var recipes = Main.recipe.Take(Recipe.numRecipes).Where(recipe => !recipe.Disabled && recipe.createItem.type == entry.Item1).ToArray();
            check(recipes.Length == 1 && recipes[0].createItem.stack == 1, "native station keeps one recipe and one output");
            Recipe recipe = recipes[0];
            check(recipe.acceptedGroups.Contains(groupId), "native finalized station recipe uses its ore-alternative group");
            check(recipe.AcceptedByItemGroups(entry.Item4, entry.Item3), "native recipe engine accepts the alternate ore station as an ingredient substitute");
            check(!recipe.AcceptedByItemGroups(ItemID.Wood, entry.Item3), "native station recipe refuses unrelated substitute items");
            check(recipe.requiredItem.Single(item => item.type == entry.Item3).stack == 1, "native station group consumes one equivalent station");
            check(recipe.requiredTile.Contains(entry.Item5), "native station keeps its required crafting location");
        }
    }
}
