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
 Check(sent.ToArray().SequenceEqual(new byte[]{0,0,1,255,255,0,0,0,0,255,255,0,0,0,0,0,0,0,0}),"Sender cannot wrap invalid field age");
}
foreach(byte[] partial in new[]{Array.Empty<byte>(),new byte[]{75}}) {
 field.Projectile.timeLeft=40;using var bytes=new MemoryStream(partial);bool rejected=false;
 try{field.ReceiveExtraAI(new BinaryReader(bytes));}catch(EndOfStreamException){rejected=true;}
 Check(rejected&&field.Projectile.timeLeft==40,"Truncated age does not partially overwrite state");
}
field=new BossArrayFieldProjectile();field.SetDefaults();field.Projectile.position=new(100,200);Main.screenPosition=new(20,30);var tint=new Microsoft.Xna.Framework.Color();Main.dedServ=false;
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

foreach(bool playerOrigin in new[]{false,true})foreach(int mode in new[]{0,1,2})foreach(int reason in new[]{0,1,2}){
 Main.netMode=0;Main.player=[new(),new()];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();var owner=Main.player[0];owner.Cultivation.tribulationTimer=120;owner.Cultivation.TribulationSession=50;var parent=Main.npc[3];parent.active=true;
 var marker=new TribulationWarningLineProjectile();marker.SetDefaults();marker.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(playerOrigin?(object)owner:parent));Main.netMode=mode;
 if(playerOrigin){if(reason==0)owner.dead=true;else if(reason==1)owner.Cultivation.tribulationTimer=0;else owner.Cultivation.TribulationSession++;}else{if(reason==0)parent.active=false;else if(reason==1)parent.target=255;else parent.ModNPC=new();}
 marker.AI();Check(marker.Projectile.timeLeft==(mode==1?36:6),"warning authority cancellation has six-tick fade");int before=Projectile.Spawned;marker.OnKill(0);Check(Projectile.Spawned==before,"canceled warning expiry never releases strike");
 Check(marker.CanDamage()==false&&!marker.CanHitPlayer(owner),"warning always harmless");
}
foreach(bool playerOrigin in new[]{false,true}){
 Main.netMode=0;Main.player=[new(),new()];Main.player[0].Cultivation.tribulationTimer=120;Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();Main.npc[3].active=true;object origin=playerOrigin?Main.player[0]:Main.npc[3];var marker=new TribulationWarningLineProjectile();marker.SetDefaults();marker.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(origin));int before=Projectile.Spawned;marker.OnKill(0);marker.OnKill(0);Check(Projectile.Spawned==before+1,"natural expiry releases at most once");Check(Projectile.LastSource is Terraria.DataStructures.EntitySource_Parent root&&ReferenceEquals(root.Entity,origin),"strike source forwards original NPC or player");
 using var payload=new MemoryStream();using(var writer=new BinaryWriter(payload,System.Text.Encoding.UTF8,true))marker.SendExtraAI(writer);Check(payload.Length==19,"warning late join uses shared nineteen-byte layout");var bytes=payload.ToArray();
 for(int length=0;length<19;length++){var other=new TribulationWarningLineProjectile();other.SetDefaults();try{using var partial=new MemoryStream(bytes[..length]);other.ReceiveExtraAI(new BinaryReader(partial));throw new Exception("truncated warning accepted");}catch(EndOfStreamException){}Check(other.Projectile.timeLeft==36,"warning truncated payload age atomic");}
}
foreach(short invalid in new short[]{-1,37,short.MaxValue}){var marker=new TribulationWarningLineProjectile();marker.SetDefaults();using var bytes=new MemoryStream();using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){writer.Write(invalid);writer.Write(false);writer.Write((short)-1);writer.Write(0);writer.Write((short)-1);writer.Write(0L);}bytes.Position=0;marker.ReceiveExtraAI(new BinaryReader(bytes));int before=Projectile.Spawned;marker.OnKill(0);Check(Projectile.Spawned==before,"invalid warning age cannot clamp into a natural release");}

Main.netMode=0;Main.dedServ=true;Main.player=[new(),new()];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();Main.npc[3].active=true;var fadingWarning=new TribulationWarningLineProjectile();fadingWarning.SetDefaults();fadingWarning.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(Main.npc[3]));Main.npc[3].active=false;fadingWarning.AI();int fadeStart=fadingWarning.Projectile.alpha;fadingWarning.Projectile.timeLeft=1;fadingWarning.AI();Check(fadingWarning.Projectile.alpha>fadeStart,"canceled warning becomes more transparent toward expiry");
fadingWarning.Projectile.timeLeft=36;Main.netMode=1;fadingWarning.AI();Check(fadingWarning.Projectile.timeLeft==6&&fadingWarning.Projectile.alpha>=180,"received cancellation bounds client fade even after stale age data");

