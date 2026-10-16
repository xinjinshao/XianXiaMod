using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Buffs;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditStationBuffs(Action<bool, string> check)
    {
        var players = Main.player.Take(3).ToArray(); var clients = (RemoteClient[])Netplay.Clients.Clone();
        int oldMode = Main.netMode, oldLocal = Main.myPlayer;
        int x = Main.maxTilesX / 2, y = 12;
        var oldTiles = new (bool Has, ushort Type)[4];
        for (int i = 0; i < 4; i++) { Tile tile = Main.tile[x + i, y]; oldTiles[i] = (tile.HasTile, tile.TileType); }
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
            int alchemy = ModContent.BuffType<AlchemyInsightBuff>(), forge = ModContent.BuffType<ArtifactResonanceBuff>();
            for (int i = 0; i < 4; i++) { Tile tile = Main.tile[x + i, y]; tile.HasTile = true;
                tile.TileType = (ushort)(i < 2 ? ModContent.TileType<AlchemyCauldronTile>() : ModContent.TileType<ArtifactForgeTile>()); }
            for (int i = 0; i < 3; i++) {
                var socket = new AuditOutputSocket(true); Netplay.Clients[i] = new RemoteClient { Id = i, Socket = socket, State = 10, IsActive = true };
                var actor = new Player { whoAmI = i, active = true }; Main.player[i] = actor;
                actor.Center = new Vector2((x + (i == 2 ? 100 : i)) * 16, y * 16);
                var effects = actor.GetModPlayer<BiomeObjectEffectsPlayer>(); effects.PostUpdateEquips();
                check(actor.HasBuff(alchemy) == (i < 2) && actor.HasBuff(forge) == (i < 2), "native independent station buffs affect both nearby players and exclude a distant player");
                check(socket.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == (i < 2 ? 2 : 0), "native authority sends each station buff to its actual owner");
                effects.PostUpdateEquips();
                check(socket.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == (i < 2 ? 2 : 0), "native healthy station leases do not broadcast every frame");
                if (i < 2) {
                    int slot = actor.FindBuffIndex(alchemy); actor.buffTime[slot] = 15; effects.PostUpdateEquips();
                    check(actor.buffTime[slot] == 30 && socket.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == 3,
                        "native station lease renews at fifteen frames without renewing healthy second buff");
                    actor.ClearBuff(alchemy); actor.ClearBuff(forge); actor.dead = true; effects.PostUpdateEquips();
                    check(!actor.HasBuff(alchemy) && !actor.HasBuff(forge), "native dead player does not receive station buffs");
                    actor.dead = false; Main.netMode = NetmodeID.MultiplayerClient; effects.PostUpdateEquips();
                    check(!actor.HasBuff(alchemy) && !actor.HasBuff(forge), "native client scan does not grant station buffs independently");
                    Main.netMode = NetmodeID.Server;
                }
            }
        } finally {
            for (int i = 0; i < 4; i++) { Tile tile = Main.tile[x + i, y]; tile.HasTile = oldTiles[i].Has; tile.TileType = oldTiles[i].Type; }
            for (int i = 0; i < 3; i++) Main.player[i] = players[i];
            clients.CopyTo(Netplay.Clients, 0); Main.netMode = oldMode; Main.myPlayer = oldLocal;
        }
        check(Main.player.Take(3).SequenceEqual(players) && Netplay.Clients.SequenceEqual(clients) && Main.netMode == oldMode && Main.myPlayer == oldLocal,
            "native station audit restores player network and engine contexts");
    }
}
