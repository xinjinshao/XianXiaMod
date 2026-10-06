using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using XianXia.Content.NPCs.Bosses;
using XianXia.Content.Projectiles;
int assertions=0;void Check(bool condition,string message){assertions++;if(!condition)throw new Exception(message);}
GardenWarden Setup(int life=2800){Main.netMode=NetmodeID.Server;Main.dedServ=true;Main.player=[new Player{Center=new Vector2(300,0)},new Player{Center=new Vector2(400,0)}];Main.npc=[new NPC(),new NPC(),new NPC()];var boss=new GardenWarden();boss.NPC.ModNPC=boss;boss.NPC.life=life;Main.npc[0]=boss.NPC;Projectile.Spawned.Clear();Collision.Blocked=false;boss.AI();return boss;}
GardenBriarPatch Patch(GardenWarden boss){var patch=new GardenBriarPatch();patch.SetDefaults();patch.Projectile.ai[0]=boss.NPC.whoAmI;patch.Projectile.ai[1]=boss.HazardSession;return patch;}
void Tick(GardenWarden boss,int ticks){for(int i=0;i<ticks;i++)boss.AI();}
foreach(var phase in new[]{(life:2800,wait:180,pairs:1),(life:1400,wait:140,pairs:1),(life:600,wait:110,pairs:2)}){
 var boss=Setup(phase.life);Check(boss.HazardSession>0&&boss.BattleTarget==0,"Server initializes a fresh synchronized source session");boss.NPC.ai[1]=phase.wait-1;boss.AI();
 var strips=Projectile.Spawned.Where(p=>p.Type==2).ToArray();Check(boss.NPC.ai[0]==1&&strips.Length==phase.pairs*2,"Stage-specific chase starts bounded briars");
 Check(strips.All(p=>p.Source==0&&p.Session==boss.HazardSession&&p.Damage==9),"Briars carry source identity and server damage");
 Check(strips.All(p=>Math.Abs(p.Position.X-boss.NPC.ai[2])>=112),"Every phase retains the central safe lane");
 Check(strips.All(p=>p.Position.Y==-4),"Briar height anchors to target feet at cast time");
 var original=strips.Select(p=>p.Position).ToArray();Main.player[0].Center=new Vector2(800,50);Tick(boss,59);Check(boss.NPC.ai[0]==1,"Cast recovery cannot start early");boss.AI();Check(boss.NPC.ai[0]==2,"Sixty-tick cast ends in recovery");
 Check(Projectile.Spawned.Where(p=>p.Type==2).Select(p=>p.Position).SequenceEqual(original),"Locked briars never follow moving target");
 Tick(boss,50);Check(boss.NPC.ai[0]==(phase.life==600?3:0),"Only final phase combines recovery with warned dash");
}
var warden=Setup(600);warden.NPC.ai[1]=109;warden.AI();Tick(warden,110);Check(warden.NPC.ai[0]==3,"Final combo enters dash warning");Vector2 lockedAim=new(warden.NPC.ai[2],warden.NPC.ai[3]);Check(Math.Abs((lockedAim-warden.NPC.Center).Length()-384f)<.001f,"Warning extends through full dash travel plus boss radius");int slot=0;
Check(!warden.CanHitPlayer(Main.player[0],ref slot),"Dash warning never deals contact damage");Main.player[0].Center=new Vector2(-500,0);Tick(warden,35);Check(warden.NPC.ai[0]==3,"Full thirty-six-tick warning is preserved");warden.AI();
Check(warden.NPC.ai[0]==4&&Math.Abs(warden.NPC.velocity.Length()-14)<.001f&&warden.NPC.ai[2]==lockedAim.X&&warden.NPC.velocity.X>0,"Dash uses locked point, not the player's new position");
Check(warden.CanHitPlayer(Main.player[0],ref slot),"Only released dash has contact damage");Tick(warden,24);Check(warden.NPC.ai[0]==5&&!warden.CanHitPlayer(Main.player[0],ref slot),"Dash ends after twenty-four ticks with harmless recovery");Tick(warden,50);Check(warden.NPC.ai[0]==0,"Recovery returns to chase");
var marker=Patch(warden);Check(marker.Projectile.width==80&&marker.Projectile.height==48&&marker.Projectile.hostile&&!marker.Projectile.friendly,"Native strip metadata matches its rectangular warning");
for(int remaining=0;remaining<=150;remaining++){marker.Projectile.timeLeft=remaining;Check(marker.CanDamage()==(remaining>15&&remaining<=105),"Exact forty-five-tick warning and fifteen-tick harmless fade "+remaining);}
marker.Projectile.timeLeft=100;marker.OnHitPlayer(Main.player[0],default);Check(Main.player[0].Buff==BuffID.Poisoned&&Main.player[0].Duration==120,"Native hit path applies bounded poison");
Collision.Blocked=true;Check(!marker.CanHitPlayer(Main.player[0])&&!warden.CanHitPlayer(Main.player[0],ref slot),"Walls block strip and contact hits");Main.player[0].Buff=0;marker.OnHitPlayer(Main.player[0],default);Check(Main.player[0].Buff==0,"Blocked hits cannot apply poison");Collision.Blocked=false;
foreach(Action invalidate in new Action[]{()=>warden.NPC.active=false,()=>warden.NPC.life=0,()=>Main.player[0].dead=true,()=>warden.NPC.target=255,()=>warden.NPC.ModNPC=null,()=>Main.player[0].Center=new Vector2(5000,0),()=>marker.Projectile.ai[0]=.5f,()=>marker.Projectile.ai[1]=.5f}){
 warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;invalidate();Check(marker.CanDamage()==false,"Invalid source/target disables harm immediately");marker.AI();Check(!marker.Projectile.active,"Server clears invalid source hazard");
}
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;Main.player[0].dead=true;warden.AI();Check(warden.BattleTarget==1&&warden.NPC.ai[0]==0&&marker.CanDamage()==false,"Retarget renews source generation and cancels old formation");
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;var replacement=Setup();Check(marker.CanDamage()==false&&replacement.HazardSession!=warden.HazardSession,"Same-slot replacement cannot inherit old briars");
warden=Setup();Main.player[0].dead=Main.player[1].dead=true;warden.AI();Check(warden.HazardSession==0&&warden.NPC.DespawnCalls==1&&!warden.CanHitPlayer(Main.player[0],ref slot),"No survivors cancel battle and request despawn");
warden=Setup();Main.player[0].Center=new Vector2(float.NaN,0);warden.AI();Check(warden.HazardSession==0,"Nonfinite target cannot create attacks");
warden=Setup();warden.NPC.ai[0]=float.NaN;warden.AI();Check(warden.NPC.ai[0]==0,"Invalid server phase resets safely");
warden=Setup();using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))warden.SendExtraAI(writer);Check(bytes.Length==8,"ExtraAI contains bounded source session and battle target");bytes.Position=0;replacement.ReceiveExtraAI(new BinaryReader(bytes));Check(replacement.HazardSession==warden.HazardSession&&replacement.BattleTarget==0,"Client source identity roundtrips");}
int prior=replacement.HazardSession;try{replacement.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[4])));throw new Exception("Expected truncation");}catch(EndOfStreamException){Check(replacement.HazardSession==prior,"Truncated ExtraAI cannot partially update source");}
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true)){writer.Write(int.MaxValue);writer.Write(255);}bytes.Position=0;replacement.ReceiveExtraAI(new BinaryReader(bytes));Check(replacement.HazardSession==0&&replacement.BattleTarget==-1,"Invalid source identity cannot enable hazards");}
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;Main.netMode=NetmodeID.MultiplayerClient;Main.player[0].dead=true;marker.AI();Check(marker.Projectile.active&&marker.CanDamage()==false,"Client suppresses invalid hazard harm without authoritative kill");
warden=Setup(600);warden.NPC.ai[0]=3;warden.NPC.ai[1]=35;Main.netMode=NetmodeID.MultiplayerClient;warden.AI();Check(warden.NPC.ai[0]==3&&Projectile.Spawned.Count==0,"Client cannot transition attacks or spawn hazards");
Color color=default;var graphics=new Microsoft.Xna.Framework.Graphics.SpriteBatch();warden.PreDraw(graphics,Vector2.Zero,color);Check(graphics.Calls==0&&Lighting.Calls==0&&CombatText.Calls==0,"Dedicated server avoids all rendering");
Main.dedServ=false;warden.PreDraw(graphics,Vector2.Zero,color);Check(graphics.Calls==1,"Client draws locked dash line");warden=Setup();marker=Patch(warden);Main.dedServ=false;Main.spriteBatch=new();marker.Projectile.timeLeft=150;marker.PreDraw(ref color);Check(Main.spriteBatch.Calls==4,"Warning border draws exact rectangle without active fill");marker.Projectile.timeLeft=100;marker.PreDraw(ref color);Check(Main.spriteBatch.Calls==9,"Active strip adds translucent fill inside border");

