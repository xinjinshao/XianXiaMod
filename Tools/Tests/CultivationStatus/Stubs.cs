namespace XianXia.Common.Players {
 public class XianXiaPlayer { public CultivationStage cultivationStage; public int spiritualEnergy,maxSpiritualEnergy,spiritPressure,tribulationTimer,tribulationWeakness; public bool Cleared; public bool CanRetryTribulation()=>CultivationRules.CanRetry(cultivationStage,tribulationTimer,Cleared); }
}
namespace Terraria.Localization {
 public static class Language {
  public static System.Text.Json.JsonElement Root;
  public static string GetTextValue(string key,params object[] args){var value=Root;foreach(var part in key.Split('.'))value=value.GetProperty(part);return string.Format(value.GetString(),args);}
 }
}
