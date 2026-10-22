using System;
using System.Collections.Generic;
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
    private static void AuditHostileIdentities(Action<bool,string> check)
    {
        int slot = Enumerable.Range(0,Main.maxNPCs).First(i => !Main.npc[i].active);
        NPC previousNpc = Main.npc[slot]; Player previousPlayer = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        long[] history = (long[])NpcEncounterIdentitySystem.ClientLastSeen.Clone();
        var shots = new List<Projectile>();
        NPC Parent() { var npc = new NPC { whoAmI = slot }; Main.npc[slot] = npc; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = slot; npc.active = true; npc.target = 0; npc.Center = new Vector2(Main.maxTilesX * 8,400); return npc; }
        byte[] NpcWire(NPC npc) { using var stream = new MemoryStream(); npc.GetGlobalNPC<NpcEncounterIdentity>().SendExtraAI(npc,null,new BinaryWriter(stream)); return stream.ToArray(); }
        void ReadNpc(NPC npc,byte[] bytes) { using var stream = new MemoryStream(bytes); npc.GetGlobalNPC<NpcEncounterIdentity>().ReceiveExtraAI(npc,null,new BinaryReader(stream)); }
        byte[] ShotWire(Projectile shot) { using var stream = new MemoryStream(); shot.ModProjectile.SendExtraAI(new BinaryWriter(stream)); return stream.ToArray(); }
        void ReadShot(Projectile shot,byte[] bytes) { using var stream = new MemoryStream(bytes); shot.ModProjectile.ReceiveExtraAI(new BinaryReader(stream)); }
        Projectile Create(int type,IEntitySource source) {
            int index = Projectile.NewProjectile(source,new Vector2(Main.maxTilesX * 8,400),Vector2.Zero,type,10,0,Main.maxPlayers);
            if (index < 0 || index >= Main.maxProjectiles) throw new InvalidOperationException("No native hostile identity projectile capacity");
            Projectile shot = Main.projectile[index]; shots.Add(shot);
            if (shot.ModProjectile is BossArrayFieldProjectile) shot.timeLeft = 50;
            return shot;
        }
        try {
            Main.myPlayer = 0; Main.player[0] = new Player { whoAmI = 0, active = true, statLife = 100 };
            Main.player[0].Center = new Vector2(Main.maxTilesX * 8 + 50,400);
            int[] types = { ModContent.ProjectileType<BossArrayFieldProjectile>(), ModContent.ProjectileType<BossSpiritBoltProjectile>(),
                ModContent.ProjectileType<EnemySpiritBoltProjectile>(),ModContent.ProjectileType<TribulationLightningProjectile>(),
                ModContent.ProjectileType<TabletJudgmentBeamProjectile>(),ModContent.ProjectileType<InspectorDecreeBeamProjectile>(),
                ModContent.ProjectileType<InspectorVerdictBladeProjectile>(),ModContent.ProjectileType<CoreSeveranceBladeProjectile>(),ModContent.ProjectileType<FurnaceSlamWaveProjectile>() };
            foreach (int type in types) {
                Main.netMode = NetmodeID.Server; NPC authority = Parent();
                Projectile oldShot = Create(type,authority.GetSource_FromAI()); byte[] firstShot = ShotWire(oldShot), firstNpc = NpcWire(authority);
                check(oldShot.ModProjectile.CanDamage() == true && firstShot.Length == 27 && BitConverter.ToInt64(firstShot,19) == BitConverter.ToInt64(firstNpc),
                    "native registered hostile shot carries positive source identity in complete 27-byte payload: " + oldShot.ModProjectile.Name);
                authority.SetDefaults(NPCID.BlueSlime); authority.whoAmI = slot; authority.active = true; authority.target = 0; authority.Center = new Vector2(Main.maxTilesX * 8,400);
                byte[] nextNpc = NpcWire(authority);
                check(!oldShot.ModProjectile.CanDamage().GetValueOrDefault(),"native authority old hostile source fails on same-object same-type identity rotation: " + oldShot.ModProjectile.Name);
                Projectile nextShot = Create(type,authority.GetSource_FromAI()); byte[] nextPacket = ShotWire(nextShot);
                Projectile receiver = Create(type,new EntitySource_Misc("XianXiaHostileIdentityReceiver"));
                Projectile nextReceiver = Create(type,new EntitySource_Misc("XianXiaHostileIdentityReceiver"));
                Main.netMode = NetmodeID.MultiplayerClient; NPC client = Parent();
                check(receiver.ModProjectile.CanDamage() == false,"native client hostile waits for complete source metadata: " + receiver.ModProjectile.Name);
                for (int length = 0; length < 27; length++) {
                    byte[] before = ShotWire(receiver);
                    try { ReadShot(receiver,firstShot[..length]); throw new InvalidOperationException("native truncated hostile source accepted"); }
                    catch (EndOfStreamException) { }
                    check(ShotWire(receiver).SequenceEqual(before) && receiver.ModProjectile.CanDamage() == false,
                        "native all hostile source prefixes preserve waiting state and complete packet atomically: " + receiver.ModProjectile.Name);
                }
                ReadShot(receiver,firstShot);
                check(receiver.ModProjectile.CanDamage() == false,"native registered client projectile waits for its parent identity packet: " + receiver.ModProjectile.Name);
                ReadNpc(client,firstNpc);
                check(receiver.ModProjectile.CanDamage() == true,"native registered client accepts matching parent identity after projectile-first ordering: " + receiver.ModProjectile.Name);
                ReadShot(nextReceiver,nextPacket);
                check(nextReceiver.ModProjectile.CanDamage() == false,"native future hostile packet does not attach to the preceding NPC instance: " + receiver.ModProjectile.Name);
                ReadNpc(client,nextNpc);
                check(receiver.ModProjectile.CanDamage() == false && !receiver.ModProjectile.CanHitPlayer(Main.player[0]),
                    "native client old shot loses damage before cancellation AI when live same-type source identity changes: " + receiver.ModProjectile.Name);
                check(nextReceiver.ModProjectile.CanDamage() == true,"native current-source shot remains valid after identity synchronization: " + receiver.ModProjectile.Name);
                ReadNpc(client,firstNpc); ReadShot(receiver,firstShot);
                check(receiver.ModProjectile.CanDamage() == false,"native replayed NPC and projectile packets cannot restore old hostile damage: " + receiver.ModProjectile.Name);
                byte[] missing = (byte[])nextPacket.Clone(); Array.Clear(missing,19,8); ReadShot(nextReceiver,missing);
                check(nextReceiver.ModProjectile.CanDamage() == false,"native zero NPC identity cannot grant damage to a bound hostile shot: " + receiver.ModProjectile.Name);
                foreach (Projectile shot in new[] { oldShot,nextShot,receiver,nextReceiver }) { shot.damage = 0; shot.active = false; }
            }
            Main.netMode = NetmodeID.Server; NPC warningParent = Parent(); Projectile warning = Create(ModContent.ProjectileType<TribulationWarningLineProjectile>(),warningParent.GetSource_FromAI());
            warningParent.SetDefaults(NPCID.BlueSlime); warningParent.whoAmI = slot; warningParent.active = true; warningParent.target = 0;
            int activeBefore = Main.projectile.Take(Main.maxProjectiles).Count(p => p.active);
            warning.ModProjectile.OnKill(0);
            check(Main.projectile.Take(Main.maxProjectiles).Count(p => p.active) == activeBefore,"native warning from retired NPC identity cannot release a new lightning strike");
        } finally {
            foreach (Projectile shot in shots) { shot.damage = 0; shot.active = false; }
            Main.npc[slot].active = false; Main.npc[slot] = previousNpc; Main.player[0] = previousPlayer;
            Main.netMode = mode; Main.myPlayer = local; history.CopyTo(NpcEncounterIdentitySystem.ClientLastSeen,0);
        }
        check(ReferenceEquals(Main.npc[slot],previousNpc) && ReferenceEquals(Main.player[0],previousPlayer) && shots.All(p => !p.active)
            && history.SequenceEqual(NpcEncounterIdentitySystem.ClientLastSeen),"native hostile identity audit restores entity slots replay history and projectile pool");
    }
}
