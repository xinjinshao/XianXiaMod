using Terraria;
using Terraria.ID;
using XianXia.Content.NPCs.Enemies;
int checks=0;
void Check(bool value,string text){checks++;if(!value)throw new Exception(text);}
var enemy=new CelestialPuppet();enemy.SetDefaults();Main.netMode=NetmodeID.Server;Main.player[0].Center=new(100,0);
void Tick(int count){for(int i=0;i<count;i++)enemy.PostAI();}
Tick(129);Check(!enemy.NPC.netUpdate&&Projectile.Spawns==0,"No early attack");
Tick(1);Check(enemy.NPC.velocity.X==7&&enemy.NPC.localAI[1]==1&&Projectile.Spawns==0,"First attack is horizontal dash");
Tick(130);Check(enemy.NPC.velocity.Y==-8&&Projectile.Spawns==1&&enemy.NPC.localAI[1]==2,"Second attack jumps and fires once");
Tick(130);Check(enemy.NPC.velocity.X==10&&enemy.NPC.velocity.Y==0&&enemy.NPC.localAI[1]==0,"Third attack aims charge and wraps phase");
Tick(130);Check(enemy.NPC.velocity.X==7&&Projectile.Spawns==1,"Next cycle begins with dash");
Main.netMode=NetmodeID.MultiplayerClient;float timer=enemy.NPC.localAI[0];Tick(390);Check(enemy.NPC.localAI[0]==timer&&Projectile.Spawns==1,"Client cannot execute attacks or advance authoritative cycle");
Main.netMode=NetmodeID.Server;Main.player[0].dead=true;Main.player[1].active=false;enemy.NPC.target=255;Tick(130);Check(Projectile.Spawns==1&&enemy.NPC.localAI[0]==0,"No live target safely resets timer");
Main.player[1].active=true;Main.player[1].Center=new(-100,0);Tick(130);Check(enemy.NPC.target==1&&Projectile.Spawns==2,"Invalid target reacquires live player before attack");
Console.WriteLine($"Actual CelestialPuppet PostAI regression passed: {checks} assertions; mocked native movement/network boundary.");

Main.netMode=NetmodeID.Server;Main.player[0].dead=false;var npc=new NPC{target=-1};
Check(XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(npc,out var target)&&target==Main.player[0],"Negative target safely reacquires");
npc.target=Main.maxPlayers;Main.player[0].dead=true;Check(XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(npc,out target)&&target==Main.player[1],"Upper bound reacquires different living player");
Main.player[1].active=false;Check(!XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(npc,out target)&&target==null,"All unavailable players return no target");
Main.netMode=NetmodeID.MultiplayerClient;Main.player[0].dead=false;npc.target=255;Check(!XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(npc,out target)&&npc.target==255,"Client waits for authoritative target instead of retargeting");
npc.target=0;Check(XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(npc,out target)&&target==Main.player[0],"Client can read valid synchronized target");
Console.WriteLine($"Enemy cycle/target regression passed: {checks} assertions.");

