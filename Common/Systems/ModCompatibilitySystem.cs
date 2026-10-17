using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public class ModCompatibilitySystem : ModSystem
{
    public static bool CalamityLoaded { get; private set; }

    public override void PostSetupContent()
    {
        CalamityLoaded = false;
        if (!ModContent.GetInstance<XianXiaConfig>().EnableSoftCompatibilityHooks)
        {
            return;
        }

        CalamityLoaded = ModLoader.TryGetMod("CalamityMod", out _);
        if (CalamityLoaded)
        {
            Mod.Logger.Info("Calamity Mod detected. Optional material recipes will be resolved during recipe setup.");
        }
    }

    public override void Unload()
    {
        CalamityLoaded = false;
    }
}
