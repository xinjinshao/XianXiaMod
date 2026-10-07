using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class AbyssalStarWomb
{
    private bool compressionNext = true;

    internal void ReleaseCompressionField(Player target, int damage)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !BossTargeting.HasLivingTarget(NPC)
            || !ReferenceEquals(Main.player[NPC.target], target)) return;
        // Alternate eligible patterns, independently of world uptime or failed creation.
        bool release = compressionNext;
        compressionNext = !compressionNext;
        NPC.netUpdate = true;
        if (!release) return;
        Vector2 position = target.Center + target.velocity * 18f;
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y)) position = target.Center;
        Projectile.NewProjectile(NPC.GetSource_FromAI(), position, Vector2.Zero,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(),
            System.Math.Max(1, damage), 1.2f, Main.myPlayer);
    }
}
