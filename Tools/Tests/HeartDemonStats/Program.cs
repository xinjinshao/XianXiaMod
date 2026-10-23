using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using XianXia.Common.Systems;
int checks=0;void Check(bool ok,string text){checks++;if(!ok)throw new Exception(text);}
byte[] Write(HeartDemonStats state,NPC npc){using var stream=new MemoryStream();state.SendExtraAI(npc,null,new BinaryWriter(stream));return stream.ToArray();}
void Read(HeartDemonStats state,NPC npc,byte[] bytes){using var stream=new MemoryStream(bytes);state.ReceiveExtraAI(npc,null,new BinaryReader(stream));}
byte[] Packet(int max,int current,int damage,short slot=0,long session=1,bool rejected=false){using var stream=new MemoryStream();using var w=new BinaryWriter(stream);w.Write(true);w.Write(max);w.Write(current);w.Write(damage);w.Write(slot);w.Write(session);w.Write(rejected);return stream.ToArray();}
Player Caster(){var p=new Player();Main.player=new[]{p,new Player{whoAmI=1}};return p;}
foreach(int type in new[]{1,2}) {
 Main.netMode=NetmodeID.Server;var actor=Caster();var npc=new NPC {type=type};var state=new HeartDemonStats();state.OnSpawn(npc,new EntitySource_Parent(actor));
 Check(state.IsHeartDemon&&state.IsSourceValid(npc)&&npc.lifeMax==50&&npc.life==50&&npc.damage==7&&npc.defDamage==7&&npc.netUpdate,"Authority binds and reduces actual heart spawn once");state.OnSpawn(npc,new EntitySource_Parent(actor));Check(npc.lifeMax==50&&npc.damage==7,"Repeated spawn callback cannot compound reduction");
 byte[] first=Write(state,npc);Check(first.Length==24,"Tagged combat and owner payload is exactly twenty-four bytes");npc.life=17;byte[] wounded=Write(state,npc);
 Main.netMode=NetmodeID.MultiplayerClient;var remote=new NPC {type=type};var replica=new HeartDemonStats();Read(replica,remote,wounded);Check(replica.IsSourceValid(remote)&&remote.lifeMax==50&&remote.life==17&&remote.damage==7&&remote.defDamage==7,"Client applies reduced stats and matching trial owner");
 for(int cut=0;cut<24;cut++){bool threw=false;try{Read(replica,remote,Packet(900,800,70)[..cut]);}catch(EndOfStreamException){threw=true;}Check(threw&&Write(replica,remote).SequenceEqual(wounded),"Every truncated prefix preserves complete combat and owner state");}
 foreach(var invalid in new[]{Packet(0,0,1),Packet(-1,0,1),Packet(50,-1,7),Packet(50,51,7),Packet(50,17,-1),Packet(50,17,7,-1),Packet(50,17,7,2),Packet(50,17,7,0,0),Packet(50,17,7,0,-1)}){Read(replica,remote,invalid);Check(Write(replica,remote).SequenceEqual(wounded),"Invalid complete combat or owner identity cannot replace prior state");}
 actor.State.TribulationSession=0;Check(!replica.IsSourceValid(remote)&&!replica.PreAI(remote)&&remote.active,"Client waits without authoritative despawn while trial epoch is missing");actor.State.TribulationSession=1;Check(replica.IsSourceValid(remote)&&replica.PreAI(remote),"Matching trial epoch enables received NPC");
 Read(replica,remote,first);Check(remote.life==50,"Full life applies exact reduced maximum");Read(replica,remote,new byte[]{0});Check(replica.IsHeartDemon&&replica.IsRejected&&!replica.IsSourceValid(remote),"Old untagged payload cannot turn marked NPC into an unbound natural enemy");Read(replica,remote,first);Check(replica.IsRejected&&remote.damage==0&&!replica.PreAI(remote),"Active metadata cannot revive terminally rejected source");
 foreach(int mode in new[]{0,2}){Main.netMode=mode;Read(state,npc,Packet(1000,999,999));Read(state,npc,new byte[]{0});Check(Write(state,npc).SequenceEqual(wounded),"Authority ignores incoming NPC metadata");}
 state.SetDefaults(npc);Check(!state.IsHeartDemon&&!state.IsRejected&&Write(state,npc).SequenceEqual(new byte[]{0}),"NPC defaults clear old source and rejection");
}
foreach(int scenario in Enumerable.Range(0,9)) {
 Main.netMode=scenario==0?1:2;var player=Caster();var npc=new NPC();var state=new HeartDemonStats();
 if(scenario==1)player.active=false;if(scenario==2)player.dead=true;if(scenario==3)player.State.tribulationTimer=0;if(scenario==4)player.State.tribulationKind=0;if(scenario==5)npc.type=99;
 state.OnSpawn(npc,scenario==6?new NaturalSource():new EntitySource_Parent(player,scenario==7?"OtherPlayerAction":scenario==8?null:HeartDemonStats.SpawnContext));
 Check(scenario is >=1 and <=4?state.IsRejected&&!npc.active&&npc.damage==0:!state.IsHeartDemon&&npc.lifeMax==101&&npc.damage==11,"Invalid explicit heart source cancels; client unrelated and natural sources remain unchanged");
}
foreach(int mode in new[]{0,2})foreach(int invalid in Enumerable.Range(0,10)) {
 Main.netMode=mode;var owner=Caster();var npc=new NPC();var state=new HeartDemonStats();state.OnSpawn(npc,new EntitySource_Parent(owner));int cooldown=0;
 Check(state.PreAI(npc)&&state.CanHitPlayer(npc,Main.player[1],ref cooldown),"Valid source remains active and keeps existing contact policy");
 switch(invalid){case 0:owner.State.tribulationTimer=0;break;case 1:owner.dead=true;break;case 2:owner.active=false;break;case 3:owner.State.TribulationSession++;break;case 4:owner.State.tribulationKind=0;break;case 5:Main.player[0]=new Player();break;case 6:npc.Center=new(4001,0);break;case 7:owner.Center=new(float.NaN,0);break;case 8:npc.velocity=new(float.PositiveInfinity,0);break;case 9:Main.player=Array.Empty<Player>();break;}
 Check(!state.IsSourceValid(npc)&&!state.CanHitPlayer(npc,new Player(),ref cooldown)&&!state.CanHitNPC(npc,new NPC())&&state.CanBeHitByItem(npc,new Player(),new Item())==false&&state.CanBeHitByProjectile(npc,new Projectile())==false,"Lost trial owner blocks outgoing and incoming hits before AI");
 npc.life=0;Check(!state.CheckDead(npc)&&state.IsRejected&&!npc.active&&npc.damage==0,"Invalid death is despawn without normal death path");npc.life=npc.lifeMax;npc.active=true;Check(!state.PreAI(npc)&&!npc.active,"Forced reactivation cannot revive rejected remnant");
}
Main.netMode=2;var valid=Caster();var dying=new NPC();var live=new HeartDemonStats();live.OnSpawn(dying,new EntitySource_Parent(valid));dying.life=0;Check(live.CheckDead(dying),"Legitimate trial enemy retains ordinary death behavior");
var tiny=new NPC {lifeMax=1,life=1,damage=0};var tinyState=new HeartDemonStats();tinyState.OnSpawn(tiny,new EntitySource_Parent(valid));Check(tiny.lifeMax==1&&tiny.life==1&&tiny.damage==1,"Reduction retains positive health and damage");
Console.WriteLine($"Heart demon actual global hooks: {checks} assertions; native spawn transport boundary mocked.");
