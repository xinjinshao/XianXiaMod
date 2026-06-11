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
            Mod.Logger.Info("Calamity Mod detected. XianXiaMod soft compatibility hooks are available.");
        }
    }

    public override void Unload()
    {
        CalamityLoaded = false;
    }
}
