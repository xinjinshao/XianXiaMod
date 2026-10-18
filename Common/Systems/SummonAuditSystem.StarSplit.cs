using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem {
    private static void AuditStarSplitHits(Action<bool,string> check) {
        var previous = Main.player[0]; var connection = Netplay.Connection; int mode = Main.netMode, local = Main.myPlayer; var clients = (RemoteClient[])Netplay.Clients.Clone();
        var before = Main.projectile.Where(p => p.active).ToHashSet(); var text = Main.combatText; var random = Main.rand; NPC target = null;
        int parentType = ModContent.ProjectileType<StarEclipseSplitBolt>(), childType = ModContent.ProjectileType<SpiritBolt>();
        void Clear() { foreach (var p in Main.projectile.Where(p => p.active && !before.Contains(p))) { p.damage = 0; p.active = false; } }
        try {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0; Netplay.Connection = new RemoteServer { Socket = new AuditOutputSocket(false) };
            for (int i = 0; i < clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            var actor = new Player { whoAmI = 0, active = true, statLife = 100, statLifeMax2 = 100 }; actor.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = actor;
            int slot = NPC.NewNPC(new EntitySource_Misc("StarSplitAudit"),(int)actor.Center.X,400,NPCID.BlueSlime);
            check(slot >= 0 && slot < Main.maxNPCs,"native split audit creates living vanilla NPC"); target = Main.npc[slot]; target.Center = actor.Center; target.lifeMax = 10000; target.damage = target.defense = 0;
            Projectile Spawn(int netMode,int observer,int damage = 201) {
                Clear(); Main.netMode = netMode; Main.myPlayer = observer;
                int index = Projectile.NewProjectile(actor.GetSource_FromThis(),target.Center,Vector2.UnitX * 10,parentType,damage,3,0);
                check(index >= 0 && index < Main.maxProjectiles,"native star split registered creation succeeds"); return Main.projectile[index];
            }
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient }) {
                var parent = Spawn(netMode,0); target.active = true; target.life = 10000; Array.Clear(target.immune); parent.Damage();
                var children = Main.projectile.Where(p => p.active && !before.Contains(p) && p.type == childType).ToArray();
                check(target.life < 10000 && children.Length == 2,"native actual NPC collision creates exactly two split children on owner");
                foreach (int side in new[] { -1,1 }) {
                    Vector2 expected = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(18f * side)) * 8.5f;
                    check(children.Count(p => Vector2.DistanceSquared(p.velocity,expected) < .001f && p.owner == 0 && p.damage == 100 && p.knockBack == 1 && p.ai[0] == 1 && p.DamageType == DamageClass.Ranged) == 1,"native split preserves plus/minus 18 degrees, 85 percent speed, half ranged damage and recursion marker");
                }
                Array.Clear(target.immune); parent.Damage();
                check(Main.projectile.Count(p => p.active && !before.Contains(p) && p.type == childType) == 4,"native second legitimate penetrating hit retains a second split pair");
                var killing = Spawn(netMode,0,1); target.active = false; target.life = 0; killing.ModProjectile.OnHitNPC(target,default,1);
                check(Main.projectile.Count(p => p.active && !before.Contains(p) && p.type == childType && p.damage == 1) == 2,"native killing hit retains split pair with minimum damage one");
            }
            foreach (int netMode in new[] { NetmodeID.MultiplayerClient,NetmodeID.Server }) {
                var parent = Spawn(netMode,netMode == NetmodeID.Server ? Main.maxPlayers : 1); parent.ModProjectile.OnHitNPC(target,default,1);
                check(!Main.projectile.Any(p => p.active && !before.Contains(p) && p.type == childType),"native observer and server do not duplicate split children");
            }
            var marked = Spawn(NetmodeID.SinglePlayer,0); marked.ai[0] = 1; marked.ModProjectile.OnHitNPC(target,default,1);
            check(!Main.projectile.Any(p => p.active && !before.Contains(p) && p.type == childType),"native split recursion marker suppresses additional children");
            var full = Spawn(NetmodeID.SinglePlayer,0); var borrowed = Main.projectile.Take(Main.maxProjectiles).Where(p => !p.active).ToArray();
            try {
                foreach (var p in borrowed) p.active = true;
                var snapshot = Main.projectile.Take(Main.maxProjectiles).Select(p => (p.type,p.identity,p.owner,p.damage)).ToArray();
                full.ModProjectile.OnHitNPC(target,default,1);
                check(Main.projectile.Take(Main.maxProjectiles).All(p => p.active) && Main.projectile.Take(Main.maxProjectiles).Select(p => (p.type,p.identity,p.owner,p.damage)).SequenceEqual(snapshot),"native full allocatable pool preserves every existing shot identity and payload");
                var free = borrowed[0]; free.active = false; full.ModProjectile.OnHitNPC(target,default,1);
                check(free.active && free.type == childType && free.damage == 100,"native one available slot permits only one split child without recycling occupied slots");
                check(Main.projectile.Take(Main.maxProjectiles).Where(p => !ReferenceEquals(p,free)).Select(p => (p.type,p.identity,p.owner,p.damage)).SequenceEqual(snapshot.Where((_,i) => !ReferenceEquals(Main.projectile[i],free))),"native partial-capacity split preserves all unrelated shot identities");
            }
            finally { foreach (var p in borrowed) p.active = false; }
        } finally { Clear(); if (target != null) { target.damage = 0; target.active = false; } Netplay.Connection = connection; Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; Main.combatText = text; Main.rand = random; clients.CopyTo(Netplay.Clients,0); }
    }
}
