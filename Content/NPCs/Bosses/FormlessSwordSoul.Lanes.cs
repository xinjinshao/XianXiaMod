using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class FormlessSwordSoul
{
    public const int LaneWarningTicks = 45, LaneRecoveryTicks = 45;
    private bool laneFrame, nextVertical;
    private int laneTarget = -1;
    private Player lanePlayer;
    private bool SwordPhaseTwo => NPC.life < (int)(NPC.lifeMax * 0.75f);
    private bool SwordFinalPhase => NPC.life < (int)(NPC.lifeMax * 0.35f);
    private bool ValidLaneState() => float.IsFinite(NPC.ai[1]) && MathF.Abs(NPC.ai[1]) <= LaneWarningTicks
        && NPC.ai[1] == MathF.Truncate(NPC.ai[1]) && float.IsFinite(NPC.ai[2])
        && NPC.ai[2] >= -LaneRecoveryTicks && NPC.ai[2] <= 300f && NPC.ai[2] == MathF.Truncate(NPC.ai[2])
        && (NPC.ai[1] == 0f || (float.IsFinite(NPC.ai[0]) && float.IsFinite(NPC.ai[3])));
    private void CancelLanes()
    {
        NPC.velocity = Vector2.Zero;
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
            laneTarget = -1; lanePlayer = null;
            NPC.netUpdate = true;
        }
        laneFrame = true;
    }
    internal bool UpdateSwordLanes(Player target, bool phaseTwo, bool finalPhase)
    {
        laneFrame = false;
        if (!ValidLaneState()) { CancelLanes(); return true; }
        if ((NPC.ai[1] != 0f || NPC.ai[2] < 0f) && Main.netMode != NetmodeID.MultiplayerClient
            && laneTarget >= 0 && (NPC.target != laneTarget || !ReferenceEquals(target, lanePlayer))) {
            CancelLanes(); return true;
        }
        if (NPC.ai[2] < 0f) {
            laneFrame = true; NPC.velocity *= 0.9f;
            if (Main.netMode != NetmodeID.MultiplayerClient && ++NPC.ai[2] == 0f) {
                NPC.ai[0] = NPC.ai[3] = 0f; laneTarget = -1; lanePlayer = null; NPC.netUpdate = true;
            }
            return true;
        }
        int interval = finalPhase ? 180 : phaseTwo ? 240 : 300;
        if (NPC.ai[1] == 0f && NPC.ai[2] < interval - LaneWarningTicks) {
            if (Main.netMode != NetmodeID.MultiplayerClient) NPC.ai[2]++;
            return false;
        }
        if (NPC.ai[1] == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
            laneTarget = NPC.target; lanePlayer = target;
            NPC.ai[0] = target.Center.X; NPC.ai[3] = target.Center.Y;
            // The sign carries orientation to clients; the authority alternates only after release.
            NPC.ai[1] = nextVertical ? -LaneWarningTicks : LaneWarningTicks;
            NPC.netUpdate = true;
        }
        laneFrame = true; NPC.velocity *= 0.85f;
        bool vertical = NPC.ai[1] < 0f;
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        NPC.ai[1] += vertical ? 1f : -1f;
        if (NPC.ai[1] != 0f) return true;
        SpawnSwordLanes(vertical, phaseTwo, finalPhase);
        nextVertical = !nextVertical;
        NPC.ai[2] = -LaneRecoveryTicks; NPC.netUpdate = true;
        return true;
    }
    private void SpawnSwordLanes(bool vertical, bool phaseTwo, bool finalPhase)
    {
        Vector2 center = new(NPC.ai[0], NPC.ai[3]);
        Vector2 along = vertical ? Vector2.UnitY : new Vector2(1f, 0f);
        Vector2 across = new(-along.Y, along.X);
        int rows = finalPhase ? 3 : phaseTwo ? 2 : 1;
        for (int row = 0; row < rows; row++) foreach (float side in new[] { -1f, 1f })
            foreach (float end in new[] { -1f, 1f }) {
                Vector2 position = center + across * (side * (112f + row * 48f)) + along * (end * 320f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), position, along * (-end * (finalPhase ? 10f : 8f)),
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    Math.Max(18, NPC.damage / 4), 1.4f, Main.myPlayer);
            }
        if (phaseTwo) SpawnSwordAdds();
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => BossTargeting.HasLivingTarget(NPC)
        && ValidLaneState() && !laneFrame && NPC.ai[1] == 0f && NPC.ai[2] >= 0f
        && target.active && !target.dead && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || !ValidLaneState() || NPC.ai[1] == 0f) return true;
        Vector2 center = new(NPC.ai[0], NPC.ai[3]);
        Vector2 along = NPC.ai[1] < 0f ? Vector2.UnitY : new Vector2(1f, 0f);
        Vector2 across = new(-along.Y, along.X);
        int rows = SwordFinalPhase ? 3 : SwordPhaseTwo ? 2 : 1;
        foreach (float side in new[] { -1f, 1f }) {
            Vector2 start = center + across * (side * 80f) - along * 320f - screenPos;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, start, null, Color.LightGreen * 0.8f,
                along.ToRotation(), Vector2.Zero, new Vector2(640f, 2f), SpriteEffects.None, 0f);
            for (int row = 0; row < rows; row++) {
                start = center + across * (side * (112f + row * 48f)) - along * 320f - screenPos;
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, start, null, Color.Cyan * 0.6f,
                    along.ToRotation(), Vector2.Zero, new Vector2(640f, 2f), SpriteEffects.None, 0f);
            }
        }
        return true;
    }
}