Main.netMode=NetmodeID.Server;Main.dedServ=true;Main.player[0].active=true;Main.player[0].dead=false;Main.player[0].Center=new(0,0);
var vine=new HerbGardenVineSpirit();vine.SetDefaults();vine.NPC.life=vine.NPC.lifeMax-2;vine.NPC.localAI[0]=89;vine.PostAI();
Check(vine.NPC.life==vine.NPC.lifeMax&&vine.NPC.netUpdate&&Dust.Calls==0,"Server heals capped amount, marks synchronization, skips particles");
Main.netMode=NetmodeID.MultiplayerClient;Main.dedServ=false;vine.NPC.life=1;vine.NPC.netUpdate=false;vine.NPC.localAI[0]=89;vine.PostAI();Check(vine.NPC.life==1&&!vine.NPC.netUpdate&&Dust.Calls==6,"Client renders heal effect without changing life");
Main.netMode=NetmodeID.Server;Main.dedServ=true;vine.NPC.life=0;vine.NPC.localAI[0]=89;vine.PostAI();Check(vine.NPC.life==0,"Healing does not resurrect zero-life enemy");
var moth=new MiasmaFlowerMoth();moth.SetDefaults();Main.player[1].active=true;Main.player[1].dead=true;Main.player[1].Center=new(0,0);moth.NPC.localAI[0]=44;int dust=Dust.Calls;moth.PostAI();Check(Main.player[0].Buffs==1&&Main.player[1].Buffs==0&&Dust.Calls==dust,"Server poison affects nearby living players and no particles");
Main.player[0].Center=new(129,0);moth.NPC.localAI[0]=44;moth.PostAI();Check(Main.player[0].Buffs==1,"Outside radius no poison");
Main.netMode=NetmodeID.MultiplayerClient;Main.dedServ=false;Main.player[0].Center=new(0,0);moth.NPC.localAI[0]=44;moth.PostAI();Check(Main.player[0].Buffs==1&&Dust.Calls==dust+10,"Client poison pulse draws ring without applying buffs");
Console.WriteLine($"Enemy authority regression total: {checks} assertions.");

Main.netMode=NetmodeID.Server;Main.player[0].dead=false;Main.player[0].Center=new(50,0);
var sword=new ObsessedSwordCultivator();sword.SetDefaults();sword.NPC.defDamage=72;
sword.OnHitByProjectile(new Projectile{owner=255},default,1);Check(sword.NPC.localAI[1]==0,"Unowned projectile does not arm counter or access player sentinel");
sword.OnHitByProjectile(new Projectile{owner=0},default,1);sword.NPC.localAI[0]=119;sword.PostAI();Check(sword.NPC.damage==93&&sword.NPC.localAI[2]==30,"Counter uses baseline damage for bounded window");
for(int i=0;i<30;i++)sword.PostAI();Check(sword.NPC.damage==72,"Counter restores base damage after 30 ticks");
sword.OnHitByProjectile(new Projectile{owner=0},default,1);sword.NPC.localAI[0]=119;sword.PostAI();Check(sword.NPC.damage==93,"Repeated counter never compounds damage");
Main.player[0].dead=true;Main.player[1].active=false;sword.PostAI();Check(sword.NPC.damage==72&&sword.NPC.defense==34&&sword.NPC.localAI[2]==0,"Losing target clears combat bonuses");
Main.netMode=NetmodeID.MultiplayerClient;sword.NPC.localAI[1]=0;sword.OnHitByProjectile(new Projectile{owner=0},default,1);Check(sword.NPC.localAI[1]==0,"Client cannot arm server counter");
Console.WriteLine($"Enemy regression total: {checks} assertions.");

using(var memory=new MemoryStream()){sword.NPC.damage=93;sword.NPC.localAI[2]=30;sword.SendExtraAI(new BinaryWriter(memory));memory.Position=0;var replica=new ObsessedSwordCultivator();replica.NPC.defDamage=72;replica.ReceiveExtraAI(new BinaryReader(memory));Check(replica.NPC.damage==93&&replica.NPC.localAI[2]==30,"ExtraAI transmits counter damage and remaining duration");for(int i=0;i<30;i++)replica.PostAI();Check(replica.NPC.damage==72,"Client expires synchronized counter without executing attacks");}
Console.WriteLine($"Enemy regression final total: {checks} assertions.");

