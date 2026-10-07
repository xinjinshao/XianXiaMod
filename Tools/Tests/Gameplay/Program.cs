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

// Native metadata hooks receive their owning item; bind a real sample instead of null.
var refinementItemType=tagType.Assembly.GetType("Terraria.Item",true);
object refinementOwner=Activator.CreateInstance(refinementItemType);
var refinementSampleType=type.Assembly.GetType("XianXia.Content.Items.Weapons.CloudpiercerFlyingSword",true);
object refinementSample=Activator.CreateInstance(refinementSampleType);
refinementSampleType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(refinementSample,refinementOwner);
refinementSampleType.GetProperty("Mod",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(refinementSample,Activator.CreateInstance(type.Assembly.GetType("XianXia.XianXia",true)));
refinementItemType.GetProperty("ModItem",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(refinementOwner,refinementSample);
foreach(var pair in new[]{("type",1),("stack",1),("maxStack",1),("damage",28)})refinementItemType.GetField(pair.Item1).SetValue(refinementOwner,pair.Item2);
Type refinedType=type.Assembly.GetType("XianXia.Common.Items.RefinedArtifact",true);
object refined=Activator.CreateInstance(refinedType);
int Refinement(object item)=>Convert.ToInt32(refinedType.GetProperty("Level").GetValue(item));
Check(Refinement(refined)==0,"Actual legacy artifact defaults to refinement level zero.");
refinedType.GetMethod("SetLevel").Invoke(refined,new object[]{3});
Check((bool)refinedType.GetMethod("TryAwaken").Invoke(refined,null),"Actual maximum-level artifact can complete the awakening transition.");
object refinementSave=Tag();refinedType.GetMethod("SaveData").Invoke(refined,new[]{refinementOwner,refinementSave});
object refinedLoaded=Activator.CreateInstance(refinedType);
refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{refinementOwner,refinementSave});
Check(Refinement(refinedLoaded)==3,"Actual item save retains maximum refinement level.");
Check((bool)refinedType.GetProperty("Awakened").GetValue(refinedLoaded),"Actual saved equipment retains crafted awakening.");
object refinedCloned=refinedType.GetMethod("Clone",new[]{qualityType.BaseType.Assembly.GetType("Terraria.Item"),qualityType.BaseType.Assembly.GetType("Terraria.Item")}).Invoke(refined,new object[]{null,null});
refinedType.GetMethod("SetLevel").Invoke(refinedCloned,new object[]{1});
Check(Refinement(refined)==3 && Refinement(refinedCloned)==1,"Actual refinement clone is independent from the source.");
using(var data=new MemoryStream()) {
 using(var writer=new BinaryWriter(data,System.Text.Encoding.UTF8,true)) refinedType.GetMethod("NetSend").Invoke(refined,new object[]{refinementOwner,writer});
 data.Position=0;using var reader=new BinaryReader(data);refinedType.GetMethod("NetReceive").Invoke(refinedLoaded,new object[]{refinementOwner,reader});
 Check(Refinement(refinedLoaded)==3 && data.Length==3,"Actual refinement item networking retains level, awakening and route metadata.");
}
refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{refinementOwner,Tag(("refinement",999))});
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
object daoSave=Tag();refinedType.GetMethod("SaveData").Invoke(refined,new[]{refinementOwner,daoSave});
refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{refinementOwner,daoSave});
Check(DaoRoute(refinedLoaded)==1,"Actual item save preserves Dao transformation.");
object daoClone=refinedType.GetMethod("Clone",new[]{qualityType.BaseType.Assembly.GetType("Terraria.Item"),qualityType.BaseType.Assembly.GetType("Terraria.Item")}).Invoke(refined,new object[]{null,null});
refinedType.GetMethod("SetLevel").Invoke(daoClone,new object[]{1});
Check(DaoRoute(daoClone)==0 && DaoRoute(refined)==1,"Actual clone changes cannot overwrite the source's Dao route.");
using(var data=new MemoryStream()) {
 using(var writer=new BinaryWriter(data,System.Text.Encoding.UTF8,true))refinedType.GetMethod("NetSend").Invoke(refined,new object[]{refinementOwner,writer});
 data.Position=0;using var reader=new BinaryReader(data);refinedType.GetMethod("NetReceive").Invoke(refinedLoaded,new object[]{refinementOwner,reader});
 Check(data.Length==3 && DaoRoute(refinedLoaded)==1,"Actual item payload preserves refinement, awakening and Dao route together.");
 for(int size=0;size<3;size++) {
  try { using var shortReader=new BinaryReader(new MemoryStream(data.ToArray()[..size]));refinedType.GetMethod("NetReceive").Invoke(refinedLoaded,new object[]{refinementOwner,shortReader});throw new Exception("Expected incomplete metadata rejection"); }
  catch(TargetInvocationException error) when(error.InnerException is EndOfStreamException) { Check(DaoRoute(refinedLoaded)==1 && Refinement(refinedLoaded)==3,"Incomplete actual metadata cannot partially erase advancement."); }
 }
}
foreach(object malformed in new[]{Tag(("refinement",3),("awakened",true),("daoRoute",999)),Tag(("refinement",2),("awakened",true),("daoRoute",1)),Tag(("refinement",3),("daoRoute",1)),Tag(("refinement",3),("awakened",true))}) {
 refinedType.GetMethod("LoadData").Invoke(refinedLoaded,new[]{refinementOwner,malformed});
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

var ammoIdType=tagType.Assembly.GetType("Terraria.ID.AmmoID",true);int nativeArrowAmmo=(int)ammoIdType.GetField("Arrow").GetValue(null);
foreach(string ammoName in new[]{"TalismanCrossbow","CinnabarTalismanArrow"}){
 var ammoType=type.Assembly.GetType("XianXia.Content.Items.Weapons."+ammoName,true);object actualAmmo=Activator.CreateInstance(ammoType),nativeAmmo=Activator.CreateInstance(itemType);
 ammoType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualAmmo,nativeAmmo);ammoType.GetMethod("SetDefaults").Invoke(actualAmmo,null);
 bool ammunition=ammoName=="CinnabarTalismanArrow";
 Check(ReferenceEquals(itemType.GetProperty("DamageType").GetValue(nativeAmmo),rangedClass),"Actual native bow/ammunition uses ranged damage");
 Check((int)itemType.GetField(ammunition?"ammo":"useAmmo").GetValue(nativeAmmo)==nativeArrowAmmo,"Actual bow and ammo match official engine arrow ID");
 Check((int)itemType.GetField("damage").GetValue(nativeAmmo)==(ammunition?8:32),"Actual native bow/ammunition base damage");
 if(ammunition)Check((bool)itemType.GetField("consumable").GetValue(nativeAmmo)&&(int)itemType.GetField("maxStack").GetValue(nativeAmmo)==9999,"Actual cinnabar arrows are consumable native stacks");
 else Check((int)itemType.GetField("useTime").GetValue(nativeAmmo)==28&&(bool)itemType.GetField("autoReuse").GetValue(nativeAmmo),"Actual crossbow native timing and repeat use");
}
var cinnabarType=type.Assembly.GetType("XianXia.Content.Projectiles.CinnabarArrowProjectile",true);object actualCinnabar=Activator.CreateInstance(cinnabarType),nativeCinnabar=Activator.CreateInstance(projectileType);
cinnabarType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualCinnabar,nativeCinnabar);cinnabarType.GetMethod("SetDefaults").Invoke(actualCinnabar,null);
Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeCinnabar),rangedClass)&&(bool)projectileType.GetField("arrow").GetValue(nativeCinnabar),"Actual cinnabar projectile is a native ranged arrow");
Check((int)projectileType.GetField("penetrate").GetValue(nativeCinnabar)==2&&(bool)projectileType.GetField("tileCollide").GetValue(nativeCinnabar)&&(int)projectileType.GetField("timeLeft").GetValue(nativeCinnabar)==180,"Actual arrow penetration collision and lifetime");
Check((bool)projectileType.GetField("usesLocalNPCImmunity").GetValue(nativeCinnabar)&&(int)projectileType.GetField("localNPCHitCooldown").GetValue(nativeCinnabar)==-1,"Actual arrow uses permanent local NPC immunity");
Console.WriteLine($"Actual engine gameplay assertions including talisman bow/ammo: {assertions}.");

var sealType=type.Assembly.GetType("XianXia.Content.Items.Weapons.HeavenTabletWardSeal",true);object actualSeal=Activator.CreateInstance(sealType),nativeSeal=Activator.CreateInstance(itemType);
sealType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualSeal,nativeSeal);sealType.GetMethod("SetDefaults").Invoke(actualSeal,null);
Check(ReferenceEquals(itemType.GetProperty("DamageType").GetValue(nativeSeal),meleeClass),"Actual tablet ward uses native melee class");
Check((int)sealType.GetMethod("GetSpiritCost").Invoke(actualSeal,new object[]{null})==28,"Actual tablet ward declares 28 energy cost");
Check((int)itemType.GetField("damage").GetValue(nativeSeal)==156&&(int)itemType.GetField("useTime").GetValue(nativeSeal)==48&&(float)itemType.GetField("knockBack").GetValue(nativeSeal)==7f,"Actual ward damage timing and knockback");
var wardType=type.Assembly.GetType("XianXia.Content.Projectiles.HeavenTabletWardProjectile",true);object actualWard=Activator.CreateInstance(wardType),nativeWard=Activator.CreateInstance(projectileType);
wardType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualWard,nativeWard);wardType.GetMethod("SetDefaults").Invoke(actualWard,null);
Check(ReferenceEquals(projectileType.GetProperty("DamageType").GetValue(nativeWard),meleeClass)&&(int)projectileType.GetField("penetrate").GetValue(nativeWard)==3&&(int)projectileType.GetField("timeLeft").GetValue(nativeWard)==120,"Actual ward melee penetration and lifetime");
Check((bool)projectileType.GetField("tileCollide").GetValue(nativeWard)&&(bool)projectileType.GetField("usesLocalNPCImmunity").GetValue(nativeWard)&&(int)projectileType.GetField("localNPCHitCooldown").GetValue(nativeWard)==20,"Actual ward native collision and local immunity");
Check((int)projectileType.GetField("width").GetValue(nativeWard)==40&&(int)projectileType.GetField("height").GetValue(nativeWard)==40,"Actual ward declares a forty-pixel collision box");
Check((int)type.Assembly.GetType("XianXia.Common.Players.HeavenTabletWardPlayer",true).GetField("DefenseBonus").GetRawConstantValue()==6,"Compiled ward declares a six-point temporary defense bonus");
Console.WriteLine($"Actual engine gameplay assertions including tablet ward: {assertions}.");

foreach(string persistentFieldName in new[]{"GreenwoodArrayField","ThunderTalismanArray","MedicineCauldronField"}){
 var persistentType=type.Assembly.GetType("XianXia.Content.Projectiles."+persistentFieldName,true);object actualPersistent=Activator.CreateInstance(persistentType),nativePersistent=Activator.CreateInstance(projectileType);
 persistentType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualPersistent,nativePersistent);persistentType.GetMethod("SetDefaults").Invoke(actualPersistent,null);
 Check((bool)projectileType.GetField("netImportant").GetValue(nativePersistent),"Actual persistent magic field opts into official native late-join synchronization");
}
Console.WriteLine($"Actual engine gameplay assertions including persistent field sync: {assertions}.");