warden=Setup();marker=Patch(warden);Check(marker.Projectile.netImportant,"Briar snapshots participate in native late join");
marker.Projectile.timeLeft=105;marker.AI();Check(marker.Projectile.netUpdate,"Activation resynchronizes phase timer");marker.Projectile.netUpdate=false;marker.Projectile.timeLeft=15;marker.AI();Check(marker.Projectile.netUpdate,"Harmless fade resynchronizes timer");
using(var bytes=new MemoryStream()){marker.Projectile.timeLeft=80;using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))marker.SendExtraAI(writer);Check(bytes.Length==2,"Briar snapshot contains bounded remaining lifetime");bytes.Position=0;var joined=Patch(warden);joined.ReceiveExtraAI(new BinaryReader(bytes));Check(joined.Projectile.timeLeft==80&&joined.CanDamage()==true,"Late join retains active age rather than restarting warning");}
try{marker.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[1])));throw new Exception("Expected short timer");}catch(EndOfStreamException){Check(marker.Projectile.timeLeft==80,"Truncated timer does not partially reset age");}
using(var bytes=new MemoryStream()){using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))writer.Write((short)999);bytes.Position=0;marker.ReceiveExtraAI(new BinaryReader(bytes));Check(marker.Projectile.timeLeft==0&&marker.CanDamage()==false,"Malformed timer cannot extend hazardous lifespan");}
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;Main.netMode=NetmodeID.MultiplayerClient;marker.OnHitPlayer(Main.player[0],default);Check(Main.player[0].Buff==BuffID.Poisoned,"Poison follows native local-player hit ownership on clients");
foreach(int mode in new[]{0,1,2})foreach(int bad in new[]{-1,151,int.MaxValue}){
 warden=Setup();marker=Patch(warden);Main.netMode=mode;marker.Projectile.timeLeft=bad;marker.AI();marker.Projectile.timeLeft=100;Check(marker.CanDamage()==false,"bad briar lifetime stays harmless after repair");
 using var packet=new MemoryStream();using(var writer=new BinaryWriter(packet,System.Text.Encoding.UTF8,true))marker.SendExtraAI(writer);var received=Patch(warden);packet.Position=0;received.ReceiveExtraAI(new BinaryReader(packet));received.Projectile.timeLeft=100;Check(received.CanDamage()==false,"briar invalid sentinel cannot revive remotely");
}
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;marker.Projectile.Center=new Vector2(float.NaN,0);marker.AI();marker.Projectile.Center=Vector2.Zero;Check(marker.CanDamage()==false,"bad briar geometry stays harmless");
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=100;Main.npc[0]=null;Check(marker.CanDamage()==false,"missing briar source slot harmless");
warden=Setup();marker=Patch(warden);marker.Projectile.timeLeft=150;Check(!marker.CanHitPlayer(Main.player[0]),"briar warning cannot hit player");marker.Projectile.timeLeft=100;Main.player[0].dead=true;Check(!marker.CanHitPlayer(Main.player[0]),"briar cannot hit dead player");
Console.WriteLine($"Actual Garden Warden battle/strip hooks passed: {assertions} assertions; engine movement, hit, spawning, graphics and transport boundaries mocked.");
