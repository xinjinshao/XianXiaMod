using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;
using XianXia.Content.Buffs;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem {
    private static void AuditSpiritOwners(Action<bool,string> check) {
        var previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone();
        var before = Main.projectile.Where(p => p.active).ToHashSet(); var text = Main.combatText; var random = Main.rand;
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        var entries = new[] {
            (ModContent.ProjectileType<SmallArtifactSpirit>(),ModContent.BuffType<SmallArtifactSpiritBuff>()),
            (ModContent.ProjectileType<FurnaceAshSpirit>(),ModContent.BuffType<FurnaceAshSpiritBuff>()),
            (ModContent.ProjectileType<StarAbyssSpirit>(),ModContent.BuffType<StarAbyssSpiritBuff>()),
            (ModContent.ProjectileType<NascentSoulSpirit>(),ModContent.BuffType<NascentSoulSpiritBuff>()),
            (ModContent.ProjectileType<CelestialPuppetSpirit>(),ModContent.BuffType<CelestialPuppetSpiritBuff>()),
            (ModContent.ProjectileType<ArchivedSoulSpirit>(),ModContent.BuffType<ArchivedSoulSpiritBuff>()) };
        try {
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.Server }) foreach (var entry in entries) foreach (int lifecycle in Enumerable.Range(0,6)) {
                Clear(); Main.netMode = netMode; Main.myPlayer = netMode == NetmodeID.Server ? Main.maxPlayers : 0;
                var actor = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); check(state.TryInitializeNetwork(state.CaptureSnapshot()),"native spirit fixture imports owner session"); var snapshot = state.CaptureSnapshot(); actor.AddBuff(entry.Item2,18000);
                int slot = Projectile.NewProjectile(actor.GetSource_FromThis(),actor.Center + new Vector2(100,0),Vector2.Zero,entry.Item1,60,1,0);
                check(slot >= 0 && slot < Main.maxProjectiles,"native registered spirit creation succeeds"); var p = Main.projectile[slot]; var logic = (SmallArtifactSpirit)p.ModProjectile;
                check(logic.CanAct && p.DamageType == DamageClass.Summon,"native fresh spirit binds owner and retains summon class");
                logic.AI(); check(p.active && p.timeLeft == 2,"native healthy spirit buff renews two-frame lease");
                p.ai[0] = 89; p.ai[1] = 18;
                switch(lifecycle) {
                    case 0: state.ResetNetworkSession(); check(state.TryInitializeNetwork(snapshot),"native spirit same-object reconnect imports session"); break;
                    case 1: actor.dead = true; state.UpdateDead(); actor.dead = false; break;
                    case 2: state.OnEnterWorld(); break;
                    case 3: var tag = new TagCompound(); state.SaveData(tag); state.LoadData(tag); break;
                    case 4: state.Initialize(); break;
                    case 5: Main.player[0] = new Player { whoAmI = 0, active = true }; Main.player[0].AddBuff(entry.Item2,18000); break;
                }
                check(!logic.CanAct && logic.CanDamage() == false,"native old-session spirit is harmless before AI despite retained buff");
                int children = Main.projectile.Count(child => child.active && !before.Contains(child) && child != p); logic.AI();
                check(!p.active && !p.friendly && p.hide && p.ai[0] == 0 && p.ai[1] == 0,"native authority spirit cancellation removes entity and resets pending attacks");
                check(Main.projectile.Count(child => child.active && !before.Contains(child) && child != p) == children,"native cancelled spirit cannot create pending volley");
                using var bytes = new MemoryStream(); logic.SendExtraAI(new BinaryWriter(bytes)); check(bytes.ToArray().SequenceEqual(new byte[] { 1 }),"native spirit emits one sticky cancellation byte");
                logic.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 }))); p.active = true; logic.AI();
                check(!p.active && !logic.CanAct,"native forced spirit reactivation and stale metadata cannot revive old session");
            }
        } finally { Clear(); Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; clients.CopyTo(Netplay.Clients,0); }
        check(ReferenceEquals(Main.player[0],previous) && Main.rand == random && Main.combatText == text,"native spirit owner fixture restores authority and visual context");
    }
}
