using System.Text.Json;
using Terraria.Localization;
using XianXia.Common.Players;
int count=0;void Check(bool ok,string name){count++;if(!ok)throw new Exception(name);}
foreach(string culture in new[]{"en-US","zh-Hans"}){
 using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0],"Localization","cultivation-status",culture+".hjson")));Language.Root=json.RootElement;
 foreach(CultivationStage stage in Enum.GetValues<CultivationStage>())Check(!string.IsNullOrWhiteSpace(CultivationStatusText.StageName(stage)),"Every realm localized");
 Check(CultivationStatusText.StageName((CultivationStage)99)==CultivationStatusText.StageName(CultivationStage.None),"Unknown realm fallback");
 var p=new XianXiaPlayer{cultivationStage=CultivationStage.Foundation,spiritualEnergy=12,maxSpiritualEnergy=90,spiritPressure=37,tribulationTimer=61,tribulationWeakness=1};
 var text=CultivationStatusText.Summary(p);Check(text.Contains("12/90")&&text.Contains("37"),"Current resources");
 Check(text.Split('\n')[1]==Language.GetTextValue("Mods.XianXia.CultivationStatus.ActiveTrial",2),"Rounds partial seconds up");
 Check(text.Split('\n')[2]==Language.GetTextValue("Mods.XianXia.CultivationStatus.Weakness",1),"Weakness rounds up");
 p.tribulationTimer=0;p.tribulationWeakness=-1;Check(CultivationStatusText.Summary(p).Contains(Language.GetTextValue("Mods.XianXia.CultivationStatus.RetryTrial")),"Failed trial retry guidance");
 p.Cleared=true;text=CultivationStatusText.Summary(p);Check(text.Contains(Language.GetTextValue("Mods.XianXia.CultivationStatus.NoActiveTrial")),"Cleared trial idle");
 Check(text.Contains(Language.GetTextValue("Mods.XianXia.CultivationStatus.Weakness",0)),"Negative weakness clamped");
 p.Cleared=false;p.cultivationStage=CultivationStage.QiAwakening;Check(CultivationStatusText.Summary(p).Contains(Language.GetTextValue("Mods.XianXia.CultivationStatus.NoActiveTrial")),"Early realm has no retry");
 Check(p.spiritualEnergy==12&&p.spiritPressure==37&&p.tribulationTimer==0&&p.tribulationWeakness==-1,"Summary read only");
 p.tribulationTimer=int.MaxValue;Check(CultivationStatusText.Summary(p).Contains(Language.GetTextValue("Mods.XianXia.CultivationStatus.ActiveTrial",35791395)),"Large tick count no overflow");
}
Console.WriteLine($"Cultivation status source and real bilingual format regression passed: {count} assertions; player/rendering boundary mocked.");
