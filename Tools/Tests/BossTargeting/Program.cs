using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using XianXia.Common.Systems;
int assertions=0;void Check(bool value,string text){assertions++;if(!value)throw new Exception(text);}
NPC Setup(){Main.netMode=NetmodeID.Server;Main.player=[new(){Center=new(100,0)},new(){Center=new(50,0)},new(){Center=new(200,0)}];return new();}
foreach(int mode in new[]{0,NetmodeID.Server,NetmodeID.MultiplayerClient}) {
 foreach(int invalid in new[]{int.MinValue,-1,3,255,int.MaxValue}) {
  var npc=Setup();Main.netMode=mode;npc.target=invalid;
  bool found=BossTargeting.TryGetLivingTarget(npc,out Player target);
  Check(found==(mode!=NetmodeID.MultiplayerClient),"Out-of-range targets must be safe on all peers");
  Check(mode==NetmodeID.MultiplayerClient?target is null&&npc.target==invalid&&!npc.netUpdate:target==Main.player[1]&&npc.target==1&&npc.netUpdate,"Only authority chooses closest living target");
 }
}
foreach(Action<Player> invalidate in new Action<Player>[] {p=>p.active=false,p=>p.dead=true,p=>p.Center=new(float.NaN,0),p=>p.Center=new(0,float.PositiveInfinity),p=>p.velocity=new(float.NegativeInfinity,0),p=>p.Center=new(4001,0)}) {
 var npc=Setup();invalidate(Main.player[0]);Check(!BossTargeting.HasLivingTarget(npc),"Invalid active target cannot allow contact");
 Check(BossTargeting.TryGetLivingTarget(npc,out var target)&&target==Main.player[1],"Authority skips invalid active target");
 npc=Setup();invalidate(Main.player[0]);Main.netMode=NetmodeID.MultiplayerClient;
 Check(!BossTargeting.TryGetLivingTarget(npc,out target)&&npc.target==0&&!npc.netUpdate,"Client waits for synchronized retarget");
}
foreach(float distance in new[]{0f,3999f,4000f,4001f,float.NaN,float.PositiveInfinity}) {
 var npc=Setup();Main.player[0].Center=new(distance,0);Main.player[1].active=Main.player[2].active=false;
 bool valid=float.IsFinite(distance)&&distance<=4000;
 Check(BossTargeting.HasLivingTarget(npc)==valid,"Exact distance limit for contact");
 Check(BossTargeting.TryGetLivingTarget(npc,out _)==valid,"Exact distance limit for targeting");
}
foreach(Action<NPC> invalidate in new Action<NPC>[] {n=>n.active=false,n=>n.life=0,n=>n.life=-1,n=>n.Center=new(float.NaN,0),n=>n.velocity=new(0,float.PositiveInfinity)}) {
 var npc=Setup();invalidate(npc);int original=npc.target;
 Check(!BossTargeting.HasLivingTarget(npc)&&!BossTargeting.TryGetLivingTarget(npc,out _)&&npc.target==original&&!npc.netUpdate,"Invalid NPC cannot mutate target or enable contact");
}
for(int activeMask=0;activeMask<8;activeMask++)for(int deadMask=0;deadMask<8;deadMask++) {
 var npc=Setup();npc.target=-1;
 for(int index=0;index<3;index++){Main.player[index].active=(activeMask&(1<<index))!=0;Main.player[index].dead=(deadMask&(1<<index))!=0;}
 int[] closestOrder=[1,0,2];int expected=closestOrder.FirstOrDefault(i=>Main.player[i].active&&!Main.player[i].dead,-1);
 bool found=BossTargeting.TryGetLivingTarget(npc,out var target);
 Check(found==(expected>=0),"Every active/death combination respects survivor set");
 Check(expected<0?target is null&&!npc.netUpdate:npc.target==expected&&target==Main.player[expected]&&npc.netUpdate,"Every survivor combination selects nearest safely");
}
var stable=Setup();Check(BossTargeting.TryGetLivingTarget(stable,out var current)&&current==Main.player[0]&&!stable.netUpdate,"Valid target stays stable even if another player is closer");
Main.player[0]=null;Check(BossTargeting.TryGetLivingTarget(stable,out current)&&current==Main.player[1],"Missing player slot cannot be dereferenced");
var tied=Setup();tied.target=-1;Main.player[0].Center=new(50,0);Check(BossTargeting.TryGetLivingTarget(tied,out current)&&tied.target==0,"Closest-distance ties are deterministic");
Console.WriteLine($"Boss targeting policy regression passed: {assertions} assertions; player/entity/network boundary mocked.");
