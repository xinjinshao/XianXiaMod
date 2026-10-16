namespace XianXia.Common.Players {
 public class XianXiaPlayer { public CultivationStage cultivationStage; public int spiritualEnergy,maxSpiritualEnergy,spiritPressure,tribulationTimer,tribulationWeakness; public bool Cleared,discoveredSpiritualEnergy; public bool CanRetryTribulation()=>CultivationRules.CanRetry(cultivationStage,tribulationTimer,Cleared); }
}
namespace Terraria.Localization {
 public static class Language {
  public static System.Text.Json.JsonElement Root, GuideRoot;
  public static string GetTextValue(string key,params object[] args){var value=key.StartsWith("Mods.XianXia.BreakthroughGuide.")?GuideRoot:Root;foreach(var part in key.Split('.'))value=value.GetProperty(part);return string.Format(value.GetString(),args);}
 }
}

namespace XianXia.Content.Items.Consumables {public class QiDrawingTalisman{public const int SpiritStoneCost=3,SpiritGelCost=2;}}

namespace XianXia.Content.Items.Materials {public class QiCondensingPill {public const int GreenwoodRootCost=3,SpiritStoneCost=5;}public class FoundationPill{public const int GreenwoodRootCost=4,FurnaceSlagCost=4,SpiritStoneCost=10;}}
