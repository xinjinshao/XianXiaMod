using System.Reflection;
using System.Runtime.Loader;

if (args.Length != 2) throw new ArgumentException("Expected compiled XianXia.dll and tModLoader directory.");
string modAssembly = Path.GetFullPath(args[0]);
string engineRoot = Path.GetFullPath(args[1]);
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string engineAssembly = Path.Combine(engineRoot, name.Name + ".dll");
    if (File.Exists(engineAssembly)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(engineAssembly);
    string dependency = Directory.EnumerateFiles(Path.Combine(engineRoot, "Libraries"), name.Name + ".dll", SearchOption.AllDirectories)
        .FirstOrDefault(path => !path.Contains(Path.DirectorySeparatorChar + "Native" + Path.DirectorySeparatorChar)
            && !path.Contains(Path.DirectorySeparatorChar + "runtime"));
    return dependency == null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(dependency);
};

Type type = Assembly.LoadFrom(modAssembly).GetType("XianXia.Common.Players.XianXiaPlayer", true);
object player = Activator.CreateInstance(type);
type.GetMethod("Initialize").Invoke(player, null);
FieldInfo energy = type.GetField("spiritualEnergy");
FieldInfo multiplier = type.GetField("spiritualEnergyCostMultiplier");
MethodInfo cost = type.GetMethod("GetSpiritualEnergyCost");
MethodInfo canConsume = type.GetMethod("CanConsumeSpiritualEnergy");
MethodInfo consume = type.GetMethod("TryConsumeSpiritualEnergy");
int assertions = 0;
void Check(bool ok, string description)
{
    assertions++;
    if (!ok) throw new Exception(description);
}
int Cost(int amount) => (int)cost.Invoke(player, new object[] { amount });
bool Can(int amount) => (bool)canConsume.Invoke(player, new object[] { amount });
bool Spend(int amount) => (bool)consume.Invoke(player, new object[] { amount });

multiplier.SetValue(player, 0.5f);
energy.SetValue(player, 2);
Check(Cost(4) == 2, "Reduced cost must enable attacks with less than raw base cost.");
Check(Can(4) && Can(4) && (int)energy.GetValue(player) == 2, "Repeated eligibility checks must not spend energy.");
Check(Spend(4) && (int)energy.GetValue(player) == 0, "Attack spends the effective cost once.");
Check(!Spend(4) && (int)energy.GetValue(player) == 0, "Failed spend must not mutate energy.");
Check(Cost(5) == 3, "Fractional costs round upward.");
Check(Cost(0) == 0 && Cost(-1) == 0, "Nonpositive base cost is free.");
multiplier.SetValue(player, float.NaN);
Check(Cost(4) == 4, "Invalid multiplier falls back to ordinary cost.");
multiplier.SetValue(player, float.PositiveInfinity);
Check(Cost(4) == 4, "Infinite multiplier falls back to ordinary cost.");
multiplier.SetValue(player, -1f);
Check(Cost(4) == 0, "Negative multiplier cannot produce negative spending.");
multiplier.SetValue(player, 20f);
Check(Cost(4) == 40, "Extreme multiplier is bounded.");
Console.WriteLine($"Gameplay regression passed: {assertions} assertions against compiled XianXiaPlayer and actual tModLoader assemblies. No game world was started.");

