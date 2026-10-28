using System.Text.Json;
using Terraria.Localization;
using XianXia.Common.Players;
int count=0;void Check(bool ok,string name){count++;if(!ok)throw new Exception(name);}
foreach(string culture in new[]{"en-US","zh-Hans"}){
 Language.NpcPrefix=culture+" localized NPC: ";
 using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0],"Localization","cultivation-status",culture+".hjson")));Language.Root=json.RootElement;
 using var guide=JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0],"Localization","breakthrough-guide",culture+".hjson")));Language.GuideRoot=guide.RootElement;
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
 foreach(CultivationStage target in Enum.GetValues<CultivationStage>().Where(stage=>stage!=CultivationStage.None)) {
  string source=CultivationStatusText.BreakthroughSource(target);
  Check(!string.IsNullOrWhiteSpace(source)&&!source.Contains("{0}")&&!source.Contains("{1}")&&!source.Contains("{2}"),"Every breakthrough source formats real bilingual text without placeholders");
 }
 Check(CultivationStatusText.BreakthroughSource(CultivationStage.QiAwakening)==Language.GetTextValue("Mods.XianXia.BreakthroughGuide.Sources.QiAwakening",3,2),"Awakening source formats shared recipe quantities");
 Check(CultivationStatusText.BreakthroughSource(CultivationStage.QiCondensation)==Language.GetTextValue("Mods.XianXia.BreakthroughGuide.Sources.QiCondensation",3,5),"Condensation source formats shared recipe quantities");
 Check(CultivationStatusText.BreakthroughSource(CultivationStage.Foundation)==Language.GetTextValue("Mods.XianXia.BreakthroughGuide.Sources.Foundation",4,4,10),"Foundation source formats shared recipe quantities");
 foreach(var entry in new[]{(CultivationStage.GoldenCore,new[]{"AbyssalStarWomb"}),(CultivationStage.NascentSoul,new[]{"ArchiveScrollSpirit"}),
  (CultivationStage.SpiritSevering,new[]{"HeavenTabletGuardian","BrokenHeavenInspector"}),
  (CultivationStage.Tribulation,new[]{"MoonboneCultivator"}),(CultivationStage.DaoSevering,new[]{"MoonboneImmortal","OldHeavenDaoCore"})}) {
  string source=CultivationStatusText.BreakthroughSource(entry.Item1);
  foreach(string npc in entry.Item2)Check(source.Contains(Language.NpcPrefix+npc),"Source reads the current NPC display name instead of an independently maintained literal");
 }
 string tribulationSource=CultivationStatusText.BreakthroughSource(CultivationStage.Tribulation);
 Check(!tribulationSource.Contains(Language.NpcPrefix+"MoonboneImmortal"),"Tribulation source must not require a boss unlocked by that same breakthrough");
 Check(tribulationSource.Contains(culture=="zh-Hans" ? "挖掘" : "mine"),"Tribulation source identifies the pre-boss mining route");
 var beginner=new XianXiaPlayer{cultivationStage=CultivationStage.None,maxSpiritualEnergy=40};
 Check(CultivationStatusText.Summary(beginner).EndsWith(Language.GetTextValue("Mods.XianXia.CultivationStatus.DiscoverySeek")),"Undiscovered novice sees material acquisition guidance");
 beginner.discoveredSpiritualEnergy=true;
 Check(CultivationStatusText.Summary(beginner).EndsWith(Language.GetTextValue("Mods.XianXia.CultivationStatus.DiscoveryFound",CultivationStatusText.StageName(CultivationStage.QiAwakening),3,2)),"Discovered novice sees localized awakening action and realm");
 Check(beginner.cultivationStage==CultivationStage.None&&beginner.spiritualEnergy==0&&beginner.spiritPressure==0&&beginner.discoveredSpiritualEnergy,"Reading discovery guidance grants no progression or resources");
 foreach(CultivationStage advanced in Enum.GetValues<CultivationStage>().Where(stage=>stage!=CultivationStage.None)) {
  beginner.cultivationStage=advanced;Check(CultivationStatusText.Summary(beginner).Split('\n').Length==3,"Starter guidance disappears in advanced realms");
 }
 p.tribulationTimer=int.MaxValue;Check(CultivationStatusText.Summary(p).Contains(Language.GetTextValue("Mods.XianXia.CultivationStatus.ActiveTrial",35791395)),"Large tick count no overflow");
}
Console.WriteLine($"Cultivation status source and real bilingual format regression passed: {count} assertions; player/rendering boundary mocked.");
