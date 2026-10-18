using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using XianXia.Common.Players;
using XianXia.Common.Systems;
using Game = Terraria.Main;

int assertions = 0;
void Check(bool condition, string message)
{
    assertions++;
    if (!condition) throw new Exception(message);
}
Player MakePlayer(int x, int y, int index)
{
    var player = new Player { whoAmI = index, Center = new(x * 16, y * 16) };
    player.Biome = new ServerBiomePlayer { Player = player };
    return player;
}
Game.netMode = NetmodeID.Server;
var first = MakePlayer(20, 20, 0);
var second = MakePlayer(100, 20, 1);
for (int x = 10; x < 30; x++)
    for (int y = 10; y < 30; y++) Game.tile[x,y] = new Tile { HasTile = true, TileType = 3 };
Check(first.Biome.Count(3) == 400 && second.Biome.Count(3) == 0, "Dedicated server scans must be independent for distant players.");
Game.tile[10,10].HasTile = false;
Check(first.Biome.Count(3) == 400, "Repeated biome queries reuse the cached scan.");
Game.GameUpdateCount = 60;
Check(first.Biome.Count(3) == 399, "Tile changes refresh after the cache interval.");
first.Center = new(100 * 16, 20 * 16);
Check(first.Biome.Count(3) == 0, "Teleporting invalidates the scan immediately.");
first.Center = new(0,0);
Game.tile[0,0] = new Tile { HasTile = true, TileType = 3 };
Check(first.Biome.Count(3) == 1, "World-edge scans clamp safely.");
Check(first.Biome.Count(-1) == 0 && first.Biome.Count(999) == 0, "Invalid tile IDs are harmless.");
first.active = false;
Check(first.Biome.Count(3) == 0, "Inactive players cannot activate server biomes.");
first.active = true;

var lightning = new AmbientLightningSystem();
lightning.ClearWorld();
Game.GameUpdateCount = 120;
first.Thunder = second.Thunder = true;
first.Center = new(100,100); second.Center = new(120,100);
var effects1 = new BiomeEffectsPlayer { Player = first, thunderCloudTimer = 419 };
var effects2 = new BiomeEffectsPlayer { Player = second, thunderCloudTimer = 419 };
effects1.PostUpdate(); effects2.PostUpdate();
Check(Projectile.Spawns == 1, "Nearby players produce one shared server lightning event, despite server myPlayer=255.");
second.Center = new(1500,100);
effects2.thunderCloudTimer = 419; effects2.PostUpdate();
Check(Projectile.Spawns == 2, "Distant groups retain independent environmental strikes.");
Game.GameUpdateCount += 420;
effects1.thunderCloudTimer = 419; effects1.PostUpdate();
Check(Projectile.Spawns == 3, "Shared strike cooldown expires normally.");
first.Rift = first.Moonbone = true;
Game.GameUpdateCount = 720;
effects1.starAbyssCorruption = 1800;
effects1.PostUpdate();
Check(first.State.spiritPressure == 2 && Dust.Spawns == 0, "Server applies environmental pressure without client Dust.");
Game.netMode = NetmodeID.MultiplayerClient;
Game.myPlayer = 0; Game.dedServ = false;
effects1.thunderCloudTimer = 419;
int pressure = first.State.spiritPressure;
int corruption = effects1.starAbyssCorruption;
effects1.PostUpdate();
Check(Projectile.Spawns == 3 && first.State.spiritPressure == pressure && effects1.starAbyssCorruption == corruption,
    "Client cannot generate hazards or mutate authoritative environmental pressure.");
Check(Dust.Spawns == 2, "Local client retains both biome visual effects.");
first.dead = true;
effects1.PostUpdate();
Check(Dust.Spawns == 2, "Dead players do not produce environmental effects.");
Console.WriteLine($"Biome/environment regression passed: {assertions} assertions against actual scan/effect sources, with engine boundary stubs.");

// Execute actual array AI with server myPlayer=255 and an owner inside the field.
Game.netMode = NetmodeID.Server;
Game.myPlayer = 255; Game.dedServ = true;
first.dead = false; first.position = Vector2.Zero; first.statLife = 50;
Game.player[0] = first; Game.player[1] = second;
var green = new XianXia.Content.Projectiles.GreenwoodArrayField();
var overlap = new XianXia.Content.Projectiles.GreenwoodArrayField();
green.SetDefaults(); overlap.SetDefaults();
green.Projectile.owner = overlap.Projectile.owner = 0;
green.OnSpawn(new Terraria.DataStructures.EntitySource_Parent { Entity=first }); overlap.OnSpawn(new Terraria.DataStructures.EntitySource_Parent { Entity=first });
Game.GameUpdateCount = 60;
green.AI(); overlap.AI();
Check(first.statLife == 51 && first.State.spiritualEnergy == 1 && NetMessage.HealMessages == 1,
    "Dedicated server array recovery executes once per owner, including healing notification, despite overlapping fields.");
