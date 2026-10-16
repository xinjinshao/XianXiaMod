using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditPacketDispatch(Action<bool, string> check)
    {
        var oldPlayers = Main.player; var oldNpcs = Main.npc; var oldClients = (RemoteClient[])Netplay.Clients.Clone();
        int oldMode = Main.netMode, oldLocal = Main.myPlayer; bool oldBroadcast = NetMessage.buffer[0].broadcast;
        Player unready = oldPlayers.FirstOrDefault(player => player != null && !player.TryGetModPlayer<XianXiaPlayer>(out _));
        check(unready != null, "native world contains a vanilla player slot without installed mod-player arrays");
        var mod = ModContent.GetInstance<global::XianXia.XianXia>();
        void Deliver(byte opcode, int sender, Action<BinaryWriter> payload = null) {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) {
                writer.Write(opcode); if (payload == null) writer.Write(new byte[64]); else payload(writer);
            }
            stream.Position = 0; using var reader = new BinaryReader(stream); mod.HandlePacket(reader, sender);
        }
        var initial = new CultivationSnapshot(40, 1, true, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        try {
            var output = new AuditOutputSocket(true);
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            NetMessage.buffer[0].broadcast = true;
            foreach (int shape in Enumerable.Range(0, 5)) {
                Main.netMode = NetmodeID.Server;
                Main.player = shape == 0 ? null : shape == 1 ? Array.Empty<Player>() : new Player[] { shape == 2 ? null : unready };
                int sender = shape == 4 ? 1 : 0, before = output.Sent.Count;
                foreach (byte opcode in new byte[] { 0, 1, 3, 5, 6, 8, 9, 10, 11, 12, 13 }) Deliver(opcode, sender);
                check(output.Sent.Count == before, "native unavailable packet sender slots skip all request dispatch without reply");
            }
            Main.netMode = NetmodeID.Server;
            var joining = new Player { whoAmI = 0, active = false }; Main.player = new Player[] { joining };
            Deliver(3, 0, writer => { writer.Write((byte)0); initial.Write(writer); });
            check(joining.GetModPlayer<XianXiaPlayer>().NetworkInitialized && joining.GetModPlayer<XianXiaPlayer>().spiritualEnergy == 40,
                "native ready mod-player accepts initial character import even before active flag");
            Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0;
            var recipient = new Player { whoAmI = 0, active = true }; Main.player = new Player[] { recipient };
            recipient.GetModPlayer<XianXiaPlayer>().ApplySnapshot(initial);
            foreach (int sender in new[] { -1, 0, 1, 255 }) {
                Deliver(4, sender, writer => { writer.Write((byte)0); (initial with { Energy = 30 }).Write(writer); });
                check(recipient.GetModPlayer<XianXiaPlayer>().spiritualEnergy == 40, "native client rejects snapshot without server origin");
            }
            Deliver(4, 256, writer => { writer.Write((byte)0); (initial with { Energy = 20 }).Write(writer); });
            check(recipient.GetModPlayer<XianXiaPlayer>().spiritualEnergy == 20, "native client still applies canonical server snapshot");
            foreach (int shape in Enumerable.Range(0, 4)) {
                Main.player = shape == 0 ? null : shape == 1 ? Array.Empty<Player>() : new Player[] { shape == 2 ? null : unready };
                Deliver(4, 256, writer => { writer.Write((byte)0); initial.Write(writer); });
                Deliver(13, 256, writer => { writer.Write((byte)0); writer.Write((byte)0); writer.Write(1UL); writer.Write(new byte[64]); });
                check(true, "native unready recipient skips state and inventory packets without indexing missing mod-player arrays");
            }
            Main.netMode = NetmodeID.Server; Main.player = new Player[] { recipient };
            foreach (var table in new NPC[][] { null, Array.Empty<NPC>(), new NPC[] { null } }) {
                Main.npc = table; Deliver(1, 0, writer => writer.Write((short)0));
                check(true, "native commission dispatch skips missing NPC table or slot");
            }
        } finally {
            Main.player = oldPlayers; Main.npc = oldNpcs; oldClients.CopyTo(Netplay.Clients, 0);
            Main.netMode = oldMode; Main.myPlayer = oldLocal; NetMessage.buffer[0].broadcast = oldBroadcast;
        }
        check(ReferenceEquals(Main.player, oldPlayers) && ReferenceEquals(Main.npc, oldNpcs) && Netplay.Clients.SequenceEqual(oldClients)
            && Main.netMode == oldMode && Main.myPlayer == oldLocal && NetMessage.buffer[0].broadcast == oldBroadcast,
            "native packet audit restores entity tables network clients and engine context");
    }
}