Type tagType = Assembly.LoadFrom(Path.Combine(engineRoot, "tModLoader.dll")).GetType("Terraria.ModLoader.IO.TagCompound", true);
// ModLoader normally registers serializers during loading. Bootstrap only the
// engine's actual Boolean serializer for this headless SaveData/LoadData test.
Type serializers = tagType.Assembly.GetType("Terraria.ModLoader.IO.TagSerializer", true);
object boolSerializer = Activator.CreateInstance(tagType.Assembly.GetType("Terraria.ModLoader.IO.BoolTagSerializer", true));
serializers.GetMethod("AddSerializer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { boolSerializer });
object Tag(params (string Key, object Value)[] entries)
{
    object tag = Activator.CreateInstance(tagType);
    foreach (var entry in entries)
        tagType.GetProperty("Item").SetValue(tag, entry.Value, new object[] { entry.Key });
    return tag;
}
void Load(object tag) => type.GetMethod("LoadData").Invoke(player, new[] { tag });
int IntField(string name) => Convert.ToInt32(type.GetField(name).GetValue(player));
Load(Tag());
Check(IntField("cultivationStage") == 0 && IntField("spiritualEnergy") == 0, "Missing save fields must safely default.");
Load(Tag(("cultivationStage", 999), ("spiritualEnergy", -100), ("spiritPressure", -50)));
Check(IntField("cultivationStage") == 0 && IntField("spiritualEnergy") == 0 && IntField("spiritPressure") == 0,
    "Invalid realm must reset, not grant the maximum realm.");
Load(Tag(("cultivationStage", 3), ("spiritualEnergy", 99999), ("spiritPressure", 999),
    ("tribulationTimer", 99999), ("tribulationStage", 3), ("tribulationComprehension", 999),
    ("tribulationKind", 99), ("tribulationAttempts", 999), ("arrayDeploymentCooldown", 99999),
    ("clearedTribulationStages", new List<int> { -1, 3, 3, 4, 99 })));
Check(IntField("tribulationComprehension") == 1 && IntField("maxSpiritualEnergy") == 125,
    "Permanent comprehension must derive from valid, unique completed trials.");
Check(IntField("spiritualEnergy") == 125 && IntField("spiritPressure") == 100, "Resources must be bounded by the loaded realm.");
Check(IntField("tribulationTimer") == 0 && IntField("tribulationKind") == 0, "A cleared trial cannot remain active after loading.");
Check(IntField("tribulationAttempts") == 10 && IntField("arrayDeploymentCooldown") == 480, "Attempts and cooldown must be bounded.");
Load(Tag(("cultivationStage", 3), ("tribulationTimer", 600), ("tribulationStage", 3), ("tribulationKind", 4)));
Check(IntField("tribulationTimer") == 600 && IntField("tribulationKind") == 1 && IntField("tribulationIntensity") == 2,
    "An active valid trial keeps its remaining time but uses the correct kind and strength.");
type.GetField("arrayDeploymentCooldown").SetValue(player, 123);
object saved = Tag();
type.GetMethod("SaveData").Invoke(player, new[] { saved });
type.GetMethod("Initialize").Invoke(player, null);
Load(saved);
Check(IntField("arrayDeploymentCooldown") == 123 && IntField("tribulationTimer") == 600, "Save/load preserves cooldown and valid trial progress.");
Console.WriteLine($"Gameplay and save regression passed: {assertions} assertions against actual compiled XianXiaPlayer and TagCompound.");

// Run the actual ModPlayer snapshot conversion and single-import guard.
Type snapshotType = type.Assembly.GetType("XianXia.Common.Players.CultivationSnapshot", true);
object snapshot = type.GetMethod("CaptureSnapshot").Invoke(player, null);
Check((bool)snapshotType.GetMethod("IsValid").Invoke(snapshot, null), "Sanitized save produces a valid full network snapshot.");
Check(Convert.ToInt32(snapshotType.GetProperty("Timer").GetValue(snapshot)) == 600, "Full network snapshot includes trial timer.");
type.GetMethod("Initialize").Invoke(player, null);
Check((bool)type.GetMethod("TryInitializeNetwork").Invoke(player, new[] { snapshot }), "Initial client-owned character snapshot imports once.");
Check(!(bool)type.GetMethod("TryInitializeNetwork").Invoke(player, new[] { snapshot }), "Repeated joining imports cannot overwrite runtime state.");
Check(IntField("tribulationTimer") == 600 && IntField("arrayDeploymentCooldown") == 123,
    "Actual snapshot application preserves trial and shared array cooldown.");
Check(IntField("tribulationComprehension") == 0 && IntField("maxSpiritualEnergy") == 120,
    "Snapshot derives permanent comprehension and maximum rather than trusting extra numeric fields.");
Console.WriteLine($"Gameplay/save/snapshot regression passed: {assertions} assertions against the compiled mod.");

type.GetMethod("AdvanceResourceRevision").Invoke(player, null);
Check(Convert.ToUInt32(type.GetProperty("ResourceRevision").GetValue(player)) == 1, "Authoritative effects advance the energy report revision.");
type.GetMethod("ResetNetworkSession").Invoke(player, null);
Check(!(bool)type.GetProperty("NetworkInitialized").GetValue(player), "Server slot reset releases the joining import guard.");
Check((bool)type.GetMethod("TryInitializeNetwork").Invoke(player, new[] { snapshot }), "A new session accepts its character snapshot.");
Console.WriteLine($"Gameplay/save/snapshot/session regression passed: {assertions} assertions.");

// Actual resource authority guard is independent of the engine bootstrap.
var authority = type.GetProperty("IsResourceAuthority");
authority.SetValue(player, false);
energy.SetValue(player, 20);
Check(!Spend(1) && (int)energy.GetValue(player) == 20, "Client resource spend is refused without changing its canonical balance.");
type.GetMethod("RestoreSpiritualEnergy").Invoke(player, new object[] { 10 });
Check((int)energy.GetValue(player) == 20, "Client resource restoration cannot modify its canonical balance.");
type.GetField("spiritPressure").SetValue(player, 20);
type.GetMethod("ReduceSpiritPressure").Invoke(player, new object[] { 10 });
Check(IntField("spiritPressure") == 20, "Client pressure reduction is refused.");
var recover = type.GetMethod("TryArrayRecovery");
Check(!(bool)recover.Invoke(player, new object[] { 60UL }), "Client cannot authorize array recovery.");
authority.SetValue(player, true);
Check((bool)recover.Invoke(player, new object[] { 60UL }) && !(bool)recover.Invoke(player, new object[] { 60UL }),
    "Overlapping arrays share one per-player recovery authorization each tick.");
Check((bool)recover.Invoke(player, new object[] { 120UL }), "A later recovery interval remains available.");
type.GetMethod("RestoreSpiritualEnergy").Invoke(player, new object[] { 1 });
int effective = Cost(1);
Check(Spend(1) && (int)energy.GetValue(player) == 21 - effective, "Authority restores and spends using the actual effective cost.");
Console.WriteLine($"Gameplay/save/authority regression passed: {assertions} assertions.");

// Actual GlobalItem metadata hooks use the real TagCompound and engine base class.
Type qualityType = type.Assembly.GetType("XianXia.Common.Systems.PillQualitySystem", true);
object quality = Activator.CreateInstance(qualityType);
int Grade(object instance) => Convert.ToInt32(qualityType.GetProperty("Quality").GetValue(instance));
void LoadQuality(object instance, object tag) => qualityType.GetMethod("LoadData").Invoke(instance, new[] { null, tag });
Check(Grade(quality) == 2, "Actual quality starts at Standard for legacy/non-crafted items.");
LoadQuality(quality, Tag(("quality", 4), ("crafted", true)));
object qualitySave = Tag();
qualityType.GetMethod("SaveData").Invoke(quality, new[] { null, qualitySave });
object loadedQuality = Activator.CreateInstance(qualityType);
LoadQuality(loadedQuality, qualitySave);
Check(Grade(loadedQuality) == 4 && (bool)qualityType.GetProperty("Crafted").GetValue(loadedQuality), "Real item save retains quality and crafting provenance.");
using (var data = new MemoryStream()) {
 using (var writer = new BinaryWriter(data, System.Text.Encoding.UTF8, true))
  qualityType.GetMethod("NetSend").Invoke(quality, new object[] { null, writer });
 data.Position = 0;
 using var reader = new BinaryReader(data);
 qualityType.GetMethod("NetReceive").Invoke(loadedQuality, new object[] { null, reader });
 Check(Grade(loadedQuality) == 4, "Real engine item network metadata retains Spirit quality.");
}
LoadQuality(loadedQuality, Tag(("quality", -7)));
Check(Grade(loadedQuality) == 2, "Invalid item save cannot grant high quality.");
LoadQuality(loadedQuality, Tag());
Check(Grade(loadedQuality) == 2 && !(bool)qualityType.GetProperty("Crafted").GetValue(loadedQuality), "Legacy item metadata safely defaults.");
object clonedQuality = qualityType.GetMethod("Clone", new[] { qualityType.BaseType.Assembly.GetType("Terraria.Item"), qualityType.BaseType.Assembly.GetType("Terraria.Item") }).Invoke(quality, new object[] { null, null });
Check(!ReferenceEquals(quality, clonedQuality) && Grade(clonedQuality) == 4, "Actual GlobalItem cloning retains per-item quality independently.");
LoadQuality(clonedQuality, Tag(("quality", 1)));
Check(Grade(quality) == 4 && Grade(clonedQuality) == 1, "Changing cloned item quality cannot change the source stack.");
Console.WriteLine($"Gameplay/save/quality regression passed: {assertions} assertions against compiled mod and official engine.");

// Actual per-equipment inscription persistence and GlobalItem cloning.
Type inscriptionType = type.Assembly.GetType("XianXia.Common.Items.InscribedEquipment", true);
Type inscriptionEnum = type.Assembly.GetType("XianXia.Common.Systems.InscriptionKind", true);
object inscribed = Activator.CreateInstance(inscriptionType);
int Inscription(object item) => Convert.ToInt32(inscriptionType.GetProperty("Kind").GetValue(item));
void SetInscription(object item, int kind) => inscriptionType.GetMethod("SetKind").Invoke(item, new[] { Enum.ToObject(inscriptionEnum, kind) });
void LoadInscription(object item, object tag) => inscriptionType.GetMethod("LoadData").Invoke(item, new[] { null, tag });
Check(Inscription(inscribed)==0, "Actual equipment starts uninscribed.");
SetInscription(inscribed, 2);
object inscriptionSave=Tag();
inscriptionType.GetMethod("SaveData").Invoke(inscribed,new[] {null,inscriptionSave});
object copiedInscription=Activator.CreateInstance(inscriptionType);
LoadInscription(copiedInscription,inscriptionSave);
Check(Inscription(copiedInscription)==2, "Real engine equipment save retains Furnace inscription.");
object cloneInscription=inscriptionType.GetMethod("Clone",new[] {qualityType.BaseType.Assembly.GetType("Terraria.Item"),qualityType.BaseType.Assembly.GetType("Terraria.Item")}).Invoke(inscribed,new object[] {null,null});
Check(!ReferenceEquals(cloneInscription,inscribed) && Inscription(cloneInscription)==2, "Actual engine cloning preserves inscription independently.");
SetInscription(cloneInscription,5);
Check(Inscription(inscribed)==2 && Inscription(cloneInscription)==5, "Replacing a clone's inscription cannot alter the original.");
using(var data=new MemoryStream()) {
 using(var writer=new BinaryWriter(data,System.Text.Encoding.UTF8,true)) inscriptionType.GetMethod("NetSend").Invoke(inscribed,new object[] {null,writer});
 Check(data.Length==1,"Actual inscription item payload contains one enum byte.");
 data.Position=0;using var reader=new BinaryReader(data);
 inscriptionType.GetMethod("NetReceive").Invoke(copiedInscription,new object[] {null,reader});
 Check(Inscription(copiedInscription)==2,"Real engine item network payload retains inscription.");
}
try {
 using var reader=new BinaryReader(new MemoryStream());
 inscriptionType.GetMethod("NetReceive").Invoke(copiedInscription,new object[] {null,reader});
 throw new Exception("Expected truncated metadata rejection.");
} catch(TargetInvocationException error) when(error.InnerException is EndOfStreamException) {
 Check(Inscription(copiedInscription)==2,"Truncated actual item metadata cannot partially change inscription.");
}
LoadInscription(copiedInscription,Tag(("inscription",999)));
Check(Inscription(copiedInscription)==0,"Invalid actual save cannot grant an inscription.");
LoadInscription(copiedInscription,Tag());
Check(Inscription(copiedInscription)==0,"Legacy actual saves keep equipment uninscribed.");
Console.WriteLine($"Gameplay/save/inscription regression passed: {assertions} assertions against compiled mod and official engine.");

Type refinedType=type.Assembly.GetType("XianXia.Common.Items.RefinedArtifact",true);
object refined=Activator.CreateInstance(refinedType);
int Refinement(object item)=>Convert.ToInt32(refinedType.GetProperty("Level").GetValue(item));
Check(Refinement(refined)==0,"Actual legacy artifact defaults to refinement level zero.");
refinedType.GetMethod("SetLevel").Invoke(refined,new object[]{3});
Check((bool)refinedType.GetMethod("TryAwaken").Invoke(refined,null),"Actual maximum-level artifact can complete the awakening transition.");
object refinementSave=Tag();refinedType.GetMethod("SaveData").Invoke(refined,new[]{null,refinementSave});
object refinedLoaded=Activator.CreateInstance(refinedType);
refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{null,refinementSave});
Check(Refinement(refinedLoaded)==3,"Actual item save retains maximum refinement level.");
Check((bool)refinedType.GetProperty("Awakened").GetValue(refinedLoaded),"Actual saved equipment retains crafted awakening.");
object refinedCloned=refinedType.GetMethod("Clone",new[]{qualityType.BaseType.Assembly.GetType("Terraria.Item"),qualityType.BaseType.Assembly.GetType("Terraria.Item")}).Invoke(refined,new object[]{null,null});
refinedType.GetMethod("SetLevel").Invoke(refinedCloned,new object[]{1});
Check(Refinement(refined)==3 && Refinement(refinedCloned)==1,"Actual refinement clone is independent from the source.");
using(var data=new MemoryStream()) {
 using(var writer=new BinaryWriter(data,System.Text.Encoding.UTF8,true)) refinedType.GetMethod("NetSend").Invoke(refined,new object[]{null,writer});
 data.Position=0;using var reader=new BinaryReader(data);refinedType.GetMethod("NetReceive").Invoke(refinedLoaded,new object[]{null,reader});
 Check(Refinement(refinedLoaded)==3 && data.Length==3,"Actual refinement item networking retains level, awakening and route metadata.");
}
refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{null,Tag(("refinement",999))});
Check(Refinement(refinedLoaded)==0,"Invalid real item save cannot grant excessive refinement.");
Console.WriteLine($"Gameplay/save/refinement regression passed: {assertions} assertions against compiled mod and official engine.");

