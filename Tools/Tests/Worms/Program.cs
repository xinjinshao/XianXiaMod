using Terraria;using Terraria.ID;using XianXia.Content.NPCs.Bosses;
int assertions=0;void Check(bool ok,string msg){assertions++;if(!ok)throw new Exception(msg);}NPC Setup(){Main.player=[new Player(),new Player()];Main.npc=Enumerable.Range(0,Main.maxNPCs).Select(i=>new NPC{whoAmI=i}).ToArray();Main.netMode=NetmodeID.Server;Main.dedServ=true;NPC.Calls=0;NPC.FailAt=0;NetMessage.Calls=0;Lighting.Calls=0;var h=Main.npc[0];h.ModNPC=new TestHead();h.ModNPC.NPC=h;h.active=true;h.type=10;h.life=60;h.lifeMax=100;h.damage=20;h.defense=8;return h;}
var head=Setup();Check(SegmentedWormAI.TrySpawnChain(head,11,12,3),"Complete chain spawns");Check(Main.npc.Count(n=>n.active)==5&&Main.npc[4].type==12,"Body count and tail");var body=Main.npc[2];SegmentedWormAI.FollowPreviousSegment(body,16,.1f,.1f,.1f,10);Check(body.life==60&&body.lifeMax==100&&body.damage==20&&body.defense==8&&body.realLife==0,"Segments inherit actual head state");Check(Lighting.Calls==0,"No dedicated lighting");head.active=false;SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(!body.active&&NetMessage.Calls==1,"Server cleans missing head and synchronizes");
head=Setup();NPC.FailAt=3;Check(!SegmentedWormAI.TrySpawnChain(head,11,12,3),"Mid-chain failure returns false");Check(Main.npc.Count(n=>n.active)==1&&head.active&&NetMessage.Calls==2,"Rollback only created segments");
head=Setup();foreach(var n in Main.npc.Skip(1))n.active=true;Check(!SegmentedWormAI.TrySpawnChain(head,11,12,3)&&NPC.Calls==0,"Capacity preflight avoids partial chains");Check(SegmentedWormAI.SpawnSegment(head,0,11,1)==-1,"Sentinel does not index array");
head=Setup();Main.netMode=NetmodeID.MultiplayerClient;Check(!SegmentedWormAI.TrySpawnChain(head,11,12,3)&&NPC.Calls==0,"Client cannot generate segments");body=Main.npc[1];body.active=true;body.ai[0]=0;body.ai[1]=0;body.damage=20;head.active=false;SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(body.active&&body.damage==0&&NetMessage.Calls==0,"Out-of-order client data waits without damage or authoritative deletion");
head=Setup();Check(SegmentedWormAI.TrySpawnChain(head,11,12,1),"Small chain");head.type=99;body=Main.npc[1];SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(!body.active,"Reused head slot cannot attach to unrelated NPC");
head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,2);Main.npc[1].realLife=7;body=Main.npc[2];SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(!body.active,"Previous segment must belong to same head");

foreach(Action<NPC> invalidate in new Action<NPC>[] {n=>n.target=-1,n=>n.target=255,n=>n.life=0,n=>Main.player[0].dead=true,n=>Main.player[0].active=false,n=>Main.player[0].Center=new(5000,0),n=>Main.player[0].Center=new(float.NaN,0)}) {
 head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,1);body=Main.npc[1];invalidate(head);
 SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(body.damage==0,"Live head with invalid target cannot lend contact damage to body");
}
head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,1);body=Main.npc[1];Main.player[0].dead=true;SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Main.player[0].dead=false;SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(body.damage==head.damage,"Valid returning head restores its current contact damage");