Main.netMode=NetmodeID.Server;Main.dedServ=true;Main.player[0].dead=false;var echo=new ScriptureArchiveEcho();echo.SetDefaults();echo.NPC.life=echo.NPC.lifeMax;int beforeSpawns=Projectile.Spawns;dust=Dust.Calls;
for(int i=0;i<315;i++)echo.PostAI();Check(echo.NPC.defense==72&&echo.NPC.localAI[2]==30&&Projectile.Spawns==beforeSpawns+9,"Third volley grants timed shield and three server bolts per volley");Check(Dust.Calls==dust,"Dedicated shield never emits particles");
for(int i=0;i<30;i++)echo.PostAI();Check(echo.NPC.defense==28&&echo.NPC.netUpdate,"Shield expiration restores base defense and synchronizes");
echo.NPC.localAI[2]=20;Main.player[0].dead=true;echo.PostAI();Check(echo.NPC.defense==28&&echo.NPC.localAI[2]==0,"Losing target cannot freeze boosted defense");
Main.player[0].dead=false;echo.NPC.life=echo.NPC.lifeMax/2-1;echo.PostAI();Check(echo.NPC.defense==36,"Unshielded low-life defense updates independently of volley");
using(var stream=new MemoryStream()){echo.NPC.localAI[2]=30;echo.SendExtraAI(new BinaryWriter(stream));stream.Position=0;Main.netMode=NetmodeID.MultiplayerClient;Main.dedServ=false;var client=new ScriptureArchiveEcho();client.SetDefaults();client.NPC.life=client.NPC.lifeMax;client.ReceiveExtraAI(new BinaryReader(stream));Check(client.NPC.defense==72&&Dust.Calls==dust+12,"Client receives shield duration and draws its activation");int count=Projectile.Spawns;for(int i=0;i<30;i++)client.PostAI();Check(client.NPC.defense==28&&Projectile.Spawns==count,"Client shield expires without attack execution");}
Console.WriteLine($"Enemy regression complete total: {checks} assertions.");

Main.netMode=NetmodeID.Server;Main.dedServ=true;Main.player[0].dead=false;Main.player[0].Center=new(100,0);var hawk=new ThunderPatternHawk();hawk.SetDefaults();dust=Dust.Calls;
for(int i=0;i<139;i++)hawk.PostAI();Check(hawk.NPC.localAI[1]==0&&!hawk.NPC.netUpdate,"Hawk waits full windup interval");hawk.PostAI();Check(hawk.NPC.velocity.X==15&&hawk.NPC.localAI[1]==1&&hawk.NPC.netUpdate,"Server starts aimed dive");
for(int i=0;i<30;i++)hawk.PostAI();Check(Math.Abs(hawk.NPC.velocity.X-4.5f)<0.001f&&hawk.NPC.localAI[1]==0&&Dust.Calls==dust,"Server exits dive after 30 ticks without particles");
hawk.NPC.localAI[0]=139;hawk.PostAI();Main.player[0].dead=true;hawk.NPC.netUpdate=false;hawk.PostAI();Check(hawk.NPC.localAI[0]==0&&hawk.NPC.localAI[1]==0&&hawk.NPC.netUpdate,"Losing target brakes and clears dive");
Main.netMode=NetmodeID.MultiplayerClient;Main.dedServ=false;hawk.NPC.velocity=new(15,0);hawk.NPC.localAI[0]=139;hawk.NPC.netUpdate=false;hawk.PostAI();Check(hawk.NPC.localAI[0]==139&&hawk.NPC.velocity.X==15&&!hawk.NPC.netUpdate&&Dust.Calls==dust+1,"Client only renders trail and cannot transition dive");
Console.WriteLine($"Enemy regression final count: {checks} assertions.");

