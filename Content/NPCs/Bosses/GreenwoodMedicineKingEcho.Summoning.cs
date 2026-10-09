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
    public const int SummonWarningTicks = 45, SummonRecoveryTicks = 30;
    private bool summonFrame;
    private int summonTarget = -1;
    private Player summonPlayer;
    private bool ValidVineSummonState() => float.IsFinite(NPC.ai[3]) && NPC.ai[3] >= -SummonRecoveryTicks
        && NPC.ai[3] <= SummonWarningTicks && NPC.ai[3] == MathF.Truncate(NPC.ai[3]);
    internal bool BeginVineSummon(Player target)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || vineSummonsCreated >= VineSummonQuota
            || NPC.ai[3] != 0f || !BossTargeting.HasLivingTarget(NPC)
            || !ReferenceEquals(Main.player[NPC.target], target)) return false;
        summonTarget = NPC.target; summonPlayer = target;
        NPC.ai[0] = NPC.ai[2] = 0f;
        NPC.ai[3] = SummonWarningTicks; NPC.netUpdate = true;
        return UpdateVineSummon(target);
    }
    private void RecoverVineSummon()
    {
        NPC.ai[0] = NPC.ai[2] = 0f; NPC.ai[3] = -SummonRecoveryTicks;
        summonTarget = -1; summonPlayer = null; NPC.netUpdate = true;
    }
    internal bool UpdateVineSummon(Player target)
    {
        summonFrame = false;
        if (!ValidVineSummonState()) {
            summonFrame = true; NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient) RecoverVineSummon();
            return true;
        }
        if (NPC.ai[3] == 0f) return false;
        summonFrame = true; NPC.velocity = Vector2.Zero;
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        if (NPC.ai[3] < 0f) { if (++NPC.ai[3] == 0f) NPC.netUpdate = true; return true; }
        if (summonTarget >= 0 && (NPC.target != summonTarget || !ReferenceEquals(target, summonPlayer))) {
            RecoverVineSummon(); return true;
        }
        if (--NPC.ai[3] > 0f) {
            if (NPC.ai[3] % 15 == 0) NPC.netUpdate = true;
            return true;
        }
        SpawnVineAdds(); RecoverVineSummon(); return true;
    }
    private void DrawVineSummon(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (Main.dedServ || !ValidVineSummonState() || NPC.ai[3] <= 0f
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return;
        Vector2 start = NPC.Center - screenPos + new Vector2(-144f, -108f);
        var pixel = TextureAssets.MagicPixel.Value;
        // Outline the boss-side area where the followers will appear.
        spriteBatch.Draw(pixel, start, null, Color.Cyan * 0.75f, 0f, Vector2.Zero, new Vector2(288f, 2f), SpriteEffects.None, 0f);
        spriteBatch.Draw(pixel, start + new Vector2(0f, 48f), null, Color.Cyan * 0.75f, 0f, Vector2.Zero, new Vector2(288f, 2f), SpriteEffects.None, 0f);
        spriteBatch.Draw(pixel, start, null, Color.Cyan * 0.75f, 0f, Vector2.Zero, new Vector2(2f, 48f), SpriteEffects.None, 0f);
        spriteBatch.Draw(pixel, start + new Vector2(288f, 0f), null, Color.Cyan * 0.75f, 0f, Vector2.Zero, new Vector2(2f, 48f), SpriteEffects.None, 0f);
    }
}
