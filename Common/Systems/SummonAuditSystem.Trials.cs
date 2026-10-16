using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Buffs;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditFailureBuffs(Action<bool, string> check)
    {
        Player oldPlayer = Main.player[0]; var oldClients = (RemoteClient[])Netplay.Clients.Clone();
        int oldMode = Main.netMode, oldLocal = Main.myPlayer; var oldConnection = Netplay.Connection;
        int disorder = ModContent.BuffType<SpiritualPressureDisorderBuff>();
        try {
            foreach (int stage in Enumerable.Range(3, 6)) foreach (int variant in new[] { 0, 1, 2 }) {
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
                var output = new AuditOutputSocket(true);
                for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
                var actor = new Player { whoAmI = 0, active = true, dead = false }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>();
                var initial = new CultivationSnapshot(0, (byte)stage, true, 50, 120, (byte)(stage - 1), (byte)stage, CultivationSnapshot.KindFor(stage), 0, 0, 0, 0);
                check(state.TryInitializeNetwork(initial), "native failure fixture imports valid active trial for every late realm");
                if (variant == 1) actor.AddBuff(disorder, 900);
                if (variant == 2) actor.buffImmune[disorder] = true;
                state.HandleTribulationFailure();
                var result = state.CaptureSnapshot();
                check(result.Timer == 0 && result.TrialStage == 0 && result.Kind == 0 && result.Attempts == 1 && result.Weakness == 10800 && result.Pressure == 40
                    && result.Energy == CultivationSnapshot.MaxEnergy(stage) / 4, "native failed trial retains expected attempts weakness pressure and quarter-energy recovery");
                check(actor.HasBuff(BuffID.Weak) && actor.buffTime[actor.FindBuffIndex(BuffID.Weak)] == 10800,
                    "native failed trial grants three-minute weakness");
                check(actor.HasBuff(disorder) == (variant != 2) && (variant == 2 || actor.buffTime[actor.FindBuffIndex(disorder)] == (variant == 1 ? 900 : 120)),
                    "native failed trial preserves longer disorder and honors immunity");
                byte[][] packets = output.Sent.Where(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff).ToArray();
                check(packets.Length == (variant == 0 ? 2 : 1), "native trial failure sends each changed penalty once without re-sending unchanged or immune disorder");
                int packetCount = output.Sent.Count; state.HandleTribulationFailure();
                check(state.CaptureSnapshot() == result && output.Sent.Count == packetCount, "native repeated failure callback cannot increment attempts restore energy or resend penalties");
                try {
                    Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                    var receiver = new Player { whoAmI = 0, active = true }; Main.player[0] = receiver;
                    if (variant == 1) receiver.AddBuff(disorder, 900);
                    if (variant == 2) receiver.buffImmune[disorder] = true;
                    foreach (byte[] packet in packets) {
                        byte[] payload = packet.Skip(2).ToArray();
                        var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = new BinaryReader(new MemoryStream(payload)) };
                        try { buffer.GetData(0, payload.Length, out int kind); check(kind == MessageID.AddPlayerBuff, "official owner decoder accepts failure penalty packet"); }
                        finally { buffer.reader.Dispose(); }
                    }
                    check(receiver.HasBuff(BuffID.Weak) && receiver.buffTime[receiver.FindBuffIndex(BuffID.Weak)] == 10800
                        && receiver.HasBuff(disorder) == (variant != 2)
                        && (variant == 2 || receiver.buffTime[receiver.FindBuffIndex(disorder)] == (variant == 1 ? 900 : 120)),
                        "official owner receives exact failure penalties and retains pre-existing longer buff");
                    if (variant != 2) {
                        receiver.statDefense = Player.DefenseStat.Default; receiver.statDefense += 20; int defenseBefore = receiver.statDefense; receiver.moveSpeed = 1; int index = receiver.FindBuffIndex(disorder);
                        ModContent.GetInstance<SpiritualPressureDisorderBuff>().Update(receiver, ref index);
                        check(receiver.statDefense == defenseBefore - 6 && Math.Abs(receiver.moveSpeed - 0.92f) < 0.0001f,
                            $"native received disorder applies real defense and movement penalties; before={defenseBefore}, after={receiver.statDefense}, speed={receiver.moveSpeed}");
                    }
                    var clientState = receiver.GetModPlayer<XianXiaPlayer>(); clientState.ApplySnapshot(initial);
                    clientState.HandleTribulationFailure();
                    check(clientState.CaptureSnapshot() == initial && output.Sent.Count == packetCount,
                        "native client cannot settle a trial failure or send authority penalties");
                } finally { Main.player[0] = actor; Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Netplay.Connection = oldConnection; }
            }
        } finally {
            Main.player[0] = oldPlayer; oldClients.CopyTo(Netplay.Clients, 0);
            Main.netMode = oldMode; Main.myPlayer = oldLocal; Netplay.Connection = oldConnection;
        }
        check(ReferenceEquals(Main.player[0], oldPlayer) && Netplay.Clients.SequenceEqual(oldClients)
            && Main.netMode == oldMode && Main.myPlayer == oldLocal && ReferenceEquals(Netplay.Connection, oldConnection),
            "native trial failure audit restores player client and engine contexts");
    }
}
