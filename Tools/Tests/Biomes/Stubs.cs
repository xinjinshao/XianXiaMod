// Actual server scan and environmental-effect sources; mocked Terraria boundary.
namespace Microsoft.Xna.Framework
{
    public record struct Point(int X, int Y);
    public record struct Vector2(float X, float Y)
    {
        public float LengthSquared() => X*X+Y*Y;
        public float ToRotation() => MathF.Atan2(Y,X);
        public static Vector2 UnitY => new(0,1);
        public static Vector2 operator *(Vector2 value, float factor) => new(value.X*factor,value.Y*factor);
        public static Vector2 Zero => new(0, 0);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static float DistanceSquared(Vector2 a, Vector2 b) => (a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y);
        public Point ToTileCoordinates() => new((int)(X / 16), (int)(Y / 16));
    }
    public record struct Rectangle(int X, int Y, int Width, int Height)
    {
        public bool Intersects(Rectangle other) => X < other.X+other.Width && X+Width > other.X && Y < other.Y+other.Height && Y+Height > other.Y;
    }
    public static class MathHelper
    {
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
    }
}
namespace Terraria
{
    public static class Main
    {
        public static int maxPlayers => player.Length;
        public static int netMode, myPlayer = 255, buffScanAreaWidth = 20, buffScanAreaHeight = 20;
        public static int maxTilesX = 200, maxTilesY = 100;
        public static ulong GameUpdateCount;
        public static bool dedServ = true;
        public static Tile[,] tile = new Tile[maxTilesX, maxTilesY];
        public static RandomStub rand = new();
        public static Player[] player = new Player[2];
    }
    public class RandomStub
    {
        public bool NextBool(int chance) => true;
        public float NextFloat(float min, float max) => (min + max) / 2;
    }
    public struct Tile { public bool HasTile; public ushort TileType; }
    public class Player
    {
        public bool active = true, dead, Rift, Thunder, Moonbone; public int AddedBuffs; public void AddBuff(int type,int time)=>AddedBuffs++;
        public int whoAmI, width = 20, height = 42, statLife, statLifeMax2 = 100;
        public Microsoft.Xna.Framework.Rectangle Hitbox => new((int)position.X,(int)position.Y,width,height);
        public void Heal(int amount) => statLife = Math.Min(statLifeMax2, statLife + amount);
        public Microsoft.Xna.Framework.Vector2 Center, position;
        public XianXia.Common.Players.ServerBiomePlayer Biome;
        public XianXia.Common.Players.XianXiaPlayer State = new();
        public T GetModPlayer<T>() => (T)(object)(typeof(T) == typeof(XianXia.Common.Players.ServerBiomePlayer) ? Biome : State);
        public bool InModBiome<T>() => typeof(T).Name switch
        { "StarAbyssRiftBiome" => Rift, "ThunderMarshCloudsBiome" => Thunder, "MoonboneAbyssBiome" => Moonbone, _ => false };
        public object GetSource_FromThis() => null;
    }
    public static class Lighting { public static void AddLight(Microsoft.Xna.Framework.Vector2 center, float r, float g, float b) { } }
    public static class NetMessage
    {
        public static int HealMessages;
        public static void SendData(int type, int toWho, int fromWho, object text, int player, float amount) => HealMessages++;
    }
    public class Projectile
    {
        public int width, height, penetrate, timeLeft, localNPCHitCooldown, owner, damage;
        public bool friendly, hostile, tileCollide, ignoreWater, usesLocalNPCImmunity, netImportant;
        public bool active=true; public void Kill()=>active=false;
        public object DamageType;
        public float rotation;
        public Microsoft.Xna.Framework.Vector2 velocity, position;
        public Microsoft.Xna.Framework.Vector2 Center => position;
        public Microsoft.Xna.Framework.Rectangle Hitbox => new((int)position.X,(int)position.Y,width,height);
        public object GetSource_FromAI() => null;
        public static int Spawns;
        public static void NewProjectile(object source, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity,
            int type, int damage, float knockback, int owner) => Spawns++;
    }
    public static class Dust
    {
        public static int Spawns;
        public static void NewDust(Microsoft.Xna.Framework.Vector2 position, int width, int height, int type,
            float x, float y, int alpha, object color, float scale) => Spawns++;
    }
}
namespace Terraria.ID
{
    public static class NetmodeID { public const int SinglePlayer = 0, MultiplayerClient = 1, Server = 2; }
    public static class MessageID { public const int SpiritHeal = 66; }
    public static class DustID { public const int GemSapphire = 1, IceTorch = 2; }
}
namespace Terraria.ModLoader
{
    public class ModProjectile { public Terraria.Projectile Projectile = new(); public virtual bool? CanDamage()=>null; public virtual void SetDefaults() { } public virtual void AI() { } public virtual void OnSpawn(Terraria.DataStructures.IEntitySource source) { } public virtual void SendExtraAI(System.IO.BinaryWriter writer) { } public virtual void ReceiveExtraAI(System.IO.BinaryReader reader) { } }
    public static class DamageClass { public static object Generic = new(), Magic = new(), Melee = new(); }
    public class ModPlayer { public Terraria.Player Player; public virtual void PostUpdateEquips() { } public virtual void ResetEffects() { } public virtual void PostUpdate() { } }
    public class ModSystem { public virtual void ClearWorld() { } }
    public static class TileLoader { public const int TileCount = 16; }
    public static class ModContent { public static int ProjectileType<T>() => 1; public static int BuffType<T>() => 1; public static int TileType<T>() => typeof(T).Name switch { "SwordTabletTile"=>11, "SingingThunderStoneTile"=>12, "RiftMembraneTile"=>13, _=>14 }; }
}
namespace XianXia.Common.Players
{
    public class XianXiaPlayer
    {
        public int spiritualEnergyRegenBonus;
        public int spiritPressure, spiritualEnergy;
        private ulong lastRecovery = ulong.MaxValue;
        public void RestoreSpiritualEnergy(int value) => spiritualEnergy += value;
        public bool TryArrayRecovery(ulong tick) { if (lastRecovery == tick) return false; lastRecovery = tick; return true; }
    }
}
namespace XianXia.Content.Biomes
{
    public class StarAbyssRiftBiome { }
    public class ThunderMarshCloudsBiome { }
    public class MoonboneAbyssBiome { }
}
namespace XianXia.Content.Projectiles { public class TribulationWarningLineProjectile { } }
namespace Terraria.DataStructures { public interface IEntitySource { } public class EntitySource_Parent : IEntitySource { public object Entity; } }

namespace XianXia.Content.Tiles { public class SwordTabletTile {} public class SingingThunderStoneTile {} public class RiftMembraneTile {} }
namespace XianXia.Content.Buffs { public class TribulationResistanceBuff {} }
