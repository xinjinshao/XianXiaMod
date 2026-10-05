using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;
using XianXia.Content.Buffs;
using XianXia.Content.Items.HandGenerated;
using Game=Terraria.Main;
int assertions=0;
void Check(bool condition,string message){assertions++;if(!condition)throw new Exception(message);}
SmallArtifactSpirit Setup(){
 Game.myPlayer=0;Game.dedServ=false;Game.player[0]=new Player {Buff=true};Game.player[1]=new Player {whoAmI=1,Buff=true};
 foreach(var npc in Game.npc)npc.active=false;foreach(var p in Game.projectile)p.active=false;
 Collision.Visible=true;Projectile.AllowSpawn=true;var spirit=new SmallArtifactSpirit();spirit.SetDefaults();return spirit;
}
var spirit=Setup();spirit.SetStaticDefaults();
Check(spirit.Projectile.minionSlots==1&&ReferenceEquals(spirit.Projectile.DamageType,DamageClass.Summon)&&Game.projPet[1],"A real minion occupies one slot and uses summon damage.");
Check(spirit.MinionContactDamage()&&spirit.CanCutTiles()==false&&!spirit.Projectile.tileCollide,"Contact damage is enabled without cutting or collision trapping.");
spirit.AI();Check(!spirit.Projectile.friendly&&spirit.Projectile.velocity.X<0&&spirit.Projectile.velocity.Y<0,"Idle spirits return behind the owner without damaging.");
spirit=Setup();Game.npc[0].active=true;Game.npc[0].Center=new(100,0);spirit.AI();Check(spirit.Projectile.friendly&&spirit.Projectile.velocity.X>0,"A visible valid enemy is chased.");
spirit=Setup();Game.npc[0].active=true;Game.npc[0].Center=new(100,0);Collision.Visible=false;spirit.AI();Check(!spirit.Projectile.friendly,"Blocked targets do not cause attacks through walls.");
spirit=Setup();Game.npc[0].active=true;Game.npc[0].Center=new(701,0);spirit.AI();Check(!spirit.Projectile.friendly,"Enemies outside the owner's target range are ignored.");
spirit=Setup();Game.npc[0].active=true;Game.npc[0].chaseable=false;Game.npc[0].Center=new(50,0);spirit.AI();Check(!spirit.Projectile.friendly,"Friendly and unchaseable NPCs are ignored.");Game.npc[0].chaseable=true;
spirit=Setup();Game.npc[0].active=Game.npc[1].active=true;Game.npc[0].Center=new(50,0);Game.npc[1].Center=new(-100,0);
Game.player[0].HasMinionAttackTargetNPC=true;Game.player[0].MinionAttackTargetNPC=1;spirit.AI();Check(spirit.Projectile.velocity.X<0,"A valid selected target takes priority over nearer enemies.");
spirit=Setup();spirit.Projectile.Center=new(1700,0);spirit.AI();Check(spirit.Projectile.Center==new Vector2(-40,-48)&&spirit.Projectile.netUpdate,"The owner teleports a distant spirit back and requests synchronization.");
spirit=Setup();Game.myPlayer=1;spirit.Projectile.Center=new(1700,0);spirit.AI();Check(spirit.Projectile.Center==new Vector2(1700,0)&&!spirit.Projectile.netUpdate,"Other clients do not issue competing teleports.");
spirit=Setup();Game.player[0].Buff=false;spirit.AI();Check(!spirit.Projectile.active,"Canceling the owner's buff ends the minion.");
spirit=Setup();Game.myPlayer=1;Game.player[0].Buff=false;spirit.AI();Check(spirit.Projectile.active&&!spirit.Projectile.friendly&&spirit.Projectile.timeLeft==180,"First-time observers wait for buff synchronization without attacking.");
spirit=Setup();Game.player[0].dead=true;spirit.AI();Check(!spirit.Projectile.active,"Death ends the minion.");
spirit=Setup();Game.player[0].active=false;spirit.AI();Check(!spirit.Projectile.active,"Disconnecting ends the minion.");
spirit=Setup();Game.dedServ=true;int lights=Lighting.Calls;spirit.AI();Check(Lighting.Calls==lights,"Dedicated servers do not execute the visual lighting effect.");
var buff=new SmallArtifactSpiritBuff();buff.SetStaticDefaults();Check(Game.buffNoSave[2]&&Game.buffNoTimeDisplay[2],"The minion buff is not persisted or timed visually.");
Setup();int slot=0;Game.player[0].ownedProjectileCounts[1]=1;buff.Update(Game.player[0],ref slot);Check(Game.player[0].buffTime[0]==18000,"Owned minions maintain the buff.");
Game.player[0].ownedProjectileCounts[1]=0;buff.Update(Game.player[0],ref slot);Check(!Game.player[0].Buff&&slot==-1,"Missing minions clear their buff correctly.");
Setup();var pendant=new SmallArtifactPendant();pendant.SetDefaults();Check(pendant.CanUseItem(Game.player[0]),"A free slot allows summoning.");
Game.player[0].slotsMinions=1;Check(!pendant.CanUseItem(Game.player[0]),"Full minion capacity prevents summoning.");
Setup();Game.projectile[0]=new Projectile {active=true,owner=0,type=1};Check(!pendant.CanUseItem(Game.player[0]),"Existing owned spirits prevent duplicates.");
Game.projectile[0].owner=1;Check(pendant.CanUseItem(Game.player[0]),"Another player's spirit does not occupy the owner's type cap.");
foreach(var p in Game.projectile)p.active=true;Check(!pendant.CanUseItem(Game.player[0]),"A full projectile pool prevents summoning.");
Setup();pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);Check(Game.projectile[0].active&&Game.projectile[0].originalDamage==12&&Game.projectile[0].damage==18&&Game.player[0].Buff,"The owner's actual summon retains base and computed damage plus the buff.");
Setup();Game.player[0].Buff=false;Projectile.AllowSpawn=false;pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);Check(!Game.player[0].Buff&&!Game.ActiveProjectiles.Any(),"Spawn failure does not create a phantom buff.");
Setup();Game.myPlayer=1;pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);Check(!Game.ActiveProjectiles.Any(),"Observers and dedicated servers cannot duplicate the owner's summon.");
Setup();pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);
Check(Game.ActiveProjectiles.Count()==1,"Reentrant shoot calls cannot bypass the one-spirit cap.");
Setup();Check(pendant.CanUseItem(Game.player[0]),"Preflight initially succeeds.");Game.player[0].slotsMinions=1;pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);
Check(!Game.ActiveProjectiles.Any(),"A slot consumed after preflight blocks the final summon.");
Setup();Game.player[0].dead=true;pendant.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,1,18,0);
Check(!Game.ActiveProjectiles.Any(),"Death between preflight and shooting cannot create a minion.");
Setup();Game.player[0].Buff=false;spirit= new SmallArtifactSpirit();spirit.SetDefaults();Game.myPlayer=255;Game.dedServ=true;spirit.AI();
Check(spirit.Projectile.active&&!spirit.Projectile.friendly&&spirit.Projectile.timeLeft==180,"The dedicated server waits for the initial owner buff sync without killing or attacking.");
Console.WriteLine($"Companion lifecycle regression passed: {assertions} assertions against actual item/buff/projectile hooks; engine boundaries mocked.");

