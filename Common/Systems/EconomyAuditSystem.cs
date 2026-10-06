using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

// Opt-in developer snapshot. Reads finalized registrations without opening shops,
// running recipe conditions, consuming items or changing world/player progress.
public class EconomyAuditSystem : ModSystem
{
    public override void PostSetupRecipes()
    {
        if (!Main.dedServ || Environment.GetEnvironmentVariable("XIANXIA_EXPORT_ECONOMY") != "1") return;
        // Registry keys preserve legacy item aliases whose sample defaults redirect type.
        var samples = ContentSamples.ItemsByType.Where(pair => pair.Key > ItemID.None).OrderBy(pair => pair.Key).ToArray();
        var items = samples.Select(pair => {
            Item item = pair.Value;
            return new { id = pair.Key, name = item.ModItem?.FullName ?? "Terraria/" + ItemID.Search.GetName(pair.Key),
                value = item.value, stack = item.maxStack, research = item.ResearchUnlockCount,
                tile = item.createTile, consumable = item.consumable, material = item.material };
        }).ToArray();
        var recipes = Main.recipe.Take(Recipe.numRecipes).Where(recipe => !recipe.Disabled)
            .Select(recipe => new { id = recipe.RecipeIndex, output = recipe.createItem.type, quantity = recipe.createItem.stack,
                ingredients = recipe.requiredItem.Where(item => !item.IsAir).Select(item => new { id = item.type, quantity = item.stack }).ToArray(),
                groups = recipe.acceptedGroups.Select(id => new { id, items = RecipeGroup.recipeGroups[id].ValidItems.OrderBy(type => type).ToArray() }).ToArray(),
                stations = recipe.requiredTile.Where(tile => tile >= 0).ToArray(),
                conditions = recipe.Conditions.Select(condition => condition.Description.Key).ToArray() }).ToArray();
        var shops = NPCShopDatabase.AllShops.OfType<NPCShop>().OrderBy(shop => shop.FullName)
            .Select(shop => new { name = shop.FullName, entries = shop.Entries.Where(entry => !entry.Disabled)
                .Select(entry => new { id = entry.Item.type, price = entry.Item.shopCustomPrice ?? entry.Item.value,
                    currency = entry.Item.shopSpecialCurrency,
                    conditions = entry.Conditions.Select(condition => condition.Description.Key).ToArray() }).ToArray() }).ToArray();
        // Ask the installed engine for unit prices, including happiness and Discount Card.
        // This detached player never joins Main.player or changes an actual inventory.
        var pricePlayer = new Player();
        var scenarios = new[] { (name: "neutral", adjustment: 1.0, discount: false),
            (name: "happy", adjustment: 0.75, discount: false), (name: "happy_discount", adjustment: 0.75, discount: true) };
        var prices = scenarios.Select(scenario => {
            pricePlayer.currentShoppingSettings = new ShoppingSettings { PriceAdjustment = scenario.adjustment };
            pricePlayer.discountEquipped = scenario.discount;
            pricePlayer.discountAvailable = scenario.discount;
            return new { scenario.name, scenario.adjustment, scenario.discount,
                items = samples.Select(pair => {
                    pricePlayer.GetItemExpectedPrice(pair.Value, out long selling, out long buying);
                    return new { id = pair.Key, buying, selling = selling <= 0 ? 0 : Math.Max(1, selling / 5) };
                }).ToArray() };
        }).ToArray();
        string directory = Path.Combine(Main.SavePath, "XianXia");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "economy-audit.json"), JsonSerializer.Serialize(new {
            schema = 1, modVersion = Mod.Version.ToString(), engineVersion = BuildInfo.tMLVersion.ToString(),
            items, recipes, shops, prices,
            limitations = "Registrations only. Conditions and ModifyActiveShop callbacks are not evaluated; no drops, harvesting, shimmer, player inventory or actual transactions are simulated."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Mod.Logger.Info($"Economy audit exported {items.Length} items, {recipes.Length} recipes and {shops.Length} shops.");
    }
}
