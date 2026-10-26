using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditWarningGeometry(Action<bool, string> check)
    {
        int oldMode = Main.netMode;
        var existing = Main.projectile.Where(p => p.active).ToHashSet();
        try {
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server, NetmodeID.MultiplayerClient })
            foreach (int axis in new[] { 0, 1, 2, 3 })
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            foreach (bool aiFirst in new[] { false, true }) {
                Main.netMode = NetmodeID.Server;
                int index = Projectile.NewProjectile(new Terraria.DataStructures.EntitySource_Misc("WarningGeometryAudit"),
                    new Vector2(Main.maxTilesX * 8, 320), Vector2.Zero, ModContent.ProjectileType<TribulationWarningLineProjectile>(), 20, 0, Main.maxPlayers);
                check(index >= 0 && index < Main.maxProjectiles, "native invalid warning fixture allocates registered projectile");
                Projectile marker = Main.projectile[index];
                marker.Center = axis == 0 ? new Vector2(invalid, 320) : axis == 1 ? new Vector2(320, invalid) : new Vector2(320, 320);
                marker.velocity = axis == 2 ? new Vector2(invalid, 0) : axis == 3 ? new Vector2(0, invalid) : Vector2.Zero;
                marker.netUpdate = false; Main.netMode = mode;
                int strikes = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<TribulationLightningProjectile>());
                if (aiFirst) marker.ModProjectile.AI();
                marker.ModProjectile.OnKill(0);
                check(Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<TribulationLightningProjectile>()) == strikes,
                    "native bad warning geometry never creates a lightning child: " + mode + "/" + axis + "/" + invalid + "/" + aiFirst);
                marker.Center = new Vector2(320, 320); marker.velocity = Vector2.Zero;
                marker.ModProjectile.AI(); marker.ModProjectile.OnKill(0);
                check(Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<TribulationLightningProjectile>()) == strikes
                    && (mode == NetmodeID.MultiplayerClient || marker.timeLeft <= 6 && marker.netUpdate),
                    "native repaired warning stays cancelled with authority fade and dirty state");
                marker.active = false;
            }
        } finally {
            foreach (var shot in Main.projectile.Where(p => p.active && !existing.Contains(p))) shot.active = false;
            Main.netMode = oldMode;
        }
    }
}
