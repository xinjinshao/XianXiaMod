using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

// Optional material bridge; no Calamity assembly/type reference is required.
public class CalamityRecipeSystem : ModSystem
{
    public const int StonesPerEssence = 8;
    public static int RegisteredRecipeCount { get; private set; }
    private static readonly List<int> essenceTypes = new();
    public static string GetSourceHint() => essenceTypes.Count == 0 ? "" : Language.GetTextValue(
        "Mods.XianXia.SoftCompatibility.SpiritStoneRecipe", string.Join(" / ", essenceTypes.Select(type => Lang.GetItemNameValue(type))), StonesPerEssence);

    public override void AddRecipes()
    {
        RegisteredRecipeCount = 0;
        essenceTypes.Clear();
        if (!ModContent.GetInstance<XianXiaConfig>().EnableSoftCompatibilityHooks
            || !ModLoader.TryGetMod("CalamityMod", out Mod calamity)) return;
        foreach (string name in new[] { "EssenceofEleum", "EssenceofHavoc", "EssenceofSunlight" }) {
            if (!calamity.TryFind<ModItem>(name, out var essence)) {
                Mod.Logger.Warn("Skipping optional Calamity spirit-stone recipe: missing item " + name);
                continue;
            }
            Recipe.Create(ModContent.ItemType<LowGradeSpiritStone>(), StonesPerEssence)
                .AddIngredient(essence.Type).AddTile(ModContent.TileType<AlchemyCauldronTile>())
                .AddCondition(Condition.Hardmode).Register();
            RegisteredRecipeCount++;
            essenceTypes.Add(essence.Type);
        }
        Mod.Logger.Info("Registered " + RegisteredRecipeCount + " optional Calamity essence-to-spirit-stone recipes.");
    }
    public override void Unload() { RegisteredRecipeCount = 0; essenceTypes.Clear(); }
}