// Actual compiled character state persists skill cooldown but never resumes ward protection.
type.GetField("activeSkillCooldown").SetValue(player,900);
type.GetField("wardGuardTimer").SetValue(player,180);
object skillSave=Tag();type.GetMethod("SaveData").Invoke(player,new[]{skillSave});
type.GetField("activeSkillCooldown").SetValue(player,0);
Load(skillSave);
Check(IntField("activeSkillCooldown")==900 && IntField("wardGuardTimer")==0,"Actual saves retain shared skill cooldown without resuming temporary protection.");
object skillSnapshot=type.GetMethod("CaptureSnapshot").Invoke(player,null);
Check(Convert.ToInt32(snapshotType.GetProperty("SkillCooldown").GetValue(skillSnapshot))==900,"Actual canonical snapshot includes shared skill cooldown.");
Load(Tag(("activeSkillCooldown",999999)));
Check(IntField("activeSkillCooldown")==1200,"Actual saved cooldown is bounded.");
Load(Tag());
Check(IntField("activeSkillCooldown")==0,"Legacy saves safely default to no active cooldown.");
Console.WriteLine($"Gameplay/save/skill regression passed: {assertions} assertions against compiled mod and official engine.");

Type routeType=type.Assembly.GetType("XianXia.Common.Systems.DownedBossSystem+EndgameRoute",true);
object rebuild=Enum.ToObject(routeType,1);
Check((bool)refinedType.GetMethod("TryTransform").Invoke(refined,new[]{rebuild}),"Actual awakened artifact can receive its permanent Dao route.");
Check(!(bool)refinedType.GetMethod("TryTransform").Invoke(refined,new[]{Enum.ToObject(routeType,2)}),"Actual artifact cannot replace an existing Dao route.");
int DaoRoute(object item)=>Convert.ToInt32(refinedType.GetProperty("DaoRoute").GetValue(item));
object daoSave=Tag();refinedType.GetMethod("SaveData").Invoke(refined,new[]{null,daoSave});
refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{null,daoSave});
Check(DaoRoute(refinedLoaded)==1,"Actual item save preserves Dao transformation.");
object daoClone=refinedType.GetMethod("Clone",new[]{qualityType.BaseType.Assembly.GetType("Terraria.Item"),qualityType.BaseType.Assembly.GetType("Terraria.Item")}).Invoke(refined,new object[]{null,null});
refinedType.GetMethod("SetLevel").Invoke(daoClone,new object[]{1});
Check(DaoRoute(daoClone)==0 && DaoRoute(refined)==1,"Actual clone changes cannot overwrite the source's Dao route.");
using(var data=new MemoryStream()) {
 using(var writer=new BinaryWriter(data,System.Text.Encoding.UTF8,true))refinedType.GetMethod("NetSend").Invoke(refined,new object[]{null,writer});
 data.Position=0;using var reader=new BinaryReader(data);refinedType.GetMethod("NetReceive").Invoke(refinedLoaded,new object[]{null,reader});
 Check(data.Length==3 && DaoRoute(refinedLoaded)==1,"Actual item payload preserves refinement, awakening and Dao route together.");
 for(int size=0;size<3;size++) {
  try { using var shortReader=new BinaryReader(new MemoryStream(data.ToArray()[..size]));refinedType.GetMethod("NetReceive").Invoke(refinedLoaded,new object[]{null,shortReader});throw new Exception("Expected incomplete metadata rejection"); }
  catch(TargetInvocationException error) when(error.InnerException is EndOfStreamException) { Check(DaoRoute(refinedLoaded)==1 && Refinement(refinedLoaded)==3,"Incomplete actual metadata cannot partially erase advancement."); }
 }
}
foreach(object malformed in new[]{Tag(("refinement",3),("awakened",true),("daoRoute",999)),Tag(("refinement",2),("awakened",true),("daoRoute",1)),Tag(("refinement",3),("daoRoute",1)),Tag(("refinement",3),("awakened",true))}) {
 refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{null,malformed});
 Check(DaoRoute(refinedLoaded)==0,"Invalid or legacy actual item saves cannot gain free Dao transformation.");
}
Console.WriteLine($"Gameplay/save/Dao regression passed: {assertions} assertions against compiled mod and official engine.");