// Execute the eligibility and metadata hooks with actual new weapon defaults and native Item instances.
foreach(string weaponName in new[]{"WoodgrainFlyingSword","SpiritwoodCrossbow","TalismanCrossbow","StarEclipseArbalest","SectMechanismCrossbow","HeavenLawArbalest","StarCalamityMechanismCase","CinnabarTalismanFlameItem","BlackFurnaceWarhammer","ThunderPatternSwordCase","ThunderTalismanArrayPlate","FormlessSwordWheel","GreenwoodMedicineCauldron","MoonboneDharmaSword","BrokenHeavenDecree","ArchiveStarCodex"}){
 var growthWeaponType=type.Assembly.GetType("XianXia.Content.Items.Weapons."+weaponName,true);
 object growthWeapon=Activator.CreateInstance(growthWeaponType),growthOwner=Activator.CreateInstance(refinementItemType);
 growthWeaponType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(growthWeapon,growthOwner);
 growthWeaponType.GetProperty("Mod",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(growthWeapon,Activator.CreateInstance(type.Assembly.GetType("XianXia.XianXia",true)));
 refinementItemType.GetProperty("ModItem",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(growthOwner,growthWeapon);
 foreach(var pair in new[]{("type",1),("stack",1),("maxStack",1)})refinementItemType.GetField(pair.Item1).SetValue(growthOwner,pair.Item2);
 growthWeaponType.GetMethod("SetDefaults").Invoke(growthWeapon,null);
 object growthMeta=Activator.CreateInstance(refinedType);
 Check((bool)refinedType.GetMethod("SupportsRefinement").Invoke(null,new[]{growthOwner})&&(bool)refinedType.GetMethod("AppliesToEntity").Invoke(growthMeta,new object[]{growthOwner,false}),weaponName+" actual defaults attach per-item refinement");
 Check(!(bool)refinedType.GetMethod("IsSample").Invoke(null,new[]{growthOwner}),weaponName+" remains outside the original sample category");
 if(weaponName=="MoonboneDharmaSword"){
  Check((bool)refinedType.GetMethod("SupportsDaoTransformation").Invoke(null,new[]{growthOwner}),"Compiled Moonbone explicitly supports route crafting");
  var nativeDaoRules=type.Assembly.GetType("XianXia.Common.Systems.DaoArtifactRules",true);
  Check(Math.Abs((float)nativeDaoRules.GetMethod("DamageScale").Invoke(null,new object[]{weaponName})-280f/220f)<.001f,"Compiled Moonbone route grows base damage to 280");
  foreach(int routeIndex in new[]{1,2,3}) {
   object nativeRoute=Enum.ToObject(nativeDaoRules.GetMethod("MoonBurstMultiplier").GetParameters()[0].ParameterType,routeIndex);
   Check((int)nativeDaoRules.GetMethod("WeaponCost").Invoke(null,new object[]{weaponName,nativeRoute})==(routeIndex==3?14:18),"Compiled Moonbone route normal cost");
   Check(Math.Abs((float)nativeDaoRules.GetMethod("MoonBurstMultiplier").Invoke(null,new[]{nativeRoute})-(routeIndex==2?2.5f:2f))<.001f,"Compiled Moonbone route burst multiplier");
   Check((int)nativeDaoRules.GetMethod("SkillCost").Invoke(null,new[]{Enum.ToObject(type.Assembly.GetType("XianXia.Common.Systems.ArtifactSkill",true),4),nativeRoute})==(routeIndex==1?36:routeIndex==2?48:30),"Compiled Moonbone route skill cost");
  }
  var nativeSkillType=type.Assembly.GetType("XianXia.Common.Systems.ArtifactSkill",true);object moonSkill=Enum.ToObject(nativeSkillType,4);var nativeSkillRules=type.Assembly.GetType("XianXia.Common.Systems.ArtifactSkillRules",true);
  Check((bool)nativeSkillRules.GetMethod("IsValid").Invoke(null,new[]{moonSkill}),"Compiled moon skill identifier appended and valid");
  Check((int)nativeSkillRules.GetMethod("Cost").Invoke(null,new[]{moonSkill})==30,"Compiled moon skill costs thirty base energy");
  Check((int)nativeSkillRules.GetMethod("Cooldown").Invoke(null,new[]{moonSkill})==1200,"Compiled moon skill shares twenty second cooldown");
  Check(Convert.ToInt32(type.Assembly.GetType("XianXia.Common.Systems.ArtifactSkillTransactions",true).GetMethod("HeldSkill").Invoke(null,new[]{growthOwner}))==4,"Compiled moon sword binds dedicated skill");
 }

 refinedType.GetMethod("LoadData").Invoke(growthMeta,new[]{growthOwner,Tag(("refinement",3),("awakened",true),("daoRoute",1))});
 Check(Refinement(growthMeta)==3&&(bool)refinedType.GetProperty("Awakened").GetValue(growthMeta)==(weaponName=="MoonboneDharmaSword")&&DaoRoute(growthMeta)==(weaponName=="MoonboneDharmaSword"?1:0),weaponName+" actual save loader keeps refinement and strips unsupported advanced flags");
 object growthSave=Tag();refinedType.GetMethod("SaveData").Invoke(growthMeta,new[]{growthOwner,growthSave});
 Check(Convert.ToBoolean(tagType.GetMethod("GetBool").Invoke(growthSave,new object[]{"awakened"}))==(weaponName=="MoonboneDharmaSword")&&Convert.ToInt32(tagType.GetMethod("GetInt").Invoke(growthSave,new object[]{"daoRoute"}))==(weaponName=="MoonboneDharmaSword"?1:0),weaponName+" native TagCompound stores supported growth only");
 using(var growthBytes=new MemoryStream()){
  using(var writer=new BinaryWriter(growthBytes,System.Text.Encoding.UTF8,true))refinedType.GetMethod("NetSend").Invoke(growthMeta,new object[]{growthOwner,writer});
  Check(growthBytes.ToArray().SequenceEqual(new byte[]{3,(byte)(weaponName=="MoonboneDharmaSword"?1:0),(byte)(weaponName=="MoonboneDharmaSword"?1:0)}),weaponName+" actual wire payload keeps supported three-byte growth");
  object growthClone=refinedType.GetMethod("Clone",new[]{refinementItemType,refinementItemType}).Invoke(growthMeta,new[]{growthOwner,growthOwner});
  refinedType.GetMethod("SetLevel").Invoke(growthClone,new object[]{1});
  Check(Refinement(growthMeta)==3&&Refinement(growthClone)==1,weaponName+" native metadata clone is independent");
 }
 using(var growthBytes=new MemoryStream(new byte[]{3,1,1}))refinedType.GetMethod("NetReceive").Invoke(growthMeta,new object[]{growthOwner,new BinaryReader(growthBytes)});
 Check(Refinement(growthMeta)==3&&(bool)refinedType.GetProperty("Awakened").GetValue(growthMeta)==(weaponName=="MoonboneDharmaSword")&&DaoRoute(growthMeta)==(weaponName=="MoonboneDharmaSword"?1:0),weaponName+" native receive accepts only supported growth metadata");
}
Console.WriteLine($"Actual engine gameplay assertions including expanded refinement: {assertions}.");

// Actual third artifact metadata preserves awakening while rejecting all sample Dao routes.
object nativeGrownWard=Activator.CreateInstance(refinementItemType),grownWardWeapon=Activator.CreateInstance(sealType),grownWardMeta=Activator.CreateInstance(refinedType);
sealType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(grownWardWeapon,nativeGrownWard);
sealType.GetProperty("Mod",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(grownWardWeapon,Activator.CreateInstance(type.Assembly.GetType("XianXia.XianXia",true)));
refinementItemType.GetProperty("ModItem",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeGrownWard,grownWardWeapon);
foreach(var pair in new[]{("type",1),("stack",1),("maxStack",1)})refinementItemType.GetField(pair.Item1).SetValue(nativeGrownWard,pair.Item2);
sealType.GetMethod("SetDefaults").Invoke(grownWardWeapon,null);
Check((bool)refinedType.GetMethod("SupportsRefinement").Invoke(null,new[]{nativeGrownWard})&&(bool)refinedType.GetMethod("SupportsAwakening").Invoke(null,new[]{nativeGrownWard})&&!(bool)refinedType.GetMethod("IsSample").Invoke(null,new[]{nativeGrownWard}),"Actual ward defaults separate three growth capabilities");
foreach(int forbiddenRoute in new[]{1,2,3}){
 refinedType.GetMethod("LoadData").Invoke(grownWardMeta,new[]{nativeGrownWard,Tag(("refinement",3),("awakened",true),("daoRoute",forbiddenRoute))});
 Check(Refinement(grownWardMeta)==3&&(bool)refinedType.GetProperty("Awakened").GetValue(grownWardMeta)&&DaoRoute(grownWardMeta)==0,"Actual ward save strips each unsupported route but keeps awakening");
 using(var bytes=new MemoryStream(new byte[]{3,1,(byte)forbiddenRoute}))refinedType.GetMethod("NetReceive").Invoke(grownWardMeta,new object[]{nativeGrownWard,new BinaryReader(bytes)});
 Check((bool)refinedType.GetProperty("Awakened").GetValue(grownWardMeta)&&DaoRoute(grownWardMeta)==0,"Actual ward wire strips each unsupported route");
}
object nativeWardSave=Tag();refinedType.GetMethod("SaveData").Invoke(grownWardMeta,new[]{nativeGrownWard,nativeWardSave});
Check(Convert.ToBoolean(tagType.GetMethod("GetBool").Invoke(nativeWardSave,new object[]{"awakened"}))&&Convert.ToInt32(tagType.GetMethod("GetInt").Invoke(nativeWardSave,new object[]{"daoRoute"}))==0,"Native ward save retains crafted awakening only");
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))refinedType.GetMethod("NetSend").Invoke(grownWardMeta,new object[]{nativeGrownWard,writer});Check(bytes.ToArray().SequenceEqual(new byte[]{3,1,0}),"Native ward serializes bounded three-byte growth state");}
object nativeWardClone=refinedType.GetMethod("Clone",new[]{refinementItemType,refinementItemType}).Invoke(grownWardMeta,new[]{nativeGrownWard,nativeGrownWard});refinedType.GetMethod("SetLevel").Invoke(nativeWardClone,new object[]{1});
Check((bool)refinedType.GetProperty("Awakened").GetValue(grownWardMeta)&&!(bool)refinedType.GetProperty("Awakened").GetValue(nativeWardClone),"Native ward cloned growth is independent");
Console.WriteLine($"Actual engine gameplay assertions including crafted ward: {assertions}.");

