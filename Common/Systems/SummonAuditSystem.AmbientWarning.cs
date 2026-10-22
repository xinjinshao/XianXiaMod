using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Projectiles;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditAmbientWarning(Action<bool,string> check)
    {
        Player previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        int tx = Main.maxTilesX / 2, ty = 20;
        var tiles = new List<(int X,int Y,bool Solid,ushort Type)>();
        var existing = Main.projectile.Where(p => p.active).ToHashSet();
        var recent = (List<(Vector2 Center,ulong Expires)>)typeof(AmbientLightningSystem).GetField("RecentStrikes",BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        var oldRecent = recent.ToArray();
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; recent.Clear();
            for (int x = tx; x < tx + 10; x++) for (int y = ty; y < ty + 10; y++) {
                Tile tile = Main.tile[x,y]; tiles.Add((x,y,tile.HasTile,tile.TileType));
                tile.HasTile = true; tile.TileType = (ushort)ModContent.TileType<ThunderCloudTile>();
            }
            var actor = new Player { whoAmI = 0, active = true, statLife = 100 }; actor.Center = new Vector2((tx + 5) * 16,(ty + 5) * 16); Main.player[0] = actor;
            LoaderManager.Get<BiomeLoader>().UpdateBiomes(actor);
            check(actor.InModBiome<Content.Biomes.ThunderMarshCloudsBiome>() && actor.GetModPlayer<XianXiaPlayer>().tribulationTimer == 0,"native ambient fixture activates actual cloud biome without personal trial");
            var effects = actor.GetModPlayer<BiomeEffectsPlayer>(); effects.thunderCloudTimer = 419; effects.PostUpdate();
            Projectile[] warnings = Main.projectile.Where(p => p.active && !existing.Contains(p) && p.ModProjectile is TribulationWarningLineProjectile).ToArray();
            check(warnings.Length == 1 && effects.thunderCloudTimer == 0,"native actual environment update creates one registered warning at cadence");
            Projectile warning = warnings.Single();
            using var wire = new System.IO.MemoryStream(); warning.ModProjectile.SendExtraAI(new System.IO.BinaryWriter(wire)); byte[] payload = wire.ToArray();
            check(payload.Length == 27 && BitConverter.ToInt16(payload,3) == -1 && BitConverter.ToInt16(payload,9) == -1,"native ambient warning binds neither NPC nor personal trial source");
            for (int frame = 0; frame < TribulationWarningLineProjectile.Lifetime; frame++) {
                warning.ModProjectile.AI(); warning.timeLeft--;
                check(!Main.projectile.Any(p => p.active && !existing.Contains(p) && p.ModProjectile is TribulationLightningProjectile),"native ambient warning does not release lightning before full warning duration");
            }
            check(warning.timeLeft == 0,"native ambient warning retains complete lifetime without personal trial");
            actor.dead = true; warning.ModProjectile.OnKill(0); warning.ModProjectile.OnKill(0);
            Projectile[] bolts = Main.projectile.Where(p => p.active && !existing.Contains(p) && p.ModProjectile is TribulationLightningProjectile).ToArray();
            check(bolts.Length == 1 && bolts[0].damage == 30 && bolts[0].ModProjectile.CanDamage() == true,"native ambient expiry releases exactly one damaging lightning independently of trigger player death");
            Main.netMode = NetmodeID.MultiplayerClient; warning.ModProjectile.OnKill(0);
            check(Main.projectile.Count(p => p.active && !existing.Contains(p) && p.ModProjectile is TribulationLightningProjectile) == 1,"native client expiry cannot duplicate environmental lightning");
            Main.netMode = NetmodeID.Server; actor.dead = false; effects.thunderCloudTimer = 419; effects.PostUpdate();
            check(Main.projectile.Count(p => p.active && !existing.Contains(p) && p.ModProjectile is TribulationWarningLineProjectile) == 1,"native existing area cooldown suppresses another environmental warning");
        } finally {
            foreach (var tile in tiles) { Tile restored = Main.tile[tile.X,tile.Y]; restored.HasTile = tile.Solid; restored.TileType = tile.Type; }
            foreach (Projectile p in Main.projectile.Where(p => p.active && !existing.Contains(p))) { p.damage = 0; p.active = false; }
            recent.Clear(); recent.AddRange(oldRecent); Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local;
        }
        check(ReferenceEquals(Main.player[0],previous) && recent.SequenceEqual(oldRecent),"native ambient warning audit restores tiles player and area cooldown context");
    }
}