// Verify actual projectile defaults against official engine types, not a stub.
var enemyBoltType=type.Assembly.GetType("XianXia.Content.Projectiles.EnemySpiritBoltProjectile",true);
object bolt=Activator.CreateInstance(enemyBoltType);
var projectileType=tagType.Assembly.GetType("Terraria.Projectile",true);
object nativeProjectile=Activator.CreateInstance(projectileType);
var entityProperty=enemyBoltType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
entityProperty.SetValue(bolt,nativeProjectile);
enemyBoltType.GetMethod("SetDefaults").Invoke(bolt,null);
Check((bool)projectileType.GetField("hostile").GetValue(nativeProjectile)&&!(bool)projectileType.GetField("friendly").GetValue(nativeProjectile),"Early enemy bolt is hostile and never friendly");
Check((bool)projectileType.GetField("tileCollide").GetValue(nativeProjectile)&&(int)projectileType.GetField("timeLeft").GetValue(nativeProjectile)==180,"Early enemy bolt retains terrain collision and lifetime");
Console.WriteLine($"Actual engine gameplay assertions including hostile bolt: {assertions}.");

// Execute the real reward against the engine's native extra-jump state.
var bottleType=type.Assembly.GetType("XianXia.Content.Items.HandGenerated.TribulationCloudBottle",true);
object bottle=Activator.CreateInstance(bottleType);
var itemType=tagType.Assembly.GetType("Terraria.Item",true);
object nativeBottle=Activator.CreateInstance(itemType);
bottleType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(bottle,nativeBottle);
bottleType.GetMethod("SetDefaults").Invoke(bottle,null);
Check((bool)itemType.GetField("accessory").GetValue(nativeBottle)&&!(bool)itemType.GetField("vanity").GetValue(nativeBottle),"Cloud bottle is a functional accessory");
var nativePlayerType=tagType.Assembly.GetType("Terraria.Player",true);
object bottlePlayer=Activator.CreateInstance(nativePlayerType);
var jumpType=tagType.Assembly.GetType("Terraria.ModLoader.ExtraJump",true);
object cloudJump=jumpType.GetProperty("CloudInABottle").GetValue(null);
var getJump=nativePlayerType.GetMethods().Single(m=>m.Name=="GetJumpState"&&m.GetParameters().Length==1).MakeGenericMethod(cloudJump.GetType());
bottleType.GetMethod("UpdateAccessory").Invoke(bottle,new[]{bottlePlayer,(object)false});
object jumpState=getJump.Invoke(bottlePlayer,new[]{cloudJump});
Check((bool)jumpState.GetType().GetProperty("Enabled").GetValue(jumpState),"Actual reward enables native cloud extra jump");
int electrified=(int)tagType.Assembly.GetType("Terraria.ID.BuffID",true).GetField("Electrified").GetRawConstantValue();
Check(((bool[])nativePlayerType.GetField("buffImmune").GetValue(bottlePlayer))[electrified],"Actual reward grants Electrified immunity");
bottleType.GetMethod("UpdateAccessory").Invoke(bottle,new[]{bottlePlayer,(object)true});
Check((bool)getJump.Invoke(bottlePlayer,new[]{cloudJump}).GetType().GetProperty("Enabled").GetValue(getJump.Invoke(bottlePlayer,new[]{cloudJump})),"Hiding accessory visuals retains the native jump");
Console.WriteLine($"Actual engine gameplay assertions including rare reward: {assertions}.");

