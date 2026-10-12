using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Projectiles;

namespace XianXia.Content.NPCs.Bosses;

public partial class ThunderMarshJiao
{
    internal const float DiveLightningOffset = 160f, DiveArrayOffset = 256f;
    internal void SpawnDiveThunderZones(Player target, bool phaseTwo, bool finalPhase)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !phaseTwo || !chargeStarted
            || !BossTargeting.HasLivingTarget(NPC) || !ReferenceEquals(target, Main.player[NPC.target])
            || !ReferenceEquals(target, chargePlayer) || NPC.target != chargeTarget) return;
        // Consume the opening event before attempting creation: no capacity retry bursts.
        chargeStarted = false;
        Vector2 origin = target.Center;
        for (int sign = -1; sign <= 1; sign += 2) {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), origin + new Vector2(sign * DiveLightningOffset, 0f),
                Vector2.Zero, ModContent.ProjectileType<TribulationWarningLineProjectile>(),
                Math.Max(18, NPC.damage / 3), 1.2f, Main.myPlayer);
            if (finalPhase)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), origin + new Vector2(sign * DiveArrayOffset, 0f),
                    Vector2.Zero, ModContent.ProjectileType<BossArrayFieldProjectile>(),
                    Math.Max(18, NPC.damage / 4), 1.2f, Main.myPlayer);
        }
    }
}
