namespace Microsoft.Xna.Framework{public record struct Vector2(float X,float Y){public static float DistanceSquared(Vector2 a,Vector2 b){float x=a.X-b.X,y=a.Y-b.Y;return x*x+y*y;}}}
namespace Terraria {
 public class Player{public bool active=true,dead;public Microsoft.Xna.Framework.Vector2 Center,velocity;}
 public class NPC{public bool active,netUpdate;public int life=100,type,target,whoAmI;public float[] ai=new float[4];public Microsoft.Xna.Framework.Vector2 Center,velocity;public object GetSource_FromAI()=>this;
 public static int Calls,FailureAt,FailureResult=int.MinValue;public static int NewNPC(object source,int x,int y,int type,float ai0){Calls++;if(FailureResult!=int.MinValue&&(FailureAt==0||Calls==FailureAt))return FailureResult;for(int index=0;index<Main.maxNPCs;index++){if(Main.npc[index].active)continue;Main.npc[index]=new(){active=true,whoAmI=index,type=type,Center=new(x,y)};Main.npc[index].ai[0]=ai0;return index;}return Main.maxNPCs;}}
 public static class Main{public static int netMode,maxNPCs=20,maxPlayers=2;public static NPC[] npc=Array.Empty<NPC>();public static Player[] player=[new(),new()];public static IEnumerable<NPC> ActiveNPCs=>npc.Where(n=>n.active);public static RandomStub rand=new();}
 public class RandomStub{public int Calls,Planned=2;public int Next(int min,int max){Calls++;return min==2&&max==4?Planned:(min+max-1)/2;}public float NextFloat(float min,float max){Calls++;return (min+max)/2;}}
}
namespace Terraria.ID{public static class NetmodeID{public const int MultiplayerClient=1,Server=2;}}
namespace Terraria.ModLoader{public class ModNPC{public Terraria.NPC NPC=new();}public static class ModContent{public static int NPCType<T>()=>7;}}
namespace XianXia.Content.NPCs.Bosses{public partial class SpiritVeinWyrm:Terraria.ModLoader.ModNPC{}}
namespace XianXia.Content.NPCs.Enemies{public class IronShardSpirit{}}

namespace XianXia.Content.NPCs.Bosses{public class ShatteredJadeWyrmMinion{}}