Game.netMode = NetmodeID.MultiplayerClient;
Game.GameUpdateCount = 120;
green.AI();
Check(first.statLife == 51 && first.State.spiritualEnergy == 1, "Client array AI cannot independently heal or restore energy.");
Game.netMode = NetmodeID.Server;
green.AI();
Check(first.statLife == 52 && first.State.spiritualEnergy == 2, "Later server recovery interval remains available.");
first.dead = true; Game.GameUpdateCount = 180; green.AI();
Check(first.statLife == 52 && first.State.spiritualEnergy == 2, "Dead array owner does not recover.");
first.dead = false; green = new XianXia.Content.Projectiles.GreenwoodArrayField(); green.SetDefaults(); green.Projectile.owner=0; green.Projectile.active=true; green.OnSpawn(new Terraria.DataStructures.EntitySource_Parent {Entity=first}); first.position = new(500,500); Game.GameUpdateCount = 240; green.AI();
Check(first.statLife == 52, "Array does not heal an owner outside its hitbox.");
first.position = Vector2.Zero; first.statLife = 100; Game.GameUpdateCount = 300; green.AI();
Check(first.statLife == 100 && first.State.spiritualEnergy == 3 && NetMessage.HealMessages == 2,
    "Full health clamps healing and sends no extra heal packet while energy recovery continues.");
var thunder = new XianXia.Content.Projectiles.ThunderTalismanArray();
thunder.SetDefaults(); thunder.Projectile.owner = 0; thunder.OnSpawn(new Terraria.DataStructures.EntitySource_Parent {Entity=first}); thunder.Projectile.timeLeft = 45; thunder.Projectile.damage = 20;
int strikes = Projectile.Spawns;
thunder.AI();
Check(Projectile.Spawns == strikes + 1, "Server thunder array generates its periodic bolt without an owner-client condition.");
Game.netMode = NetmodeID.MultiplayerClient; thunder.AI();
Check(Projectile.Spawns == strikes + 1, "Client thunder array cannot duplicate the server bolt.");
Console.WriteLine($"Biome/environment/array regression passed: {assertions} assertions against actual source with engine boundary stubs.");
foreach(object damageClass in new[]{Terraria.ModLoader.DamageClass.Melee,Terraria.ModLoader.DamageClass.Magic}) {
 var bolt=new XianXia.Content.Projectiles.MinorThunderboltProjectile();bolt.SetDefaults();
 bolt.OnSpawn(new Terraria.DataStructures.EntitySource_Parent {Entity=new Projectile {DamageType=damageClass}});
 Check(ReferenceEquals(bolt.Projectile.DamageType,damageClass),"Shared lightning inherits its actual parent weapon class.");
 using var data=new MemoryStream();using(var writer=new BinaryWriter(data,System.Text.Encoding.UTF8,true))bolt.SendExtraAI(writer);
 var remote=new XianXia.Content.Projectiles.MinorThunderboltProjectile();remote.SetDefaults();data.Position=0;using var reader=new BinaryReader(data);remote.ReceiveExtraAI(reader);
 Check(data.Length==1 && ReferenceEquals(remote.Projectile.DamageType,damageClass),"Shared lightning damage class survives projectile synchronization.");
}
Console.WriteLine($"Biome/environment/array/class regression passed: {assertions} assertions against actual source with engine boundary stubs.");

Game.netMode=NetmodeID.Server;Game.GameUpdateCount=120; first.active=true;first.dead=false;first.Center=new(50*16,50*16);second.Center=new(150*16,50*16);
Game.tile[50,50]=new Tile{HasTile=true,TileType=11};Game.tile[51,50]=new Tile{HasTile=true,TileType=12};Game.tile[52,50]=new Tile{HasTile=true,TileType=12};Game.tile[53,50]=new Tile{HasTile=true,TileType=13};
first.State.spiritPressure=0;first.State.spiritualEnergyRegenBonus=0;first.AddedBuffs=0;second.State.spiritPressure=0;second.State.spiritualEnergyRegenBonus=0;second.AddedBuffs=0;
var object1=new BiomeObjectEffectsPlayer{Player=first};var object2=new BiomeObjectEffectsPlayer{Player=second};object1.PostUpdateEquips();object2.PostUpdateEquips();
Check(first.AddedBuffs==1&&first.State.spiritPressure==1&&first.State.spiritualEnergyRegenBonus==1,"Server effects apply once per player, independent of multiple thunder cells");
Check(second.AddedBuffs==0&&second.State.spiritPressure==0&&second.State.spiritualEnergyRegenBonus==0,"Distant player receives no object effects");
Game.netMode=NetmodeID.MultiplayerClient;first.State.spiritualEnergyRegenBonus=0;object1.PostUpdateEquips();Check(first.State.spiritualEnergyRegenBonus==0&&first.State.spiritPressure==1&&first.AddedBuffs==1,"Client never mutates resources or object buffs");
Game.netMode=NetmodeID.Server;first.dead=true;object1.PostUpdateEquips();Check(first.State.spiritualEnergyRegenBonus==0&&first.AddedBuffs==1,"Dead player receives no effects");first.dead=false;first.Center=second.Center;object1.PostUpdateEquips();Check(first.State.spiritualEnergyRegenBonus==0&&first.AddedBuffs==1,"Teleport immediately invalidates cached object effects");
first.Center=new(50*16,50*16);first.State.spiritPressure=100;object1.PostUpdateEquips();Check(first.State.spiritPressure==100,"Rift pressure clamps at cap");
Console.WriteLine($"Including nearby-object authority regression: {assertions} assertions.");

