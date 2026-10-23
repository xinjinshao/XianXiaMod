namespace Terraria {
 public static class Main { public static int netMode; }
 public class NPC { public int type=1,lifeMax=101,life=101,damage=11,defDamage=11;public bool netUpdate; }
 public class Player {public bool active=true,dead; public XianXia.Common.Players.XianXiaPlayer State=new();public bool TryGetModPlayer<T>(out T state) where T:class {state=State as T;return state!=null;} }
}
namespace Terraria.ID { public static class NetmodeID {public const int SinglePlayer=0,MultiplayerClient=1,Server=2;} }
namespace Terraria.DataStructures { public interface IEntitySource {} public class EntitySource_Parent(object entity,string context="XianXiaHeartDemon"):IEntitySource {public object Entity=entity;public string Context=context;} public class NaturalSource:IEntitySource {} }
namespace Terraria.ModLoader.IO {public class BitReader {} public class BitWriter {} }
namespace Terraria.ModLoader {
 public class GlobalNPC {public virtual bool InstancePerEntity=>false;public virtual bool AppliesToEntity(Terraria.NPC n,bool late)=>true;public virtual void SetDefaults(Terraria.NPC n){}public virtual void OnSpawn(Terraria.NPC n,Terraria.DataStructures.IEntitySource s){}public virtual void SendExtraAI(Terraria.NPC n,IO.BitWriter b,System.IO.BinaryWriter w){}public virtual void ReceiveExtraAI(Terraria.NPC n,IO.BitReader b,System.IO.BinaryReader r){} }
 public static class ModContent {public static int NPCType<T>()=>typeof(T).Name=="TribulationCloudling"?1:2;}
}
namespace XianXia.Content.NPCs.Enemies {public class TribulationCloudling {} public class StarEclipsedCultivator {} }
namespace XianXia.Common.Players {public class XianXiaPlayer {public enum TribulationKind {None,HeartDemon}public TribulationKind tribulationKind=TribulationKind.HeartDemon;public int tribulationTimer=120;} }
