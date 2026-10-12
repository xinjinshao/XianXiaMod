using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace XianXia.Content.NPCs.Bosses;

public partial class BlackFurnaceIronGolem
{
    public const int SlamWarningTicks = 60, SlamRecoveryTicks = 45;
    private bool slamFrame, slamAnnounced, slamReleased;
    private int slamTarget = -1;
    private Player slamPlayer;
    internal void ClearFurnaceSlam() { slamFrame = true; slamTarget = -1; slamPlayer = null; slamAnnounced = false; }
    internal bool UpdateFurnaceSlam(Player target, bool phaseTwo, bool finalPhase)
    {
        slamFrame = false;
        bool invalidChargeClock = !float.IsFinite(NPC.ai[1]) || NPC.ai[1] != MathF.Truncate(NPC.ai[1])
            || NPC.ai[1] < -(ChargeWarningTicks + ChargeDashTicks + ChargeRecoveryTicks) || NPC.ai[1] > 180f;
        if (!invalidChargeClock && NPC.ai[1] < 0f) return false; // Charge owns the frame until its complete recovery.
        if (invalidChargeClock || !float.IsFinite(NPC.ai[2]) || NPC.ai[2] != MathF.Truncate(NPC.ai[2])
            || NPC.ai[2] < -(SlamWarningTicks + SlamRecoveryTicks) || NPC.ai[2] > 270f || (NPC.ai[2] < 0f && !float.IsFinite(NPC.ai[3]))) {
            ClearFurnaceExposure();
            NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient) { NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f; NPC.netUpdate = true; }
            slamFrame = true; return true;
        }
        if (NPC.ai[2] < 0f && Main.netMode != NetmodeID.MultiplayerClient && slamTarget >= 0
            && (NPC.target != slamTarget || !ReferenceEquals(target, slamPlayer))) {
            ClearFurnaceExposure();
            NPC.ai[0] = NPC.ai[2] = NPC.ai[3] = 0f; NPC.velocity = Vector2.Zero;
            slamTarget = -1; slamPlayer = null; slamFrame = true; NPC.netUpdate = true; return true;
        }
        if (NPC.ai[2] >= 0f) {
            slamAnnounced = false;
            if (!phaseTwo || Main.netMode == NetmodeID.MultiplayerClient) return false;
            if (++NPC.ai[2] < (finalPhase ? 150 : 210)) return false;
            slamTarget = NPC.target; slamPlayer = target; slamReleased = false; NPC.ai[3] = target.Center.Y; NPC.ai[2] = -1f; NPC.ai[0] = 0f; NPC.netUpdate = true;
        }
        ClearFurnaceExposure();
        NPC.velocity = Vector2.Zero;
        int elapsed = (int)-NPC.ai[2];
        if (elapsed <= SlamWarningTicks && !slamAnnounced) {
            if (!Main.dedServ) CombatText.NewText(NPC.Hitbox, Color.OrangeRed,
                Language.GetTextValue("Mods.XianXia.Progression.BossPhase.FurnaceSlam"));
            slamAnnounced = true;
        }
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            if (elapsed == SlamWarningTicks + 1 && !slamReleased) {
                slamReleased = true;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X, NPC.ai[3]),
                    Vector2.Zero, ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceSlamWaveProjectile>(),
                    Math.Max(18, NPC.damage / 3), 1.5f, Main.myPlayer);
                SpawnShardAdds();
            }
            if (elapsed >= SlamWarningTicks + SlamRecoveryTicks) { NPC.ai[0] = NPC.ai[2] = NPC.ai[3] = 0f; slamTarget = -1; slamPlayer = null; }
            else NPC.ai[2]--;
            if (elapsed % 15 == 0 || elapsed == SlamWarningTicks + 1) NPC.netUpdate = true;
        }
        slamFrame = true; return true;
    }
    private bool DrawFurnaceSlam(SpriteBatch batch, Vector2 screenPos)
    {
        if (Main.dedServ || !float.IsFinite(NPC.ai[2]) || NPC.ai[2] >= 0f || NPC.ai[2] < -SlamWarningTicks
            || !float.IsFinite(NPC.ai[3]) || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return false;
        Vector2 corner = new Vector2(NPC.Center.X - 140f, NPC.ai[3] - 16f) - screenPos;
        var pixel = TextureAssets.MagicPixel.Value;
        foreach(float edgeY in new[]{0f,30f}) batch.Draw(pixel, corner + new Vector2(0f,edgeY), null, Color.OrangeRed * .75f, 0f, Vector2.Zero,new Vector2(280f,2f),SpriteEffects.None,0f);
        foreach(float edgeX in new[]{0f,278f}) batch.Draw(pixel, corner + new Vector2(edgeX,0f), null, Color.OrangeRed * .75f, 0f, Vector2.Zero,new Vector2(2f,32f),SpriteEffects.None,0f);
        return true;
    }
}
