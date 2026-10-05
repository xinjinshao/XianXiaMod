namespace Microsoft.Xna.Framework {
 public struct Rectangle {}
 public record struct Vector2(float X,float Y) {
  public static Vector2 Zero=>new(0,0);
  public float LengthSquared()=>X*X+Y*Y;
  public Vector2 SafeNormalize(Vector2 fallback)=>LengthSquared()>0?this/MathF.Sqrt(LengthSquared()):fallback;
  public static float DistanceSquared(Vector2 a,Vector2 b)=>(a-b).LengthSquared();
  public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.X+b.X,a.Y+b.Y);
  public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.X-b.X,a.Y-b.Y);
  public static Vector2 operator *(Vector2 a,float b)=>new(a.X*b,a.Y*b);
  public static Vector2 operator /(Vector2 a,float b)=>new(a.X/b,a.Y/b);
 }
}
namespace Terraria {
 using Microsoft.Xna.Framework;
 public static class Main {
  public static int myPlayer=0,maxNPCs=4,maxProjectiles=8;
  public static bool dedServ;
  public static bool[] projPet=new bool[16],buffNoSave=new bool[16],buffNoTimeDisplay=new bool[16],vanityPet=new bool[16];
  public static Player[] player=new Player[2];
  public static NPC[] npc=Enumerable.Range(0,4).Select(_=>new NPC()).ToArray();
  public static Projectile[] projectile=Enumerable.Range(0,8).Select(_=>new Projectile()).ToArray();
  public static IEnumerable<NPC> ActiveNPCs=>npc.Where(n=>n.active);
  public static IEnumerable<Projectile> ActiveProjectiles=>projectile.Where(p=>p.active);
 }
 public class Player {
  public bool active=true,dead,HasMinionAttackTargetNPC;
  public int whoAmI,direction=1,MinionAttackTargetNPC,maxMinions=1,itemTime;
  public object GetSource_Buff(int index)=>null;
  public float slotsMinions;
  public Vector2 Center;
  public int[] ownedProjectileCounts=new int[16],buffTime=new int[16];
  public bool Buff;
  public bool HasBuff(int type)=>Buff;
  public void AddBuff(int type,int time){Buff=true;buffTime[0]=time;}
  public void DelBuff(int index){Buff=false;buffTime[index]=0;}
  public T GetModPlayer<T>() where T:new()=>new T();
 }
 public class NPC {
  public bool active,chaseable=true;
  public Vector2 Center,position;
  public int width=20,height=20;
  public bool CanBeChasedBy(object source)=>active&&chaseable;
 }
 public class Projectile {
  public int owner,type,width,height,penetrate,timeLeft,localNPCHitCooldown,originalDamage,damage,spriteDirection;
  public bool active,friendly,minion,tileCollide,ignoreWater,usesLocalNPCImmunity,netUpdate,netImportant;
  public float minionSlots,rotation;
  public object DamageType;
  public Vector2 Center,position,velocity;
  public void Kill()=>active=false;
  public static bool AllowSpawn=true;
  public static int NewProjectile(object source,Vector2 position,Vector2 velocity,int type,int damage,float knockback,int owner){
   int index=Array.FindIndex(Main.projectile,p=>!p.active);if(!AllowSpawn||index<0)return Main.maxProjectiles;
   Main.projectile[index]=new Projectile {active=true,Center=position,velocity=velocity,type=type,damage=damage,owner=owner};return index;
  }
 }
 public class Item {
  public int width,height,maxStack,value,rare,damage,mana,useStyle,useTime,useAnimation,buffType,shoot,ResearchUnlockCount;
  public bool accessory,noMelee;
  public object DamageType;
  public void DefaultToVanitypet(int projectile,int buff){shoot=projectile;buffType=buff;maxStack=1;}
  public static int buyPrice(int gold=0)=>gold;
 }
 public static class Collision { public static bool Visible=true; public static bool CanHitLine(Vector2 a,int b,int c,Vector2 d,int e,int f)=>Visible; }
 public static class Lighting {public static int Calls;public static void AddLight(Vector2 a,float b,float c,float d)=>Calls++;}
 public class Recipe {public Recipe AddIngredient<T>(int count=1)=>this;public Recipe AddIngredient(int type,int count=1)=>this;public Recipe AddTile(int type)=>this;public void Register(){} }
}
namespace Terraria.ModLoader {
 public static class DamageClass {public static object Summon=new();}
 public static class ModContent {public static int ProjectileType<T>()=>1;public static int BuffType<T>()=>2;public static int TileType<T>()=>3;}
 public class ModProjectile {
  public int Type=1;public Terraria.Projectile Projectile=new(){active=true};public virtual string Texture=>"";
  public virtual void SetStaticDefaults(){}public virtual void SetDefaults(){}public virtual bool MinionContactDamage()=>false;public virtual bool? CanCutTiles()=>null;public virtual bool? CanDamage()=>null;public virtual void AI(){}
 }
 public class ModBuff {public int Type=2;public virtual string Texture=>"";public virtual void SetStaticDefaults(){}public virtual void Update(Terraria.Player player,ref int index){} }
 public class ModItem {
  public int Type=1;public Terraria.Item Item=new();public virtual void SetStaticDefaults(){}public virtual void SetDefaults(){}
  public virtual void UpdateAccessory(Terraria.Player p,bool hide){}public virtual bool CanUseItem(Terraria.Player p)=>true;
  public virtual bool Shoot(Terraria.Player p,Terraria.DataStructures.EntitySource_ItemUse_WithAmmo s,Microsoft.Xna.Framework.Vector2 position,Microsoft.Xna.Framework.Vector2 velocity,int type,int damage,float knockback)=>true;
  public virtual void UseStyle(Terraria.Player player,Microsoft.Xna.Framework.Rectangle frame){}
  public virtual void AddRecipes(){}public Terraria.Recipe CreateRecipe()=>new();
 }
}
namespace Terraria.ID {
 public static class ProjectileID {public static class Sets {public static bool[] MinionTargettingFeature=new bool[16],MinionSacrificable=new bool[16],CultistIsResistantTo=new bool[16];}}
 public static class ItemID {public const int FallenStar=1;public static class Sets {public static float[] StaffMinionSlotsRequired=new float[16];}}
 public static class ItemRarityID {public const int Blue=1,Orange=2,Yellow=3;}
 public static class ItemUseStyleID {public const int HoldUp=1;}
}
namespace Terraria.DataStructures {public class EntitySource_ItemUse_WithAmmo{}}
namespace XianXia.Common.Players {public class XianXiaPlayer{public int spiritualEnergyRegenBonus;}}
namespace XianXia.Content.Items.Materials {public class LowGradeSpiritStone{}}
namespace XianXia.Content.Tiles.Stations {public class ArtifactForgeTile{}}
