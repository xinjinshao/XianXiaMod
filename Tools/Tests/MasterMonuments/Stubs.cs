namespace Microsoft.Xna.Framework { public record struct Color(int R,int G,int B); }
namespace Terraria.DataStructures { public record struct Point16(int X,int Y); public record struct AnchorData(Terraria.Enums.AnchorType Type,int Width,int Offset); }
namespace Terraria.Enums { [System.Flags] public enum AnchorType { SolidTile=1,SolidWithTop=2,Table=4 } }
namespace Terraria { public class Item { public const int CommonMaxStack=9999; public int ResearchUnlockCount,width,height,maxStack,useStyle,useTime,useAnimation,createTile,rare,value; public bool consumable,autoReuse,useTurn,master; public static int buyPrice(int gold)=>gold*10000; } public static class Main { public static bool dedServ; public static List<string> Chat=new(); public static void NewText(string text,byte r,byte g,byte b)=>Chat.Add(text); public static bool[] tileFrameImportant=new bool[100],tileNoAttach=new bool[100],tileLavaDeath=new bool[100],tileLighted=new bool[100]; } }
namespace Terraria.ID { public static class ItemUseStyleID { public const int Swing=1; } public static class ItemRarityID { public const int Master=-13; } public static class DustID { public const int GemTopaz=1; } }
namespace Terraria.ModLoader { public class ModItem { public Terraria.Item Item=new(); public virtual void SetStaticDefaults(){} public virtual void SetDefaults(){} } public class ModTile { public int Type=7,DustType,Drop; public virtual bool RightClick(int i,int j)=>false; public virtual string Texture=>""; public virtual void ModifyLight(int i,int j,ref float r,ref float g,ref float b){} public virtual void SetStaticDefaults(){} public void RegisterItemDrop(int type)=>Drop=type; public object CreateMapEntryName()=>null; public void AddMapEntry(Microsoft.Xna.Framework.Color c,object name){} } }
namespace Terraria.ObjectData { public class TileObjectData { public static TileObjectData newTile=new(),Style1x1=new(); public static TileObjectData Registered; public int Width,Height,CoordinateWidth,CoordinatePadding; public int[] CoordinateHeights; public bool LavaDeath; public Terraria.DataStructures.Point16 Origin; public Terraria.DataStructures.AnchorData AnchorBottom; public void CopyFrom(TileObjectData other){} public static void addTile(int type)=>Registered=newTile; } }

namespace Terraria.ModLoader { public static class ModContent {public static int ItemType<T>()=>typeof(T).Name.Length;} }
namespace XianXia.Content.Items.HandGenerated { public class SpiritVeinWyrmTrophy {}public class AbyssalStarWombLamp {}public class MedicineKingCauldronDecoration {}public class SilentTabletDecoration {} public class EndgameRouteFrame {} }

namespace Terraria.Localization { public static class Language { public static string GetTextValue(string key,params object[] args)=>key+":"+string.Join(",",args); } }
namespace XianXia.Common.Systems {
 public static class DownedBossSystem { public enum EndgameRoute { None,RebuildHeaven,SeverHeaven,AcceptStarAbyss } public static EndgameRoute ChosenRoute; }
 public static class EndgameRouteTransactions { public static string NameKey(DownedBossSystem.EndgameRoute route)=>"Mods.XianXia.Routes."+route; }
}