// Same player and same coordinates, different world tiles and a reset world clock.
var cacheLifecycle = new BiomeScanCacheSystem();
for(int x=30;x<=70;x++) for(int y=35;y<=65;y++) Game.tile[x,y]=default;
foreach (string lifecycle in new[]{"world load","world unload","disconnect","initialize","enter world"}) {
    var occupant=MakePlayer(50,50,0); Game.player[0]=occupant; Game.player[1]=null;
    var nearby=occupant.GetModPlayer<BiomeObjectEffectsPlayer>();
    Game.netMode=NetmodeID.Server; Game.GameUpdateCount=10000;
    Game.tile[50,50]=new Tile {HasTile=true,TileType=3};
    Game.tile[51,50]=new Tile {HasTile=true,TileType=(ushort)Terraria.ModLoader.ModContent.TileType<XianXia.Content.Tiles.SingingThunderStoneTile>()};
    Check(occupant.Biome.Count(3)>0,"Old world populated scan is primed");
    occupant.State.spiritualEnergyRegenBonus=0; nearby.PostUpdateEquips();
    Check(occupant.State.spiritualEnergyRegenBonus==1,"Old world thunder object scan is primed");
    Game.tile[50,50]=default; Game.tile[51,50]=default; Game.GameUpdateCount=0;
    Check(occupant.Biome.Count(3)>0,"Clock rollback alone cannot expire the old scan");
    occupant.State.spiritualEnergyRegenBonus=0; nearby.PostUpdateEquips();
    Check(occupant.State.spiritualEnergyRegenBonus==1,"Same-position old object cache demonstrates stale effect before lifecycle reset");
    switch(lifecycle) {
        case "world load": cacheLifecycle.OnWorldLoad(); break;
        case "world unload": cacheLifecycle.OnWorldUnload(); break;
        case "disconnect": occupant.active=false; cacheLifecycle.PostUpdatePlayers(); occupant.active=true; break;
        case "initialize": occupant.Biome.Initialize(); nearby.Initialize(); break;
        case "enter world": occupant.Biome.OnEnterWorld(); nearby.OnEnterWorld(); break;
    }
    Check(occupant.Biome.Count(3)==0,$"{lifecycle} discards prior world tile counts immediately");
    occupant.State.spiritualEnergyRegenBonus=0; nearby.PostUpdateEquips();
    Check(occupant.State.spiritualEnergyRegenBonus==0,$"{lifecycle} discards prior world object effects immediately");
}
foreach(string lifecycle in new[]{"world load","world unload","disconnect","initialize","enter world","inactive update"}) {
 var actor=MakePlayer(50,50,0); Game.player[0]=actor; var exposure=actor.GetModPlayer<BiomeEffectsPlayer>();
 exposure.starAbyssCorruption=3600; exposure.thunderCloudTimer=419; int spawned=Projectile.Spawns;
 switch(lifecycle) {
  case "world load": cacheLifecycle.OnWorldLoad(); break;
  case "world unload": cacheLifecycle.OnWorldUnload(); break;
  case "disconnect": actor.active=false; cacheLifecycle.PostUpdatePlayers(); actor.active=true; break;
  case "initialize": exposure.Initialize(); break;
  case "enter world": exposure.OnEnterWorld(); break;
  case "inactive update": actor.active=false; exposure.PostUpdate(); actor.active=true; break;
 }
 Check(exposure.starAbyssCorruption==0&&exposure.thunderCloudTimer==0,$"{lifecycle} clears previous occupant/world environmental exposure");
 actor.Thunder=true; Game.netMode=NetmodeID.Server; exposure.PostUpdate();
 Check(exposure.thunderCloudTimer==1&&Projectile.Spawns==spawned,$"{lifecycle} grants a full fresh lightning interval");
}
Console.WriteLine($"Including world/occupant scan lifecycle regression: {assertions} assertions; tile and transport boundaries mocked.");