var actualBriarType=type.Assembly.GetType("XianXia.Content.Projectiles.GardenBriarPatch",true);object actualBriar=Activator.CreateInstance(actualBriarType),nativeBriar=Activator.CreateInstance(projectileType);
actualBriarType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualBriar,nativeBriar);actualBriarType.GetMethod("SetDefaults").Invoke(actualBriar,null);
Check((int)projectileType.GetField("width").GetValue(nativeBriar)==80&&(int)projectileType.GetField("height").GetValue(nativeBriar)==48,"Actual briar declares its warning-sized hitbox");
Check((bool)projectileType.GetField("hostile").GetValue(nativeBriar)&&!(bool)projectileType.GetField("friendly").GetValue(nativeBriar)&&(int)projectileType.GetField("penetrate").GetValue(nativeBriar)==-1,"Actual briar native hostile stationary field");
Check((bool)projectileType.GetField("netImportant").GetValue(nativeBriar)&&!(bool)projectileType.GetField("tileCollide").GetValue(nativeBriar)&&(int)projectileType.GetField("timeLeft").GetValue(nativeBriar)==150,"Actual briar participates in native late join and finite lifetime");
foreach(short lifetime in new short[]{0,15,80,105,150}){
 using var bytes=new MemoryStream();using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))writer.Write(lifetime);bytes.Position=0;actualBriarType.GetMethod("ReceiveExtraAI").Invoke(actualBriar,new object[]{new BinaryReader(bytes)});
 Check((int)projectileType.GetField("timeLeft").GetValue(nativeBriar)==lifetime,"Actual remaining-age hook retains every phase boundary");
}
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))writer.Write((short)999);bytes.Position=0;actualBriarType.GetMethod("ReceiveExtraAI").Invoke(actualBriar,new object[]{new BinaryReader(bytes)});Check((int)projectileType.GetField("timeLeft").GetValue(nativeBriar)==0,"Actual native field rejects extended lifetime");}
var gardenType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.GardenWarden",true);object nativeGarden=Activator.CreateInstance(gardenType);
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){writer.Write(66);writer.Write(0);}bytes.Position=0;gardenType.GetMethod("ReceiveExtraAI").Invoke(nativeGarden,new object[]{new BinaryReader(bytes)});Check((int)gardenType.GetProperty("HazardSession").GetValue(nativeGarden)==66&&(int)gardenType.GetProperty("BattleTarget").GetValue(nativeGarden)==0,"Actual partial boss retains source session and target");}
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))gardenType.GetMethod("SendExtraAI").Invoke(nativeGarden,new object[]{writer});Check(bytes.ToArray().SequenceEqual(new byte[]{66,0,0,0,0,0,0,0}),"Actual partial boss source identity serializes eight bytes");}
Check(Convert.ToSingle(gardenType.GetField("DashLineLength").GetRawConstantValue())==384f,"Compiled dash warning covers movement plus body radius");
Console.WriteLine($"Actual engine gameplay assertions including Garden Warden metadata: {assertions}.");


var nativeConfigType = type.Assembly.GetType("XianXia.Common.Systems.XianXiaConfig", true);
object nativeConfig = Activator.CreateInstance(nativeConfigType);
Check(!(bool)nativeConfigType.GetProperty("DebugDrops").GetValue(nativeConfig), "Release default disables debug drops");
Check((float)nativeConfigType.GetProperty("PermanentGrowthMultiplier").GetValue(nativeConfig) == 1f, "Release default uses baseline permanent growth");
Check((bool)nativeConfigType.GetProperty("EnableWorldGeneration").GetValue(nativeConfig), "Release default enables new-world generation");
Check((bool)nativeConfigType.GetProperty("EnableSoftCompatibilityHooks").GetValue(nativeConfig), "Release default allows optional integrations");
Check(nativeConfigType.GetProperty("Mode").GetValue(nativeConfig).ToString() == "ServerSide", "Gameplay configuration remains server-owned");
Console.WriteLine($"Actual engine gameplay assertions including release defaults: {assertions}.");


var actualFieldType=type.Assembly.GetType("XianXia.Content.Projectiles.BossArrayFieldProjectile",true);
object actualField=Activator.CreateInstance(actualFieldType),nativeField=Activator.CreateInstance(projectileType);
actualFieldType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualField,nativeField);
actualFieldType.GetMethod("SetDefaults").Invoke(actualField,null);
Check((int)projectileType.GetField("width").GetValue(nativeField)==96&&(int)projectileType.GetField("height").GetValue(nativeField)==96,"Native shared hostile field warning matches hitbox");
Check((bool)projectileType.GetField("hostile").GetValue(nativeField)&&!(bool)projectileType.GetField("friendly").GetValue(nativeField)&&(bool)projectileType.GetField("netImportant").GetValue(nativeField),"Native shared field is hostile and late-join important");
foreach(int remaining in new[]{0,1,15,16,75,76,120,121}) {
 projectileType.GetField("timeLeft").SetValue(nativeField,remaining);
 Check((bool)actualFieldType.GetMethod("CanDamage").Invoke(actualField,null)==(remaining>15&&remaining<=75),"Native compiled warning/active/fade boundary");
}
foreach(short remaining in new short[]{0,1,15,16,75,76,120}) {
 using var bytes=new MemoryStream();using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){writer.Write(remaining);writer.Write(false);writer.Write((short)-1);writer.Write(0);writer.Write((short)-1);writer.Write(0L);}bytes.Position=0;
 actualFieldType.GetMethod("ReceiveExtraAI").Invoke(actualField,new object[]{new BinaryReader(bytes)});
 Check((int)projectileType.GetField("timeLeft").GetValue(nativeField)==remaining,"Native field receives exact remaining phase age");
 using var sent=new MemoryStream();using(var writer=new BinaryWriter(sent,System.Text.Encoding.UTF8,true))actualFieldType.GetMethod("SendExtraAI").Invoke(actualField,new object[]{writer});
 Check(sent.ToArray().SequenceEqual(BitConverter.GetBytes(remaining).Concat(new byte[]{0,255,255,0,0,0,0,255,255,0,0,0,0,0,0,0,0})),"Native field writes bounded age and source state");
}
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){writer.Write((short)121);writer.Write(false);writer.Write((short)-1);writer.Write(0);writer.Write((short)-1);writer.Write(0L);}bytes.Position=0;actualFieldType.GetMethod("ReceiveExtraAI").Invoke(actualField,new object[]{new BinaryReader(bytes)});Check((int)projectileType.GetField("timeLeft").GetValue(nativeField)==0,"Native field invalid lifetime expires harmlessly");}
// Start a fresh field after the malformed-payload negative control.
actualField=Activator.CreateInstance(actualFieldType);nativeField=Activator.CreateInstance(projectileType);actualFieldType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualField,nativeField);actualFieldType.GetMethod("SetDefaults").Invoke(actualField,null);
Console.WriteLine($"Actual engine gameplay assertions including shared field telegraph: {assertions}.");


