using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using XianXia.Common.Systems;
int checks=0;void Check(bool ok,string text){checks++;if(!ok)throw new Exception(text);}
byte[] Write(HeartDemonStats state,NPC npc){using var stream=new MemoryStream();state.SendExtraAI(npc,null,new BinaryWriter(stream));return stream.ToArray();}
void Read(HeartDemonStats state,NPC npc,byte[] bytes){using var stream=new MemoryStream(bytes);state.ReceiveExtraAI(npc,null,new BinaryReader(stream));}
byte[] Packet(int max,int current,int damage){using var stream=new MemoryStream();using var w=new BinaryWriter(stream);w.Write(true);w.Write(max);w.Write(current);w.Write(damage);return stream.ToArray();}
foreach(int type in new[]{1,2}) {
 Main.netMode=NetmodeID.Server;var actor=new Player();var npc=new NPC {type=type};var state=new HeartDemonStats();state.OnSpawn(npc,new EntitySource_Parent(actor));
 Check(state.IsHeartDemon&&npc.lifeMax==50&&npc.life==50&&npc.damage==7&&npc.defDamage==7&&npc.netUpdate,"Authority reduces actual heart spawn once");state.OnSpawn(npc,new EntitySource_Parent(actor));Check(npc.lifeMax==50&&npc.damage==7,"Repeated spawn callback cannot compound reduction");
 byte[] first=Write(state,npc);Check(first.Length==13,"Tagged combat payload is exactly thirteen bytes");npc.life=17;byte[] wounded=Write(state,npc);
 Main.netMode=NetmodeID.MultiplayerClient;var remote=new NPC {type=type};var replica=new HeartDemonStats();Read(replica,remote,wounded);Check(replica.IsHeartDemon&&remote.lifeMax==50&&remote.life==17&&remote.damage==7&&remote.defDamage==7,"Client applies full reduced stats and current wounded life");
 for(int cut=0;cut<13;cut++){bool threw=false;try{Read(replica,remote,Packet(900,800,70)[..cut]);}catch(EndOfStreamException){threw=true;}Check(threw&&Write(replica,remote).SequenceEqual(wounded),"Every truncated prefix preserves previous complete combat state");}
 foreach(var invalid in new[]{Packet(0,0,1),Packet(-1,0,1),Packet(50,-1,7),Packet(50,51,7),Packet(50,17,-1)}){Read(replica,remote,invalid);Check(Write(replica,remote).SequenceEqual(wounded),"Invalid complete life or damage cannot replace prior state");}
 Read(replica,remote,first);Check(remote.life==50,"Full life applies exact reduced maximum");Read(replica,remote,new byte[]{0});Check(!replica.IsHeartDemon&&remote.lifeMax==50&&remote.life==50,"Natural tag clears only metadata without rewriting base stats");
 foreach(int mode in new[]{0,2}){Main.netMode=mode;Read(state,npc,Packet(1000,999,999));Read(state,npc,new byte[]{0});Check(Write(state,npc).SequenceEqual(wounded),"Authority ignores incoming NPC combat metadata");}
 state.SetDefaults(npc);Check(!state.IsHeartDemon&&Write(state,npc).SequenceEqual(new byte[]{0}),"NPC defaults clear previous instance tag");
}
foreach(int scenario in Enumerable.Range(0,9)) {
 Main.netMode=scenario==0?1:2;var player=new Player();var npc=new NPC();var state=new HeartDemonStats();
 if(scenario==1)player.active=false;if(scenario==2)player.dead=true;if(scenario==3)player.State.tribulationTimer=0;if(scenario==4)player.State.tribulationKind=0;if(scenario==5)npc.type=99;
 state.OnSpawn(npc,scenario==6?new NaturalSource():new EntitySource_Parent(player,scenario==7?"OtherPlayerAction":scenario==8?null:HeartDemonStats.SpawnContext));Check(!state.IsHeartDemon&&npc.lifeMax==101&&npc.damage==11,"Client inactive dead expired unrelated and natural sources stay unmodified");
}
Main.netMode=2;var tiny=new NPC {lifeMax=1,life=1,damage=0};var tinyState=new HeartDemonStats();tinyState.OnSpawn(tiny,new EntitySource_Parent(new Player()));Check(tiny.lifeMax==1&&tiny.life==1&&tiny.damage==1,"Reduction retains positive health and contact damage");
Console.WriteLine($"Heart demon actual global hooks: {checks} assertions; native spawn transport boundary mocked.");
