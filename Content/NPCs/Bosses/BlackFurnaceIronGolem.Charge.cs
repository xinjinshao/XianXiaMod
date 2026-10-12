using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace XianXia.Content.NPCs.Bosses;

public partial class BlackFurnaceIronGolem
{
    public const int ChargeWarningTicks = 40, ChargeDashTicks = 24, ChargeRecoveryTicks = 40;
    public const float RecoveryDefenseReduction = 10f;
    private bool chargeDashing, chargeFrame, armorExposed, exposureAnnounced;
    private Player exposedPlayer;
    private int exposedTarget = -1;
    internal bool ArmorExposed => armorExposed && chargeFrame && !chargeDashing
        && global::XianXia.Common.Systems.BossTargeting.HasLivingTarget(NPC)
        && NPC.target == exposedTarget && ReferenceEquals(Main.player[NPC.target], exposedPlayer)
        && float.IsFinite(NPC.ai[1]) && NPC.ai[1] == System.MathF.Truncate(NPC.ai[1])
        && NPC.ai[1] >= -(ChargeWarningTicks + ChargeDashTicks + ChargeRecoveryTicks)
        && (NPC.ai[1] <= -(ChargeWarningTicks + ChargeDashTicks + 1f) || NPC.ai[1] == 0f);

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (ArmorExposed) modifiers.Defense.Flat -= RecoveryDefenseReduction;
    }

    private int chargeTarget = -1;
    private Player chargePlayer;
    internal void ClearFurnaceExposure()
    {
        armorExposed = false; exposureAnnounced = false;
        exposedPlayer = null; exposedTarget = -1;
        chargeDashing = false; chargeFrame = true;
        chargePlayer = null; chargeTarget = -1;
    }
    internal bool UpdateFurnaceCharge(Player target, bool finalPhase)
    {
        armorExposed = false;
        exposedPlayer = null; exposedTarget = -1;
        chargeDashing = false;
        chargeFrame = false;
        if (!float.IsFinite(NPC.ai[1]) || NPC.ai[1] != System.MathF.Truncate(NPC.ai[1]) || NPC.ai[1] < -(ChargeWarningTicks + ChargeDashTicks + ChargeRecoveryTicks)
            || NPC.ai[1] > 180f || (NPC.ai[1] < 0f && !float.IsFinite(NPC.ai[3]))) {
            NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
                NPC.netUpdate = true;
            }
            chargeFrame = true;
            return true;
        }
        if (NPC.ai[1] < 0f && Main.netMode != NetmodeID.MultiplayerClient && chargeTarget >= 0
            && (NPC.target != chargeTarget || !ReferenceEquals(target, chargePlayer))) {
            NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
            NPC.velocity = Vector2.Zero;
            chargeTarget = -1;
            chargePlayer = null;
            NPC.netUpdate = true;
            chargeFrame = true;
            return true;
        }
        if (NPC.ai[1] >= 0f) {
            exposureAnnounced = false;
            if (!finalPhase || Main.netMode == NetmodeID.MultiplayerClient) return false;
            if (++NPC.ai[1] < 180f) return false;
            chargeTarget = NPC.target;
            chargePlayer = target;
            NPC.ai[1] = -1f;
            NPC.ai[3] = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation();
            NPC.netUpdate = true;
        }
        // Lock the direction at the beginning: movement during the warning evades the dash.
        Vector2 direction = new Vector2(1f, 0f).RotatedBy(NPC.ai[3]);
        int elapsed = (int)-NPC.ai[1];
        NPC.rotation = NPC.ai[3];
        armorExposed = elapsed > ChargeWarningTicks + ChargeDashTicks;
        if (armorExposed) {
            exposedPlayer = target; exposedTarget = NPC.target;
            if (!exposureAnnounced && !Main.dedServ)
                CombatText.NewText(NPC.Hitbox, Color.LightGreen,
                    Language.GetTextValue("Mods.XianXia.Progression.BossPhase.FurnaceArmorExposed"));
            exposureAnnounced = true;
        }
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
        chargeFrame = true;
        return true;
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) =>
        global::XianXia.Common.Systems.BossTargeting.HasLivingTarget(NPC) && target.active && !target.dead
        && !slamFrame && float.IsFinite(NPC.ai[2]) && NPC.ai[2] >= 0f
        && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && float.IsFinite(NPC.ai[1]) && NPC.ai[1] == System.MathF.Truncate(NPC.ai[1]) && NPC.ai[1] <= 180f
        && NPC.ai[1] >= -(ChargeWarningTicks + ChargeDashTicks + ChargeRecoveryTicks)
        && ((!chargeFrame && NPC.ai[1] >= 0f) || (chargeDashing && float.IsFinite(NPC.ai[3])))
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (DrawFurnaceSlam(spriteBatch, screenPos)) return true;
        if (!Main.dedServ && ArmorExposed && float.IsFinite(NPC.Center.X) && float.IsFinite(NPC.Center.Y)) {
            Vector2 topLeft = NPC.Center - new Vector2(NPC.width / 2f, NPC.height / 2f) - screenPos;
            var armorPixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(armorPixel, topLeft, null, Color.LightGreen * 0.75f, 0f, Vector2.Zero,
                new Vector2(NPC.width, 2f), SpriteEffects.None, 0f);
            spriteBatch.Draw(armorPixel, topLeft + new Vector2(0f, NPC.height - 2f), null, Color.LightGreen * 0.75f, 0f, Vector2.Zero,
                new Vector2(NPC.width, 2f), SpriteEffects.None, 0f);
            spriteBatch.Draw(armorPixel, topLeft, null, Color.LightGreen * 0.75f, 0f, Vector2.Zero,
                new Vector2(2f, NPC.height), SpriteEffects.None, 0f);
            spriteBatch.Draw(armorPixel, topLeft + new Vector2(NPC.width - 2f, 0f), null, Color.LightGreen * 0.75f, 0f, Vector2.Zero,
                new Vector2(2f, NPC.height), SpriteEffects.None, 0f);
        }
        if (Main.dedServ || NPC.ai[1] >= 0f || NPC.ai[1] < -ChargeWarningTicks
            || !float.IsFinite(NPC.ai[1]) || !float.IsFinite(NPC.ai[3])
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return true;
        Vector2 direction = new Vector2(1f, 0f).RotatedBy(NPC.ai[3]);
        Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
        float along = System.MathF.Abs(direction.X) * NPC.width + System.MathF.Abs(direction.Y) * NPC.height;
        float across = System.MathF.Abs(direction.Y) * NPC.width + System.MathF.Abs(direction.X) * NPC.height;
        float length = 14f * ChargeDashTicks + along;
        Vector2 start = NPC.Center - direction * (along * 0.5f) - screenPos;
        var pixel = TextureAssets.MagicPixel.Value;
        // The corridor includes the projected rectangular hitbox for diagonal dashes.
        spriteBatch.Draw(pixel, start - side * (across * 0.5f), null, Color.OrangeRed * 0.12f,
            NPC.ai[3], Vector2.Zero, new Vector2(length, across), SpriteEffects.None, 0f);
        foreach (float sign in new[] { -1f, 1f })
            spriteBatch.Draw(pixel, start + side * (sign * across * 0.5f), null, Color.OrangeRed * 0.75f,
                NPC.ai[3], Vector2.Zero, new Vector2(length, 2f), SpriteEffects.None, 0f);
        return true;
    }
}
