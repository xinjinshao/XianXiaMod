namespace Microsoft.Xna.Framework {
 public record struct Color(int R,int G,int B);
 public record struct Point(int X,int Y);
 public record struct Vector2(float X,float Y) { public Point ToTileCoordinates()=>new((int)X/16,(int)Y/16); }
}
namespace Terraria {
 public static class Main {
  public static int netMode,maxTilesX=60,maxTilesY=60,buffScanAreaWidth=50,buffScanAreaHeight=50; public static ulong GameUpdateCount;
  public static bool hardMode; public static Tile[,] tile=new Tile[60,60];
  public static bool[] tileSolid=new bool[512],tileMergeDirt=new bool[512],tileBlockLight=new bool[512],tileFrameImportant=new bool[512],tileNoAttach=new bool[512],tileLavaDeath=new bool[512],tileLighted=new bool[512];
 }
 public static class NPC { public static bool downedPlantBoss,downedGolemBoss,downedMoonlord; }
 public struct Tile { public bool HasTile; public ushort TileType; }
 public class Player {
  public bool active=true; public Microsoft.Xna.Framework.Vector2 Center=new(480,480);
  public XianXia.Common.Players.ServerBiomePlayer Biome;
  public T GetModPlayer<T>()=>(T)(object)Biome;
 }
 public class Item {
  public int width,height,maxStack,value,rare,createTile,ResearchUnlockCount;
  public void DefaultToPlaceableTile(int tile)=>createTile=tile;
 }
 public class Condition {
  public System.Func<bool> Check; public Condition(System.Func<bool> check)=>Check=check;
  public static Condition Hardmode=new(()=>Main.hardMode),DownedPlantera=new(()=>NPC.downedPlantBoss),DownedGolem=new(()=>NPC.downedGolemBoss),DownedMoonLord=new(()=>NPC.downedMoonlord);
 }
 public class Recipe {
  public static List<Recipe> All=new(); public int Result,Amount;
  public List<(int,int)> Ingredients=new(); public List<int> Tiles=new(); public List<Condition> Conditions=new();
  public Recipe AddIngredient(int id,int n=1){Ingredients.Add((id,n));return this;}
  public Recipe AddIngredient<T>(int n=1)=>AddIngredient(Terraria.ModLoader.ModContent.ItemType<T>(),n);
  public Recipe AddTile(int id){Tiles.Add(id);return this;}
  public Recipe AddCondition(Condition c){Conditions.Add(c);return this;}
  public void Register()=>All.Add(this);
 }
}
namespace Terraria.ID {
 public static class NetmodeID { public const int Server=2; }
 public static class ItemID { public const int StoneBlock=1,DirtBlock=2,AshBlock=3,Cloud=4,FallenStar=5,Bone=6,GoldBar=7; }
 public static class TileID { public const int WorkBenches=1; }
 public static class ItemRarityID { public const int White=0,LightRed=4; }
 public static class DustID { public const int Stone=1,GoldCoin=2,IceTorch=3,Electric=4,GemSapphire=5; }
}
namespace Terraria.ModLoader {
 public class ModPlayer { public Terraria.Player Player; }
 public class ModSystem { public virtual void TileCountsAvailable(ReadOnlySpan<int> counts){} }
 public enum SceneEffectPriority { BiomeLow }
 public class ModBiome {
  public virtual int Music=>0; public virtual SceneEffectPriority Priority=>0;
  public virtual string BestiaryIcon=>""; public virtual string BackgroundPath=>""; public virtual string MapBackground=>"";
  public virtual Microsoft.Xna.Framework.Color? BackgroundColor=>null;
  public virtual bool IsBiomeActive(Terraria.Player player)=>false;
 }
 public class ModItem {
  public Terraria.Item Item=new(); public virtual string Texture=>"";
  public virtual void SetStaticDefaults(){} public virtual void SetDefaults(){} public virtual void AddRecipes(){}
  public Terraria.Recipe CreateRecipe(int amount=1)=>new(){Result=ModContent.Id(GetType()),Amount=amount};
 }
 public class ModTile {
  public int Type=>ModContent.Id(GetType()); public int DustType,RegisteredDrop; public float MineResist; public int MinPick;
  public virtual string Texture=>""; public virtual void SetStaticDefaults(){}
  public virtual bool CanExplode(int i,int j)=>true;
  public virtual void ModifyLight(int i,int j,ref float r,ref float g,ref float b){}
  public void RegisterItemDrop(int id)=>RegisteredDrop=id;
  public object CreateMapEntryName()=>null; public void AddMapEntry(Microsoft.Xna.Framework.Color c,object name){}
 }
 public static class TileLoader { public static int TileCount=>512; }
 public static class ModContent {
  static Dictionary<Type,int> ids=new(); static Dictionary<Type,object> instances=new();
  public static int Id(Type t){if(!ids.ContainsKey(t))ids[t]=ids.Count+20;return ids[t];}
  public static int TileType<T>()=>Id(typeof(T)); public static int ItemType<T>()=>Id(typeof(T));
  public static T GetInstance<T>() where T:new(){if(!instances.ContainsKey(typeof(T)))instances[typeof(T)]=new T();return (T)instances[typeof(T)];}
 }
}
namespace XianXia.Content.Items.Materials { public class LowGradeSpiritStone {} }
namespace XianXia.Content.Tiles {
 public class SpiritOreTile{} public class SpiritMossTile{} public class GreenwoodSoilTile{} public class SpiritHerbTile{}
 public class FurnaceSlagTile{} public class ThunderCloudTile{} public class StarAbyssCrystalTile{} public class SectRuinBrickTile{} public class FallenHeavenJadeTile{} public class MoonboneTile{}
}

namespace Terraria.DataStructures { public record struct Point16(int X,int Y); public record struct AnchorData(Terraria.Enums.AnchorType Type,int Width,int Offset); }
namespace Terraria.Enums { [Flags] public enum AnchorType { SolidTile=1,SolidWithTop=2,Table=4 } }
namespace Terraria.ObjectData {
 public class TileObjectData {
  public int Width,Height,CoordinatePadding; public int[] CoordinateHeights; public Terraria.DataStructures.Point16 Origin;
  public Terraria.DataStructures.AnchorData AnchorBottom; public bool LavaDeath;
  public static TileObjectData newTile=new(),Style1x1=new();public static Dictionary<int,TileObjectData> Registered=new();
  public void CopyFrom(TileObjectData other){}
  public static void addTile(int id){Registered[id]=newTile;newTile=new();}
 }
}
