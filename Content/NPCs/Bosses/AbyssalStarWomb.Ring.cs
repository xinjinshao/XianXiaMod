using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace XianXia.Content.NPCs.Bosses;

public partial class AbyssalStarWomb
{
    private bool ringFrame;
    private int ringTarget = -1;
    private Player ringPlayer;
    private bool StarPhaseTwo => NPC.life < (int)(NPC.lifeMax * 0.65f);
    private bool StarFinalPhase => NPC.life < (int)(NPC.lifeMax * 0.3f);
    public const int RingWarningTicks = 60, RingRecoveryTicks = 45;
    private bool ValidRingState() => float.IsFinite(NPC.ai[1]) && NPC.ai[1] >= 0f && NPC.ai[1] <= RingWarningTicks
        && NPC.ai[1] == MathF.Truncate(NPC.ai[1])
        && float.IsFinite(NPC.ai[2]) && NPC.ai[2] >= -RingRecoveryTicks && NPC.ai[2] <= 300f
        && NPC.ai[2] == MathF.Truncate(NPC.ai[2])
        && (NPC.ai[1] == 0f || float.IsFinite(NPC.ai[3]));
    internal bool UpdateStarRing(Player target, bool phaseTwo, bool finalPhase)
    {
        if (!ValidRingState()) {
            ringFrame = true;
            NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
                ringTarget = -1; ringPlayer = null;
                NPC.netUpdate = true;
            }
            return true;
        }
        if ((NPC.ai[1] > 0f || NPC.ai[2] < 0f) && Main.netMode != NetmodeID.MultiplayerClient
            && ringTarget >= 0 && (NPC.target != ringTarget || !ReferenceEquals(target, ringPlayer))) {
            NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
            ringTarget = -1; ringPlayer = null;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
            ringFrame = true;
            return true;
        }
        int interval = finalPhase ? 180 : phaseTwo ? 240 : 300;
        ringFrame = NPC.ai[2] < 0f || NPC.ai[1] > 0f || NPC.ai[2] >= interval - RingWarningTicks;
        if (NPC.ai[2] < 0f) {
            NPC.velocity *= 0.9f;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[2]++;
                if (NPC.ai[2] == 0f) { NPC.ai[0] = 0f; ringTarget = -1; ringPlayer = null; NPC.netUpdate = true; }
            }
            return true;
        }
        if (NPC.ai[1] == 0f && NPC.ai[2] < interval - RingWarningTicks) {
            if (Main.netMode != NetmodeID.MultiplayerClient) NPC.ai[2]++;
            return false;
        }
        if (NPC.ai[1] == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
            ringTarget = NPC.target; ringPlayer = target;
            NPC.ai[1] = RingWarningTicks;
            NPC.ai[3] = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation();
            NPC.netUpdate = true;
        }
        NPC.rotation = NPC.ai[3];
        NPC.velocity *= 0.9f;
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        if (--NPC.ai[1] > 0f) return true;
        SpawnStarRing(target, phaseTwo, finalPhase);
        NPC.ai[2] = -RingRecoveryTicks;
        NPC.netUpdate = true;
        return true;
    }
    private void SpawnStarRing(Player target, bool phaseTwo, bool finalPhase)
    {
        int ringDmg = Math.Max(18, NPC.damage / 4);

        int spokes = finalPhase ? 12 : phaseTwo ? 8 : 6;
        // Two opposite openings let players escape toward or away from the locked target direction.
        for (int i = 1; i < spokes; i++) {
            if (i == spokes / 2) continue;
            Vector2 velocity = new Vector2(1f, 0f).RotatedBy(MathHelper.TwoPi * i / spokes + NPC.ai[3]) * (finalPhase ? 8f : 6f);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), ringDmg, 1.4f, Main.myPlayer);
        }
        if (phaseTwo) ReleaseCompressionField(target, ringDmg);
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        global::XianXia.Common.Systems.BossTargeting.HasLivingTarget(NPC) && ValidRingState()
        && !ringFrame && NPC.ai[1] == 0f && NPC.ai[2] >= 0f
        && target.active && !target.dead && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || !ValidRingState() || NPC.ai[1] <= 0f
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return true;
        int spokes = StarFinalPhase ? 12 : StarPhaseTwo ? 8 : 6;
        foreach (float opening in new[] { 0f, MathHelper.TwoPi / 2f })
        foreach (float sign in new[] { -1f, 1f }) {
            float angle = NPC.ai[3] + opening + sign * MathHelper.TwoPi / (2 * spokes);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, NPC.Center - screenPos, null, Color.Cyan * 0.65f,
                angle, Vector2.Zero, new Vector2(400f, 2f), SpriteEffects.None, 0f);
        }
        return true;
    }

}