foreach(bool playerOrigin in new[]{false,true})foreach(int mode in new[]{0,1,2})foreach(int reason in new[]{0,1,2}){
 Main.netMode=0;Main.dedServ=true;Main.player=[new(),new()];Main.player[0].Cultivation.tribulationTimer=120;Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();Main.npc[3].active=true;
 var bolt=new TribulationLightningProjectile();bolt.SetDefaults();bolt.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(playerOrigin?(object)Main.player[0]:Main.npc[3]));bolt.Projectile.velocity=new(0,11);Check(bolt.CanDamage()==true&&bolt.Projectile.netImportant,"valid released lightning remains damaging and late-join important");Main.netMode=mode;
 if(playerOrigin){if(reason==0)Main.player[0].dead=true;else if(reason==1)Main.player[0].Cultivation.tribulationTimer=0;else Main.player[0].Cultivation.TribulationSession++;}else{if(reason==0)Main.npc[3].active=false;else if(reason==1)Main.npc[3].target=255;else Main.npc[3].ModNPC=new();}
 bool awaitsCancel=mode==1&&reason==2;Check(bolt.CanDamage()==awaitsCancel,"lightning source invalidity blocks damage, client replacement awaits authority");bolt.AI();Check(bolt.Projectile.timeLeft==(mode==1?120:6),"lightning authority cancellation bounded fade");
 if(!awaitsCancel){Check(bolt.Projectile.velocity==Microsoft.Xna.Framework.Vector2.Zero,"invalid lightning stops movement");var target=new Player();int before=target.BuffCalls;bolt.OnHitPlayer(target,default);Check(target.BuffCalls==before,"invalid lightning cannot apply disorder");}
}
Main.netMode=0;var activeBolt=new TribulationLightningProjectile();activeBolt.SetDefaults();activeBolt.Projectile.velocity=new(0,11);var boltVictim=new Player();Collision.Blocked=false;activeBolt.OnHitPlayer(boltVictim,default);Check(boltVictim.BuffCalls==1&&boltVictim.Duration==180,"valid lightning preserves three-second buff");Collision.Blocked=true;activeBolt.OnHitPlayer(boltVictim,default);Check(!activeBolt.CanHitPlayer(boltVictim)&&boltVictim.BuffCalls==1,"wall blocks released lightning hit and buff");Collision.Blocked=false;
using(var payload=new MemoryStream()){using(var writer=new BinaryWriter(payload,System.Text.Encoding.UTF8,true))activeBolt.SendExtraAI(writer);Check(payload.Length==19,"released lightning uses shared nineteen-byte layout");var bytes=payload.ToArray();for(int length=0;length<19;length++){var partialBolt=new TribulationLightningProjectile();partialBolt.SetDefaults();try{using var partial=new MemoryStream(bytes[..length]);partialBolt.ReceiveExtraAI(new BinaryReader(partial));throw new Exception("truncated lightning accepted");}catch(EndOfStreamException){}Check(partialBolt.Projectile.timeLeft==120,"lightning truncated payload age remains atomic");}}
activeBolt.Projectile.Center=new(float.NaN,0);Check(activeBolt.CanDamage()==false,"non-finite lightning geometry cannot damage");

foreach(int mode in new[]{0,1,2}){Main.netMode=mode;var corruptBolt=new TribulationLightningProjectile();corruptBolt.SetDefaults();corruptBolt.Projectile.velocity=new(float.NaN,0);corruptBolt.AI();Check(corruptBolt.CanDamage()==false&&corruptBolt.Projectile.velocity==Microsoft.Xna.Framework.Vector2.Zero,"invalid velocity repair cannot reenable lightning damage");Check(corruptBolt.Projectile.timeLeft==(mode==1?120:6),"invalid geometry authority fade only");}