Main.netMode=NetmodeID.Server;Main.player[0].dead=false;Main.player[0].Center=new(500,0);var eclipse=new StarEclipsedCultivator();eclipse.SetDefaults();eclipse.NPC.life=1;beforeSpawns=Projectile.Spawns;
for(int i=0;i<181;i++)eclipse.PostAI();Check(eclipse.NPC.velocity.X==0&&Projectile.Spawns==beforeSpawns+1,"No early retreat; server fires at 135 ticks");eclipse.NPC.netUpdate=false;eclipse.PostAI();Check(eclipse.NPC.velocity.X==-6&&eclipse.NPC.localAI[1]==0&&eclipse.NPC.netUpdate,"Low-life retreat after 182 ticks marks movement sync");
eclipse.NPC.localAI[1]=100;eclipse.NPC.life=eclipse.NPC.lifeMax;eclipse.PostAI();Check(eclipse.NPC.localAI[1]==0,"Leaving low-life state resets retreat buildup");
eclipse.NPC.localAI[0]=100;eclipse.NPC.localAI[1]=100;Main.player[0].dead=true;eclipse.PostAI();Check(eclipse.NPC.localAI[0]==0&&eclipse.NPC.localAI[1]==0,"No living target clears timers");
Main.netMode=NetmodeID.MultiplayerClient;eclipse.NPC.localAI[0]=134;eclipse.NPC.localAI[1]=181;eclipse.NPC.life=1;float vx=eclipse.NPC.velocity.X;int projectiles=Projectile.Spawns;eclipse.PostAI();Check(eclipse.NPC.velocity.X==vx&&eclipse.NPC.localAI[0]==134&&eclipse.NPC.localAI[1]==181&&Projectile.Spawns==projectiles,"Client never executes retreat or shot timers");
Main.netMode=NetmodeID.Server;Main.player[0].dead=false;Main.player[0].Center=new(100,0);eclipse.NPC.life=eclipse.NPC.lifeMax;eclipse.NPC.localAI[0]=30;eclipse.NPC.netUpdate=false;eclipse.PostAI();Check(eclipse.NPC.velocity.X<vx&&eclipse.NPC.netUpdate,"Server proximity avoidance periodically synchronizes movement");
Console.WriteLine($"Enemy regression count: {checks} assertions.");

Main.netMode=NetmodeID.Server;Main.dedServ=true;Main.player[0].dead=false;Main.player[0].Center=new(100,0);var larva=new StarAbyssLarva();larva.SetDefaults();for(int i=0;i<90;i++)larva.PostAI();Check(larva.NPC.velocity.X==8&&larva.NPC.velocity.Y==-4&&larva.NPC.localAI[1]==90,"Server executes larva leap at 90 ticks");
Main.player[0].Center=new(30,0);Main.player[0].velocity=new(10,0);int buffs=Main.player[0].Buffs;larva.PostAI();Check(Main.player[0].Buffs==buffs+1&&Main.player[0].velocity.X==10,"Larva slow uses native buff rather than player velocity mutation");Main.player[0].Center=new(40,0);larva.PostAI();Check(Main.player[0].Buffs==buffs+1,"Slow excludes 40px boundary");
Main.player[0].dead=true;larva.PostAI();Check(larva.NPC.localAI[0]==0&&larva.NPC.localAI[1]==0,"Missing target clears larva attack phase");Main.netMode=NetmodeID.MultiplayerClient;larva.NPC.localAI[0]=89;larva.PostAI();Check(larva.NPC.localAI[0]==89&&Main.player[0].Buffs==buffs+1,"Client cannot leap or apply slow");
Main.netMode=NetmodeID.Server;Main.player[0].dead=false;Main.player[0].Center=new(100,0);var moon=new MoonboneCultivator();moon.SetDefaults();projectiles=Projectile.Spawns;for(int i=0;i<69;i++)moon.PostAI();Check(Projectile.Spawns==projectiles,"Moon attack waits full interval");moon.PostAI();Check(Projectile.Spawns==projectiles+1&&moon.NPC.velocity.X==12&&moon.NPC.netUpdate&&Lighting.Calls==0,"Server moon attack shoots and dashes without lighting");Main.netMode=NetmodeID.MultiplayerClient;Main.dedServ=false;moon.NPC.localAI[0]=69;moon.NPC.velocity=new(0,0);moon.PostAI();Check(moon.NPC.velocity.X==0&&moon.NPC.localAI[0]==69&&Lighting.Calls==1,"Client draws moon light without attack transition");
Console.WriteLine($"Enemy behavior assertions: {checks}.");