byte[] Save(Terraria.ModLoader.ModNPC mod){using var stream=new System.IO.MemoryStream();using var writer=new System.IO.BinaryWriter(stream);mod.SendExtraAI(writer);return stream.ToArray();}
void Read(Terraria.ModLoader.ModNPC mod,byte[] bytes){using var stream=new System.IO.MemoryStream(bytes);using var reader=new System.IO.BinaryReader(stream);mod.ReceiveExtraAI(reader);}
foreach(int mode in new[]{0,1,2})foreach(bool replaceHead in new[]{false,true}){
 head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,2);body=Main.npc[2];int cooldown=0;
 Check(body.ModNPC.CanHitPlayer(Main.player[0],ref cooldown),"valid generation contact");
 int slot=replaceHead?0:1;var old=Main.npc[slot];var replacement=new NPC{active=true,whoAmI=slot,type=old.type,life=old.life,lifeMax=old.lifeMax,damage=20,realLife=old.realLife,ai=(float[])old.ai.Clone()};Main.npc[slot]=replacement;
 replacement.ModNPC=replaceHead?new TestHead():new TestSegment();replacement.ModNPC.NPC=replacement;
 if(!replaceHead)((XianXia.Common.NPCs.LinkedWormNPC)replacement.ModNPC).Bind((XianXia.Common.NPCs.LinkedWormNPC)head.ModNPC,(XianXia.Common.NPCs.LinkedWormNPC)head.ModNPC);
 Save(replacement.ModNPC);Main.netMode=mode;
 Check(!body.ModNPC.CanHitPlayer(Main.player[0],ref cooldown)&&!body.ModNPC.CanHitNPC(new()),"same-type same-slot replacement denies contact before AI");
 SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(body.damage==0&&body.active==(mode==1),"old generation cleanup or harmless client wait");
 int synced=NetMessage.Calls;SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(NetMessage.Calls==synced,"cleanup sync idempotent");
}
foreach(float bad in new[]{-1f,0.5f,float.NaN,float.PositiveInfinity,float.MaxValue,Main.maxNPCs})foreach(int aiSlot in new[]{0,1}){
 head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,2);body=Main.npc[2];body.ai[aiSlot]=bad;SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(!body.active&&body.damage==0,"invalid/fractional link indices reject before array read");
}
head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,2);body=Main.npc[2];var packet=Save(body.ModNPC);Check(packet.Length==24,"fixed twenty-four byte link state");
for(int length=0;length<24;length++){var before=Save(body.ModNPC);try{Read(body.ModNPC,packet[..length]);throw new Exception("accepted truncated link state");}catch(System.IO.EndOfStreamException){}Check(Save(body.ModNPC).SequenceEqual(before),"truncated linkage packet atomic");}
Main.netMode=1;var parentPacket=Save(head.ModNPC);var originalParent=head.ModNPC;head.ModNPC=new TestHead{NPC=head};Check(!SegmentedWormAI.HasValidLinks(body,out _),"missing client head identity waits");Read(head.ModNPC,parentPacket);Check(SegmentedWormAI.HasValidLinks(body,out _),"received parent identity restores existing client link");
foreach(long invalid in new[]{-1L,0L,long.MinValue})foreach(int field in new[]{0,1,2}){var malformed=(byte[])packet.Clone();BitConverter.GetBytes(invalid).CopyTo(malformed,field*8);Read(body.ModNPC,malformed);Check(!SegmentedWormAI.HasValidLinks(body,out _),"invalid identity cannot fall back to slot");}Read(body.ModNPC,packet);
head=Setup();Check(!SegmentedWormAI.TrySpawnChain(head,11,12,int.MaxValue)&&NPC.Calls==0,"oversized count cannot overflow preflight");
foreach(Action<NPC,NPC> invalidate in new Action<NPC,NPC>[] {(h,b)=>Main.npc[1].Center=new(float.NaN,0),(h,b)=>b.Center=new(float.PositiveInfinity,0),(h,b)=>{Main.npc[1].Center=new(float.MaxValue,0);b.Center=new(-float.MaxValue,0);},(h,b)=>Main.npc[1].life=0}){head=Setup();SegmentedWormAI.TrySpawnChain(head,11,12,2);body=Main.npc[2];invalidate(head,body);SegmentedWormAI.FollowPreviousSegment(body,16,0,0,0,10);Check(!body.active,"invalid or overflowing geometry/dead previous clears safely");}
Console.WriteLine($"Actual segmented-worm source regression passed: {assertions} assertions; mocked spawn/network boundary.");
