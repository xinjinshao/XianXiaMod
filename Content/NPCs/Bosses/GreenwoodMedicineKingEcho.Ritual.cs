using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class GreenwoodMedicineKingEcho
{
    public const int RitualWarningTicks = 90, RitualRecoveryTicks = 60, MaximumRitualHeals = 3;
    private int ritualHeals, ritualLifeStart, ritualTarget = -1;
    private Player ritualPlayer;
    private bool ritualFrame;
    private bool ValidRitualState() => float.IsFinite(NPC.ai[1]) && NPC.ai[1] >= -150f
        && NPC.ai[1] <= 240f && NPC.ai[1] == MathF.Truncate(NPC.ai[1]);
    private void RecoverRitual()
    {
        NPC.ai[0] = NPC.ai[2] = 0f;
        NPC.ai[1] = -(RitualWarningTicks + 1);
        NPC.velocity = Vector2.Zero;
        NPC.netUpdate = true;
        ritualTarget = -1; ritualPlayer = null;
    }
    internal bool UpdateMedicineRitual(Player target, bool phaseTwo)
    {
        ritualFrame = false;
        if (!ValidRitualState()) {
            NPC.velocity = Vector2.Zero; ritualFrame = true;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = 0f; NPC.netUpdate = true;
                ritualTarget = -1; ritualPlayer = null;
            }
            return true;
        }
        if (NPC.ai[1] >= 0f) {
            if (!phaseTwo || ritualHeals >= MaximumRitualHeals || Main.netMode == NetmodeID.MultiplayerClient) return false;
            if (++NPC.ai[1] < 240f) return false;
            ritualLifeStart = NPC.life; ritualTarget = NPC.target; ritualPlayer = target;
            NPC.ai[1] = -1f; NPC.netUpdate = true;
        }
        ritualFrame = true; NPC.velocity *= 0.85f;
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        int elapsed = (int)-NPC.ai[1];
        if (elapsed <= RitualWarningTicks) {
            int threshold = Math.Max(1, NPC.lifeMax / 100);
            if (ritualTarget >= 0 && (NPC.target != ritualTarget || !ReferenceEquals(target, ritualPlayer)
                || (long)ritualLifeStart - NPC.life >= threshold)) {
                RecoverRitual(); return true;
            }
            if (elapsed == RitualWarningTicks) {
                int heal = Math.Min(threshold, Math.Max(0, NPC.lifeMax - NPC.life));
                if (NPC.life > 0 && heal > 0) { NPC.life += heal; ritualHeals++; }
                RecoverRitual(); return true;
            }
        }
        if (elapsed >= RitualWarningTicks + RitualRecoveryTicks) {
            NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = 0f; NPC.netUpdate = true;
        } else {
            NPC.ai[1]--;
            if (elapsed % 15 == 0) NPC.netUpdate = true;
        }
        return true;
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => BossTargeting.HasLivingTarget(NPC)
        && ValidRitualState() && ValidVineSummonState() && !summonFrame && NPC.ai[3] == 0f && !ritualFrame && NPC.ai[1] >= 0f
        && target.active && !target.dead && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawVineSummon(spriteBatch, screenPos);
        if (Main.dedServ || !ValidRitualState() || NPC.ai[1] >= 0f || NPC.ai[1] < -RitualWarningTicks
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return true;
        Vector2 start = NPC.Center - screenPos + new Vector2(-50f, -NPC.height / 2f - 12f);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, start, null, Color.LightGreen * 0.3f,
            0f, Vector2.Zero, new Vector2(100f, 6f), SpriteEffects.None, 0f);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, start, null, Color.LightGreen * 0.9f,
            0f, Vector2.Zero, new Vector2(100f * -NPC.ai[1] / RitualWarningTicks, 6f), SpriteEffects.None, 0f);
        return true;
    }
}
