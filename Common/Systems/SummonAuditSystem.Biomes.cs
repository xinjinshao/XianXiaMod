using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditBiomeScans(Action<bool, string> check)
    {
        Player[] previousPlayers = Main.player.Take(3).ToArray();
        RemoteClient[] previousClients = (RemoteClient[])Netplay.Clients.Clone();
        int previousMode = Main.netMode, previousLocal = Main.myPlayer;
        bool[] previousBroadcast = NetMessage.buffer.Take(3).Select(buffer => buffer.broadcast).ToArray();
        int tx = Main.maxTilesX / 2, ty = 20;
        int[] types = { ModContent.TileType<SwordTabletTile>(), ModContent.TileType<SingingThunderStoneTile>(), ModContent.TileType<RiftMembraneTile>() };
        var tiles = new List<(int X, bool HasTile, ushort Type)>();
        for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tiles.Add((tx + i, tile.HasTile, tile.TileType)); }
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket(i < 3), IsActive = i < 3, State = i < 3 ? 10 : 0 };
            for (int i = 0; i < 3; i++) {
                NetMessage.buffer[i].broadcast = true;
                Main.player[i] = new Player { whoAmI = i, active = true, dead = false };
                Main.player[i].Center = new Vector2((tx + (i == 1 ? Main.buffScanAreaWidth * 2 : i) + 0.5f) * 16, (ty + 0.5f) * 16);
                foreach (int tileType in types) check(Main.player[i].GetModPlayer<ServerBiomePlayer>().Count(tileType) == 0, "native biome scan fixture starts without nearby test objects");
            }
            for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tile.HasTile = true; tile.TileType = (ushort)types[i / 2]; }
            var caches = new BiomeScanCacheSystem(); caches.OnWorldLoad();
            int buff = ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>();
            int pressureStep = Main.GameUpdateCount % 120 == 0 ? 1 : 0;
            for (int i = 0; i < 3; i++) {
                Player actor = Main.player[i]; var state = actor.GetModPlayer<XianXiaPlayer>();
                state.spiritPressure = 50; state.spiritualEnergyRegenBonus = 0;
                actor.GetModPlayer<BiomeObjectEffectsPlayer>().PostUpdateEquips();
                bool near = i != 1;
                foreach (int tileType in types) check(actor.GetModPlayer<ServerBiomePlayer>().Count(tileType) == (near ? 2 : 0), "native per-player tile scan counts two objects independently");
                check(state.spiritualEnergyRegenBonus == (near ? 1 : 0), "native nearby thunder stones grant one regen bonus without stacking");
                check(state.spiritPressure == 50 + (near ? pressureStep : 0), "native nearby rift objects apply bounded pressure only at cadence");
                check(actor.HasBuff(buff) == near, "native sword tablets apply registered resistance buff only nearby");
            }
            var buffOutput = (AuditOutputSocket)Netplay.Clients[0].Socket;
            byte[][] resistancePackets = buffOutput.Sent.Where(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff).ToArray();
            check(resistancePackets.Length == 1, "native server sword tablet sends one registered buff packet to the owner; captured=" + string.Join(",", buffOutput.Sent.Select(packet => packet.Length > 3 ? $"{packet[2]}:{packet[3]}" : "short")));
            Player buffActor = Main.player[0]; var buffEffects = buffActor.GetModPlayer<BiomeObjectEffectsPlayer>();
            int buffIndex = buffActor.FindBuffIndex(buff);
            check(buffIndex >= 0 && buffActor.buffTime[buffIndex] == BiomeObjectEffectsPlayer.SwordBuffDuration, "native server applies thirty-frame sword buff lease");
            buffEffects.PostUpdateEquips();
            check(buffOutput.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == 1, "native healthy sword buff does not broadcast each frame");
            buffActor.buffTime[buffIndex] = BiomeObjectEffectsPlayer.SwordBuffRefreshThreshold;
            buffEffects.PostUpdateEquips();
            check(buffActor.buffTime[buffIndex] == BiomeObjectEffectsPlayer.SwordBuffDuration
                && buffOutput.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == 2, "native sword buff refresh threshold emits one renewal");
            buffActor.buffTime[buffIndex] = 180; buffEffects.PostUpdateEquips();
            check(buffActor.buffTime[buffIndex] == 180 && buffOutput.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == 2,
                "native sword tablet neither shortens a longer resistance buff nor resends it");
            var savedConnection = Netplay.Connection;
            try {
                Main.netMode = NetmodeID.MultiplayerClient; Main.myPlayer = 0; Netplay.Connection = new RemoteServer();
                var buffReceiver = new Player { whoAmI = 0, active = true, dead = false }; Main.player[0] = buffReceiver;
                byte[] payload = resistancePackets[0].Skip(2).ToArray();
                var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = new System.IO.BinaryReader(new System.IO.MemoryStream(payload)) };
                try { buffer.GetData(0, payload.Length, out int messageType); check(messageType == MessageID.AddPlayerBuff, "official owner decoder parses server sword buff packet"); }
                finally { buffer.reader.Dispose(); }
                int receivedBuff = buffReceiver.FindBuffIndex(buff);
                check(receivedBuff >= 0 && buffReceiver.buffTime[receivedBuff] == BiomeObjectEffectsPlayer.SwordBuffDuration, "official owner decoder applies registered sword resistance with usable duration");
                var clientState = buffReceiver.GetModPlayer<XianXiaPlayer>(); clientState.ResetEffects(); clientState.spiritPressure = 50;
                ModContent.GetInstance<global::XianXia.Content.Buffs.TribulationResistanceBuff>().Update(buffReceiver, ref receivedBuff);
                check(Math.Abs(buffReceiver.endurance - 0.08f) < 0.0001f && clientState.spiritPressure == 50, "native received buff grants client defense without mutating authoritative pressure");
            } finally { Main.player[0] = buffActor; Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Netplay.Connection = savedConnection; }
            var authorityState = buffActor.GetModPlayer<XianXiaPlayer>(); authorityState.spiritPressure = 50; buffActor.endurance = 0;
            ModContent.GetInstance<global::XianXia.Content.Buffs.TribulationResistanceBuff>().Update(buffActor, ref buffIndex);
            check(Math.Abs(buffActor.endurance - 0.08f) < 0.0001f && authorityState.spiritPressure == 49, "native authority resistance buff applies defense and pressure reduction");

            Vector2 buffCenter = buffActor.Center; buffActor.Center = Main.player[1].Center;
            buffActor.buffTime[buffIndex] = 15; buffEffects.PostUpdateEquips();
            check(buffActor.buffTime[buffIndex] == 15 && buffOutput.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == 2,
                "native leaving sword tablet range stops buff renewal");
            buffActor.Center = buffCenter; buffActor.ClearBuff(buff); buffActor.buffImmune[buff] = true; buffEffects.PostUpdateEquips();
            check(!buffActor.HasBuff(buff) && buffOutput.Sent.Count(packet => packet.Length > 2 && packet[2] == MessageID.AddPlayerBuff) == 2,
                "native immune player receives neither resistance buff nor misleading packet");
            buffActor.buffImmune[buff] = false;
            for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tile.HasTile = false; }
            check(Main.player[0].GetModPlayer<ServerBiomePlayer>().Count(types[0]) == 2, "native tile counts retain the normal same-tick scan cache");
            foreach (string reset in new[] { "load", "unload", "disconnect", "initialize", "enter" }) {
                Player actor = Main.player[0]; var effects = actor.GetModPlayer<BiomeObjectEffectsPlayer>(); var scan = actor.GetModPlayer<ServerBiomePlayer>();
                // Prime with actual loaded tile storage, then remove without advancing the world clock.
                for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tile.HasTile = true; }
                scan.Initialize(); effects.Initialize();
                check(scan.Count(types[0]) == 2, "native lifecycle fixture primes real tile cache"); effects.PostUpdateEquips();
                for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tile.HasTile = false; }
                switch (reset) {
                    case "load": caches.OnWorldLoad(); break;
                    case "unload": caches.OnWorldUnload(); break;
                    case "disconnect": actor.active = false; caches.PostUpdatePlayers(); actor.active = true; break;
                    case "initialize": scan.Initialize(); effects.Initialize(); break;
                    case "enter": scan.OnEnterWorld(); effects.OnEnterWorld(); break;
                }
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritualEnergyRegenBonus = 0; state.spiritPressure = 50;
                Array.Clear(actor.buffType); Array.Clear(actor.buffTime);
                effects.PostUpdateEquips();
                check(scan.Count(types[0]) == 0 && state.spiritualEnergyRegenBonus == 0 && state.spiritPressure == 50 && !actor.HasBuff(buff), "native lifecycle invalidation refreshes removed objects without old effects");
            }
            Player subject = Main.player[0];
            for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tile.HasTile = true; }
            subject.GetModPlayer<BiomeObjectEffectsPlayer>().Initialize(); subject.GetModPlayer<ServerBiomePlayer>().Initialize();
            subject.dead = true; subject.GetModPlayer<BiomeObjectEffectsPlayer>().PostUpdateEquips();
            check(subject.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus == 0 && !subject.HasBuff(buff), "native dead player receives no nearby object effects");
            subject.dead = false; Main.netMode = NetmodeID.MultiplayerClient;
            subject.GetModPlayer<BiomeObjectEffectsPlayer>().PostUpdateEquips();
            check(subject.GetModPlayer<ServerBiomePlayer>().Count(types[0]) == 0 && subject.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus == 0 && !subject.HasBuff(buff), "native client cannot apply server biome scans or object effects");
        } finally {
            foreach (var saved in tiles) { Tile tile = Main.tile[saved.X, ty]; tile.HasTile = saved.HasTile; tile.TileType = saved.Type; }
            for (int i = 0; i < previousPlayers.Length; i++) Main.player[i] = previousPlayers[i];
            previousClients.CopyTo(Netplay.Clients, 0);
            for (int i = 0; i < 3; i++) NetMessage.buffer[i].broadcast = previousBroadcast[i];
            Main.netMode = previousMode; Main.myPlayer = previousLocal;
        }
        check(tiles.All(saved => Main.tile[saved.X, ty].HasTile == saved.HasTile && Main.tile[saved.X, ty].TileType == saved.Type)
            && Main.player.Take(3).SequenceEqual(previousPlayers) && Netplay.Clients.SequenceEqual(previousClients)
            && Main.netMode == previousMode && Main.myPlayer == previousLocal
            && NetMessage.buffer.Take(3).Select(buffer => buffer.broadcast).SequenceEqual(previousBroadcast), "native biome audit restores tile fields player/client slots and engine context");
    }
}