// Execute new contract item, minion and buff sources using independent type IDs.
foreach(var contract in new XianXia.Content.Items.HandGenerated.SpiritContractItem[]{new XianXia.Content.Items.HandGenerated.FurnaceAshSpiritContract(),new XianXia.Content.Items.HandGenerated.StarAbyssLarvaContract()}) {
 Setup();Game.hardMode=true;Game.player[0].Buff=false;contract.SetStaticDefaults();contract.SetDefaults();
 Check(contract.Item.accessory&&contract.Item.DamageType==Terraria.ModLoader.DamageClass.Summon&&contract.Item.noMelee,"Contract remains accessory and gains native summon weapon defaults");
 Check(contract.CanUseItem(Game.player[0]),"Contract accepts a free minion slot");
 contract.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,999,70,0);
 Check(Game.projectile[0].type==contract.Item.shoot&&Game.projectile[0].originalDamage==contract.Item.damage&&Game.player[0].Buff,"Contract uses canonical summon type, damage and buff");
 contract.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,999,70,0);
 Check(Game.ActiveProjectiles.Count()==1,"Each contract prevents duplicate same-type spirits");
 Setup();Game.myPlayer=255;Game.player[0].Buff=false;contract.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,contract.Item.shoot,70,0);
 Check(!Game.ActiveProjectiles.Any()&&!Game.player[0].Buff,"Dedicated server does not duplicate owner summon");
 Setup();Game.player[0].slotsMinions=1;Check(!contract.CanUseItem(Game.player[0]),"Contract respects exhausted minion slots");
 Setup();Game.player[0].Buff=false;Projectile.AllowSpawn=false;contract.Shoot(Game.player[0],null,Vector2.Zero,Vector2.Zero,contract.Item.shoot,70,0);
 Check(!Game.player[0].Buff&&!Game.ActiveProjectiles.Any(),"Failed contract spawning never grants phantom buff");
}
Setup();Game.hardMode=false;var starContract=new XianXia.Content.Items.HandGenerated.StarAbyssLarvaContract();starContract.SetDefaults();Check(!starContract.CanUseItem(Game.player[0]),"Star contract cannot summon in a pre-Hardmode world");
var furnace=new XianXia.Content.Projectiles.FurnaceAshSpirit();furnace.SetDefaults();var burnt=new NPC();furnace.OnHitNPC(burnt,default,28);Check(burnt.LastBuff==Terraria.ID.BuffID.OnFire3&&burnt.BuffDuration==120,"Furnace contact inflicts exactly two seconds of fire");
Setup();var larva=new XianXia.Content.Projectiles.StarAbyssSpirit();larva.SetDefaults();Game.npc[0].active=true;Game.npc[0].Center=new(300,0);
for(int i=0;i<59;i++)larva.AI();Check(!Game.ActiveProjectiles.Any()&&!larva.Projectile.friendly,"Star spirit waits one second and never deals contact damage");
larva.Projectile.damage=72;larva.AI();Check(Game.ActiveProjectiles.Count()==1&&Game.projectile[0].type==5&&Game.projectile[0].damage==72,"Star spirit fires one owner-authoritative summon bolt using scaled damage");
Check(larva.Projectile.velocity.X>0,"Ranged spirit approaches a position offset from its target");
Setup();Game.myPlayer=255;larva=new();larva.SetDefaults();Game.npc[0].active=true;Game.npc[0].Center=new(300,0);for(int i=0;i<120;i++)larva.AI();Check(!Game.ActiveProjectiles.Any(),"Server never repeats star spirit shots");
Setup();larva=new();larva.SetDefaults();Game.npc[0].active=true;Collision.Visible=false;for(int i=0;i<120;i++)larva.AI();Check(!Game.ActiveProjectiles.Any(),"Star spirit cannot fire through walls");
Setup();larva=new();larva.SetDefaults();Game.player[0].Buff=false;larva.AI();Check(!larva.Projectile.active,"Canceling star buff ends its minion");
Setup();spirit=Setup();spirit.Projectile.owner=999;spirit.AI();Check(!spirit.Projectile.active,"Invalid projectile owners cannot index player array");
var shot=new XianXia.Content.Projectiles.ContractSpiritBolt();shot.SetStaticDefaults();shot.SetDefaults();Check(shot.Projectile.DamageType==Terraria.ModLoader.DamageClass.Summon&&shot.Projectile.friendly&&shot.Projectile.tileCollide,"Contract bolt is a terrain-colliding native summon shot");
Console.WriteLine($"Companion regression including contracts: {assertions} assertions.");

foreach(var pair in new (XianXia.Content.Buffs.ContractSpiritBuff Buff,int Id)[]{(new XianXia.Content.Buffs.FurnaceAshSpiritBuff(),3),(new XianXia.Content.Buffs.StarAbyssSpiritBuff(),4)}){
 Setup();pair.Buff.Type=pair.Id;pair.Buff.SetStaticDefaults();
 Check(Game.buffNoSave[pair.Id]&&Game.buffNoTimeDisplay[pair.Id],"Contract buff is not saved or timed visually");
 Game.player[0].ownedProjectileCounts[pair.Id]=1;int index=0;pair.Buff.Update(Game.player[0],ref index);
 Check(Game.player[0].buffTime[0]==18000,"Contract buff recognizes its own spirit type");
 Game.player[0].ownedProjectileCounts[pair.Id]=0;Game.player[0].ownedProjectileCounts[1]=1;pair.Buff.Update(Game.player[0],ref index);
 Check(index==-1&&!Game.player[0].Buff,"Another minion type cannot maintain a canceled contract spirit buff");
}
Console.WriteLine($"Including separate contract buff lifecycles: {assertions} assertions.");
