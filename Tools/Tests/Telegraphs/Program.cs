using Terraria;using Terraria.ID;using XianXia.Content.Projectiles;
int assertions=0;void Check(bool ok,string text){assertions++;if(!ok)throw new Exception(text);}
var field=new BossArrayFieldProjectile();field.SetDefaults();Check(field.Projectile.timeLeft==120&&field.Projectile.hostile,"Native hostile field defaults");
for(int left=0;left<=120;left++){field.Projectile.timeLeft=left;Check(field.CanDamage()==(left>15&&left<=75),$"Warning/active boundary {left}");}
Main.dedServ=true;field.Projectile.timeLeft=120;field.AI();Check(field.Projectile.alpha==200&&Lighting.Calls==0&&Dust.Calls==0&&Main.rand.Calls==0,"Dedicated warning state without graphics/RNG");field.Projectile.timeLeft=75;field.AI();Check(field.Projectile.alpha==40,"Fully visible at activation");
var warning=new TribulationWarningLineProjectile();warning.SetDefaults();Check(!warning.Projectile.hostile&&!warning.Projectile.friendly&&warning.Projectile.timeLeft==36,"Warning is harmless");warning.AI();Check(Lighting.Calls==0&&Dust.Calls==0&&Main.rand.Calls==0,"Dedicated marker avoids visuals");Main.netMode=NetmodeID.Server;warning.OnKill(5);Check(Projectile.Spawned==0,"Canceled warning does not fire");warning.OnKill(0);Check(Projectile.Spawned==1,"Natural warning expiry releases strike on server");Main.netMode=NetmodeID.MultiplayerClient;warning.OnKill(0);Check(Projectile.Spawned==1,"Client does not duplicate strike");Main.dedServ=false;warning.AI();Check(Lighting.Calls==1&&Dust.Calls==1,"Client warning remains visible");

