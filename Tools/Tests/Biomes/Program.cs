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
first.dead = false; first.position = new(500,500); Game.GameUpdateCount = 240; green.AI();
Check(first.statLife == 52, "Array does not heal an owner outside its hitbox.");
first.position = Vector2.Zero; first.statLife = 100; Game.GameUpdateCount = 300; green.AI();
Check(first.statLife == 100 && first.State.spiritualEnergy == 3 && NetMessage.HealMessages == 2,
    "Full health clamps healing and sends no extra heal packet while energy recovery continues.");
var thunder = new XianXia.Content.Projectiles.ThunderTalismanArray();
thunder.SetDefaults(); thunder.Projectile.owner = 0; thunder.Projectile.timeLeft = 45; thunder.Projectile.damage = 20;
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
