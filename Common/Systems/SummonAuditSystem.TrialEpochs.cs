using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditTrialEpochs(Action<bool,string> check)
    {
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        var clients = (RemoteClient[])Netplay.Clients.Clone(); var broadcast = NetMessage.buffer.Take(3).Select(b => b.broadcast).ToArray();
        long[] history = (long[])TribulationEpochSystem.ClientLastSeen.Clone(); var shots = new List<Projectile>();
        var mod = ModContent.GetInstance<global::XianXia.XianXia>();
        byte[] LastEpoch(AuditOutputSocket output) => output.Sent.Last(packet => packet.Length >= 10 && packet[^10] == 14 && packet[^9] == 0)[^10..];
        void Deliver(byte[] payload,int sender = 256) { using var stream = new MemoryStream(payload); mod.HandlePacket(new BinaryReader(stream),sender); }
        byte[] ShotWire(Projectile shot) { using var stream = new MemoryStream(); shot.ModProjectile.SendExtraAI(new BinaryWriter(stream)); return stream.ToArray(); }
        void ReadShot(Projectile shot,byte[] payload) { using var stream = new MemoryStream(payload); shot.ModProjectile.ReceiveExtraAI(new BinaryReader(stream)); }
        Projectile Spawn(IEntitySource source) {
            int index = Projectile.NewProjectile(source,new Vector2(Main.maxTilesX * 8,400),Vector2.Zero,ModContent.ProjectileType<BossArrayFieldProjectile>(),10,0,Main.maxPlayers);
            if (index < 0 || index >= Main.maxProjectiles) throw new InvalidOperationException("No native trial epoch projectile slot");
            Projectile shot = Main.projectile[index]; shot.timeLeft = 50; shots.Add(shot); return shot;
        }
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
            var outputs = Enumerable.Range(0,3).Select(_ => new AuditOutputSocket(true)).ToArray();
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i < 3 ? outputs[i] : new AuditOutputSocket(), State = i < 3 ? 10 : 0, IsActive = i < 3 };
            for (int i = 0; i < 3; i++) NetMessage.buffer[i].broadcast = true;
            var authority = new Player { whoAmI = 0, active = true, statLife = 100 }; authority.Center = new Vector2(Main.maxTilesX * 8,400); Main.player[0] = authority;
            var state = authority.GetModPlayer<XianXiaPlayer>();
            var active = new CultivationSnapshot(20,3,true,5,120,2,3,1,0,0,0,0);
            check(state.TryInitializeNetwork(active),"native trial epoch fixture imports actual active trial state");
            long firstSession = state.TribulationSession; var system = new TribulationEpochSystem(); system.PostUpdatePlayers();
            byte[] first = LastEpoch(outputs[0]);
            check(first.Length == 10 && BitConverter.ToInt64(first,2) == firstSession && firstSession > 0,
                "native trial authority emits ten-byte opcode14 matching hostile source session to captured socket");
            check(LastEpoch(outputs[1]).SequenceEqual(first) && LastEpoch(outputs[2]).SequenceEqual(first),"native trial epoch broadcasts identical source identity to owner observers and joining peer");
            int[] counts = outputs.Select(o => o.Sent.Count).ToArray(); system.PostUpdatePlayers(); system.PostUpdatePlayers();
            check(outputs.Select(o => o.Sent.Count).SequenceEqual(counts),"native unchanged trial epoch is not broadcast every update");
            authority.GetModPlayer<TribulationEpochSyncPlayer>().SyncPlayer(2,-1,false);
            check(outputs[2].Sent.Count == counts[2] + 1 && outputs[0].Sent.Count == counts[0] && LastEpoch(outputs[2]).SequenceEqual(first),
                "native registered late-join sync resends current trial identity only to requested peer");
            Projectile firstShot = Spawn(authority.GetSource_FromThis()); byte[] firstShotWire = ShotWire(firstShot);
            check(firstShot.ModProjectile.CanDamage() == true && BitConverter.ToInt64(firstShotWire,11) == firstSession,"native trial hazard captures the same authority identity as synchronization");
            state.tribulationTimer = 0; system.PostUpdatePlayers(); byte[] retired = LastEpoch(outputs[0]);
            check(BitConverter.ToInt64(retired,2) < -firstSession && firstShot.ModProjectile.CanDamage() == false,"native trial retirement advances identity and immediately stops old authority hazards");
            check(state.TryRetryTribulation(),"native actual trial retry starts another session");
            long secondSession = state.TribulationSession; system.PostUpdatePlayers(); byte[] retry = LastEpoch(outputs[0]);
            check(secondSession > Math.Abs(BitConverter.ToInt64(retired,2)) && BitConverter.ToInt64(retry,2) == secondSession,"native actual retry emits a new active identity beyond retired session");
            Projectile nextShot = Spawn(authority.GetSource_FromThis()), receiver = Spawn(new EntitySource_Misc("TrialEpochReceiver")); byte[] nextWire = ShotWire(nextShot);
            foreach (int localClient in new[] { 0,1 }) {
                Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = localClient; Array.Clear(TribulationEpochSystem.ClientLastSeen);
                var client = new Player { whoAmI = 0, active = true, statLife = 100 }; client.Center = authority.Center; Main.player[0] = client;
                var replica = client.GetModPlayer<XianXiaPlayer>(); replica.ApplySnapshot(active);
                ReadShot(receiver,firstShotWire);
                check(receiver.ModProjectile.CanDamage() == false,"native owner/observer trial hazard waits for source identity even with active snapshot");
                Deliver(first,0); check(replica.TribulationSession == 0,"native trial metadata rejects non-server sender");
                Deliver(first); check(replica.TribulationSession == firstSession && receiver.ModProjectile.CanDamage() == true && replica.CaptureSnapshot() == active,
                    "native actual mod dispatcher installs matching owner/observer identity without changing resources");
                for (int length = 0; length < 10; length++) { Deliver(retired[..length]); check(replica.TribulationSession == firstSession && replica.CaptureSnapshot() == active,"native all truncated trial metadata prefixes preserve identity and resources"); }
                Deliver(retired); check(replica.TribulationSession == 0 && receiver.ModProjectile.CanDamage() == false && replica.CaptureSnapshot() == active,
                    "native retirement cancels owner/observer old hazard while older active resource snapshot remains");
                Deliver(first); check(receiver.ModProjectile.CanDamage() == false,"native old active trial packet cannot replay after retirement");
                Deliver(retry); check(replica.TribulationSession == secondSession && receiver.ModProjectile.CanDamage() == false,"native new trial identity cannot take over old trial projectile");
                ReadShot(receiver,nextWire); check(receiver.ModProjectile.CanDamage() == true,"native matching current trial projectile remains damaging");
                replica.OnEnterWorld(); Deliver(retry);
                check(replica.TribulationSession == 0 && receiver.ModProjectile.CanDamage() == false,"native world entry invalidates trial replica and rejects previous identity replay");
                Array.Clear(TribulationEpochSystem.ClientLastSeen); Deliver(retry);
                replica.UpdateDead(); client.dead = false;
                Deliver(retry); check(replica.TribulationSession == 0 && receiver.ModProjectile.CanDamage() == false,"native death clears local trial source and refuses prior active identity after revival");
                Main.player[0] = new Player { whoAmI = 0, active = true }; Main.player[0].GetModPlayer<XianXiaPlayer>().ApplySnapshot(active);
                Deliver(retry); check(Main.player[0].GetModPlayer<XianXiaPlayer>().TribulationSession == 0,"native replacement player in same slot refuses previous occupant identity replay");
            }
            Main.netMode = NetmodeID.Server; Main.player[0] = authority;
            using var forged = new MemoryStream(); using (var writer = new BinaryWriter(forged,System.Text.Encoding.UTF8,true)) { writer.Write((byte)14); writer.Write((byte)0); writer.Write(long.MaxValue); }
            Deliver(forged.ToArray(),0); check(state.TribulationSession == secondSession,"native server ignores client-forged trial epoch");
        } finally {
            foreach (Projectile shot in shots) { shot.damage = 0; shot.active = false; }
            Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; clients.CopyTo(Netplay.Clients,0);
            for (int i = 0; i < 3; i++) NetMessage.buffer[i].broadcast = broadcast[i]; history.CopyTo(TribulationEpochSystem.ClientLastSeen,0);
        }
        check(ReferenceEquals(Main.player[0],previous) && shots.All(p => !p.active) && history.SequenceEqual(TribulationEpochSystem.ClientLastSeen),"native trial epoch audit restores player projectile and replay context");
    }
}