// Test each reward's real native stats in an isolated process. Registration itself
// is exercised by normal mod loading; this harness supplies existing array slots.
var flightBase=type.Assembly.GetType("XianXia.Content.Items.HandGenerated.FlightReward",true);
var statsType=tagType.Assembly.GetType("Terraria.ID.ArmorIDs+Wing+Sets",true);
var statsField=statsType.GetField("Stats");
// The standalone assembly harness does not run the engine set factory.
var wingStats=Array.CreateInstance(statsField.FieldType.GetElementType(),3);
statsField.SetValue(null,wingStats);
int testSlot=1;
foreach(var reward in new[]{("ThunderMarshJiaoWing",120,6f,1.5f),("MoonboneImmortalWingAccessory",180,9f,2.5f)}) {
 var rewardType=type.Assembly.GetType("XianXia.Content.Items.HandGenerated."+reward.Item1,true);
 object wings=Activator.CreateInstance(rewardType);
 object wingItem=Activator.CreateInstance(itemType);
 rewardType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(wings,wingItem);
 flightBase.GetField("wingSlot",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(wings,testSlot);
 rewardType.GetMethod("SetStaticDefaults").Invoke(wings,null);
 rewardType.GetMethod("SetDefaults").Invoke(wings,null);
 Check((bool)itemType.GetField("accessory").GetValue(wingItem)&&!(bool)itemType.GetField("vanity").GetValue(wingItem),reward.Item1+" is a functional accessory");
 Check((int)itemType.GetField("wingSlot").GetValue(wingItem)==testSlot,reward.Item1+" uses its registered equip slot");
 object stats=wingStats.GetValue(testSlot);
 Check((int)stats.GetType().GetField("FlyTime").GetValue(stats)==reward.Item2,reward.Item1+" has stage-specific flight time");
 Check((float)stats.GetType().GetField("AccRunSpeedOverride").GetValue(stats)==reward.Item3&&(float)stats.GetType().GetField("AccRunAccelerationMult").GetValue(stats)==reward.Item4,reward.Item1+" has stage-specific speed and acceleration");
 object[] vertical={null,0f,0f,0f,0f,0f};
 rewardType.GetMethod("VerticalWingSpeeds").Invoke(wings,vertical);
 Check((float)vertical[1]==0.85f&&(float)vertical[5]==0.135f,reward.Item1+" supplies native ascent parameters");
 testSlot++;
}
Console.WriteLine($"Actual engine gameplay assertions including flight rewards: {assertions}.");

// Real vanity defaults and robe matching, with explicitly injected test slots.
var maskBase=type.Assembly.GetType("XianXia.Content.Items.HandGenerated.BossMaskReward",true);
foreach(string name in new[]{"GardenWardenMask","InspectorMask","FormlessSwordSoulCostume"}) {
 var vanityType=type.Assembly.GetType("XianXia.Content.Items.HandGenerated."+name,true);
 object vanity=Activator.CreateInstance(vanityType);
 object nativeVanity=Activator.CreateInstance(itemType);
 vanityType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(vanity,nativeVanity);
 bool costume=name=="FormlessSwordSoulCostume";
 var slotOwner=costume?vanityType:maskBase;
 slotOwner.GetField(costume?"bodySlot":"headSlot",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(vanity,300);
 if(costume) vanityType.GetField("robeSlot",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(vanity,301);
 vanityType.GetMethod("SetDefaults").Invoke(vanity,null);
 Check((bool)itemType.GetField("vanity").GetValue(nativeVanity)&&!(bool)itemType.GetField("accessory").GetValue(nativeVanity),name+" occupies an armor vanity slot");
 Check((int)itemType.GetField(costume?"bodySlot":"headSlot").GetValue(nativeVanity)==300,name+" retains registered equip slot");
 Check((int)itemType.GetField("defense").GetValue(nativeVanity)==0,name+" grants no unintended defense");
 if(costume)foreach(bool male in new[]{true,false}) {
  object[] match={male,5,false};
  vanityType.GetMethod("SetMatch").Invoke(vanity,match);
  Check((int)match[1]==301&&(bool)match[2],"Sword soul robe matches lower-body frames for either character style");
 }
}
Console.WriteLine($"Actual engine gameplay assertions including vanity rewards: {assertions}.");

var summonClass=tagType.Assembly.GetType("Terraria.ModLoader.DamageClass",true).GetProperty("Summon").GetValue(null);
foreach(string spiritName in new[]{"FurnaceAshSpirit","StarAbyssSpirit","ContractSpiritBolt","NascentSoulSpirit","CelestialPuppetSpirit","ArchivedSoulSpirit"}){
 var spiritType=type.Assembly.GetType("XianXia.Content.Projectiles."+spiritName,true);
 object actualSpirit=Activator.CreateInstance(spiritType);object nativeSpirit=Activator.CreateInstance(projectileType);
 spiritType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualSpirit,nativeSpirit);
 spiritType.GetMethod("SetDefaults").Invoke(actualSpirit,null);
 Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeSpirit),summonClass),spiritName+" uses actual engine summon damage");
 if(spiritName=="ContractSpiritBolt")Check((bool)projectileType.GetField("friendly").GetValue(nativeSpirit)&&(bool)projectileType.GetField("tileCollide").GetValue(nativeSpirit),"Actual contract shot collides with terrain and damages enemies");
 else Check((bool)projectileType.GetField("minion").GetValue(nativeSpirit)&&(float)projectileType.GetField("minionSlots").GetValue(nativeSpirit)==(spiritName=="ArchivedSoulSpirit"?2f:1f),"Actual contract spirit occupies its configured native minion slots");
}
Console.WriteLine($"Actual engine gameplay assertions including contract spirits: {assertions}.");

