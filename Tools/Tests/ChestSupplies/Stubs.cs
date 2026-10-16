namespace Terraria {
 public struct Tile {public bool HasTile;public ushort TileType;public short TileFrameX,TileFrameY;}
 public class Item {public int type,stack,prefix,width,height,maxStack,value,rare,ResearchUnlockCount;public static int buyPrice(int silver=0)=>silver*100;public bool IsAir=>type==0||stack<=0;public void SetDefaults(int value){type=value;stack=1;}}
 public class Chest {public int x,y;public Item[] item=Enumerable.Range(0,40).Select(_=>new Item()).ToArray();}
 public static class Main {public static int netMode,maxTilesX=840,maxTilesY=600;public static double worldSurface=100;public static Chest[] chest=new Chest[20];public static Tile[,] tile=new Tile[840,600];}

 public static class WorldGen {public static Terraria.Utilities.UnifiedRandom genRand=new();}
}
namespace Terraria.ID {public static class NetmodeID{public const int MultiplayerClient=1;}public static class ItemRarityID{public const int White=0;}public static class TileID{public const int Containers=21;}}
namespace Terraria.ModLoader {public class ModSystem{public virtual void ClearWorld(){}public virtual void PostWorldGen(){}}public static class ModContent{public static T GetInstance<T>()where T:new()=>Instance<T>.Value;public static int ItemType<T>()=>900;private static class Instance<T>where T:new(){public static T Value=new();}}}
namespace XianXia {public class XianXiaConfig{public bool EnableWorldGeneration=true;}}
namespace XianXia.Common.Players {public class XianXiaPlayer{public bool discoveredSpiritualEnergy;public int Revisions;public void AdvanceResourceRevision()=>Revisions++;}}
namespace Terraria {public class Player{public bool active=true,dead,Ready=true;public XianXia.Common.Players.XianXiaPlayer State=new();public T GetModPlayer<T>(){if(!Ready)throw new IndexOutOfRangeException("Mod-player array not installed");return (T)(object)State;}public bool TryGetModPlayer<T>(out T value){value=Ready?GetModPlayer<T>():default;return Ready;}}}
namespace Terraria.ModLoader {public class ModItem{public Terraria.Item Item=new();public virtual void SetStaticDefaults(){}public virtual void SetDefaults(){}public virtual bool OnPickup(Terraria.Player player)=>true;public virtual void UpdateInventory(Terraria.Player player){}}}

namespace Terraria.Utilities {public class UnifiedRandom {public bool Win=true;public int Calls;public bool PickMaximum;public bool NextBool(int n){Calls++;return Win;}public int Next(int min,int max)=>PickMaximum?max-1:min;public int Next(int max)=>PickMaximum?max-1:0;}}
