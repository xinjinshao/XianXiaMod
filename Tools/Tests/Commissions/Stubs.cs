using System.IO;
namespace Microsoft.Xna.Framework {public record struct Vector2(float X,float Y){public static float DistanceSquared(Vector2 a,Vector2 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y);}}
namespace Terraria {
public class Player {public bool active=true,dead,CCed,noItems;public int talkNPC=-1;public Microsoft.Xna.Framework.Vector2 Center;public XianXia.Common.Players.XianXiaPlayer State=new();public List<(int Type,int Stack)> Rewards=new();public T GetModPlayer<T>()=>(T)(object)State;public bool HasItem(int type)=>false;public void QuickSpawnItem(object source,int type,int stack)=>Rewards.Add((type,stack));}
public class NPC {public static bool downedPlantBoss,downedGolemBoss,downedMoonlord;public bool active=true,townNPC,friendly;public int whoAmI,width,height,aiStyle,damage,defense,lifeMax;public float knockBackResist;public object HitSound,DeathSound;public Terraria.ModLoader.ModNPC ModNPC;public Microsoft.Xna.Framework.Vector2 Center;public HappinessStub Happiness=new();public object GetSource_FromThis()=>this;}
public class HappinessStub {public void SetBiomeAffection<T>(Terraria.GameContent.Personalities.AffectionLevel level){}}
public class Item {public int type;public void TurnToAir()=>type=0;}
public static class Main {public static int netMode;public static bool hardMode;public static int[] npcFrameCount=new int[5000];public static Player LocalPlayer=new();public static IEnumerable<Player> ActivePlayers=>new[]{LocalPlayer};public static string npcChatText;}
public static class NetMessage {public static int Broadcasts;public static void SendData(int type)=>Broadcasts++;}
}
namespace Terraria.ID {public static class NetmodeID {public const int SinglePlayer=0,MultiplayerClient=1,Server=2;}public static class MessageID {public const int WorldData=7;}public static class NPCID {public const int Guide=22;}public static class NPCAIStyleID {public const int Passive=7;}public static class SoundID {public const int NPCHit1=1,NPCDeath1=2;}}
namespace Terraria.GameContent {public class Dummy {}}
namespace Terraria.GameContent.Bestiary {public class BestiaryDatabase{}public class BestiaryEntry {public List<object> Info=new();}public class FlavorTextBestiaryInfoElement {public FlavorTextBestiaryInfoElement(string key){}}}
namespace Terraria.GameContent.Personalities {public enum AffectionLevel {Like,Love,Dislike}public class ForestBiome{}public class JungleBiome{}public class DesertBiome{}public class UndergroundBiome{}public class OceanBiome{}public class HallowBiome{}}
namespace Terraria.Localization {public class NetworkText {public string Key;public object[] Args;public static NetworkText FromKey(string key,params object[] args)=>new(){Key=key,Args=args};public override string ToString()=>Language.GetTextValue(Key,Args);}public static class Language {public static Dictionary<string,string> Texts=new();public static string GetTextValue(string key,params object[] args)=>string.Format(Texts.GetValueOrDefault(key,key),args);}}
namespace Terraria.ModLoader.IO {public class TagCompound:Dictionary<string,object> {public bool GetBool(string key)=>TryGetValue(key,out var v)&&v is true;public int GetInt(string key)=>TryGetValue(key,out var v)?(int)v:0;public IList<T> GetList<T>(string key)=>TryGetValue(key,out var v)?(IList<T>)v:new List<T>();}}
namespace Terraria.ModLoader {
public class AutoloadHeadAttribute:Attribute{}
public class Mod {public ModPacket GetPacket()=>new();}
public class ModPacket:BinaryWriter {public static List<byte[]> Sent=new();public ModPacket():base(new MemoryStream()){}public void Send()=>Sent.Add(((MemoryStream)BaseStream).ToArray());}
public class ModSystem {public virtual void ClearWorld(){}public virtual void SaveWorldData(IO.TagCompound t){}public virtual void LoadWorldData(IO.TagCompound t){}public virtual void NetSend(BinaryWriter w){}public virtual void NetReceive(BinaryReader r){}}
public class ModItem{}
public class ModNPC {public Terraria.NPC NPC;public string Name=>GetType().Name;public int Type=>ModContent.Id(GetType());public int AIType;public Mod Mod=new();public ModNPC(){NPC=new(){ModNPC=this};}public virtual void SetStaticDefaults(){}public virtual void SetDefaults(){}public virtual void FindFrame(int h){}public virtual void SetBestiary(Terraria.GameContent.Bestiary.BestiaryDatabase d,Terraria.GameContent.Bestiary.BestiaryEntry e){}public virtual bool CanTownNPCSpawn(int n)=>false;public virtual List<string> SetNPCNameList()=>new();public virtual string GetChat()=>"";public virtual void AddShops(){}public virtual void ModifyActiveShop(string name,Terraria.Item[] items){}public virtual void SetChatButtons(ref string b,ref string b2){}public virtual void OnChatButtonClicked(bool first,ref string shop){}}
public class NPCShop {public NPCShop(int type){}public void Add<T>() where T:ModItem{}public void Register(){}}
public static class ModContent {private static Dictionary<Type,int> ids=new();public static int Id(Type t){if(!ids.ContainsKey(t))ids[t]=100+ids.Count;return ids[t];}public static int ItemType<T>() where T:ModItem=>Id(typeof(T));public static string ItemName(int type)=>ids.Single(p=>p.Value==type).Key.Name;}
}
namespace XianXia.Common.Players {public class XianXiaPlayer {public CultivationStage cultivationStage;public int spiritPressure,tribulationTimer;}}
namespace XianXia.Common.Animation {public static class NpcFrameAnimator {public const int TownFrameCount=25;public static void Animate(Terraria.NPC npc,int h,int count,int ticks){}}}
namespace XianXia.Content.Items.Stations {public class AlchemyCauldron : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class ArchiveRemnantLight : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class ArtifactBlankShard : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Stations {public class ArtifactForge : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class ArtifactQuenchingCrystal : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class BlankSectScroll : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class BrokenHeavenCrownSeal : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class BrokenHeavenDecree : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class BrokenHeavenInscriptionNeedle : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class CloudpiercerFlyingSword : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class ColdMoonDust : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class DaoSeveringRing : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class EndgameRouteFrame : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class FormlessSwordWheel : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class FoundationPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class FurnaceHeartRing : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class FurnaceInscriptionNeedle : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class FurnaceSlagIron : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class GreenwoodArrayPlate : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class GreenwoodInscriptionNeedle : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class GreenwoodRoot : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class HeavenDaoFragment : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class HeavenDaoRouteHint : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class InscriptionRemovalStone : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class LightningAvoidanceRune : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class LightningWardJade : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class LowGradeSpiritStone : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class NascentSoulJadeBox : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class OldHeavenDaoScroll : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class QiCondensingPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class QiGatheringPendant : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class QiRecoveryPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Guides {public class SectLedger : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class SectTrialToken : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class SpiritHerbSeeds : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Accessories {public class SpiritwoodCharm : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class SpiritwoodCrossbow : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class SpringReturnPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class StarAbyssInscriptionNeedle : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.BossSummons {public class SummonThunderCallingJade : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class ThunderBurstPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class ThunderInscriptionNeedle : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class ThunderTalismanArrayPlate : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class TribulationCloudDew : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Guides {public class TribulationGauge : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class TribulationResistingPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.HandGenerated {public class TribulationTrainingToken : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Materials {public class WindStepPill : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Weapons {public class WoodgrainFlyingSword : Terraria.ModLoader.ModItem{}}
namespace XianXia.Content.Items.Consumables {public class NamespaceMarker{}}
