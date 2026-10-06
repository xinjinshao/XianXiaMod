using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace XianXia.Content.NPCs.Bosses;

public partial class BlackFurnaceIronGolem
{
    public const int ChargeWarningTicks = 40, ChargeDashTicks = 24, ChargeRecoveryTicks = 40;
    private bool chargeDashing;
    internal bool UpdateFurnaceCharge(Player target, bool finalPhase)
    {
        chargeDashing = false;
        if (NPC.ai[1] >= 0f) {
            if (!finalPhase || Main.netMode == NetmodeID.MultiplayerClient) return false;
            if (++NPC.ai[1] < 180f) return false;
            NPC.ai[1] = -1f;
            NPC.ai[3] = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation();
            NPC.netUpdate = true;
        }
        // Lock the direction at the beginning: movement during the warning evades the dash.
        Vector2 direction = new Vector2(1f, 0f).RotatedBy(NPC.ai[3]);
        int elapsed = (int)-NPC.ai[1];
        NPC.rotation = NPC.ai[3];
        chargeDashing = elapsed > ChargeWarningTicks && elapsed <= ChargeWarningTicks + ChargeDashTicks;
        if (elapsed <= ChargeWarningTicks) NPC.velocity *= 0.85f;
        else if (elapsed <= ChargeWarningTicks + ChargeDashTicks) NPC.velocity = direction * 14f;
        else NPC.velocity *= 0.85f;
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            if (elapsed >= ChargeWarningTicks + ChargeDashTicks + ChargeRecoveryTicks) {
                NPC.ai[1] = 0f;
                NPC.ai[0] = NPC.ai[2] = 0f;
                NPC.netUpdate = true;
            } else {
                NPC.ai[1]--;
                if (elapsed == ChargeWarningTicks || elapsed == ChargeWarningTicks + ChargeDashTicks) NPC.netUpdate = true;
            }
        }
        return true;
    }
}
