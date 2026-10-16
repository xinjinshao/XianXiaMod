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
        foreach (var entry in new[] {
            (ModContent.ItemType<EarthClayFurnace>(), 10), (ModContent.ItemType<SimpleTalismanTable>(), 15),
            (ModContent.ItemType<global::XianXia.Content.Items.Weapons.WoodgrainFlyingSword>(), 12),
            (ModContent.ItemType<global::XianXia.Content.Items.Weapons.SpiritwoodCrossbow>(), 16) }) {
            var recipes = Main.recipe.Take(Recipe.numRecipes).Where(recipe => !recipe.Disabled && recipe.createItem.type == entry.Item1).ToArray();
            check(recipes.Length == 1, "native early wood item retains one finalized recipe");
            Recipe recipe = recipes[0];
            check(recipe.acceptedGroups.Contains(RecipeGroupID.Wood) && recipe.requiredItem.Single(item => item.type == ItemID.Wood).stack == entry.Item2,
                "native early recipe uses the vanilla wood group with unchanged quantity");
            foreach (int wood in new int[] { ItemID.BorealWood, ItemID.PalmWood, ItemID.RichMahogany, ItemID.Ebonwood, ItemID.Shadewood })
                check(recipe.AcceptedByItemGroups(wood, ItemID.Wood), "native early recipe accepts an equivalent biome wood");
            check(!recipe.AcceptedByItemGroups(ItemID.StoneBlock, ItemID.Wood), "native early recipe refuses stone as wood");
        }
    }
}
