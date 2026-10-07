using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace XianXia.Content.NPCs.Bosses;

public partial class MoonboneImmortal
{
    private bool ringFrame;
    public const int RingWarningTicks = 45, RingRecoveryTicks = 30;
    private bool ValidRingState() => float.IsFinite(NPC.ai[1]) && NPC.ai[1] >= 0f && NPC.ai[1] <= RingWarningTicks
        && NPC.ai[1] == MathF.Truncate(NPC.ai[1])
        && float.IsFinite(NPC.ai[2]) && NPC.ai[2] >= -RingRecoveryTicks && NPC.ai[2] <= 270f
        && NPC.ai[2] == MathF.Truncate(NPC.ai[2])
        && (NPC.ai[1] == 0f || float.IsFinite(NPC.ai[3]));
    internal bool UpdateMoonRing(Player target, bool phaseTwo, bool finalPhase)
    {
        if (!ValidRingState()) {
            ringFrame = true;
            NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0f;
                NPC.netUpdate = true;
            }
            return true;
        }
        int interval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        ringFrame = NPC.ai[2] < 0f || NPC.ai[1] > 0f || NPC.ai[2] >= interval - RingWarningTicks;
        if (NPC.ai[2] < 0f) {
            NPC.velocity *= 0.9f;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[2]++;
                if (NPC.ai[2] == 0f) { NPC.ai[0] = 0f; NPC.netUpdate = true; }
            }
            return true;
        }
        if (NPC.ai[1] == 0f && NPC.ai[2] < interval - RingWarningTicks) {
            if (Main.netMode != NetmodeID.MultiplayerClient) NPC.ai[2]++;
            return false;
        }
        if (NPC.ai[1] == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
            NPC.ai[1] = RingWarningTicks;
            NPC.ai[3] = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation();
            NPC.netUpdate = true;
        }
        NPC.rotation = NPC.ai[3];
        NPC.velocity *= 0.9f;
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        if (--NPC.ai[1] > 0f) return true;
        SpawnMoonRing(target, phaseTwo, finalPhase);
        NPC.ai[2] = -RingRecoveryTicks;
        NPC.netUpdate = true;
        return true;
    }
    private void SpawnMoonRing(Player target, bool phaseTwo, bool finalPhase)
    {
            int ringDmg = Math.Max(18, NPC.damage / 4);

            if (phaseTwo && NPC.localAI[1] == 0) {

                NPC.localAI[1] = 1f;

                for (int a = 0; a < 2; a++)

                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-80, 81), (int)NPC.Center.Y + Main.rand.Next(-40, 41),

                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.ArchivedImmortalSoul>(), ai0: NPC.whoAmI);

            }


        int spokes = finalPhase ? 12 : phaseTwo ? 8 : 6;
        // Leave the locked direction empty; adjacent spokes bound the escape opening.
        for (int i = 1; i < spokes; i++) {
            Vector2 velocity = new Vector2(1f, 0f).RotatedBy(MathHelper.TwoPi * i / spokes + NPC.ai[3]) * (finalPhase ? 8f : 6f);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), ringDmg, 1.4f, Main.myPlayer);
        }
        if (phaseTwo)
            Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 18f, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), ringDmg, 1.2f, Main.myPlayer);
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
        int spokes = NPC.life < NPC.lifeMax * 0.3f ? 12 : NPC.life < NPC.lifeMax * 0.6f ? 8 : 6;
        foreach (float sign in new[] { -1f, 1f }) {
            float angle = NPC.ai[3] + sign * MathHelper.TwoPi / (2 * spokes);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, NPC.Center - screenPos, null, Color.LightGreen * 0.7f,
                angle, Vector2.Zero, new Vector2(400f, 2f), SpriteEffects.None, 0f);
        }
        return true;
    }

}
