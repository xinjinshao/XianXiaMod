using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditThunderHitProc(Action<bool,string> check)
    {
        Player previous = Main.player[0]; var clients = (RemoteClient[])Netplay.Clients.Clone(); var connection = Netplay.Connection;
        int mode = Main.netMode, local = Main.myPlayer; var random = Main.rand; var text = Main.combatText;
        var before = Main.projectile.Where(projectile => projectile.active).ToHashSet(); NPC target = null;
        var output = new AuditOutputSocket(true);
        void ClearShots() { foreach (var projectile in Main.projectile.Where(projectile => projectile.active && !before.Contains(projectile))) { projectile.damage = 0; projectile.active = false; } }
        try {
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(false), State = 0 };
            Netplay.Connection = new RemoteServer(); Netplay.Connection.Socket = output;
            Main.combatText = Enumerable.Range(0,100).Select(_ => new CombatText { active = true }).ToArray();
            var actor = new Player { active = true, whoAmI = 0, statLife = 100, statLifeMax2 = 100 }; Main.player[0] = actor;
            actor.Center = new Vector2(Main.maxTilesX * 8 + 800,400);
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
            int npc = NPC.NewNPC(new EntitySource_Misc("XianXiaThunderHitAudit"),(int)actor.Center.X,400,NPCID.BlueSlime);
            check(npc >= 0 && npc < Main.maxNPCs,"native thunder hit target created"); target = Main.npc[npc]; target.Center = actor.Center;
            target.damage = target.defense = 0; target.life = target.lifeMax = 10000; target.noGravity = true;
            int parentType = ModContent.ProjectileType<ThunderSwordProjectile>(), boltType = ModContent.ProjectileType<MinorThunderboltProjectile>();
            Projectile Parent(int netMode,int observer=0,int damage=201) {
                ClearShots(); Main.netMode = netMode; Main.myPlayer = observer;
                int slot = Projectile.NewProjectile(actor.GetSource_FromThis(),target.Center,Vector2.UnitX * 13,parentType,damage,3,0);
                if (slot < 0 || slot >= Main.maxProjectiles) throw new InvalidOperationException("Thunder parent fixture failed");
                output.Sent.Clear(); return Main.projectile[slot];
            }
            foreach (int netMode in new[] { NetmodeID.SinglePlayer,NetmodeID.MultiplayerClient }) {
                bool triggered = false;
                for (int seed = 0; seed < 64 && !triggered; seed++) {
                    Projectile parent = Parent(netMode); target.active = true; target.life = 10000; Array.Clear(target.immune); Main.rand = new Terraria.Utilities.UnifiedRandom(seed);
                    parent.Damage();
                    check(target.life < 10000,"native owner-side projectile Damage actually strikes NPC before hit proc");
                    var bolts = Main.projectile.Where(projectile => projectile.active && !before.Contains(projectile) && projectile.type == boltType).ToArray();
                    if (bolts.Length == 0) continue;
                    check(bolts.Length == 1,"native actual NPC collision creates exactly one thunder proc on owner authority"); Projectile bolt = bolts[0]; triggered = true;
                    check(bolt.owner == 0 && bolt.damage == 100 && bolt.DamageType == DamageClass.Melee && bolt.knockBack == .5f,"native thunder proc retains half damage player owner melee class and knockback");
                    check(bolt.Center.X >= target.Center.X - 32 && bolt.Center.X < target.Center.X + 32 && bolt.Center.Y == target.Center.Y - 240
                        && bolt.velocity == Vector2.UnitY * 12 && bolt.tileCollide,"native thunder proc actual fall geometry and tile collision");
                    using (var extra = new MemoryStream()) { bolt.ModProjectile.SendExtraAI(new BinaryWriter(extra)); check(extra.ToArray().SequenceEqual(new byte[] { 1,0 }),"native hit-parent source sets melee and active extra AI on spawned lightning"); }
                    if (netMode == NetmodeID.MultiplayerClient) {
                        if (!output.Sent.Any(packet => packet.Length > 2 && packet[2] == MessageID.SyncProjectile)) bolt.Update(bolt.whoAmI);
                        byte[] packet = output.Sent.LastOrDefault(bytes => bytes.Length > 2 && bytes[2] == MessageID.SyncProjectile);
                        check(packet != null,"native owning client produces vanilla projectile synchronization for thunder proc");
                        byte[] payload = packet.Skip(2).ToArray(); Main.myPlayer = 1; bolt.DamageType = DamageClass.Magic;
                        for (int replay = 0; replay < 2; replay++) {
                            var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = new BinaryReader(new MemoryStream(payload)) };
                            try { buffer.GetData(0,payload.Length,out int kind); check(kind == MessageID.SyncProjectile,"official observer decoder accepts native thunder synchronization"); } finally { buffer.reader.Dispose(); }
                        }
                        check(Main.projectile.Count(projectile => projectile.active && projectile.type == boltType && projectile.owner == 0 && projectile.identity == bolt.identity) == 1
                            && bolt.DamageType == DamageClass.Melee,"native observer replays update one projectile identity and retain melee classification");
                    }
                }
                check(triggered,"native actual NPC-hit sequence reaches original one-in-three lightning effect");
            }
            int yes = Enumerable.Range(0,1000).First(seed => new Terraria.Utilities.UnifiedRandom(seed).NextBool(3));
            int no = Enumerable.Range(0,1000).First(seed => !new Terraria.Utilities.UnifiedRandom(seed).NextBool(3));
            foreach (int netMode in new[] { NetmodeID.Server,NetmodeID.MultiplayerClient }) {
                Projectile parent = Parent(netMode,netMode == NetmodeID.Server ? Main.maxPlayers : 1); Main.rand = new Terraria.Utilities.UnifiedRandom(yes);
                parent.ModProjectile.OnHitNPC(target,default,1);
                check(!Main.projectile.Any(projectile => projectile.active && projectile.type == boltType && !before.Contains(projectile))
                    && Main.rand.Next() == new Terraria.Utilities.UnifiedRandom(yes).Next(),"native dedicated server and observer do not roll or duplicate owner hit effect");
            }
            Projectile failed = Parent(NetmodeID.MultiplayerClient); Main.rand = new Terraria.Utilities.UnifiedRandom(no); failed.ModProjectile.OnHitNPC(target,default,1);
            check(!Main.projectile.Any(projectile => projectile.active && projectile.type == boltType && !before.Contains(projectile)),"native failed probability roll produces no extra projectile");
            foreach (int damage in new[] { 1,2 }) {
                Projectile parent = Parent(NetmodeID.MultiplayerClient,0,damage); Main.rand = new Terraria.Utilities.UnifiedRandom(yes); parent.ModProjectile.OnHitNPC(target,default,1);
                check(Main.projectile.Single(projectile => projectile.active && projectile.type == boltType && !before.Contains(projectile)).damage == 1,"native thunder half damage has a one-damage minimum");
            }
            for (int invalid = 0; invalid < 7; invalid++) {
                Projectile parent = Parent(NetmodeID.MultiplayerClient); actor.active = true; actor.dead = false;
                if (invalid == 0) parent.owner = -1; if (invalid == 1) parent.owner = Main.maxPlayers;
                if (invalid == 2) actor.active = false; if (invalid == 3) actor.dead = true;
                if (invalid == 4) parent.Center = new Vector2(float.NaN,400); if (invalid == 5) parent.velocity = new Vector2(1e30f,0);
                Vector2 center = target.Center; if (invalid == 6) target.Center = new Vector2(float.PositiveInfinity,400);
                Main.rand = new Terraria.Utilities.UnifiedRandom(yes);
                try { parent.ModProjectile.OnHitNPC(target,default,1); } finally { target.Center = center; actor.active = true; actor.dead = false; }
                check(!Main.projectile.Any(projectile => projectile.active && projectile.type == boltType && !before.Contains(projectile)),"native invalid owner lifecycle or geometry suppresses thunder proc");
            }
            Projectile fullParent = Parent(NetmodeID.MultiplayerClient);
            var borrowed = Main.projectile.Take(Main.maxProjectiles).Where(projectile => !projectile.active).ToArray();
            foreach (var projectile in borrowed) projectile.active = true;
            Main.rand = new Terraria.Utilities.UnifiedRandom(yes);
            try {
                int packets = output.Sent.Count; fullParent.ModProjectile.OnHitNPC(target,default,1);
                check(Main.projectile.Take(Main.maxProjectiles).All(projectile => projectile.active) && output.Sent.Count == packets
                    && Main.rand.Next() == new Terraria.Utilities.UnifiedRandom(yes).Next(),
                    "native full allocatable projectile pool suppresses hit proc without using dummy slot or consuming RNG");
            } finally { foreach (var projectile in borrowed) projectile.active = false; }
            Projectile killing = Parent(NetmodeID.MultiplayerClient); target.active = false; target.life = 0; Main.rand = new Terraria.Utilities.UnifiedRandom(yes); killing.ModProjectile.OnHitNPC(target,default,1);
            check(Main.projectile.Count(projectile => projectile.active && projectile.type == boltType && !before.Contains(projectile)) == 1,"native killing-hit callback preserves thunder effect after target deactivation");
        } finally {
            ClearShots(); if (target != null) { target.damage = 0; target.active = false; }
            Main.player[0] = previous; clients.CopyTo(Netplay.Clients,0); Netplay.Connection = connection;
            Main.netMode = mode; Main.myPlayer = local; Main.rand = random; Main.combatText = text;
        }
    }
}