var codexType=type.Assembly.GetType("XianXia.Content.Items.Weapons.ArchiveStarCodex",true);
object actualCodex=Activator.CreateInstance(codexType);object nativeCodex=Activator.CreateInstance(itemType);
codexType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualCodex,nativeCodex);
codexType.GetMethod("SetDefaults").Invoke(actualCodex,null);
var magicClass=tagType.Assembly.GetType("Terraria.ModLoader.DamageClass",true).GetProperty("Magic").GetValue(null);
Check(ReferenceEquals(itemType.GetProperty("DamageType").GetValue(nativeCodex),magicClass)&&(int)itemType.GetField("mana").GetValue(nativeCodex)==0,"Real codex uses magic damage without mana cost");
Check((int)codexType.GetMethod("GetSpiritCost").Invoke(actualCodex,new object[]{null})==36,"Real codex declares exactly 36 base spirit cost");
Check((int)itemType.GetField("damage").GetValue(nativeCodex)==230&&(int)itemType.GetField("useTime").GetValue(nativeCodex)==48,"Real codex has final-stage base damage and timing");
var orbType=type.Assembly.GetType("XianXia.Content.Projectiles.ArchiveStarOrb",true);object actualOrb=Activator.CreateInstance(orbType);object nativeOrb=Activator.CreateInstance(projectileType);
orbType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualOrb,nativeOrb);orbType.GetMethod("SetDefaults").Invoke(actualOrb,null);
Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeOrb),magicClass)&&(bool)projectileType.GetField("tileCollide").GetValue(nativeOrb)&&(int)projectileType.GetField("penetrate").GetValue(nativeOrb)==4,"Real orb has magic collision and bounded penetration defaults");
Check((bool)orbType.GetMethod("CanDamage").Invoke(actualOrb,null)==false,"Real orb denies damage before charge");
((float[])projectileType.GetField("ai").GetValue(nativeOrb))[0]=18;
Check(orbType.GetMethod("CanDamage").Invoke(actualOrb,null)==null,"Real orb defers to native damage rules after charge");
Console.WriteLine($"Actual engine gameplay assertions including final magic codex: {assertions}.");