Check(field.Projectile.netImportant, "Field participates in native late join");
var victim=new Player();
for(int left=0;left<=120;left++) {
 field.Projectile.timeLeft=left;Collision.Blocked=false;
 Check(field.CanHitPlayer(victim)==(left>15&&left<=75),$"Direct player-hit active window {left}");
 int before=victim.BuffCalls;field.OnHitPlayer(victim,default);
 Check(victim.BuffCalls-before==(left>15&&left<=75?1:0),$"Buff respects active window {left}");
}
Check(victim.Duration==120,"Native field debuff duration preserved");
field.Projectile.timeLeft=50;Collision.Blocked=true;int buffBefore=victim.BuffCalls;
Check(!field.CanHitPlayer(victim),"Solid wall blocks field damage");field.OnHitPlayer(victim,default);Check(victim.BuffCalls==buffBefore,"Solid wall blocks debuff");Collision.Blocked=false;
victim.dead=true;Check(!field.CanHitPlayer(victim),"Dead players cannot receive field hits");victim.dead=false;victim.active=false;Check(!field.CanHitPlayer(victim),"Inactive players cannot receive field hits");victim.active=true;
foreach(int mode in new[]{0,NetmodeID.Server,NetmodeID.MultiplayerClient}) {
 Main.netMode=mode;Main.dedServ=true;
 foreach(int left in new[]{120,76,75,16,15,1}) {
  field.Projectile.timeLeft=left;field.Projectile.netUpdate=false;field.AI();
  Check(field.Projectile.netUpdate==(mode!=NetmodeID.MultiplayerClient&&(left==75||left==15)),"Only authority broadcasts phase boundaries");
 }
}
for(short remaining=0;remaining<=120;remaining++) {
 using var bytes=new MemoryStream();field.Projectile.timeLeft=remaining;
 using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))field.SendExtraAI(writer);
 Check(bytes.Length==19,"Field age and source use nineteen-byte payload");bytes.Position=0;field.Projectile.timeLeft=120;
 field.ReceiveExtraAI(new BinaryReader(bytes));Check(field.Projectile.timeLeft==remaining,"Late join restores actual remaining age");
}
foreach(short remaining in new short[]{-1,121,short.MinValue,short.MaxValue}) {
 using var bytes=new MemoryStream();using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){writer.Write(remaining);writer.Write(false);writer.Write((short)-1);writer.Write(0);writer.Write((short)-1);writer.Write(0L);}
 bytes.Position=0;field.ReceiveExtraAI(new BinaryReader(bytes));Check(field.Projectile.timeLeft==0&&field.CanDamage()==false,"Invalid age becomes harmless expiry");
 field.Projectile.timeLeft=remaining;using var sent=new MemoryStream();using(var writer=new BinaryWriter(sent,System.Text.Encoding.UTF8,true))field.SendExtraAI(writer);
 Check(sent.ToArray().SequenceEqual(new byte[]{0,0,0,255,255,0,0,0,0,255,255,0,0,0,0,0,0,0,0}),"Sender cannot wrap invalid field age");
}
foreach(byte[] partial in new[]{Array.Empty<byte>(),new byte[]{75}}) {
 field.Projectile.timeLeft=40;using var bytes=new MemoryStream(partial);bool rejected=false;
 try{field.ReceiveExtraAI(new BinaryReader(bytes));}catch(EndOfStreamException){rejected=true;}
 Check(rejected&&field.Projectile.timeLeft==40,"Truncated age does not partially overwrite state");
}
field.Projectile.position=new(100,200);Main.screenPosition=new(20,30);var tint=new Microsoft.Xna.Framework.Color();Main.dedServ=false;
foreach(int remaining in new[]{120,76,75,16,15,1,0,121}) {
 field.Projectile.timeLeft=remaining;Main.spriteBatch.Boxes.Clear();Check(!field.PreDraw(ref tint),"Field draws collision outline instead of unrelated texture");
 int expected=remaining<=0||remaining>120?0:remaining>15&&remaining<=75?5:4;
 Check(Main.spriteBatch.Boxes.Count==expected,"Warning/fade outline and active fill");
 if(expected>0)Check(Main.spriteBatch.Boxes[^4]==new Microsoft.Xna.Framework.Rectangle(80,170,96,2)
  &&Main.spriteBatch.Boxes[^1]==new Microsoft.Xna.Framework.Rectangle(174,170,2,96),"Outline matches screen-space native hitbox");
}
Main.dedServ=true;Main.spriteBatch.Boxes.Clear();field.Projectile.timeLeft=50;field.PreDraw(ref tint);Check(Main.spriteBatch.Boxes.Count==0,"Dedicated server does not draw field");

