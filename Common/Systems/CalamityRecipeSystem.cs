using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

// Optional material bridge; no Calamity assembly/type reference is required.
public class CalamityRecipeSystem : ModSystem
{
    public const int StonesPerEssence = 8;
    public static int RegisteredRecipeCount { get; private set; }

    public override void AddRecipes()
    {
        RegisteredRecipeCount = 0;
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
        }
        Mod.Logger.Info("Registered " + RegisteredRecipeCount + " optional Calamity essence-to-spirit-stone recipes.");
    }
    public override void Unload() => RegisteredRecipeCount = 0;
}