var rangedClass=tagType.Assembly.GetType("Terraria.ModLoader.DamageClass",true).GetProperty("Ranged").GetValue(null);
string[] rangedNames={"SectMechanismCrossbow","HeavenLawArbalest","StarCalamityMechanismCase"};int[] rangedCosts={20,28,36},rangedDamage={110,170,260},rangedTime={26,36,44};
for(int n=0;n<3;n++){
 var rangedType=type.Assembly.GetType("XianXia.Content.Items.Weapons."+rangedNames[n],true);object actualRanged=Activator.CreateInstance(rangedType),nativeRanged=Activator.CreateInstance(itemType);
 rangedType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualRanged,nativeRanged);rangedType.GetMethod("SetDefaults").Invoke(actualRanged,null);
 Check(ReferenceEquals(itemType.GetProperty("DamageType").GetValue(nativeRanged),rangedClass),"Actual mechanism weapon uses native ranged class");
 Check((int)rangedType.GetMethod("GetSpiritCost").Invoke(actualRanged,new object[]{null})==rangedCosts[n],"Actual ranged base energy cost");
 Check((int)itemType.GetField("damage").GetValue(nativeRanged)==rangedDamage[n]&&(int)itemType.GetField("useTime").GetValue(nativeRanged)==rangedTime[n],"Actual ranged damage and timing");
}
string[] mechanismNames={"SectMechanismBolt","HeavenLawBolt","StarCalamityMechanismBolt"};int[] mechanismHits={3,2,4};
for(int n=0;n<3;n++){
 var mechanismType=type.Assembly.GetType("XianXia.Content.Projectiles."+mechanismNames[n],true);object actualMechanism=Activator.CreateInstance(mechanismType),nativeMechanism=Activator.CreateInstance(projectileType);
 mechanismType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualMechanism,nativeMechanism);mechanismType.GetMethod("SetDefaults").Invoke(actualMechanism,null);
 Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeMechanism),rangedClass)&&(bool)projectileType.GetField("tileCollide").GetValue(nativeMechanism)&&(int)projectileType.GetField("penetrate").GetValue(nativeMechanism)==mechanismHits[n],"Actual mechanism projectile collision and penetration");
}
Console.WriteLine($"Actual engine gameplay assertions including ranged progression: {assertions}.");