foreach(int mode in new[]{0,1,2})foreach(int badAge in new[]{-1,121,int.MaxValue}){
 Main.netMode=mode;var overshot=new TribulationLightningProjectile();overshot.SetDefaults();overshot.Projectile.timeLeft=badAge;overshot.AI();overshot.Projectile.timeLeft=120;Check(overshot.CanDamage()==false,"bad lifetime cannot recover damage after returning to valid range");
 using var payload=new MemoryStream();using(var writer=new BinaryWriter(payload,System.Text.Encoding.UTF8,true))overshot.SendExtraAI(writer);var remote=new TribulationLightningProjectile();remote.SetDefaults();payload.Position=0;remote.ReceiveExtraAI(new BinaryReader(payload));remote.Projectile.timeLeft=120;Check(remote.CanDamage()==false,"bad lifetime cancellation reaches receiver and stays locked");
}
foreach(short badAge in new short[]{-1,121,short.MaxValue}){var overshot=new TribulationLightningProjectile();overshot.SetDefaults();using var payload=new MemoryStream();using(var writer=new BinaryWriter(payload,System.Text.Encoding.UTF8,true)){writer.Write(badAge);writer.Write(false);writer.Write((short)-1);writer.Write(0);writer.Write((short)-1);writer.Write(0L);}payload.Position=0;overshot.ReceiveExtraAI(new BinaryReader(payload));overshot.Projectile.timeLeft=120;Check(overshot.CanDamage()==false,"raw invalid lifetime never reactivates received lightning");}
var senderOvershot=new TribulationLightningProjectile();senderOvershot.SetDefaults();senderOvershot.Projectile.timeLeft=121;using(var payload=new MemoryStream()){using var writer=new BinaryWriter(payload);senderOvershot.SendExtraAI(writer);}senderOvershot.Projectile.timeLeft=120;Check(senderOvershot.CanDamage()==false,"serialization itself latches an observed invalid lifetime before AI");

foreach(int mode in new[]{0,1,2})foreach(int reason in new[]{0,1,2,3,4}){
 Main.netMode=0;Main.dedServ=true;Main.player=[new(),new()];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();var parent=Main.npc[3];parent.active=true;
 var bolt=new BossSpiritBoltProjectile();bolt.SetDefaults();bolt.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(parent));bolt.Projectile.velocity=new(2,3);Check(bolt.CanDamage()==true,"spirit bolt valid source");Main.netMode=mode;
 if(reason==0)parent.active=false;else if(reason==1)parent.target=255;else if(reason==2)parent.ModNPC=new();else if(reason==3)bolt.Projectile.timeLeft=241;else bolt.Projectile.velocity=new(float.NaN,0);
 bolt.AI();bool clientIdentityWait=mode==1&&reason==2;Check(bolt.CanDamage()==clientIdentityWait,"spirit bolt invalid source or state refuses damage");
 if(!clientIdentityWait){Check(bolt.Projectile.velocity==Microsoft.Xna.Framework.Vector2.Zero,"invalid spirit bolt stops");int before=Main.player[0].BuffCalls;bolt.OnHitPlayer(Main.player[0],default);Check(Main.player[0].BuffCalls==before,"invalid spirit bolt no buff");}
 using var stream=new MemoryStream();using(var writer=new BinaryWriter(stream,System.Text.Encoding.UTF8,true))bolt.SendExtraAI(writer);Check(stream.Length==19,"spirit bolt wire layout");
 if(mode!=1){var remote=new BossSpiritBoltProjectile();remote.SetDefaults();stream.Position=0;remote.ReceiveExtraAI(new BinaryReader(stream));remote.Projectile.timeLeft=240;Check(remote.CanDamage()==false,"spirit bolt cancelled packet cannot revive");}
}
Main.netMode=0;var normalBolt=new BossSpiritBoltProjectile();normalBolt.SetDefaults();Collision.Blocked=false;var spiritBoltVictim=new Player();normalBolt.OnHitPlayer(spiritBoltVictim,default);Check(spiritBoltVictim.Duration==60,"spirit bolt preserves buff duration");Collision.Blocked=true;int boltBuffCount=spiritBoltVictim.BuffCalls;normalBolt.OnHitPlayer(spiritBoltVictim,default);Check(spiritBoltVictim.BuffCalls==boltBuffCount,"spirit bolt wall blocks buff");Collision.Blocked=false;
foreach(int mode in new[]{0,1,2})foreach(int reason in new[]{0,1,2,3}){
 Main.netMode=0;Main.dedServ=true;Main.player=[new(),new()];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();var parent=Main.npc[3];parent.active=true;var enemyBolt=new EnemySpiritBoltProjectile();enemyBolt.SetDefaults();enemyBolt.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(parent));Check(enemyBolt.Projectile.hostile&&!enemyBolt.Projectile.friendly&&enemyBolt.Projectile.timeLeft==180,"enemy bolt preserves hostile metadata");Check(enemyBolt.CanDamage()==true,"enemy bolt living source permits harm");Main.netMode=mode;
 if(reason==0)parent.active=false;else if(reason==1)Main.player[0].dead=true;else if(reason==2)enemyBolt.Projectile.timeLeft=181;else enemyBolt.Projectile.velocity=new(float.NaN,0);
 enemyBolt.AI();Check(enemyBolt.CanDamage()==false,"enemy bolt invalid state cannot damage");Check(enemyBolt.Projectile.velocity==Microsoft.Xna.Framework.Vector2.Zero,"enemy bolt invalid state stops movement");int before=Main.player[1].BuffCalls;enemyBolt.OnHitPlayer(Main.player[1],default);Check(Main.player[1].BuffCalls==before,"early enemy bolt never adds pressure buff");
 using var packet=new MemoryStream();using(var writer=new BinaryWriter(packet,System.Text.Encoding.UTF8,true))enemyBolt.SendExtraAI(writer);Check(packet.Length==19,"enemy bolt source packet nineteen bytes");
}
Main.netMode=0;var cleanEnemyBolt=new EnemySpiritBoltProjectile();cleanEnemyBolt.SetDefaults();Collision.Blocked=true;Check(!cleanEnemyBolt.CanHitPlayer(new Player()),"enemy bolt wall filter");Collision.Blocked=false;Check(cleanEnemyBolt.CanHitPlayer(new Player()),"enemy bolt valid player hit");

