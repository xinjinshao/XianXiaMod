namespace Terraria {
 public static class Main { public static int netMode;public const int maxPlayers=2;public static Player[] player=new Player[2]; }
 public class NPC { public int type=1,lifeMax=101,life=101,damage=11,defDamage=11,whoAmI;public bool netUpdate,active=true;public Microsoft.Xna.Framework.Vector2 Center,velocity; }
 public class Player {public int whoAmI;public Microsoft.Xna.Framework.Vector2 Center,velocity;public bool active=true,dead; public XianXia.Common.Players.XianXiaPlayer State=new();public bool TryGetModPlayer<T>(out T state) where T:class {state=State as T;return state!=null;} }
}
namespace Terraria.ID { public static class NetmodeID {public const int SinglePlayer=0,MultiplayerClient=1,Server=2;} }
namespace Terraria.DataStructures { public interface IEntitySource {} public class EntitySource_Parent(object entity,string context="XianXiaHeartDemon"):IEntitySource {public object Entity=entity;public string Context=context;} public class NaturalSource:IEntitySource {} }
namespace Terraria.ModLoader.IO {public class BitReader {} public class BitWriter {} }
namespace Terraria.ModLoader {
 public class GlobalNPC {public virtual bool PreAI(Terraria.NPC n)=>true;public virtual bool CheckDead(Terraria.NPC n)=>true;public virtual bool CanHitPlayer(Terraria.NPC n,Terraria.Player p,ref int c)=>true;public virtual bool CanHitNPC(Terraria.NPC n,Terraria.NPC t)=>true;public virtual bool? CanBeHitByItem(Terraria.NPC n,Terraria.Player p,Terraria.Item i)=>null;public virtual bool? CanBeHitByProjectile(Terraria.NPC n,Terraria.Projectile p)=>null;public virtual bool InstancePerEntity=>false;public virtual bool AppliesToEntity(Terraria.NPC n,bool late)=>true;public virtual void SetDefaults(Terraria.NPC n){}public virtual void OnSpawn(Terraria.NPC n,Terraria.DataStructures.IEntitySource s){}public virtual void SendExtraAI(Terraria.NPC n,IO.BitWriter b,System.IO.BinaryWriter w){}public virtual void ReceiveExtraAI(Terraria.NPC n,IO.BitReader b,System.IO.BinaryReader r){} }
 public static class ModContent {public static int NPCType<T>()=>typeof(T).Name=="TribulationCloudling"?1:2;}
}
namespace XianXia.Content.NPCs.Enemies {public class TribulationCloudling {} public class StarEclipsedCultivator {} }
namespace XianXia.Common.Players {public class XianXiaPlayer {public enum TribulationKind {None,HeartDemon}public TribulationKind tribulationKind=TribulationKind.HeartDemon;public int tribulationTimer=120;public long TribulationSession=1;} }

namespace Microsoft.Xna.Framework {public record struct Vector2(float X,float Y) {public static Vector2 Zero=>new();public static float DistanceSquared(Vector2 a,Vector2 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y);}}
namespace Terraria {public class Item {} public class Projectile {} public static class NetMessage {public static int Calls;public static void SendData(int type,int remote=-1,int ignore=-1,string text=null,int number=0)=>Calls++;}}
namespace Terraria.ID {public static class MessageID {public const int SyncNPC=23;}}
