// Actual crop hooks, mocked engine placement/render/network boundaries.
namespace Microsoft.Xna.Framework {
 public record struct Color(int R, int G, int B);
 public record struct Vector2(float X, float Y) {
  public Vector2(float value) : this(value,value) { }
  public static Vector2 Zero => new(0,0);
  public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X+b.X,a.Y+b.Y);
  public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X-b.X,a.Y-b.Y);
 }
}
namespace Microsoft.Xna.Framework.Graphics {
 public enum SpriteEffects { None, FlipHorizontally }
 public class Texture2D { public int Width=16, Height=24; }
 public class SpriteBatch {
  public float LastScale;
  public void Draw(Texture2D texture, Microsoft.Xna.Framework.Vector2 position, object source, Microsoft.Xna.Framework.Color color,
   float rotation, Microsoft.Xna.Framework.Vector2 origin, float scale, SpriteEffects effects, float depth) => LastScale=scale;
 }
}
namespace Terraria {
 public class Tile { public bool HasTile; public ushort TileType; public short TileFrameX; public byte LiquidAmount; }
 public static class Framing { public static Tile Cell = new(); public static Tile GetTileSafely(int i,int j) => Cell; }
 public class Item { public int type,stack; public Item(int type,int stack) {this.type=type;this.stack=stack;} }
 public class RandomStub { public bool Grow=true; public int Roll=2; public bool NextBool(int denominator)=>Grow; public int Next(int max)=>Roll; }
 public static class Main {
  public static bool[] tileFrameImportant=new bool[100],tileCut=new bool[100],tileNoFail=new bool[100],tileLavaDeath=new bool[100],tileLighted=new bool[100];
  public static int netMode,offScreenRange; public static bool drawToScreen=true;
  public static Microsoft.Xna.Framework.Vector2 screenPosition; public static RandomStub rand=new();
 }
 public static class WorldGen { public static RandomStub genRand=new(); }
 public static class NetMessage { public static int Updates; public static void SendTileSquare(int who,int i,int j,int size)=>Updates++; }
 public static class Lighting { public static Microsoft.Xna.Framework.Color GetColor(int i,int j)=>new(255,255,255); }
}
namespace Terraria.GameContent {
 public class Asset { public Microsoft.Xna.Framework.Graphics.Texture2D Value=new(); }
 public static class TextureAssets { public static Asset[] Tile=Enumerable.Range(0,100).Select(_=>new Asset()).ToArray(); }
}
namespace Terraria.ID {
 public static class TileID {
  public const int Dirt=0,Grass=2,JungleGrass=60,ClayPot=78,PlanterBox=380;
  public static class Sets { public static bool[] IgnoredInHouseScore=new bool[100],IgnoredByGrowingSaplings=new bool[100]; }
 }
 public static class SoundID { public const int Grass=1; }
 public static class DustID { public const int Grass=1; }
 public static class NetmodeID { public const int SinglePlayer=0,MultiplayerClient=1,Server=2; }
}
namespace Terraria.ObjectData {
 public class TileObjectData {
  public static TileObjectData newTile=new(),StyleAlch=new();
  public int[] AnchorValidTiles,AnchorAlternateTiles; public bool WaterDeath,LavaDeath;
  public void CopyFrom(TileObjectData source) { }
  public static void addTile(int type) { }
 }
}
namespace Terraria.ModLoader {
 public static class ModContent {
  public static int TileType<T>() => typeof(T).Name == "GreenwoodSoilTile" ? 70 : 71;
  public static int ItemType<T>() => typeof(T).Name == "GreenwoodRoot" ? 80 : 81;
 }
 public class ModTile {
  public int Type=72,HitSound,DustType;
  public object CreateMapEntryName()=>null;
  public void AddMapEntry(Microsoft.Xna.Framework.Color color,object name) { }
  public virtual void SetStaticDefaults() { }
  public virtual bool CanPlace(int i,int j)=>true;
  public virtual void RandomUpdate(int i,int j) { }
  public virtual IEnumerable<Terraria.Item> GetItemDrops(int i,int j)=>Array.Empty<Terraria.Item>();
  public virtual bool IsTileSpelunkable(int i,int j)=>false;
  public virtual void ModifyLight(int i,int j,ref float r,ref float g,ref float b) { }
  public virtual void SetDrawPositions(int i,int j,ref int width,ref int offsetY,ref int height,ref short x,ref short y) { }
  public virtual bool PreDraw(int i,int j,Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)=>true;
 }
}
namespace XianXia.Content.Tiles { public class GreenwoodSoilTile { } public class SpiritMossTile { } }
namespace XianXia.Content.Items.HandGenerated { public class SpiritHerbSeeds { } }
namespace XianXia.Content.Items.Materials { public class GreenwoodRoot { } }