foreach(int mode in new[]{0,1,2}) {
 Main.netMode=mode;Main.dedServ=true;Main.player=[new(),new()];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i,active=false}).ToArray();var parent=Main.npc[3];parent.active=true;
 var beam=new TabletJudgmentBeamProjectile();beam.SetDefaults();beam.OnSpawn(new Terraria.DataStructures.EntitySource_Parent(parent));
 Check(beam.Projectile.width==32&&beam.Projectile.height==480&&beam.Projectile.netImportant&&beam.Projectile.hostile&&!beam.Projectile.friendly,"Tablet beam actual defaults and late join metadata");
 for(int age=0;age<=31;age++){beam.Projectile.timeLeft=age;Check(beam.CanDamage()==(age>0&&age<=30),"Tablet beam bounded active lifetime");}
 beam.Projectile.timeLeft=30;Collision.Blocked=true;Check(!beam.CanHitPlayer(new Player()),"Tablet beam does not hit through wall");Collision.Blocked=false;
 Check(beam.CanHitPlayer(new Player()),"Tablet beam accepts valid player");parent.active=false;beam.AI();Check(beam.CanDamage()==false,"Tablet beam source loss immediately harmless");Check(beam.Projectile.timeLeft==(mode==1?30:6),"Tablet beam authority fades invalid source; client waits");
 using var packet=new MemoryStream();using(var writer=new BinaryWriter(packet,System.Text.Encoding.UTF8,true))beam.SendExtraAI(writer);Check(packet.Length==19,"Tablet beam retains shared source and age wire format");
}
foreach(int reason in new[]{0,1,2,3}) {
 Main.netMode=0;var beam=new TabletJudgmentBeamProjectile();beam.SetDefaults();if(reason==0)beam.Projectile.Center=new(float.NaN,0);else if(reason==1)beam.Projectile.velocity=new(1,0);else if(reason==2)beam.Projectile.width=33;else beam.Projectile.timeLeft=31;
 Check(!beam.CanHitPlayer(new Player()),"Tablet beam malformed state blocks contact before AI");beam.AI();beam.Projectile.Center=new(100,100);beam.Projectile.velocity=Microsoft.Xna.Framework.Vector2.Zero;beam.Projectile.width=32;beam.Projectile.timeLeft=30;Check(beam.CanDamage()==false,"Tablet beam malformed state cannot revive after correction");
}
Main.netMode=0;Main.dedServ=false;Main.screenPosition=new(20,30);Main.spriteBatch.Boxes.Clear();var visibleBeam=new TabletJudgmentBeamProjectile();visibleBeam.SetDefaults();visibleBeam.Projectile.Center=new(100,300);var beamTint=default(Microsoft.Xna.Framework.Color);Check(!visibleBeam.PreDraw(ref beamTint)&&Main.spriteBatch.Boxes.Single()==new Microsoft.Xna.Framework.Rectangle(64,30,32,480),"Tablet beam draws its actual damage body with camera offset");Main.dedServ=true;visibleBeam.PreDraw(ref beamTint);Check(Main.spriteBatch.Boxes.Count==1,"Tablet beam dedicated server skips graphics");
Console.WriteLine($"Actual telegraph hook regression passed: {assertions} assertions; mocked graphics/spawn boundary.");