var medicineType=type.Assembly.GetType("XianXia.Content.Items.Weapons.GreenwoodMedicineCauldron",true);object actualMedicine=Activator.CreateInstance(medicineType),nativeMedicine=Activator.CreateInstance(itemType);
medicineType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualMedicine,nativeMedicine);medicineType.GetMethod("SetDefaults").Invoke(actualMedicine,null);
Check(ReferenceEquals(itemType.GetProperty("DamageType").GetValue(nativeMedicine),magicClass)&&(int)itemType.GetField("mana").GetValue(nativeMedicine)==0,"Actual cauldron uses magic without mana cost");
Check((bool)medicineType.GetProperty("DeploysArray").GetValue(actualMedicine)&&(int)medicineType.GetMethod("GetSpiritCost").Invoke(actualMedicine,new object[]{null})==32,"Actual cauldron uses shared array transaction and 32 energy");
Check((int)itemType.GetField("damage").GetValue(nativeMedicine)==82&&(int)itemType.GetField("useTime").GetValue(nativeMedicine)==40&&(float)itemType.GetField("shootSpeed").GetValue(nativeMedicine)==0f,"Actual cauldron damage timing and stationary placement");
foreach(string medicineName in new[]{"MedicineCauldronField","MedicineSpiritBolt"}){
 var medicineProjectileType=type.Assembly.GetType("XianXia.Content.Projectiles."+medicineName,true);object actualMedicineProjectile=Activator.CreateInstance(medicineProjectileType),nativeMedicineProjectile=Activator.CreateInstance(projectileType);
 medicineProjectileType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualMedicineProjectile,nativeMedicineProjectile);medicineProjectileType.GetMethod("SetDefaults").Invoke(actualMedicineProjectile,null);
 Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeMedicineProjectile),magicClass),"Actual medicine field and spirit preserve magic class");
 bool field=medicineName=="MedicineCauldronField";
 Check((int)projectileType.GetField("timeLeft").GetValue(nativeMedicineProjectile)==(field?300:90)&&(bool)projectileType.GetField("tileCollide").GetValue(nativeMedicineProjectile)==!field,"Actual medicine lifetime and terrain defaults");
 if(field)Check((bool)medicineProjectileType.GetMethod("CanDamage").Invoke(actualMedicineProjectile,null)==false,"Actual cauldron field has no contact damage");
}
Console.WriteLine($"Actual engine gameplay assertions including medicine cauldron: {assertions}.");

var hammerType=type.Assembly.GetType("XianXia.Content.Items.Weapons.BlackFurnaceWarhammer",true);object actualHammer=Activator.CreateInstance(hammerType),nativeHammer=Activator.CreateInstance(itemType);
hammerType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualHammer,nativeHammer);hammerType.GetMethod("SetDefaults").Invoke(actualHammer,null);
var meleeClass=tagType.Assembly.GetType("Terraria.ModLoader.DamageClass",true).GetProperty("Melee").GetValue(null);
Check(ReferenceEquals(itemType.GetProperty("DamageType").GetValue(nativeHammer),meleeClass),"Actual furnace hammer uses native melee class");
Check((int)hammerType.GetMethod("GetSpiritCost").Invoke(actualHammer,new object[]{null})==12,"Actual hammer declares 12 energy cost");
Check((int)itemType.GetField("damage").GetValue(nativeHammer)==54&&(int)itemType.GetField("useTime").GetValue(nativeHammer)==44&&(float)itemType.GetField("knockBack").GetValue(nativeHammer)==8f,"Actual hammer heavy damage timing and knockback");
foreach(string furnaceName in new[]{"FurnaceHammerProjectile","FurnaceImpactBurst"}){
 var furnaceType=type.Assembly.GetType("XianXia.Content.Projectiles."+furnaceName,true);object actualFurnace=Activator.CreateInstance(furnaceType),nativeFurnace=Activator.CreateInstance(projectileType);
 furnaceType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualFurnace,nativeFurnace);furnaceType.GetMethod("SetDefaults").Invoke(actualFurnace,null);
 bool burst=furnaceName=="FurnaceImpactBurst";
 Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeFurnace),meleeClass),"Actual hammer and impact preserve melee class");
 Check((int)projectileType.GetField("timeLeft").GetValue(nativeFurnace)==(burst?10:90)&&(bool)projectileType.GetField("tileCollide").GetValue(nativeFurnace)==!burst,"Actual furnace projectile lifetime and terrain defaults");
 if(burst){
  Check((bool)projectileType.GetField("usesLocalNPCImmunity").GetValue(nativeFurnace)&&(int)projectileType.GetField("localNPCHitCooldown").GetValue(nativeFurnace)==-1,"Actual impact has native one-hit local immunity");
  Check(furnaceType.GetMethod("CanDamage").Invoke(actualFurnace,null)==null,"Actual impact initial damage window");
  projectileType.GetField("timeLeft").SetValue(nativeFurnace,8);Check((bool)furnaceType.GetMethod("CanDamage").Invoke(actualFurnace,null)==false,"Actual impact visual tail cannot damage");
 }
}
Console.WriteLine($"Actual engine gameplay assertions including furnace hammer: {assertions}.");
