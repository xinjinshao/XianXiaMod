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
 Check(bytes.Length==2,"Remaining field age uses two-byte payload");bytes.Position=0;field.Projectile.timeLeft=120;
 field.ReceiveExtraAI(new BinaryReader(bytes));Check(field.Projectile.timeLeft==remaining,"Late join restores actual remaining age");
}
foreach(short remaining in new short[]{-1,121,short.MinValue,short.MaxValue}) {
 using var bytes=new MemoryStream();using(var writer=new BinaryWriter(bytes,System.Text.Encoding.UTF8,true))writer.Write(remaining);
 bytes.Position=0;field.ReceiveExtraAI(new BinaryReader(bytes));Check(field.Projectile.timeLeft==0&&field.CanDamage()==false,"Invalid age becomes harmless expiry");
 field.Projectile.timeLeft=remaining;using var sent=new MemoryStream();using(var writer=new BinaryWriter(sent,System.Text.Encoding.UTF8,true))field.SendExtraAI(writer);
 Check(sent.ToArray().SequenceEqual(new byte[]{0,0}),"Sender cannot wrap invalid field age");
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
Console.WriteLine($"Actual telegraph hook regression passed: {assertions} assertions; mocked graphics/spawn boundary.");
