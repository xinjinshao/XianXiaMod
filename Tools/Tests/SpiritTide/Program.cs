using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using XianXia.Common.Systems;
int count=0;void Check(bool ok,string message){count++;if(!ok)throw new Exception(message);}
var system=ModContent.GetInstance<SpiritTideSystem>();
void Start(){Main.netMode=2;Main.dayTime=true;system.OnWorldLoad();Main.dayTime=false;system.PostUpdateWorld();}
Start();Check(SpiritTideSystem.Active&&SpiritTideSystem.Wave==1,"Eligible dusk starts three-wave event");
var pool=new Dictionary<int,float>{{0,1}};var hook=new SpiritTideEnemies();hook.EditSpawnPool(pool,new());Check(pool.Count==3&&!pool.ContainsKey(0),"Surface event selects three spirits");
var protectedPool=new Dictionary<int,float>{{0,1}};hook.EditSpawnPool(protectedPool,new NPCSpawnInfo{PlayerInTown=true});Check(protectedPool.ContainsKey(0),"Town spawn pool preserved");
void Kill(){var n=new NPC();var h=new SpiritTideEnemies();h.OnSpawn(n,new EntitySource_SpawnNPC());h.OnKill(n);h.OnKill(n);}
var boosted=new NPC();var boostedHook=new SpiritTideEnemies();boostedHook.OnSpawn(boosted,new EntitySource_SpawnNPC());Check(boosted.lifeMax==80&&boosted.damage==13&&boosted.defense==3&&boosted.netUpdate,"Wave one stats and native sync");
using(var memory=new MemoryStream()) {
 using(var writer=new BinaryWriter(memory,System.Text.Encoding.UTF8,true))boostedHook.SendExtraAI(boosted,new BitWriter(),writer);
 memory.Position=0;var remoteNpc=new NPC();var remoteHook=new SpiritTideEnemies();remoteHook.ReceiveExtraAI(remoteNpc,new BitReader(),new BinaryReader(memory));
 Check(remoteNpc.lifeMax==80&&remoteNpc.damage==13&&remoteNpc.defense==3,"Boosted combat stats reach clients through extra AI");
 Main.netMode=1;remoteHook.OnKill(remoteNpc);Check(SpiritTideSystem.Kills==0,"Synced enemy cannot credit client-side death");Main.netMode=2;
}
int before=SpiritTideSystem.Kills;foreach(var n in new[]{new NPC{SpawnedFromStatue=true},new NPC{Center=new(0,2000)},new NPC{type=99}}){var h=new SpiritTideEnemies();h.OnSpawn(n,new EntitySource_SpawnNPC());h.OnKill(n);}hook.OnSpawn(new NPC(),new OtherSource());hook.OnKill(new NPC());Check(SpiritTideSystem.Kills==before,"Statues underground unrelated and artificial enemies excluded");
Kill();Check(SpiritTideSystem.Kills==1,"Each tagged death credited once");
for(int i=1;i<12;i++)Kill();Check(SpiritTideSystem.Wave==2&&SpiritTideSystem.Kills==0&&SpiritTideSystem.RequiredKills==18,"Wave two transition");
for(int i=0;i<18;i++)Kill();Check(SpiritTideSystem.Wave==3&&SpiritTideSystem.RequiredKills==24,"Wave three transition");
var tag=new TagCompound();system.SaveWorldData(tag);Kill();system.LoadWorldData(tag);Check(SpiritTideSystem.Wave==3&&SpiritTideSystem.Kills==0,"Saved wave progress restored");
using(var memory=new MemoryStream()){using(var writer=new BinaryWriter(memory,System.Text.Encoding.UTF8,true))system.NetSend(writer);system.OnWorldUnload();memory.Position=0;system.NetReceive(new BinaryReader(memory));Check(SpiritTideSystem.Active&&SpiritTideSystem.Wave==3,"World network round trip");}
Main.netMode=1;Kill();system.PostUpdateWorld();Check(SpiritTideSystem.Kills==0,"Clients cannot progress event");Main.netMode=2;
Item.Drops.Clear();for(int i=0;i<24;i++)Kill();Check(!SpiritTideSystem.Active&&Item.Drops.Count==4&&Item.Drops.Any(d=>d.Type==4&&d.Amount==1),"Final wave drops unique pearl and supplies once");Kill();Check(Item.Drops.Count==4,"Post-completion deaths cannot reward twice");
Start();var oldNpc=new NPC();var oldHook=new SpiritTideEnemies();oldHook.OnSpawn(oldNpc,new EntitySource_SpawnNPC());Main.dayTime=true;system.PostUpdateWorld();Check(!SpiritTideSystem.Active,"Dawn terminates unfinished event");Main.dayTime=false;system.PostUpdateWorld();oldHook.OnKill(oldNpc);Check(SpiritTideSystem.Kills==0,"Old-run enemy cannot credit next night");
foreach(var invalid in new[]{new TagCompound{{"spiritTideActive",true},{"spiritTideWave",99},{"spiritTideKills",5}},new TagCompound{{"spiritTideActive",false},{"spiritTideWave",3},{"spiritTideKills",24}}}){system.LoadWorldData(invalid);Check(!SpiritTideSystem.Active&&SpiritTideSystem.Kills==0,"Invalid save resets safely");}
foreach(Action blocker in new Action[]{()=>Main.bloodMoon=true,()=>Main.pumpkinMoon=true,()=>Main.snowMoon=true,()=>Main.invasionType=1,()=>Main.hardMode=false,()=>Main.player[0].dead=true,()=>Main.npc=new[]{new NPC{boss=true}}}){
 Main.bloodMoon=Main.pumpkinMoon=Main.snowMoon=false;Main.invasionType=0;Main.hardMode=true;Main.player[0].dead=false;Main.npc=Array.Empty<NPC>();Main.dayTime=true;system.OnWorldLoad();blocker();Main.dayTime=false;system.PostUpdateWorld();Check(!SpiritTideSystem.Active,"Conflicting event/progression/player/boss blocks start");
}
var pearl=new XianXia.Content.Items.Accessories.SpiritTidePearl();pearl.SetDefaults();Main.player[0].statManaMax2=100;pearl.UpdateAccessory(Main.player[0],false);Check(pearl.Item.accessory&&Main.player[0].statManaMax2==120&&Main.player[0].State.spiritualEnergyRegenBonus==1,"Actual unique reward accessory effect");
Console.WriteLine($"Spirit Tide actual world/enemy/reward hooks passed: {count} assertions; native spawns, world transport, HUD and loot engine mocked.");
