using System.Text.Json;
using Terraria.ModLoader;
using Terraria.Localization;
using XianXia.Common.Systems;
int count=0;void Check(bool ok,string text){count++;if(!ok)throw new Exception(text);}
string repo=args[0];var system=new BossChecklistIntegrationSystem();
system.PostSetupContent();Check(BossChecklistIntegrationSystem.RegisteredBossCount==0,"Missing optional mod is supported");
ModLoader.Checklist=new Mod();ModContent.Config.EnableSoftCompatibilityHooks=false;system.PostSetupContent();Check(ModLoader.Checklist.Calls.Count==0,"Disabled compatibility does not call external mod");
ModContent.Config.EnableSoftCompatibilityHooks=true;ModLoader.Checklist.Version=new Version(1,5);system.PostSetupContent();Check(ModLoader.Checklist.Calls.Count==0&&system.Mod.Logger.Warnings.Count==1,"Old version skips API with warning");
var expected=new (string Boss,string Id,string Summon,float Order,string Extra)[]{
 ("SpiritVeinWyrm","spirit_vein_wyrm","SpiritVeinIncense",.8f,"SpiritVeinWyrmTrophy"),
 ("GardenWarden","garden_warden","SummonGardenBrokenKey",4.2f,"GardenWardenMask"),
 ("BlackFurnaceIronGolem","black_furnace_iron_golem","SummonOldFurnaceEmber",4.4f,"BlackFurnaceIronGolemPet"),
 ("TribulationCloudAvatar","tribulation_cloud_avatar","SummonThunderCallingJade",6.5f,""),
 ("ThunderMarshJiao","thunder_marsh_jiao","SummonThunderCallingJadeThunderMarshJiao",8.2f,""),
 ("AbyssalStarWomb","abyssal_star_womb","SummonStarAbyssMembrane",8.4f,"AbyssalStarWombLamp"),
 ("FormlessSwordSoul","formless_sword_soul","SummonSectTrialToken",12.2f,"FormlessSwordSoulCostume"),
 ("GreenwoodMedicineKingEcho","greenwood_medicine_king_echo","SummonSectTrialTokenGreenwoodMedicineKingEcho",12.4f,""),
 ("HeavenTabletGuardian","heaven_tablet_guardian","SummonHeavenTabletRubbing",13.1f,"SmallTabletPet"),
 ("BrokenHeavenInspector","broken_heaven_inspector","SummonHeavenTabletRubbingBrokenHeavenInspector",13.2f,"InspectorMask"),
 ("MoonboneImmortal","moonbone_immortal","SummonMoonboneRitualTalisman",18.2f,""),
 ("OldHeavenDaoCore","old_heaven_dao_core","SummonMoonboneRitualTalismanOldHeavenDaoCore",18.4f,"")};
foreach(string lang in new[]{"en-US","zh-Hans"}) {
 using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(repo,"Localization/boss-checklist",lang+".hjson")));
 var texts=json.RootElement.GetProperty("Mods").GetProperty("XianXia").GetProperty("BossChecklistIntegration");
 foreach(var entry in texts.EnumerateObject())Language.Values["Mods.XianXia.BossChecklistIntegration."+entry.Name+".SpawnInfo"]=entry.Value.GetProperty("SpawnInfo").GetString();
 ModLoader.Checklist=new Mod();system.PostSetupContent();Check(ModLoader.Checklist.Calls.Count==12&&BossChecklistIntegrationSystem.RegisteredBossCount==12,"All twelve actual registrations accepted");
 for(int i=0;i<12;i++) {
  var call=ModLoader.Checklist.Calls[i];var entry=expected[i];
  Check(call.Length==7&&(string)call[0]=="LogBoss"&&ReferenceEquals(call[1],system.Mod),"Native seven-argument LogBoss layout");
  Check((string)call[2]==entry.Boss&&(float)call[3]==entry.Order&&ModContent.Name((int)call[5])==entry.Boss,"Correct stable name, order and main NPC");
  var data=(Dictionary<string,object>)call[6];Check(ModContent.Name((int)data["spawnItems"])==entry.Summon,"Summon item links to its actual boss");
  var text=(LocalizedText)data["spawnInfo"];Check(text.Value!=text.Key&&text.Value.Contains("[i:XianXia/"+entry.Summon+"]"),"Both real languages contain source item and resolved hint");
  var collectibles=(List<int>)data["collectibles"];Check(ModContent.Name(collectibles[0])==entry.Boss+"Monument"&&collectibles.Count==(entry.Extra.Length==0?1:2),"Monument and exact cosmetic extras");
  if(entry.Extra.Length>0)Check(ModContent.Name(collectibles[1])==entry.Extra,"Correct boss cosmetic reward");
  var downed=(Func<bool>)call[4];DownedBossSystem.DownedBosses.Clear();Check(!downed(),"Fresh world downed callback false");DownedBossSystem.DownedBosses.Add(entry.Id);Check(downed(),"Live world changes reflected without re-registering");DownedBossSystem.DownedBosses.Clear();Check(!downed(),"World reset reflected without stale snapshot");
  string npcSource=File.ReadAllText(Path.Combine(repo,"Content/NPCs/Bosses",entry.Boss+".cs"));Check(npcSource.Contains("MarkDowned(\""+entry.Id+"\")"),"Callback ID matches authoritative production OnKill");
  string summonSource=File.ReadAllText(Path.Combine(repo,"Content/Items/BossSummons",entry.Summon+".cs"));Check(summonSource.Contains("NPCType<"+entry.Boss+">()")||summonSource.Contains("Bosses."+entry.Boss+">()"),"Registered source item targets correct native NPC");
 }
}
foreach(bool throws in new[]{false,true}){ModLoader.Checklist=new Mod{RejectFirst=!throws,ThrowFirst=throws};system.PostSetupContent();Check(ModLoader.Checklist.Calls.Count==12&&BossChecklistIntegrationSystem.RegisteredBossCount==11,"External rejection or exception isolated, subsequent entries still attempted");}
system.Unload();Check(BossChecklistIntegrationSystem.RegisteredBossCount==0,"Unload clears registration state");
Check(File.ReadAllText(Path.Combine(repo,"build.txt")).Contains("softReferences = BossChecklist"),"Native optional dependency metadata");
Console.WriteLine($"Actual Boss Checklist integration hooks passed: {count} assertions; external Mod.Call mocked, real bilingual hints and OnKill/source-item mappings checked.");
