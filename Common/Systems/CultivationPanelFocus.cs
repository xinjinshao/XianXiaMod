using Terraria.ModLoader;
namespace XianXia.Common.Systems;

public static class CultivationPanelFocus
{
    public static bool MaterialPanelOpen => ModContent.GetInstance<InscriptionUISystem>().IsOpen
        || ModContent.GetInstance<EndgameRouteUISystem>().IsOpen;
}
