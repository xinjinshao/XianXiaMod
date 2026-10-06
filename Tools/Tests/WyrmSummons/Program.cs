using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using XianXia.Content.NPCs.Bosses;

int checks=0;
void Check(bool ok,string name){checks++;if(!ok)throw new Exception(name);}
void Reset(int mode=0){Main.netMode=mode;Main.dedServ=true;Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i}).ToArray();Main.player=[new(),new(){active=false}];NetMessage.Calls=NPC.Calls=Lighting.Calls=0;NPC.FailAt=0;}
SpiritVeinWyrm Parent(){var boss=new SpiritVeinWyrm();boss.NPC=new(){active=true,life=100,whoAmI=3,target=0,type=7,ModNPC=boss};Main.npc[3]=boss.NPC;return boss;}
ShatteredJadeWyrmMinion Child(SpiritVeinWyrm boss){var child=new ShatteredJadeWyrmMinion();child.SetDefaults();child.NPC.active=true;child.NPC.life=70;child.NPC.whoAmI=4;child.NPC.type=8;child.NPC.ModNPC=child;Main.npc[4]=child.NPC;child.OnSpawn(new EntitySource_Parent(boss.NPC));return child;}
byte[] Save(Terraria.ModLoader.ModNPC npc){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);npc.SendExtraAI(writer);return stream.ToArray();}
void Read(Terraria.ModLoader.ModNPC npc,byte[] data){using var stream=new MemoryStream(data);using var reader=new BinaryReader(stream);npc.ReceiveExtraAI(reader);}
bool Hit(ShatteredJadeWyrmMinion child){int cooldown=0;return child.CanHitPlayer(Main.player[0],ref cooldown)&&child.CanHitNPC(new());}
Reset();var stray=new ShatteredJadeWyrmMinion();stray.NPC.active=true;stray.NPC.life=70;stray.OnSpawn(new NaturalSource());Check(!Hit(stray)&&!stray.PreAI()&&!stray.NPC.active,"unbound boss minion removed");
for(int mode=0;mode<=2;mode++)for(int failure=0;failure<14;failure++){
 Reset();var boss=Parent();var child=Child(boss);Check(Hit(child),"valid source contact");Main.netMode=mode;
 switch(failure){case 0:boss.NPC.active=false;break;case 1:boss.NPC.life=0;break;case 2:Main.player[0].dead=true;break;case 3:boss.NPC.target=255;break;case 4:child.NPC.Center=new(4001,0);break;case 5:child.NPC.velocity=new(float.NaN,0);break;case 6:Main.npc[3]=new(){active=true,life=100,type=7};break;case 7:Main.netMode=0;Parent();Main.netMode=mode;break;case 8:boss.NPC.Center=new(float.PositiveInfinity,0);break;case 9:child.NPC.ai[3]=float.NaN;break;case 10:child.NPC.ai[3]=-1;break;case 11:child.NPC.ai[3]=900;break;case 12:child.NPC.ai[3]=float.PositiveInfinity;break;case 13:child.NPC.life=0;break;}
 Check(!Hit(child)&&!child.PreAI(),"invalid source blocks AI and damage");child.AI();Check(NPC.Calls==0&&Lighting.Calls==0,"invalid direct AI creates no chain or graphics");
 Check(child.NPC.active==(mode==1),"only authority removes child");Check(NetMessage.Calls==(mode==2?1:0),"one server despawn sync");child.PreAI();Check(NetMessage.Calls==(mode==2?1:0),"cleanup idempotent");
}
Reset(2);var expiryBoss=Parent();var expiring=Child(expiryBoss);for(int tick=1;tick<900;tick++)Check(expiring.PreAI()&&expiring.NPC.ai[3]==tick,"authority age exact");Check(!expiring.PreAI()&&!Hit(expiring)&&!expiring.NPC.active&&NetMessage.Calls==1,"expires at tick900");
Reset(2);var liveBoss=Parent();var live=Child(liveBoss);Check(live.PreAI(),"valid battle starts");live.AI();Check(NPC.Calls==5&&Main.npc.Count(n=>n.active&&n.realLife==4)==5,"actual AI creates four bodies and one tail");Check(live.NPC.velocity.Length()>0&&Lighting.Calls==0,"actual movement with no dedicated lighting");live.AI();Check(NPC.Calls==5,"chain only once");liveBoss.NPC.active=false;Check(!live.PreAI(),"head removed with parent");foreach(var segment in Main.npc.Where(n=>n.active&&n.realLife==4).ToArray()){SegmentedWormAI.FollowPreviousSegment(segment,18,0,0,0,8);Check(!segment.active&&segment.damage==0,"segments removed after child head");}
Reset();var first=Parent();var firstSession=first.SummonSession;var second=Parent();Check(firstSession>0&&second.SummonSession!=firstSession,"replacement session unique");var valid=Child(second);var wire=Save(valid);Check(wire.Length==10&&Save(second).Length==8,"fixed source/session wire sizes");
for(int length=0;length<10;length++){var copy=Child(second);var before=Save(copy);try{Read(copy,wire[..length]);throw new Exception("accepted truncated child");}catch(EndOfStreamException){}Check(Save(copy).SequenceEqual(before),"truncated source packet atomic");}
for(int length=0;length<8;length++){var before=Save(second);try{Read(second,before[..length]);throw new Exception("accepted truncated boss");}catch(EndOfStreamException){}Check(Save(second).SequenceEqual(before),"truncated parent packet atomic");}
Reset();var authority=Parent();var serverChild=Child(authority);var bossWire=Save(authority);var childWire=Save(serverChild);Main.netMode=1;var remote=new SpiritVeinWyrm();remote.NPC=authority.NPC;remote.NPC.ModNPC=remote;var remoteChild=new ShatteredJadeWyrmMinion();remoteChild.SetDefaults();remoteChild.NPC.active=true;remoteChild.NPC.life=70;Read(remoteChild,childWire);
Check(!remoteChild.PreAI()&&!Hit(remoteChild)&&remoteChild.NPC.active,"out-of-order child waits harmlessly");Read(remote,bossWire);Check(remoteChild.PreAI()&&Hit(remoteChild),"parent packet restores client");for(int i=0;i<1000;i++){Check(remoteChild.PreAI(),"client prediction alive");remoteChild.AI();}Check(remoteChild.NPC.ai[3]==0&&NPC.Calls==0,"client AI never ages or spawns chain");
Main.netMode=2;Main.player[1].active=true;authority.NPC.target=1;remoteChild.NPC.target=0;Check(remoteChild.PreAI()&&remoteChild.NPC.target==1&&remoteChild.NPC.netUpdate,"authority follows parent target");
Main.netMode=1;remoteChild.NPC.target=0;Check(!remoteChild.PreAI()&&!Hit(remoteChild)&&remoteChild.NPC.active,"client waits for authoritative child target");
foreach(int mode in new[]{0,1,2})foreach(bool tail in new[]{false,true}){
 Reset();var segmentBoss=Parent();var segmentHead=Child(segmentBoss);segmentHead.PreAI();segmentHead.AI();var segmentNpc=Main.npc.First(n=>n.active&&n.type==(tail?10:9));
 Terraria.ModLoader.ModNPC segment=tail?new ShatteredJadeWyrmMinionTail():new ShatteredJadeWyrmMinionBody();segment.NPC=segmentNpc;segmentNpc.life=70;int cooldown=0;
 Check(segment.CanHitPlayer(Main.player[0],ref cooldown)&&segment.CanHitNPC(new()),"live segment contact");Main.netMode=mode;segmentBoss.NPC.active=false;
 Check(!segment.CanHitPlayer(Main.player[0],ref cooldown)&&!segment.CanHitNPC(new()),"parent loss immediately blocks body and tail damage before head AI");segment.AI();Check(segmentNpc.damage==0&&segmentNpc.active==(mode==1),"segment authority cleanup/client harmless wait");
 if(mode==1){segmentBoss.NPC.active=true;segment.AI();Check(segmentNpc.damage==14&&segmentNpc.active,"client segment damage restored with valid source");}
}
Console.WriteLine($"Wyrm summon source/age/actual AI: {checks} checks passed; engine spawn/transport boundary mocked.");
