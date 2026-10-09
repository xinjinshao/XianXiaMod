namespace Microsoft.Xna.Framework{public record struct Vector2(float X,float Y){public static Vector2 Zero=>new(0,0);public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.X+b.X,a.Y+b.Y);public static Vector2 UnitY=>new(0,1);public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.X-b.X,a.Y-b.Y);public static Vector2 operator *(Vector2 a,float b)=>new(a.X*b,a.Y*b);public Vector2 SafeNormalize(Vector2 fallback){float length=MathF.Sqrt(X*X+Y*Y);return length>0?this*(1/length):fallback;}public float ToRotation()=>MathF.Atan2(Y,X);public Vector2 RotatedBy(float a)=>new(X*MathF.Cos(a)-Y*MathF.Sin(a),X*MathF.Sin(a)+Y*MathF.Cos(a));public static float DistanceSquared(Vector2 a,Vector2 b){float x=a.X-b.X,y=a.Y-b.Y;return x*x+y*y;}}}
namespace Terraria {
 public class Player{public struct HurtInfo{}public bool active=true,dead;public Microsoft.Xna.Framework.Vector2 Center,velocity;}
 public class NPC{public bool active,netUpdate;public int lifeMax=100;public int damage=34;public float[] localAI=new float[4];public int width=96,height=96;public int life=100,type,target,whoAmI;public float rotation;public float[] ai=new float[4];public Microsoft.Xna.Framework.Vector2 Center,velocity;public object GetSource_FromAI()=>this;
 public static int Calls,FailureResult=int.MinValue;public static int NewNPC(object source,int x,int y,int type,float ai0){Calls++;if(FailureResult!=int.MinValue)return FailureResult;for(int index=0;index<Main.maxNPCs;index++){if(Main.npc[index].active)continue;Main.npc[index]=new(){active=true,whoAmI=index,type=type,Center=new(x,y)};Main.npc[index].ai[0]=ai0;return index;}return Main.maxNPCs;}}
 public static class Main{public static bool dedServ;public static int myPlayer;public static int netMode,maxNPCs=20,maxPlayers=2,maxTilesX=8400,maxTilesY=2400;public static NPC[] npc=Array.Empty<NPC>();public static Player[] player=[new(),new()];public static IEnumerable<NPC> ActiveNPCs=>npc.Where(n=>n.active);public static RandomStub rand=new();}
 public class RandomStub{public int Calls;public Queue<int> Values=new();public int Next(int min,int max){Calls++;return Values.Count>0?Values.Dequeue():(min+max-1)/2;}}
}
namespace Terraria.ID{public static class NetmodeID{public const int MultiplayerClient=1,Server=2;}}
namespace Terraria.ModLoader{public class ModNPC{public Terraria.NPC NPC=new();public virtual bool CanHitPlayer(Terraria.Player p,ref int slot)=>true;public virtual bool PreDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch s,Microsoft.Xna.Framework.Vector2 p,Microsoft.Xna.Framework.Color c)=>true;}public static class ModContent{public static int NPCType<T>()=>7;public static int ProjectileType<T>()=>typeof(T).Name=="BossSpiritBoltProjectile"?1:2;}}
namespace XianXia.Content.NPCs.Bosses{public partial class BlackFurnaceIronGolem:Terraria.ModLoader.ModNPC{}}
namespace XianXia.Content.NPCs.Enemies{public class IronShardSpirit{}}

namespace Microsoft.Xna.Framework{public struct Color{public static Color Cyan=>new();public static Color LightGreen=>new();public static Color OrangeRed=>new();public static Color operator *(Color c,float value)=>c;}public static class MathHelper{public const float TwoPi=MathF.PI*2;public const float PiOver2=MathF.PI/2;}}
namespace Microsoft.Xna.Framework.Graphics{public enum SpriteEffects{None}public class SpriteBatch{public int Calls;public float LastAngle;public Microsoft.Xna.Framework.Vector2 LastScale;public void Draw(object texture,Microsoft.Xna.Framework.Vector2 pos,object rect,Microsoft.Xna.Framework.Color c,float angle,Microsoft.Xna.Framework.Vector2 origin,Microsoft.Xna.Framework.Vector2 scale,SpriteEffects effects,float depth){Calls++;LastScale=scale;LastAngle=angle;}}}
namespace Terraria.GameContent{public static class TextureAssets{public static Asset MagicPixel=new();}public class Asset{public object Value=new();}}

namespace Terraria {public static class Collision{public static bool SpawnBlocked,SpawnLava;public static int BlockedSamples,SolidCalls,LavaCalls;public static Microsoft.Xna.Framework.Vector2 LastBody;public static int LastWidth,LastHeight;public static bool LastAcceptTop;public static bool SolidCollision(Microsoft.Xna.Framework.Vector2 p,int w,int h,bool acceptTopSurfaces=false){LastAcceptTop=acceptTopSurfaces;SolidCalls++;LastBody=p;LastWidth=w;LastHeight=h;if(SpawnBlocked)return true;if(BlockedSamples>0){BlockedSamples--;return true;}return false;}public static bool LavaCollision(Microsoft.Xna.Framework.Vector2 p,int w,int h){LavaCalls++;return SpawnLava;}public static bool Blocked;public static bool CanHitLine(Microsoft.Xna.Framework.Vector2 a,int w,int h,Microsoft.Xna.Framework.Vector2 b,int w2,int h2)=>!Blocked;}}

namespace XianXia.Content.NPCs.Bosses{public partial class MoonboneImmortal:Terraria.ModLoader.ModNPC{}}
namespace XianXia.Content.NPCs.Enemies{public class ArchivedImmortalSoul{}}
namespace XianXia.Content.Projectiles{public class BossSpiritBoltProjectile{}public class BossArrayFieldProjectile{}}
namespace Terraria{public static class Projectile{public static List<Microsoft.Xna.Framework.Vector2> Positions=new();public static List<(int Type,Microsoft.Xna.Framework.Vector2 Velocity)> Shots=new();public static int NewProjectile(object source,Microsoft.Xna.Framework.Vector2 p,Microsoft.Xna.Framework.Vector2 v,int type,int damage,float kb,int owner){Positions.Add(p);Shots.Add((type,v));return 0;}}}

namespace XianXia.Content.NPCs.Bosses{public partial class TribulationCloudAvatar:Terraria.ModLoader.ModNPC{}}
namespace XianXia.Content.NPCs.Enemies{public class TribulationCloudling{}}

namespace XianXia.Content.NPCs.Bosses{public partial class AbyssalStarWomb:Terraria.ModLoader.ModNPC{}}

namespace XianXia.Content.NPCs.Bosses{public partial class FormlessSwordSoul:Terraria.ModLoader.ModNPC{}}
namespace XianXia.Content.NPCs.Enemies{public class ObsessedSwordCultivator{}}

namespace XianXia.Content.NPCs.Bosses{public partial class GreenwoodMedicineKingEcho:Terraria.ModLoader.ModNPC{}}

namespace XianXia.Content.NPCs.Enemies{public class HerbGardenVineSpirit{}}