var nativeTargetMain=projectileType.Assembly.GetType("Terraria.Main",true);
var nativeTargetNpcType=projectileType.Assembly.GetType("Terraria.NPC",true);
var nativeTargetPlayerType=projectileType.Assembly.GetType("Terraria.Player",true);
var nativeTargetVectorType=nativeTargetNpcType.GetField("position").FieldType;
// The engine entry point normally initializes this before Terraria.Main's static constructor.
// This test process does not launch the game or touch user saves.
var nativeTargetProgram=projectileType.Assembly.GetType("Terraria.Program",true);
var nativeTargetSaveField=nativeTargetProgram.GetField("SavePath",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
if(nativeTargetSaveField==null)throw new Exception("Official engine save-path field not found");
nativeTargetSaveField.SetValue(null,Path.Combine(Path.GetTempPath(),"XianXia-native-hook-tests"));
var nativeTargetOldPlayers=nativeTargetMain.GetField("player").GetValue(null);
object nativeTargetOldMode=nativeTargetMain.GetField("netMode").GetValue(null),nativeTargetOldDedicated=nativeTargetMain.GetField("dedServ").GetValue(null);
int nativeTargetMax=(int)nativeTargetMain.GetField("maxPlayers").GetValue(null);
var nativeTargetPlayers=Array.CreateInstance(nativeTargetPlayerType,nativeTargetMax+1);
for(int index=0;index<nativeTargetPlayers.Length;index++)nativeTargetPlayers.SetValue(Activator.CreateInstance(nativeTargetPlayerType),index);
nativeTargetMain.GetField("player").SetValue(null,nativeTargetPlayers);
nativeTargetMain.GetField("dedServ").SetValue(null,true);
try {
 foreach(string nativeTargetName in new[]{"AbyssalStarWomb","BlackFurnaceIronGolem","BrokenHeavenInspector","FormlessSwordSoul","GreenwoodMedicineKingEcho","HeavenTabletGuardian","MoonboneImmortal","OldHeavenDaoCore","SpiritVeinWyrm","ThunderMarshJiao","TribulationCloudAvatar"}) {
  var nativeTargetBossType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses."+nativeTargetName,true);
  foreach(int nativeTargetMode in new[]{0,1,2})foreach(int nativeTargetIndex in new[]{-1,nativeTargetMax,int.MaxValue}) {
   object nativeTargetBoss=Activator.CreateInstance(nativeTargetBossType),nativeTargetNpc=Activator.CreateInstance(nativeTargetNpcType);
   nativeTargetBossType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeTargetBoss,nativeTargetNpc);
   nativeTargetNpcType.GetField("active").SetValue(nativeTargetNpc,true);nativeTargetNpcType.GetField("life").SetValue(nativeTargetNpc,100);
   nativeTargetNpcType.GetField("target").SetValue(nativeTargetNpc,nativeTargetIndex);nativeTargetNpcType.GetField("timeLeft").SetValue(nativeTargetNpc,300);
   var nativeTargetAi=(float[])nativeTargetNpcType.GetField("ai").GetValue(nativeTargetNpc);nativeTargetAi[0]=17;nativeTargetAi[1]=18;nativeTargetAi[2]=19;
   nativeTargetMain.GetField("netMode").SetValue(null,nativeTargetMode);
   nativeTargetBossType.GetMethod("AI").Invoke(nativeTargetBoss,null);
   Check(nativeTargetNpcType.GetField("velocity").GetValue(nativeTargetNpc).Equals(Activator.CreateInstance(nativeTargetVectorType,new object[]{0f,-2f})),"Actual "+nativeTargetName+" exits without unsafe target read or spawning");
   Check(nativeTargetMode==1?nativeTargetAi.Take(3).SequenceEqual(new float[]{17,18,19}):nativeTargetAi.Take(3).All(value=>value==0),"Actual "+nativeTargetName+" resets attacks only on authority");
   Check((int)nativeTargetNpcType.GetField("timeLeft").GetValue(nativeTargetNpc)==(nativeTargetMode==1?300:30),"Actual "+nativeTargetName+" requests authority-only despawn");
   object[] nativeTargetContactArgs={nativeTargetPlayers.GetValue(0),0};
   Check(!(bool)nativeTargetBossType.GetMethod("CanHitPlayer").Invoke(nativeTargetBoss,nativeTargetContactArgs),"Actual "+nativeTargetName+" cannot contact-hit without living target");
   if(nativeTargetMode!=1) {
    nativeTargetNpcType.GetField("netUpdate").SetValue(nativeTargetNpc,false);
    nativeTargetBossType.GetMethod("AI").Invoke(nativeTargetBoss,null);
    Check(!(bool)nativeTargetNpcType.GetField("netUpdate").GetValue(nativeTargetNpc),"Actual "+nativeTargetName+" does not repeatedly dirty unchanged departure");
   }
  }
  object nativeTargetRecoveredBoss=Activator.CreateInstance(nativeTargetBossType),nativeTargetRecoveredNpc=Activator.CreateInstance(nativeTargetNpcType);
  nativeTargetBossType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeTargetRecoveredBoss,nativeTargetRecoveredNpc);
  nativeTargetNpcType.GetField("active").SetValue(nativeTargetRecoveredNpc,true);nativeTargetNpcType.GetField("life").SetValue(nativeTargetRecoveredNpc,100);nativeTargetNpcType.GetField("lifeMax").SetValue(nativeTargetRecoveredNpc,100);
  nativeTargetNpcType.GetField("target").SetValue(nativeTargetRecoveredNpc,-1);((float[])nativeTargetNpcType.GetField("localAI").GetValue(nativeTargetRecoveredNpc))[3]=1;
  nativeTargetPlayerType.GetField("active").SetValue(nativeTargetPlayers.GetValue(0),true);nativeTargetMain.GetField("netMode").SetValue(null,2);
  nativeTargetBossType.GetMethod("AI").Invoke(nativeTargetRecoveredBoss,null);
  Check((int)nativeTargetNpcType.GetField("target").GetValue(nativeTargetRecoveredNpc)==0,"Actual "+nativeTargetName+" recovers to live player on authority");
  Check((float[])nativeTargetNpcType.GetField("ai").GetValue(nativeTargetRecoveredNpc) is var recoveredAi&&recoveredAi[0]==1,"Actual "+nativeTargetName+" runs first movement tick with valid target");
  object[] nativeTargetRecoveredContact={nativeTargetPlayers.GetValue(0),0};Check((bool)nativeTargetBossType.GetMethod("CanHitPlayer").Invoke(nativeTargetRecoveredBoss,nativeTargetRecoveredContact),"Actual "+nativeTargetName+" preserves living-target contact");
  nativeTargetPlayerType.GetField("active").SetValue(nativeTargetPlayers.GetValue(0),false);
 }
 var nativeShardBossType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.BlackFurnaceIronGolem",true);
 Check((int)nativeShardBossType.GetField("MaximumNearbyShards").GetRawConstantValue()==6,"Compiled furnace summon population is six");
 Check((float)nativeShardBossType.GetField("ShardArenaRadius").GetRawConstantValue()==1600f,"Compiled furnace arena cap radius is 1600 pixels");
 foreach(int nativeShardScenario in new[]{0,1,2,3}) {
  object nativeShardBoss=Activator.CreateInstance(nativeShardBossType),nativeShardNpc=Activator.CreateInstance(nativeTargetNpcType);
  nativeShardBossType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeShardBoss,nativeShardNpc);
  nativeTargetNpcType.GetField("active").SetValue(nativeShardNpc,nativeShardScenario!=3);nativeTargetNpcType.GetField("life").SetValue(nativeShardNpc,100);
  nativeTargetNpcType.GetField("target").SetValue(nativeShardNpc,nativeShardScenario==0?-1:0);
  nativeTargetPlayerType.GetField("active").SetValue(nativeTargetPlayers.GetValue(0),nativeShardScenario==2);
  nativeTargetMain.GetField("netMode").SetValue(null,nativeShardScenario==2?1:2);
  nativeShardBossType.GetMethod("SpawnShardAdds",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(nativeShardBoss,null);
  Check(!(bool)nativeTargetNpcType.GetField("netUpdate").GetValue(nativeShardNpc),"Compiled spawn hook refuses invalid battle/client before touching native creation or RNG");
 }
 // Exercise the compiled NPC extra-AI contract with the official engine types.
 var nativeSummonType=type.Assembly.GetType("XianXia.Content.NPCs.Enemies.IronShardSpirit",true);
 object nativeSummon=Activator.CreateInstance(nativeSummonType),nativeSummonNpc=Activator.CreateInstance(nativeTargetNpcType);
 nativeSummonType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeSummon,nativeSummonNpc);
 byte[] SummonWire(){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);nativeSummonType.GetMethod("SendExtraAI").Invoke(nativeSummon,new object[]{writer});return stream.ToArray();}
 Check((int)nativeSummonType.GetField("MaximumSummonLifetime").GetRawConstantValue()==900,"Compiled summon lifetime 900 ticks");
 Check(SummonWire().Length==13,"Compiled summon extra-AI fixed thirteen bytes");
 Check((bool)nativeSummonType.GetMethod("PreAI").Invoke(nativeSummon,null),"Compiled natural shard has no boss binding");
 using(var payload=new MemoryStream()){
  using(var writer=new BinaryWriter(payload,System.Text.Encoding.UTF8,true)){writer.Write(true);writer.Write((short)-1);writer.Write(123L);writer.Write((short)900);}
  var bytes=payload.ToArray();
  for(int length=0;length<bytes.Length;length++){
   var before=SummonWire();using var truncated=new MemoryStream(bytes[..length]);
   try{nativeSummonType.GetMethod("ReceiveExtraAI").Invoke(nativeSummon,new object[]{new BinaryReader(truncated)});throw new Exception("Compiled summon accepted truncated packet");}catch(TargetInvocationException ex)when(ex.InnerException is EndOfStreamException){}
   Check(SummonWire().SequenceEqual(before),"Compiled truncated summon packet leaves state intact");
  }
  payload.Position=0;nativeSummonType.GetMethod("ReceiveExtraAI").Invoke(nativeSummon,new object[]{new BinaryReader(payload)});
  Check(SummonWire().SequenceEqual(bytes),"Compiled summon wire roundtrip");
 }
 nativeTargetMain.GetField("netMode").SetValue(null,1);nativeTargetNpcType.GetField("active").SetValue(nativeSummonNpc,true);nativeTargetNpcType.GetField("life").SetValue(nativeSummonNpc,70);
 Check(!(bool)nativeSummonType.GetMethod("PreAI").Invoke(nativeSummon,null)&&(bool)nativeTargetNpcType.GetField("active").GetValue(nativeSummonNpc),"Compiled invalid source blocks client AI without authority removal");
 object[] nativeSummonContact={nativeTargetPlayers.GetValue(0),0};Check(!(bool)nativeSummonType.GetMethod("CanHitPlayer").Invoke(nativeSummon,nativeSummonContact),"Compiled invalid source cannot hit players");
 nativeTargetMain.GetField("netMode").SetValue(null,0);nativeSummonType.GetMethod("PreAI").Invoke(nativeSummon,null);
 Check(!(bool)nativeTargetNpcType.GetField("active").GetValue(nativeSummonNpc)&&(int)nativeTargetNpcType.GetField("damage").GetValue(nativeSummonNpc)==0,"Compiled invalid source despawns on authority");
 nativeSummonType.GetMethod("PostAI").Invoke(nativeSummon,null);Check(!(bool)nativeTargetNpcType.GetField("netUpdate").GetValue(nativeSummonNpc),"Compiled PostAI stops after source cleanup");
 var nativeCloudBindingType=type.Assembly.GetType("XianXia.Content.NPCs.Enemies.TribulationCloudling",true);
 object nativeCloudBinding=Activator.CreateInstance(nativeCloudBindingType),nativeCloudBindingNpc=Activator.CreateInstance(nativeTargetNpcType);
 nativeCloudBindingType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeCloudBinding,nativeCloudBindingNpc);
 byte[] CloudBindingWire(){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);nativeCloudBindingType.GetMethod("SendExtraAI").Invoke(nativeCloudBinding,new object[]{writer});return stream.ToArray();}
 Check((int)nativeCloudBindingType.GetField("MaximumSummonLifetime").GetRawConstantValue()==900,"Compiled summon lifetime 900 ticks");
 Check(CloudBindingWire().Length==13,"Compiled summon extra-AI fixed thirteen bytes");
 Check((bool)nativeCloudBindingType.GetMethod("PreAI").Invoke(nativeCloudBinding,null),"Compiled natural cloudling has no boss binding");
 using(var payload=new MemoryStream()){
  using(var writer=new BinaryWriter(payload,System.Text.Encoding.UTF8,true)){writer.Write(true);writer.Write((short)-1);writer.Write(123L);writer.Write((short)900);}
  var bytes=payload.ToArray();
  for(int length=0;length<bytes.Length;length++){
   var before=CloudBindingWire();using var truncated=new MemoryStream(bytes[..length]);
   try{nativeCloudBindingType.GetMethod("ReceiveExtraAI").Invoke(nativeCloudBinding,new object[]{new BinaryReader(truncated)});throw new Exception("Compiled summon accepted truncated packet");}catch(TargetInvocationException ex)when(ex.InnerException is EndOfStreamException){}
   Check(CloudBindingWire().SequenceEqual(before),"Compiled truncated summon packet leaves state intact");
  }
  payload.Position=0;nativeCloudBindingType.GetMethod("ReceiveExtraAI").Invoke(nativeCloudBinding,new object[]{new BinaryReader(payload)});
  Check(CloudBindingWire().SequenceEqual(bytes),"Compiled summon wire roundtrip");
 }
 nativeTargetMain.GetField("netMode").SetValue(null,1);nativeTargetNpcType.GetField("active").SetValue(nativeCloudBindingNpc,true);nativeTargetNpcType.GetField("life").SetValue(nativeCloudBindingNpc,70);
 Check(!(bool)nativeCloudBindingType.GetMethod("PreAI").Invoke(nativeCloudBinding,null)&&(bool)nativeTargetNpcType.GetField("active").GetValue(nativeCloudBindingNpc),"Compiled invalid source blocks client AI without authority removal");
 object[] nativeCloudBindingContact={nativeTargetPlayers.GetValue(0),0};Check(!(bool)nativeCloudBindingType.GetMethod("CanHitPlayer").Invoke(nativeCloudBinding,nativeCloudBindingContact),"Compiled invalid source cannot hit players");
 nativeTargetMain.GetField("netMode").SetValue(null,0);nativeCloudBindingType.GetMethod("PreAI").Invoke(nativeCloudBinding,null);
 Check(!(bool)nativeTargetNpcType.GetField("active").GetValue(nativeCloudBindingNpc)&&(int)nativeTargetNpcType.GetField("damage").GetValue(nativeCloudBindingNpc)==0,"Compiled invalid source despawns on authority");
 nativeCloudBindingType.GetMethod("PostAI").Invoke(nativeCloudBinding,null);Check(!(bool)nativeTargetNpcType.GetField("netUpdate").GetValue(nativeCloudBindingNpc),"Compiled PostAI stops after source cleanup");
 var nativeWyrmType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.SpiritVeinWyrm",true);
 var nativeWyrmChildType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.ShatteredJadeWyrmMinion",true);
 var nativeOldNpcTable=nativeTargetMain.GetField("npc").GetValue(null);
 try {
  int capacity=(int)nativeTargetMain.GetField("maxNPCs").GetValue(null);var npcTable=Array.CreateInstance(nativeTargetNpcType,capacity);
  for(int i=0;i<capacity;i++)npcTable.SetValue(Activator.CreateInstance(nativeTargetNpcType),i);
  nativeTargetMain.GetField("npc").SetValue(null,npcTable);nativeTargetMain.GetField("netMode").SetValue(null,0);nativeTargetPlayerType.GetField("active").SetValue(nativeTargetPlayers.GetValue(0),true);
  object parent=Activator.CreateInstance(nativeWyrmType),parentNpc=npcTable.GetValue(3),child=Activator.CreateInstance(nativeWyrmChildType),childNpc=npcTable.GetValue(4);
  nativeWyrmType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(parent,parentNpc);
  nativeWyrmChildType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(child,childNpc);
  nativeTargetNpcType.GetProperty("ModNPC").SetValue(parentNpc,parent);nativeTargetNpcType.GetProperty("ModNPC").SetValue(childNpc,child);
  foreach(var pair in new[]{(parentNpc,3),(childNpc,4)}){nativeTargetNpcType.GetField("active").SetValue(pair.Item1,true);nativeTargetNpcType.GetField("life").SetValue(pair.Item1,70);nativeTargetNpcType.GetField("whoAmI").SetValue(pair.Item1,pair.Item2);nativeTargetNpcType.GetField("target").SetValue(pair.Item1,0);}
  var nativeParentSourceType=projectileType.Assembly.GetType("Terraria.DataStructures.EntitySource_Parent",true);
  object source=Activator.CreateInstance(nativeParentSourceType,new object[]{parentNpc,null});nativeWyrmChildType.GetMethod("OnSpawn").Invoke(child,new[]{source});
  byte[] WyrmChildWire(){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);nativeWyrmChildType.GetMethod("SendExtraAI").Invoke(child,new object[]{writer});return stream.ToArray();}
  var boundWire=WyrmChildWire();Check(boundWire.Length==34&&BitConverter.ToInt16(boundWire)==3&&BitConverter.ToInt64(boundWire,2)>0,"Official parent source captures slot and positive wyrm instance");
  for(int length=0;length<34;length++){using var stream=new MemoryStream(boundWire[..length]);try{nativeWyrmChildType.GetMethod("ReceiveExtraAI").Invoke(child,new object[]{new BinaryReader(stream)});throw new Exception("Accepted truncated wyrm source");}catch(TargetInvocationException ex)when(ex.InnerException is EndOfStreamException){}Check(WyrmChildWire().SequenceEqual(boundWire),"Compiled wyrm source read atomic");}
  var childAge=(float[])nativeTargetNpcType.GetField("ai").GetValue(childNpc);
  Check((bool)nativeWyrmChildType.GetMethod("PreAI").Invoke(child,null)&&childAge[3]==1,"Compiled valid wyrm source advances authority age");
  nativeTargetMain.GetField("netMode").SetValue(null,1);Check((bool)nativeWyrmChildType.GetMethod("PreAI").Invoke(child,null)&&childAge[3]==1,"Compiled client source never advances age");
  foreach(string suffix in new[]{"Body","Tail"}){
   var segmentType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.ShatteredJadeWyrmMinion"+suffix,true);object segment=Activator.CreateInstance(segmentType),segmentNpc=npcTable.GetValue(5);
   segmentType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(segment,segmentNpc);nativeTargetNpcType.GetField("active").SetValue(segmentNpc,true);nativeTargetNpcType.GetField("life").SetValue(segmentNpc,70);nativeTargetNpcType.GetField("realLife").SetValue(segmentNpc,4);((float[])nativeTargetNpcType.GetField("ai").GetValue(segmentNpc))[1]=4;
   nativeTargetMain.GetField("netMode").SetValue(null,0);nativeTargetNpcType.GetProperty("ModNPC").SetValue(segmentNpc,segment);((float[])nativeTargetNpcType.GetField("ai").GetValue(segmentNpc))[0]=4;
   segmentType.BaseType.GetMethod("Bind",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(segment,new[]{child,child});
   using(var linkBytes=new MemoryStream()){using var writer=new BinaryWriter(linkBytes);segmentType.GetMethod("SendExtraAI").Invoke(segment,new object[]{writer});Check(linkBytes.Length==24,"Compiled segment carries only twenty-four linkage bytes");}
   nativeTargetMain.GetField("netMode").SetValue(null,1);
   object[] segmentContact={nativeTargetPlayers.GetValue(0),0};Check((bool)segmentType.GetMethod("CanHitPlayer").Invoke(segment,segmentContact),"Compiled live wyrm "+suffix+" contact allowed");
   nativeTargetNpcType.GetField("active").SetValue(parentNpc,false);Check(!(bool)segmentType.GetMethod("CanHitPlayer").Invoke(segment,segmentContact),"Compiled parent loss blocks "+suffix+" before child head AI");
   segmentType.GetMethod("AI").Invoke(segment,null);Check((bool)nativeTargetNpcType.GetField("active").GetValue(segmentNpc)&&(int)nativeTargetNpcType.GetField("damage").GetValue(segmentNpc)==0,"Compiled client "+suffix+" remains harmless awaiting authority");nativeTargetNpcType.GetField("active").SetValue(parentNpc,true);
  }
  nativeTargetMain.GetField("netMode").SetValue(null,0);childAge[3]=899;Check(!(bool)nativeWyrmChildType.GetMethod("PreAI").Invoke(child,null)&&!(bool)nativeTargetNpcType.GetField("active").GetValue(childNpc),"Compiled wyrm expires at authority tick900");
  nativeTargetNpcType.GetField("active").SetValue(childNpc,true);childAge[3]=0;
  object replacement=Activator.CreateInstance(nativeWyrmType);nativeWyrmType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(replacement,parentNpc);nativeTargetNpcType.GetProperty("ModNPC").SetValue(parentNpc,replacement);
  object[] childContact={nativeTargetPlayers.GetValue(0),0};Check(!(bool)nativeWyrmChildType.GetMethod("CanHitPlayer").Invoke(child,childContact),"Compiled same-slot new wyrm instance denies old child contact");
  Check(!(bool)nativeWyrmChildType.GetMethod("PreAI").Invoke(child,null)&&!(bool)nativeTargetNpcType.GetField("active").GetValue(childNpc),"Compiled same-slot new wyrm clears old child");
  nativeTargetMain.GetField("netMode").SetValue(null,0);
  object bossSourceField=Activator.CreateInstance(actualFieldType),bossSourceProjectile=Activator.CreateInstance(projectileType);
  actualFieldType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(bossSourceField,bossSourceProjectile);actualFieldType.GetMethod("SetDefaults").Invoke(bossSourceField,null);actualFieldType.GetMethod("OnSpawn").Invoke(bossSourceField,new[]{Activator.CreateInstance(nativeParentSourceType,new object[]{parentNpc,null})});projectileType.GetField("timeLeft").SetValue(bossSourceProjectile,50);
  Check((bool)actualFieldType.GetMethod("CanDamage").Invoke(bossSourceField,null),"Official NPC parent field preserves valid damage phase");
  object savedSourceMod=nativeTargetNpcType.GetProperty("ModNPC").GetValue(parentNpc),newSourceMod=Activator.CreateInstance(nativeWyrmType);nativeWyrmType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(newSourceMod,parentNpc);nativeTargetNpcType.GetProperty("ModNPC").SetValue(parentNpc,newSourceMod);
  Check(!(bool)actualFieldType.GetMethod("CanDamage").Invoke(bossSourceField,null),"Compiled source field immediately blocks same-slot new ModNPC on authority");actualFieldType.GetMethod("AI").Invoke(bossSourceField,null);
  Check((int)projectileType.GetField("timeLeft").GetValue(bossSourceProjectile)==15&&(bool)projectileType.GetField("netUpdate").GetValue(bossSourceProjectile),"Compiled source field enters synchronized harmless fade");
  using(var sourcePacket=new MemoryStream()){using(var writer=new BinaryWriter(sourcePacket,System.Text.Encoding.UTF8,true))actualFieldType.GetMethod("SendExtraAI").Invoke(bossSourceField,new object[]{writer});Check(sourcePacket.Length==19,"Compiled field source packet nineteen bytes");}
  nativeTargetNpcType.GetProperty("ModNPC").SetValue(parentNpc,savedSourceMod);
  var compiledWarningType=type.Assembly.GetType("XianXia.Content.Projectiles.TribulationWarningLineProjectile",true);object sourceWarning=Activator.CreateInstance(compiledWarningType),sourceWarningEntity=Activator.CreateInstance(projectileType);
  compiledWarningType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(sourceWarning,sourceWarningEntity);compiledWarningType.GetMethod("SetDefaults").Invoke(sourceWarning,null);
  Check((int)projectileType.GetField("timeLeft").GetValue(sourceWarningEntity)==36&&(bool)projectileType.GetField("netImportant").GetValue(sourceWarningEntity),"Compiled warning lifetime and late-join importance");Check(!(bool)compiledWarningType.GetMethod("CanDamage").Invoke(sourceWarning,null),"Compiled warning cannot damage");
  compiledWarningType.GetMethod("OnSpawn").Invoke(sourceWarning,new[]{Activator.CreateInstance(nativeParentSourceType,new object[]{parentNpc,null})});nativeTargetNpcType.GetField("active").SetValue(parentNpc,false);compiledWarningType.GetMethod("AI").Invoke(sourceWarning,null);
  Check((int)projectileType.GetField("timeLeft").GetValue(sourceWarningEntity)==6&&(bool)projectileType.GetField("netUpdate").GetValue(sourceWarningEntity),"Official parent loss cancels warning with six-tick fade");compiledWarningType.GetMethod("OnKill").Invoke(sourceWarning,new object[]{0});Check((bool)projectileType.GetField("netUpdate").GetValue(sourceWarningEntity),"Compiled canceled expiry exits before unregistered native strike creation");
  using(var warningBytes=new MemoryStream()){using var writer=new BinaryWriter(warningBytes);compiledWarningType.GetMethod("SendExtraAI").Invoke(sourceWarning,new object[]{writer});Check(warningBytes.Length==19,"Compiled warning shares nineteen-byte source layout");}
  object independentWarning=compiledWarningType.GetMethod("NewInstance",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(sourceWarning,new[]{Activator.CreateInstance(projectileType)});
  var sourceBindingField=compiledWarningType.GetField("sourceBinding",BindingFlags.Instance|BindingFlags.NonPublic);Check(!ReferenceEquals(sourceBindingField.GetValue(sourceWarning),sourceBindingField.GetValue(independentWarning)),"Official projectile NewInstance gives independent source component");
  nativeTargetNpcType.GetField("active").SetValue(parentNpc,true);
  var compiledLightningType=type.Assembly.GetType("XianXia.Content.Projectiles.TribulationLightningProjectile",true);object releasedLightning=Activator.CreateInstance(compiledLightningType),releasedEntity=Activator.CreateInstance(projectileType);
  compiledLightningType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(releasedLightning,releasedEntity);compiledLightningType.GetMethod("SetDefaults").Invoke(releasedLightning,null);compiledLightningType.GetMethod("OnSpawn").Invoke(releasedLightning,new[]{Activator.CreateInstance(nativeParentSourceType,new object[]{parentNpc,null})});
  Check((bool)compiledLightningType.GetMethod("CanDamage").Invoke(releasedLightning,null)&&(bool)projectileType.GetField("netImportant").GetValue(releasedEntity),"Official released lightning valid source and late-join state");nativeTargetNpcType.GetField("active").SetValue(parentNpc,false);
  Check(!(bool)compiledLightningType.GetMethod("CanDamage").Invoke(releasedLightning,null),"Official released lightning parent loss blocks immediate damage");compiledLightningType.GetMethod("AI").Invoke(releasedLightning,null);
  Check((int)projectileType.GetField("timeLeft").GetValue(releasedEntity)==6&&(bool)projectileType.GetField("netUpdate").GetValue(releasedEntity),"Compiled lightning source loss synchronizes six-tick fade");Check(projectileType.GetField("velocity").GetValue(releasedEntity).Equals(Activator.CreateInstance(nativeTargetVectorType,new object[]{0f,0f})),"Compiled canceled lightning stops movement");
  using(var lightningBytes=new MemoryStream()){using var writer=new BinaryWriter(lightningBytes);compiledLightningType.GetMethod("SendExtraAI").Invoke(releasedLightning,new object[]{writer});Check(lightningBytes.Length==19,"Compiled released lightning source layout nineteen bytes");}
  object independentLightning=compiledLightningType.GetMethod("NewInstance",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(releasedLightning,new[]{Activator.CreateInstance(projectileType)});var lightningSourceField=compiledLightningType.GetField("sourceBinding",BindingFlags.Instance|BindingFlags.NonPublic);Check(!ReferenceEquals(lightningSourceField.GetValue(releasedLightning),lightningSourceField.GetValue(independentLightning)),"Official lightning NewInstance source component independent");nativeTargetNpcType.GetField("active").SetValue(parentNpc,true);
  object invalidLifetimeBolt=Activator.CreateInstance(compiledLightningType),invalidLifetimeEntity=Activator.CreateInstance(projectileType);compiledLightningType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(invalidLifetimeBolt,invalidLifetimeEntity);compiledLightningType.GetMethod("SetDefaults").Invoke(invalidLifetimeBolt,null);
  Check((bool)compiledLightningType.GetMethod("CanDamage").Invoke(invalidLifetimeBolt,null),"Compiled valid lifetime control can damage");projectileType.GetField("timeLeft").SetValue(invalidLifetimeEntity,121);Check(!(bool)compiledLightningType.GetMethod("CanDamage").Invoke(invalidLifetimeBolt,null),"Compiled oversized lifetime initially harmless");compiledLightningType.GetMethod("AI").Invoke(invalidLifetimeBolt,null);projectileType.GetField("timeLeft").SetValue(invalidLifetimeEntity,120);Check(!(bool)compiledLightningType.GetMethod("CanDamage").Invoke(invalidLifetimeBolt,null),"Compiled invalid lifetime remains canceled after age repair");
  using(var lifetimePacket=new MemoryStream()){using var writer=new BinaryWriter(lifetimePacket);compiledLightningType.GetMethod("SendExtraAI").Invoke(invalidLifetimeBolt,new object[]{writer});Check(lifetimePacket.ToArray()[2]==1,"Compiled invalid lifetime uses existing cancellation bit");}
  object sendLifetimeBolt=Activator.CreateInstance(compiledLightningType),sendLifetimeEntity=Activator.CreateInstance(projectileType);compiledLightningType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(sendLifetimeBolt,sendLifetimeEntity);compiledLightningType.GetMethod("SetDefaults").Invoke(sendLifetimeBolt,null);projectileType.GetField("timeLeft").SetValue(sendLifetimeEntity,121);using(var lifetimePacket=new MemoryStream()){using var writer=new BinaryWriter(lifetimePacket);compiledLightningType.GetMethod("SendExtraAI").Invoke(sendLifetimeBolt,new object[]{writer});}projectileType.GetField("timeLeft").SetValue(sendLifetimeEntity,120);Check(!(bool)compiledLightningType.GetMethod("CanDamage").Invoke(sendLifetimeBolt,null),"Compiled sender latches invalid lifetime even before AI");
  var compiledLinkType=type.Assembly.GetType("XianXia.Common.NPCs.LinkedWormNPC",true);var compiledWormAI=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.SegmentedWormAI",true);
  foreach(string family in new[]{"SpiritVeinWyrm","ThunderMarshJiao","ShatteredJadeWyrmMinion"}){
   nativeTargetMain.GetField("netMode").SetValue(null,0);
   var familyHeadType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses."+family,true);var familyBodyType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses."+family+"Body",true);
   object familyHead=Activator.CreateInstance(familyHeadType),familyPrevious=Activator.CreateInstance(familyBodyType),familySegment=Activator.CreateInstance(familyBodyType);
   foreach(var pair in new[]{(familyHead,0),(familyPrevious,1),(familySegment,2)}){
    var entity=npcTable.GetValue(pair.Item2);pair.Item1.GetType().GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(pair.Item1,entity);nativeTargetNpcType.GetProperty("ModNPC").SetValue(entity,pair.Item1);
    nativeTargetNpcType.GetField("active").SetValue(entity,true);nativeTargetNpcType.GetField("life").SetValue(entity,70);nativeTargetNpcType.GetField("whoAmI").SetValue(entity,pair.Item2);nativeTargetNpcType.GetField("type").SetValue(entity,pair.Item2==0?50:51);nativeTargetNpcType.GetField("target").SetValue(entity,0);nativeTargetNpcType.GetField("realLife").SetValue(entity,0);
    var segmentAi=(float[])nativeTargetNpcType.GetField("ai").GetValue(entity);Array.Clear(segmentAi);segmentAi[0]=pair.Item2==2?1:0;
   }
   if(family=="ShatteredJadeWyrmMinion")familyHeadType.GetMethod("OnSpawn").Invoke(familyHead,new[]{Activator.CreateInstance(nativeParentSourceType,new object[]{parentNpc,null})});
   compiledLinkType.GetMethod("Bind",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(familyPrevious,new[]{familyHead,familyHead});compiledLinkType.GetMethod("Bind",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(familySegment,new[]{familyHead,familyPrevious});
   var currentEntity=npcTable.GetValue(2);object[] linkageArgs={currentEntity,null};object[] familyContact={nativeTargetPlayers.GetValue(0),0};
   Check((bool)compiledWormAI.GetMethod("HasValidLinks").Invoke(null,linkageArgs),"Official "+family+" linkage accepts correct instance chain");Check((bool)familyBodyType.GetMethod("CanHitPlayer").Invoke(familySegment,familyContact),"Official "+family+" correct chain contact allowed");
   foreach(bool replaceHead in new[]{false,true}){
    int slot=replaceHead?0:1;var replacementType=replaceHead?familyHeadType:familyBodyType;object replaced=replaceHead?familyHead:familyPrevious,replacementMod=Activator.CreateInstance(replacementType),replacementEntity=npcTable.GetValue(slot);
    replacementType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(replacementMod,replacementEntity);nativeTargetNpcType.GetProperty("ModNPC").SetValue(replacementEntity,replacementMod);
    if(!replaceHead)compiledLinkType.GetMethod("Bind",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(replacementMod,new[]{familyHead,familyHead});
    if(replaceHead&&family=="ShatteredJadeWyrmMinion")familyHeadType.GetMethod("OnSpawn").Invoke(replacementMod,new[]{Activator.CreateInstance(nativeParentSourceType,new object[]{parentNpc,null})});
    Check(!(bool)compiledWormAI.GetMethod("HasValidLinks").Invoke(null,linkageArgs),"Official "+family+" same-slot generation replacement rejected");Check(!(bool)familyBodyType.GetMethod("CanHitPlayer").Invoke(familySegment,familyContact),"Official "+family+" stale generation cannot contact before AI");
    compiledWormAI.GetMethod("FollowPreviousSegment").Invoke(null,new object[]{currentEntity,18f,0f,0f,0f,50});Check(!(bool)nativeTargetNpcType.GetField("active").GetValue(currentEntity)&&(int)nativeTargetNpcType.GetField("damage").GetValue(currentEntity)==0,"Official "+family+" stale generation clears without reward hooks");
    nativeTargetNpcType.GetProperty("ModNPC").SetValue(replacementEntity,replaced);nativeTargetNpcType.GetField("active").SetValue(currentEntity,true);
   }
  }
 } finally {nativeTargetMain.GetField("npc").SetValue(null,nativeOldNpcTable);}
 var compiledSplitType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.SpiritVeinWyrm",true);
 Check((int)compiledSplitType.GetField("SplitRetryInterval").GetRawConstantValue()==60,"Compiled split retries every sixty ticks");
 foreach(int splitScenario in new[]{0,1,2,3,4}){
  object splitBoss=Activator.CreateInstance(compiledSplitType),splitEntity=Activator.CreateInstance(nativeTargetNpcType);compiledSplitType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(splitBoss,splitEntity);
  nativeTargetNpcType.GetField("active").SetValue(splitEntity,splitScenario!=2);nativeTargetNpcType.GetField("life").SetValue(splitEntity,splitScenario==3?0:100);nativeTargetNpcType.GetField("target").SetValue(splitEntity,splitScenario==4?int.MaxValue:0);nativeTargetMain.GetField("netMode").SetValue(null,splitScenario==1?1:0);
  compiledSplitType.GetMethod("SpawnSplitMinions",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(splitBoss,new object[]{splitScenario!=0});
  Check((int)compiledSplitType.GetField("plannedChildren",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(splitBoss)==0,"Compiled invalid battle/client/first phase never starts quota");
 }
 nativeTargetMain.GetField("netMode").SetValue(null,0);
 var sessionProperty=type.GetProperty("TribulationSession",BindingFlags.Instance|BindingFlags.NonPublic);object sessionCultivation=Activator.CreateInstance(type),sessionOwner=Activator.CreateInstance(nativeTargetPlayerType);type.GetMethod("Initialize").Invoke(sessionCultivation,null);type.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(sessionCultivation,sessionOwner);
 int localPlayer=(int)nativeTargetMain.GetField("myPlayer").GetValue(null);nativeTargetPlayerType.GetField("whoAmI").SetValue(sessionOwner,localPlayer==nativeTargetMax-1?nativeTargetMax-2:nativeTargetMax-1);
 type.GetField("tribulationTimer").SetValue(sessionCultivation,120);long restoredSession=(long)sessionProperty.GetValue(sessionCultivation);Check(restoredSession>0&&(long)sessionProperty.GetValue(sessionCultivation)==restoredSession,"Compiled restored active tribulation allocates stable authority session");
 object foundationStage=Enum.Parse(type.GetField("cultivationStage").FieldType,"Foundation");var beginSession=type.GetMethod("BeginTribulation",BindingFlags.Instance|BindingFlags.NonPublic);beginSession.Invoke(sessionCultivation,new[]{foundationStage});long firstAttempt=(long)sessionProperty.GetValue(sessionCultivation);Check(firstAttempt>0&&firstAttempt!=restoredSession,"Compiled beginning tribulation replaces prior session");beginSession.Invoke(sessionCultivation,new[]{foundationStage});long secondAttempt=(long)sessionProperty.GetValue(sessionCultivation);Check(secondAttempt!=firstAttempt,"Compiled same-stage restart cannot inherit old hazards");
 type.GetField("tribulationTimer").SetValue(sessionCultivation,0);Check((long)sessionProperty.GetValue(sessionCultivation)==0,"Compiled ended/failed tribulation has no active session");
 int foundationValue=Convert.ToInt32(foundationStage);type.GetMethod("LoadData").Invoke(sessionCultivation,new[]{Tag(("cultivationStage",foundationValue),("tribulationStage",foundationValue),("tribulationTimer",120))});Check((long)sessionProperty.GetValue(sessionCultivation)>0&&(long)sessionProperty.GetValue(sessionCultivation)!=secondAttempt,"Compiled load starts fresh active identity without persisting old session");
 nativeTargetMain.GetField("netMode").SetValue(null,1);object clientSession=Activator.CreateInstance(type);type.GetMethod("Initialize").Invoke(clientSession,null);type.GetField("tribulationTimer").SetValue(clientSession,120);Check((long)sessionProperty.GetValue(clientSession)==0,"Compiled client does not allocate authority session");type.GetMethod("Initialize").Invoke(sessionCultivation,null);Check((long)sessionProperty.GetValue(sessionCultivation)==0,"Compiled initialization resets active session");
 // Joining a new character on a reused ModPlayer must invalidate old trial sources.
 nativeTargetMain.GetField("netMode").SetValue(null,2);
 object rejoinCultivation=Activator.CreateInstance(type),rejoinOwner=Activator.CreateInstance(nativeTargetPlayerType);
 type.GetMethod("Initialize").Invoke(rejoinCultivation,null);type.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(rejoinCultivation,rejoinOwner);
 nativeTargetPlayerType.GetField("active").SetValue(rejoinOwner,true);
 type.GetMethod("LoadData").Invoke(rejoinCultivation,new[]{Tag(("cultivationStage",foundationValue),("tribulationStage",foundationValue),("tribulationTimer",120))});
 object rejoinSnapshot=type.GetMethod("CaptureSnapshot").Invoke(rejoinCultivation,null);long beforeJoin=(long)sessionProperty.GetValue(rejoinCultivation);
 Check((bool)type.GetMethod("TryInitializeNetwork").Invoke(rejoinCultivation,new[]{rejoinSnapshot})&&(long)sessionProperty.GetValue(rejoinCultivation)!=beforeJoin&&(int)type.GetField("tribulationTimer").GetValue(rejoinCultivation)==120,"Actual initial character import expires previously allocated trial identity while preserving saved time");
 long acceptedJoin=(long)sessionProperty.GetValue(rejoinCultivation);
 Check(!(bool)type.GetMethod("TryInitializeNetwork").Invoke(rejoinCultivation,new[]{rejoinSnapshot})&&(long)sessionProperty.GetValue(rejoinCultivation)==acceptedJoin,"Rejected duplicate import cannot replace current live trial identity");
 var rejoinSessionField=type.GetField("tribulationSession",BindingFlags.Instance|BindingFlags.NonPublic);
 for(int reconnect=0;reconnect<3;reconnect++) {
  long oldIdentity=(long)sessionProperty.GetValue(rejoinCultivation);type.GetMethod("ResetNetworkSession").Invoke(rejoinCultivation,null);
  Check((long)rejoinSessionField.GetValue(rejoinCultivation)==0&&!(bool)type.GetProperty("NetworkInitialized").GetValue(rejoinCultivation),"Actual slot reset discards cached trial source identity before new import");
  Check((bool)type.GetMethod("TryInitializeNetwork").Invoke(rejoinCultivation,new[]{rejoinSnapshot})&&(long)sessionProperty.GetValue(rejoinCultivation)>0&&(long)sessionProperty.GetValue(rejoinCultivation)!=oldIdentity&&(int)type.GetField("tribulationTimer").GetValue(rejoinCultivation)==120,"Actual same-owner reconnect cannot inherit prior trial session identity");
 }
 type.GetMethod("ResetNetworkSession").Invoke(rejoinCultivation,null);long pendingIdentity=(long)sessionProperty.GetValue(rejoinCultivation);
 object invalidJoin=Activator.CreateInstance(snapshotType);snapshotType.GetProperty("Energy").SetValue(invalidJoin,-1);
 Check(!(bool)type.GetMethod("TryInitializeNetwork").Invoke(rejoinCultivation,new[]{invalidJoin})&&(long)sessionProperty.GetValue(rejoinCultivation)==pendingIdentity,"Malformed joining snapshot leaves existing trial identity unchanged");
 Check((bool)type.GetMethod("TryInitializeNetwork").Invoke(rejoinCultivation,new[]{rejoinSnapshot})&&(long)sessionProperty.GetValue(rejoinCultivation)!=pendingIdentity,"Valid import after rejection receives its own fresh authority trial session");
 // Actual compiled Moonbone warning captures authority target identity; replacement
 // cancels before any unregistered projectile can be created in this isolated process.
 var nativeMoonRingType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.MoonboneImmortal",true);
 var nativeMoonUpdate=nativeMoonRingType.GetMethod("UpdateMoonRing",BindingFlags.Instance|BindingFlags.NonPublic);
 foreach(int moonMode in new[]{0,2})foreach(bool moonRecovery in new[]{false,true})foreach(bool moonReplace in new[]{false,true}) {
  object nativeMoon=Activator.CreateInstance(nativeMoonRingType),nativeMoonNpc=Activator.CreateInstance(nativeTargetNpcType);
  nativeMoonRingType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(nativeMoon,nativeMoonNpc);
  nativeTargetNpcType.GetField("active").SetValue(nativeMoonNpc,true);nativeTargetNpcType.GetField("life").SetValue(nativeMoonNpc,100);nativeTargetNpcType.GetField("lifeMax").SetValue(nativeMoonNpc,100);nativeTargetNpcType.GetField("target").SetValue(nativeMoonNpc,0);
  var nativeMoonAi=(float[])nativeTargetNpcType.GetField("ai").GetValue(nativeMoonNpc);nativeMoonAi[2]=225;
  object nativeMoonTarget=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(nativeMoonTarget,true);nativeTargetPlayers.SetValue(nativeMoonTarget,0);
  nativeTargetMain.GetField("netMode").SetValue(null,moonMode);nativeMoonUpdate.Invoke(nativeMoon,new[]{nativeMoonTarget,(object)false,false});
  if(moonRecovery){nativeMoonAi[1]=0;nativeMoonAi[2]=-30;}
  object replacement=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(replacement,true);
  if(moonReplace)nativeTargetPlayers.SetValue(replacement,0);else{nativeTargetPlayers.SetValue(replacement,1);nativeTargetNpcType.GetField("target").SetValue(nativeMoonNpc,1);}
  nativeTargetNpcType.GetField("netUpdate").SetValue(nativeMoonNpc,false);nativeTargetNpcType.GetField("velocity").SetValue(nativeMoonNpc,Activator.CreateInstance(nativeTargetVectorType,new object[]{5f,5f}));
  Check((bool)nativeMoonUpdate.Invoke(nativeMoon,new[]{replacement,(object)false,false}),"Compiled Moonbone target replacement consumes cancellation frame");
  Check(nativeMoonAi.All(v=>v==0)&&(bool)nativeTargetNpcType.GetField("netUpdate").GetValue(nativeMoonNpc)&&nativeTargetNpcType.GetField("velocity").GetValue(nativeMoonNpc).Equals(Activator.CreateInstance(nativeTargetVectorType)),"Compiled Moonbone clears old warning and velocity with native dirty flag");
  object[] moonHitArguments={replacement,0};Check(!(bool)nativeMoonRingType.GetMethod("CanHitPlayer").Invoke(nativeMoon,moonHitArguments),"Compiled Moonbone cancellation frame denies native contact");
 }
 // Exercise compiled healing without a graphical world. For native packet receives,
 // occupied combat-text slots suppress rendering while leaving health parsing real.
 var nativeHealingType=type.Assembly.GetType("XianXia.Common.Systems.AuthoritativeHealing",true);
 var nativeHealingApply=nativeHealingType.GetMethod("Apply");
 object nativeHealingPatient=nativeTargetPlayers.GetValue(0);
 nativeTargetPlayerType.GetField("whoAmI").SetValue(nativeHealingPatient,0);nativeTargetPlayerType.GetField("active").SetValue(nativeHealingPatient,true);nativeTargetPlayerType.GetField("dead").SetValue(nativeHealingPatient,false);
 nativeTargetPlayerType.GetField("statLife").SetValue(nativeHealingPatient,95);nativeTargetPlayerType.GetField("statLifeMax2").SetValue(nativeHealingPatient,100);
 var nativeHealingMyPlayer=nativeTargetMain.GetField("myPlayer");object nativeHealingOldMyPlayer=nativeHealingMyPlayer.GetValue(null);
 var nativeHealingTextField=nativeTargetMain.GetField("combatText");object nativeHealingOldText=nativeHealingTextField.GetValue(null);
 var nativeHealingNetplay=tagType.Assembly.GetType("Terraria.Netplay",true);var nativeHealingConnection=nativeHealingNetplay.GetField("Connection");object nativeHealingOldConnection=nativeHealingConnection.GetValue(null);
 try {
  nativeHealingMyPlayer.SetValue(null,255);nativeTargetMain.GetField("netMode").SetValue(null,0);
  Check((int)nativeHealingApply.Invoke(null,new[]{nativeHealingPatient,(object)20})==5&&(int)nativeTargetPlayerType.GetField("statLife").GetValue(nativeHealingPatient)==100,"Compiled authority healing caps actual single-player engine heal");
  Check((int)nativeHealingApply.Invoke(null,new[]{nativeHealingPatient,(object)20})==0,"Compiled full-life healing is a no-op");
  nativeTargetPlayerType.GetField("statLife").SetValue(nativeHealingPatient,50);nativeTargetMain.GetField("netMode").SetValue(null,1);
  Check((int)nativeHealingApply.Invoke(null,new[]{nativeHealingPatient,(object)20})==0&&(int)nativeTargetPlayerType.GetField("statLife").GetValue(nativeHealingPatient)==50,"Compiled multiplayer client cannot apply authority heal");
  nativeTargetMain.GetField("netMode").SetValue(null,2);nativeTargetPlayerType.GetField("dead").SetValue(nativeHealingPatient,true);
  Check((int)nativeHealingApply.Invoke(null,new[]{nativeHealingPatient,(object)20})==0,"Compiled dead player receives no authority heal");nativeTargetPlayerType.GetField("dead").SetValue(nativeHealingPatient,false);
  Check((int)nativeHealingApply.Invoke(null,new[]{nativeHealingPatient,(object)-1})==0,"Compiled negative heal is rejected");
  Check((int)nativeHealingApply.Invoke(null,new object[]{null,20})==0,"Compiled null target is rejected");
  var nativeHealingTextType=tagType.Assembly.GetType("Terraria.CombatText",true);var nativeHealingText=Array.CreateInstance(nativeHealingTextType,100);
  for(int index=0;index<100;index++){object occupied=Activator.CreateInstance(nativeHealingTextType);nativeHealingTextType.GetField("active").SetValue(occupied,true);nativeHealingText.SetValue(occupied,index);}nativeHealingTextField.SetValue(null,nativeHealingText);
  nativeHealingConnection.SetValue(null,Activator.CreateInstance(tagType.Assembly.GetType("Terraria.RemoteServer",true)));
  var nativeHealingBufferType=tagType.Assembly.GetType("Terraria.MessageBuffer",true);
  foreach(int receivingPlayer in new[]{0,1}) {
   nativeTargetMain.GetField("netMode").SetValue(null,1);nativeHealingMyPlayer.SetValue(null,receivingPlayer);nativeTargetPlayerType.GetField("statLife").SetValue(nativeHealingPatient,95);
   object nativeHealingBuffer=Activator.CreateInstance(nativeHealingBufferType);nativeHealingBufferType.GetField("whoAmI").SetValue(nativeHealingBuffer,256);byte[] nativeHealingBytes={66,0,5,0};
   nativeHealingBufferType.GetField("readBuffer").SetValue(nativeHealingBuffer,nativeHealingBytes);
   using var nativeHealingReader=new BinaryReader(new MemoryStream(nativeHealingBytes));nativeHealingBufferType.GetField("reader").SetValue(nativeHealingBuffer,nativeHealingReader);
   object[] receiveArguments={0,4,0};nativeHealingBufferType.GetMethod("GetData").Invoke(nativeHealingBuffer,receiveArguments);
   Check((int)receiveArguments[2]==66&&(int)nativeTargetPlayerType.GetField("statLife").GetValue(nativeHealingPatient)==100,"Official SpiritHeal packet increments owner and observer player copies exactly once");
  }
 } finally {nativeHealingMyPlayer.SetValue(null,nativeHealingOldMyPlayer);nativeHealingTextField.SetValue(null,nativeHealingOldText);nativeHealingConnection.SetValue(null,nativeHealingOldConnection);}
 // Compiled persistent fields reject bad geometry before client prediction or contact.
 nativeTargetMain.GetField("netMode").SetValue(null,1);
 var nativeFieldOwnerRules=type.Assembly.GetType("XianXia.Common.Projectiles.FriendlyFieldOwner",true).GetMethod("Find",BindingFlags.Static|BindingFlags.NonPublic);
 foreach(string fieldName in new[]{"GreenwoodArrayField","ThunderTalismanArray","MedicineCauldronField"})foreach(int invalidField in Enumerable.Range(0,8)) {
  object safeOwner=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(safeOwner,true);nativeTargetPlayers.SetValue(safeOwner,0);
  var realFieldType=type.Assembly.GetType("XianXia.Content.Projectiles."+fieldName,true);object realField=Activator.CreateInstance(realFieldType),realFieldProjectile=Activator.CreateInstance(projectileType);realFieldType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(realField,realFieldProjectile);realFieldType.GetMethod("SetDefaults").Invoke(realField,null);
  projectileType.GetField("owner").SetValue(realFieldProjectile,0);projectileType.GetField("active").SetValue(realFieldProjectile,true);
  switch(invalidField){case 0:nativeTargetPlayers.SetValue(null,0);break;case 1:nativeTargetPlayerType.GetProperty("Center").SetValue(safeOwner,Activator.CreateInstance(nativeTargetVectorType,new object[]{float.NaN,0f}));break;case 2:nativeTargetPlayerType.GetProperty("Center").SetValue(safeOwner,Activator.CreateInstance(nativeTargetVectorType,new object[]{float.PositiveInfinity,0f}));break;case 3:projectileType.GetProperty("Center").SetValue(realFieldProjectile,Activator.CreateInstance(nativeTargetVectorType,new object[]{float.NaN,0f}));break;case 4:projectileType.GetField("velocity").SetValue(realFieldProjectile,Activator.CreateInstance(nativeTargetVectorType,new object[]{float.NaN,0f}));break;case 5:projectileType.GetField("timeLeft").SetValue(realFieldProjectile,0);break;case 6:projectileType.GetField("timeLeft").SetValue(realFieldProjectile,fieldName=="ThunderTalismanArray"?241:301);break;case 7:nativeTargetPlayerType.GetProperty("Center").SetValue(safeOwner,Activator.CreateInstance(nativeTargetVectorType,new object[]{float.MaxValue,0f}));break;}
  Check(realFieldType.GetMethod("CanDamage").Invoke(realField,null) is false,"Actual persistent field rejects invalid owner geometry and lifetime before contact");
  if(fieldName=="GreenwoodArrayField")Check(!(bool)realFieldType.GetMethod("CanPulse",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(realField,new[]{safeOwner}),"Actual invalid array cannot authorize healing pulse");
  realFieldType.GetMethod("AI").Invoke(realField,null);Check((bool)projectileType.GetField("active").GetValue(realFieldProjectile),"Actual invalid-field client waits for authority removal");
  if(fieldName!="MedicineCauldronField"&&invalidField>=3&&invalidField<=6){projectileType.GetProperty("Center").SetValue(realFieldProjectile,Activator.CreateInstance(nativeTargetVectorType));projectileType.GetField("velocity").SetValue(realFieldProjectile,Activator.CreateInstance(nativeTargetVectorType));projectileType.GetField("timeLeft").SetValue(realFieldProjectile,fieldName=="ThunderTalismanArray"?240:300);Check(realFieldType.GetMethod("CanDamage").Invoke(realField,null) is false,"Actual repaired field cannot regain latched unsafe contact");if(fieldName=="GreenwoodArrayField")Check(!(bool)realFieldType.GetMethod("CanPulse",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(realField,new[]{safeOwner}),"Actual repaired cancelled array cannot authorize a pulse");}
 }
 foreach(string fieldName in new[]{"GreenwoodArrayField","ThunderTalismanArray","MedicineCauldronField"})foreach(float fieldDistance in new[]{1599f,1600f,1601f}) {
  object safeOwner=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(safeOwner,true);nativeTargetPlayerType.GetProperty("Center").SetValue(safeOwner,Activator.CreateInstance(nativeTargetVectorType));nativeTargetPlayers.SetValue(safeOwner,0);
  var realFieldType=type.Assembly.GetType("XianXia.Content.Projectiles."+fieldName,true);object realField=Activator.CreateInstance(realFieldType),realFieldProjectile=Activator.CreateInstance(projectileType);realFieldType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(realField,realFieldProjectile);realFieldType.GetMethod("SetDefaults").Invoke(realField,null);projectileType.GetField("owner").SetValue(realFieldProjectile,0);projectileType.GetProperty("Center").SetValue(realFieldProjectile,Activator.CreateInstance(nativeTargetVectorType,new object[]{fieldDistance,0f}));
  Check(ReferenceEquals(nativeFieldOwnerRules.Invoke(null,new[]{realFieldProjectile,(object)(fieldName=="ThunderTalismanArray"?240:300)}),safeOwner)==(fieldDistance<=1600),"Actual persistent field owner guard includes exact leash boundary");
 }
 foreach(float pulseDistance in new[]{159f,160f,161f}) {
  object pulseOwner=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(pulseOwner,true);nativeTargetPlayerType.GetProperty("Center").SetValue(pulseOwner,Activator.CreateInstance(nativeTargetVectorType));nativeTargetPlayers.SetValue(pulseOwner,0);
  var pulseType=type.Assembly.GetType("XianXia.Content.Projectiles.GreenwoodArrayField",true);object actualPulse=Activator.CreateInstance(pulseType),pulseEntity=Activator.CreateInstance(projectileType);pulseType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(actualPulse,pulseEntity);pulseType.GetMethod("SetDefaults").Invoke(actualPulse,null);projectileType.GetField("owner").SetValue(pulseEntity,0);projectileType.GetField("active").SetValue(pulseEntity,true);projectileType.GetProperty("Center").SetValue(pulseEntity,Activator.CreateInstance(nativeTargetVectorType,new object[]{pulseDistance,0f}));
  Check((bool)pulseType.GetMethod("CanPulse",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actualPulse,new[]{pulseOwner})==(pulseDistance<=160),"Actual healthy array pulse includes exact 160 pixel boundary");
 }
 // Compiled cloud summon guard: only rejected paths, with no native NPC registration/world.
 var nativeCloudType=type.Assembly.GetType("XianXia.Content.NPCs.Bosses.TribulationCloudAvatar",true);
 var nativeCloudSpawn=nativeCloudType.GetMethod("SpawnCloudAdd",BindingFlags.Instance|BindingFlags.NonPublic);
 var nativeCloudCreated=nativeCloudType.GetField("cloudAddCreated",BindingFlags.Instance|BindingFlags.NonPublic);
 foreach(int rejectedCloud in Enumerable.Range(0,9)) {
  object cloudOwner=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(cloudOwner,true);nativeTargetPlayers.SetValue(cloudOwner,0);
  object cloud=Activator.CreateInstance(nativeCloudType),cloudNpc=Activator.CreateInstance(nativeTargetNpcType);nativeCloudType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(cloud,cloudNpc);
  nativeTargetNpcType.GetField("active").SetValue(cloudNpc,true);nativeTargetNpcType.GetField("life").SetValue(cloudNpc,100);nativeTargetNpcType.GetField("target").SetValue(cloudNpc,0);nativeTargetMain.GetField("netMode").SetValue(null,2);
  switch(rejectedCloud){case 0:nativeTargetMain.GetField("netMode").SetValue(null,1);break;case 1:nativeTargetNpcType.GetField("active").SetValue(cloudNpc,false);break;case 2:nativeTargetNpcType.GetField("life").SetValue(cloudNpc,0);break;case 3:nativeTargetNpcType.GetField("target").SetValue(cloudNpc,-1);break;case 4:nativeTargetNpcType.GetField("target").SetValue(cloudNpc,255);break;case 5:nativeTargetPlayerType.GetField("dead").SetValue(cloudOwner,true);break;case 6:nativeTargetPlayerType.GetField("active").SetValue(cloudOwner,false);break;case 7:nativeTargetPlayerType.GetProperty("Center").SetValue(cloudOwner,Activator.CreateInstance(nativeTargetVectorType,new object[]{5000f,0f}));break;case 8:nativeTargetNpcType.GetProperty("Center").SetValue(cloudNpc,Activator.CreateInstance(nativeTargetVectorType,new object[]{float.NaN,0f}));break;}
  nativeCloudSpawn.Invoke(cloud,null);Check(!(bool)nativeCloudCreated.GetValue(cloud),"Compiled cloud rejected summon path retains missing quota without native creation");
 }
 var compiledCloudCharge=nativeCloudType.GetMethod("UpdateCloudCharge",BindingFlags.Instance|BindingFlags.NonPublic);
 foreach(int cloudChargeMode in new[]{0,2}) {
  object chargeOwner=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(chargeOwner,true);nativeTargetPlayers.SetValue(chargeOwner,0);
  object chargeBoss=Activator.CreateInstance(nativeCloudType),chargeNpc=Activator.CreateInstance(nativeTargetNpcType);nativeCloudType.GetProperty("Entity",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(chargeBoss,chargeNpc);
  nativeTargetNpcType.GetField("active").SetValue(chargeNpc,true);nativeTargetNpcType.GetField("life").SetValue(chargeNpc,100);nativeTargetNpcType.GetField("target").SetValue(chargeNpc,0);var chargeAi=(float[])nativeTargetNpcType.GetField("ai").GetValue(chargeNpc);chargeAi[1]=209;nativeTargetMain.GetField("netMode").SetValue(null,cloudChargeMode);
  for(int chargeFrame=1;chargeFrame<=45;chargeFrame++) {
   Check((bool)compiledCloudCharge.Invoke(chargeBoss,new[]{chargeOwner,(object)true}),"Compiled cloud consumes entire warning frame without overlapping attacks");object[] hitArgs={chargeOwner,0};Check(!(bool)nativeCloudType.GetMethod("CanHitPlayer").Invoke(chargeBoss,hitArgs),"Compiled cloud warning refuses native contact");
  }
  object changedOwner=Activator.CreateInstance(nativeTargetPlayerType);nativeTargetPlayerType.GetField("active").SetValue(changedOwner,true);nativeTargetPlayers.SetValue(changedOwner,0);compiledCloudCharge.Invoke(chargeBoss,new[]{changedOwner,(object)true});
  Check(chargeAi.All(v=>v==0)&&(bool)nativeTargetNpcType.GetField("netUpdate").GetValue(chargeNpc),"Compiled cloud owner replacement cancels locked warning and marks dirty");object[] cancellationHit={changedOwner,0};Check(!(bool)nativeCloudType.GetMethod("CanHitPlayer").Invoke(chargeBoss,cancellationHit),"Compiled cloud cancellation frame remains harmless");
 }
} finally {
 nativeTargetMain.GetField("player").SetValue(null,nativeTargetOldPlayers);
 nativeTargetMain.GetField("netMode").SetValue(null,nativeTargetOldMode);
 nativeTargetMain.GetField("dedServ").SetValue(null,nativeTargetOldDedicated);
}
Console.WriteLine($"Actual engine gameplay assertions including boss targets and worm instance links: {assertions}.");
