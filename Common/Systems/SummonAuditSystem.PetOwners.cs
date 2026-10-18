using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;
using XianXia.Content.Buffs;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem {
    private static void AuditPetOwners(Action<bool,string> check) {
        var previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone();
        var before = Main.projectile.Where(p => p.active).ToHashSet(); var text = Main.combatText;
        var entries = new[] { (ModContent.ProjectileType<SmallTabletPetProjectile>(),ModContent.BuffType<SmallTabletPetBuff>()), (ModContent.ProjectileType<BlackFurnaceIronGolemPetProjectile>(),ModContent.BuffType<BlackFurnaceIronGolemPetBuff>()) };
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        Projectile Spawn((int Type,int Buff) entry,int netMode) {
            Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
            var actor = new Player { whoAmI = 0, active = true }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
            actor.AddBuff(entry.Buff,18000); int index = Projectile.NewProjectile(actor.GetSource_FromThis(),actor.Center + new Vector2(100,0),Vector2.Zero,entry.Type,0,0,0);
            check(index >= 0 && index < Main.maxProjectiles,"native registered vanity pet creation succeeds"); return Main.projectile[index];
        }
        try {
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            foreach (var entry in entries) foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (int lifecycle in Enumerable.Range(0,6)) {
                var p = Spawn(entry,netMode); var actor = Main.player[0]; var state = actor.GetModPlayer<XianXiaPlayer>(); var snapshot = state.CaptureSnapshot();
                p.ModProjectile.AI(); check(p.active && p.timeLeft == 2 && !p.minion && p.minionSlots == 0 && p.ModProjectile.CanDamage() == false,"native vanity pet buff lease remains cosmetic without minion slots");
                switch(lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native vanity pet same-object reconnect accepted"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true }; Main.player[0].AddBuff(entry.Item2,18000); break;
                }
                p.ModProjectile.AI(); check(!p.active && p.hide && !p.friendly,"native old-session pet ends despite revived owner or retained buff");
                using var bytes = new MemoryStream(); p.ModProjectile.SendExtraAI(new BinaryWriter(bytes)); check(bytes.ToArray().SequenceEqual(new byte[] { 1 }),"native vanity pet encodes sticky cancellation byte");
                p.ModProjectile.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 }))); p.active = true; p.ModProjectile.AI(); check(!p.active,"native stale pet packet and reactivation cannot resume following");
            }
            foreach (var entry in entries) foreach (int owner in new[] { -1,Main.maxPlayers,999 }) {
                var p = Spawn(entry,NetmodeID.SinglePlayer); p.owner = owner; p.ModProjectile.AI(); check(!p.active,"native malformed vanity pet owner avoids unsafe vanilla Kill");
            }
            foreach (var entry in entries) {
                var p = Spawn(entry,NetmodeID.SinglePlayer); var actor = Main.player[0]; p.Center = actor.Center + new Vector2(3000,0); p.ModProjectile.AI();
                check(p.Center == actor.Center + new Vector2(-48 * actor.direction,-56) && p.velocity == Vector2.Zero && p.netUpdate,"native owner pet teleport retains original idle offset");
                Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 1; actor.ClearBuff(entry.Item2); p.ModProjectile.AI(); check(p.active && p.timeLeft == 2,"native observing pet waits for missing buff");
                actor.AddBuff(entry.Item2,18000); p.ModProjectile.AI(); check(p.active && p.timeLeft == 2,"native delayed buff preserves uncancelled pet");
                p.ModProjectile.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 1 }))); p.ModProjectile.AI(); check(p.hide && !p.friendly && p.timeLeft <= 6,"native received pet cancellation stops movement and display");
            }
        } finally { Clear(); Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; clients.CopyTo(Netplay.Clients,0); }
    }
}