byte[] SourceWire(BossArrayFieldProjectile entity){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);entity.SendExtraAI(writer);return stream.ToArray();}
foreach(int mode in new[]{0,1,2})foreach(int reason in Enumerable.Range(0,7)){
 Main.netMode=0;Main.player=[new(),new(){active=false}];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();var parent=Main.npc[3];parent.active=true;
 var linked=new BossArrayFieldProjectile();linked.SetDefaults();linked.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(parent));linked.Projectile.timeLeft=50;Check(linked.CanDamage()==true,"valid NPC source preserves active phase");Main.netMode=mode;
 switch(reason){case 0:parent.active=false;break;case 1:parent.life=0;break;case 2:Main.player[0].dead=true;break;case 3:parent.target=255;break;case 4:parent.Center=new(float.NaN,0);break;case 5:parent.type++;break;case 6:parent.ModNPC=new();break;}
 bool clientSameTypeReplacement=mode==1&&reason==6;
 Check(linked.CanDamage()==clientSameTypeReplacement,"invalid source blocks damage; client instance replacement awaits authority cancellation");linked.AI();
 Check(linked.Projectile.timeLeft==(mode==1?50:15),"authority source failure enters fade");Check(linked.Projectile.netUpdate==(mode!=1),"authority broadcasts cancellation");
 if(mode!=1){var canceled=SourceWire(linked);parent.active=true;parent.life=100;parent.target=0;Main.player[0].dead=false;Main.netMode=1;var remoteField=new BossArrayFieldProjectile();remoteField.SetDefaults();using var stream=new MemoryStream(canceled);remoteField.ReceiveExtraAI(new BinaryReader(stream));remoteField.Projectile.timeLeft=50;Check(remoteField.CanDamage()==false,"cancel packet cannot revive old field");using var oldPacket=new MemoryStream();using(var writer=new BinaryWriter(oldPacket,System.Text.Encoding.UTF8,true)){writer.Write((short)50);writer.Write(false);writer.Write((short)3);writer.Write(parent.type);writer.Write((short)-1);writer.Write(0L);}oldPacket.Position=0;remoteField.ReceiveExtraAI(new BinaryReader(oldPacket));Check(remoteField.CanDamage()==false,"later uncancelled source data cannot revive cancellation");}
}
Main.netMode=0;Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();Main.npc[3].active=true;Main.player=[new(),new()];var atomic=new BossArrayFieldProjectile();atomic.SetDefaults();atomic.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(Main.npc[3]));var fullPacket=SourceWire(atomic);
for(int length=0;length<19;length++){var before=SourceWire(atomic);try{using var stream=new MemoryStream(fullPacket[..length]);atomic.ReceiveExtraAI(new BinaryReader(stream));throw new Exception("truncated source accepted");}catch(EndOfStreamException){}Check(SourceWire(atomic).SequenceEqual(before),"source+age packet read atomic");}
var playerSource=new Player();playerSource.Cultivation.tribulationTimer=120;Main.player[0]=playerSource;var playerField=new BossArrayFieldProjectile();playerField.SetDefaults();playerField.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(playerSource));playerField.Projectile.timeLeft=50;Check(playerField.CanDamage()==true,"active player tribulation source remains harmful");

foreach(int mode in new[]{0,1,2})foreach(int reason in Enumerable.Range(0,8)){
 Main.netMode=0;Main.player=[new(),new(){active=false}];var owner=Main.player[0];owner.Cultivation.tribulationTimer=120;owner.Cultivation.TribulationSession=42;
 var hazard=new BossArrayFieldProjectile();hazard.SetDefaults();hazard.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(owner));hazard.Projectile.timeLeft=50;Check(hazard.CanDamage()==true,"active captured tribulation can damage");Main.netMode=mode;
 switch(reason){case 0:owner.active=false;break;case 1:owner.dead=true;break;case 2:owner.Cultivation.tribulationTimer=0;break;case 3:owner.Cultivation.TribulationSession++;break;case 4:owner.Center=new(float.NaN,0);break;case 5:owner.velocity=new(float.PositiveInfinity,0);break;case 6:Main.player[0]=new(){Cultivation=new(){tribulationTimer=120,TribulationSession=43}};break;case 7:owner.Cultivation.TribulationSession=0;break;}
 bool awaitsSessionCancel=mode==1&&(reason==3||reason==6||reason==7);Check(hazard.CanDamage()==awaitsSessionCancel,"invalid tribulation rejects damage; client session change awaits authority");hazard.AI();Check(hazard.Projectile.timeLeft==(mode==1?50:15),"tribulation authority cancellation enters fade");
 if(mode!=1){var payload=SourceWire(hazard);Main.netMode=1;Main.player[0]=new(){Cultivation=new(){tribulationTimer=120,TribulationSession=44}};var remote=new BossArrayFieldProjectile();remote.SetDefaults();using var stream=new MemoryStream(payload);remote.ReceiveExtraAI(new BinaryReader(stream));remote.Projectile.timeLeft=50;Check(remote.CanDamage()==false,"new tribulation cannot revive old canceled player field");}
}
Console.WriteLine($"Actual telegraph hook regression passed: {assertions} assertions; mocked graphics/spawn boundary.");
