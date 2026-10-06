namespace Microsoft.Xna.Framework {public record struct Vector2(float X,float Y){public static float DistanceSquared(Vector2 a,Vector2 b){float x=a.X-b.X,y=a.Y-b.Y;return x*x+y*y;}}}
namespace Terraria {
 public class NPC {public bool active=true,netUpdate;public int life=100,target;public Microsoft.Xna.Framework.Vector2 Center,velocity;}
 public class Player{public bool active=true,dead;public Microsoft.Xna.Framework.Vector2 Center,velocity;}
 public static class Main{public static int maxPlayers=3,netMode;public static Player[] player=[new(),new(),new()];}
}
namespace Terraria.ID{public static class NetmodeID{public const int MultiplayerClient=1,Server=2;}}
