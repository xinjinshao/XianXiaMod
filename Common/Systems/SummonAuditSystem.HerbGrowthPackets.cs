using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditHerbGrowthPackets(Action<bool, string> check)
    {
        int x = Main.maxTilesX / 2, y = 12, mode = Main.netMode, local = Main.myPlayer;
        var clients = (RemoteClient[])Netplay.Clients.Clone();
        var connection = Netplay.Connection;
        bool broadcast = NetMessage.buffer[0].broadcast;
        var crop = Main.tile[x, y]; var ground = Main.tile[x, y + 1];
        var savedCrop = (Tile)crop.Clone(); var savedGround = (Tile)ground.Clone();
        var herb = ModContent.GetInstance<CultivatedSpiritHerbTile>();
        try {
            var output = new AuditOutputSocket(true);
            for (int i = 0; i < Netplay.Clients.Length; i++)
                Netplay.Clients[i] = new RemoteClient { Id = i, Socket = i == 0 ? output : new AuditOutputSocket(), State = i == 0 ? 10 : 0, IsActive = i == 0 };
            Netplay.Clients[0].TileSections[x / 200, y / 150] = true;
            NetMessage.buffer[0].broadcast = true;
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
            crop.ClearEverything(); crop.HasTile = true; crop.TileType = (ushort)herb.Type;
            ground.ClearEverything(); ground.HasTile = true; ground.TileType = TileID.Dirt;
            for (int stage = 1; stage <= 2; stage++) {
                output.Sent.Clear(); int updates = 0;
                while (crop.TileFrameX < stage * 18 && updates++ < 4096) herb.RandomUpdate(x, y);
                byte[][] packets = output.Sent.Where(packet => packet.Length > 2 && packet[2] == MessageID.TileSquare).ToArray();
                check(crop.TileFrameX == stage * 18 && packets.Length == 1,
                    "native herb growth emits exactly one actual tile packet per transition: stage=" + stage + " frame=" + crop.TileFrameX + " packets=" + packets.Length);
                Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                crop.TileFrameX = (short)((stage - 1) * 18);
                foreach (int replay in new[] { 0, 1 }) {
                    foreach (byte[] packet in packets) {
                        byte[] payload = packet[2..];
                        using var reader = new BinaryReader(new MemoryStream(payload));
                        var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = reader };
                        buffer.GetData(0, payload.Length, out int kind);
                        check(kind == MessageID.TileSquare, "official herb owner decoder accepts actual growth packet");
                    }
                    check(crop.HasTile && crop.TileType == herb.Type && crop.TileFrameX == stage * 18
                        && herb.GetMapOption(x, y) == stage && herb.IsTileSpelunkable(x, y) == (stage == 2),
                        "official herb tile replay converges frame map and maturity without extra growth: stage=" + stage + "/" + replay);
                    int sent = output.Sent.Count; herb.RandomUpdate(x, y);
                    check(crop.TileFrameX == stage * 18 && output.Sent.Count == sent,
                        "native client cannot grow or rebroadcast a received herb stage");
                }
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Netplay.Connection = connection;
            }
            int before = output.Sent.Count; herb.RandomUpdate(x, y);
            check(crop.TileFrameX == 36 && output.Sent.Count == before, "native mature herb emits no unchanged tile traffic");
        } finally {
            crop.CopyFrom(savedCrop); ground.CopyFrom(savedGround);
            clients.CopyTo(Netplay.Clients, 0); Netplay.Connection = connection;
            Main.netMode = mode; Main.myPlayer = local;
            NetMessage.buffer[0].broadcast = broadcast;
        }
        check(Main.netMode == mode && Main.myPlayer == local
            && ReferenceEquals(Netplay.Connection, connection), "native herb transport audit restores engine context");
    }
}
