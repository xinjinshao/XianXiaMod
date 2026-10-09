using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class HeavenTabletGuardian
{
    public const int JudgmentWarningTicks = 60, JudgmentRecoveryTicks = 45;
    private bool judgmentFrame;
    private int judgmentTarget = -1;
    private Player judgmentPlayer;
    private byte judgmentRows = 1;
    private bool ValidJudgmentState() => judgmentRows >= 1 && judgmentRows <= 3
        && float.IsFinite(NPC.ai[1]) && NPC.ai[1] >= 0 && NPC.ai[1] <= JudgmentWarningTicks
        && NPC.ai[1] == MathF.Truncate(NPC.ai[1]) && float.IsFinite(NPC.ai[2])
        && NPC.ai[2] >= -JudgmentRecoveryTicks && NPC.ai[2] <= 300 && NPC.ai[2] == MathF.Truncate(NPC.ai[2])
        && (NPC.ai[1] == 0 || (float.IsFinite(NPC.ai[0]) && float.IsFinite(NPC.ai[3])));

    private void CancelJudgment()
    {
        judgmentFrame = true; NPC.velocity = Vector2.Zero;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0;
        judgmentTarget = -1; judgmentPlayer = null; judgmentRows = 1; NPC.netUpdate = true;
    }

    internal bool UpdateTabletJudgment(Player target, bool phaseTwo, bool finalPhase)
    {
        judgmentFrame = false;
        if (!ValidJudgmentState()) { CancelJudgment(); return true; }
        if ((NPC.ai[1] > 0 || NPC.ai[2] < 0) && Main.netMode != NetmodeID.MultiplayerClient
            && judgmentTarget >= 0 && (NPC.target != judgmentTarget || !ReferenceEquals(target, judgmentPlayer))) {
            CancelJudgment(); return true;
        }
        if (NPC.ai[2] < 0) {
            judgmentFrame = true; NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient && ++NPC.ai[2] == 0) {
                NPC.ai[0] = NPC.ai[3] = 0; judgmentTarget = -1; judgmentPlayer = null; NPC.netUpdate = true;
            }
            return true;
        }
        int interval = finalPhase ? 180 : phaseTwo ? 240 : 300;
        if (NPC.ai[1] == 0 && NPC.ai[2] < interval - JudgmentWarningTicks) {
            if (Main.netMode != NetmodeID.MultiplayerClient) NPC.ai[2]++;
            return false;
        }
        judgmentFrame = true; NPC.velocity = Vector2.Zero;
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        if (NPC.ai[1] == 0) {
            judgmentTarget = NPC.target; judgmentPlayer = target;
            NPC.ai[0] = target.Center.X; NPC.ai[3] = target.Center.Y;
            judgmentRows = (byte)(finalPhase ? 3 : phaseTwo ? 2 : 1);
            NPC.ai[1] = JudgmentWarningTicks; NPC.netUpdate = true;
        }
        if (--NPC.ai[1] > 0) {
            if (NPC.ai[1] % 15 == 0) NPC.netUpdate = true;
            return true;
        }
        // Lock both geometry and density at the start of the warning, even across phase changes.
        for (int row = 0; row < judgmentRows; row++) foreach (float side in new[] { -1f, 1f })
            Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.ai[0] + side * (112 + row * 64), NPC.ai[3]),
                Vector2.Zero, ModContent.ProjectileType<global::XianXia.Content.Projectiles.TabletJudgmentBeamProjectile>(),
                Math.Max(18, NPC.damage / 3), 1.2f, Main.myPlayer);
        NPC.ai[2] = -JudgmentRecoveryTicks; NPC.netUpdate = true;
        return true;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(judgmentRows);
    public override void ReceiveExtraAI(BinaryReader reader) => judgmentRows = reader.ReadByte();

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => BossTargeting.HasLivingTarget(NPC)
        && ValidJudgmentState() && !judgmentFrame && NPC.ai[1] == 0 && NPC.ai[2] >= 0
        && target.active && !target.dead && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || !ValidJudgmentState() || NPC.ai[1] <= 0) return true;
        var pixel = TextureAssets.MagicPixel.Value;
        foreach (float side in new[] { -1f, 1f }) {
            Vector2 edge = new Vector2(NPC.ai[0] + side * 80, NPC.ai[3] - 240) - screenPos;
            spriteBatch.Draw(pixel, edge, null, Color.LightGreen * 0.8f, 0, Vector2.Zero, new Vector2(2, 480), SpriteEffects.None, 0);
            for (int row = 0; row < judgmentRows; row++) {
                Vector2 start = new Vector2(NPC.ai[0] + side * (112 + row * 64) - 16, NPC.ai[3] - 240) - screenPos;
                spriteBatch.Draw(pixel, start, null, Color.Cyan * 0.3f, 0, Vector2.Zero, new Vector2(32, 480), SpriteEffects.None, 0);
            }
        }
        Vector2 bar = NPC.Center - screenPos + new Vector2(-50, -NPC.height / 2f - 12);
        spriteBatch.Draw(pixel, bar, null, Color.Cyan * 0.8f, 0, Vector2.Zero,
            new Vector2(100 * (JudgmentWarningTicks - NPC.ai[1]) / JudgmentWarningTicks, 5), SpriteEffects.None, 0);
        return true;
    }
}