// Malformed network coordinates must not convert into valid tiles or retain caches.
var scanOwner=MakePlayer(50,50,0); Game.player[0]=scanOwner;
var scanObjects=scanOwner.GetModPlayer<BiomeObjectEffectsPlayer>();
foreach(var badPosition in new[]{new Vector2(float.NaN,800),new Vector2(800,float.NaN),
    new Vector2(float.PositiveInfinity,800),new Vector2(800,float.NegativeInfinity),
    new Vector2(float.MaxValue,800),new Vector2(800,float.MinValue),
    new Vector2(-1,800),new Vector2(800,-1),
    new Vector2(Game.maxTilesX*16,800),new Vector2(800,Game.maxTilesY*16)}) {
    scanOwner.Biome.Initialize(); scanObjects.Initialize();
    Game.netMode=NetmodeID.Server; Game.GameUpdateCount=500;
    scanOwner.Center=new(800,800);
    Game.tile[50,50]=new Tile {HasTile=true,TileType=3};
    Game.tile[51,50]=new Tile {HasTile=true,TileType=12};
    Check(scanOwner.Biome.Count(3)==1,"Valid scan before bad position is primed");
    scanOwner.State.spiritualEnergyRegenBonus=0; scanObjects.PostUpdateEquips();
    Check(scanOwner.State.spiritualEnergyRegenBonus==1,"Valid object effect before bad position is primed");
    scanOwner.Center=badPosition; scanOwner.State.spiritualEnergyRegenBonus=0;
    Check(scanOwner.Biome.Count(3)==0,"Invalid coordinate cannot activate cached server biome");
    scanObjects.PostUpdateEquips();
    Check(scanOwner.State.spiritualEnergyRegenBonus==0,"Invalid coordinate cannot apply cached object effect");
    Game.tile[50,50]=default; Game.tile[51,50]=default; scanOwner.Center=new(800,800);
    Check(scanOwner.Biome.Count(3)==0,"Returning to the old coordinate requires a fresh biome scan");
    scanObjects.PostUpdateEquips();
    Check(scanOwner.State.spiritualEnergyRegenBonus==0,"Returning to the old coordinate cannot revive a removed object effect");
}
Console.WriteLine($"Including bounded coordinate scan regression: {assertions} assertions; engine/map boundaries mocked.");

Game.player[0]=new Player {active=false,ModPlayersReady=false}; Game.player[1]=null;
cacheLifecycle.OnWorldLoad(); cacheLifecycle.OnWorldUnload(); cacheLifecycle.PostUpdatePlayers();
Check(true,"World/disconnect cleanup safely skips vanilla-only uninitialized player slots");
Console.WriteLine($"Including uninitialized world slots: {assertions} assertions.");

Game.netMode=NetmodeID.Server; Game.GameUpdateCount=600;
int swordPacketsBefore=NetMessage.BuffMessages;
var swordOwner=MakePlayer(50,50,0);Game.player[0]=swordOwner;var swordEffects=swordOwner.GetModPlayer<BiomeObjectEffectsPlayer>();
Game.tile[50,50]=new Tile{HasTile=true,TileType=11};
swordEffects.PostUpdateEquips();
Check(swordOwner.AddedBuffs==1&&swordOwner.buffTime[0]==30&&NetMessage.BuffMessages==swordPacketsBefore+1,"Server sword lease explicitly synchronizes the registered buff");
for(int time=30;time>15;time--) {swordOwner.buffTime[0]=time;swordEffects.PostUpdateEquips();}
Check(swordOwner.AddedBuffs==1&&NetMessage.BuffMessages==swordPacketsBefore+1,"Healthy sword buff does not generate per-frame sync traffic");
swordOwner.buffTime[0]=15;swordEffects.PostUpdateEquips();
Check(swordOwner.AddedBuffs==2&&swordOwner.buffTime[0]==30&&NetMessage.BuffMessages==swordPacketsBefore+2,"Sword buff refreshes exactly at the lease threshold");
Game.tile[50,50]=default; swordOwner.Center=new(1600,800); swordOwner.buffTime[0]=15;
swordEffects.PostUpdateEquips();
Check(swordOwner.AddedBuffs==2&&swordOwner.buffTime[0]==15,"Leaving sword tablet range stops lease refresh");
Game.netMode=NetmodeID.SinglePlayer;swordOwner.Center=new(800,800);Game.tile[50,50]=new Tile{HasTile=true,TileType=11};swordOwner.buffTime[0]=0;
swordEffects.PostUpdateEquips();
Check(swordOwner.buffTime[0]==2&&swordOwner.LastBuffQuiet,"Single player retains two-frame local buff refresh without sync");
Console.WriteLine($"Including sword tablet buff sync lease: {assertions} assertions; transport mocked.");
