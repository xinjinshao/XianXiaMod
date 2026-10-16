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
        int tx = Main.maxTilesX / 2, ty = 20;
        int[] types = { ModContent.TileType<SwordTabletTile>(), ModContent.TileType<SingingThunderStoneTile>(), ModContent.TileType<RiftMembraneTile>() };
        var tiles = new List<(int X, bool HasTile, ushort Type)>();
        for (int i = 0; i < 6; i++) { Tile tile = Main.tile[tx + i, ty]; tiles.Add((tx + i, tile.HasTile, tile.TileType)); }
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
            for (int i = 0; i < Netplay.Clients.Length; i++) Netplay.Clients[i] = new RemoteClient { Id = i, Socket = new AuditOutputSocket() };
            for (int i = 0; i < 3; i++) {
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
            previousClients.CopyTo(Netplay.Clients, 0); Main.netMode = previousMode; Main.myPlayer = previousLocal;
        }
        check(tiles.All(saved => Main.tile[saved.X, ty].HasTile == saved.HasTile && Main.tile[saved.X, ty].TileType == saved.Type)
            && Main.player.Take(3).SequenceEqual(previousPlayers) && Netplay.Clients.SequenceEqual(previousClients)
            && Main.netMode == previousMode && Main.myPlayer == previousLocal, "native biome audit restores tile fields player/client slots and engine context");
    }
}
