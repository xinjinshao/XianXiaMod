using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class OldHeavenDaoCore
{
    public const int ModuleWarningTicks = 60, ModuleRecoveryTicks = 45;
    private byte currentModule, nextModule, moduleDensity = 1, moduleRoute, routeCycle;
    private int moduleTarget = -1, announcedRoute;
    private Player modulePlayer;
    private bool moduleFrame, invalidModulePacket;
    private bool ValidModuleState() => !invalidModulePacket && currentModule <= 2 && nextModule <= 2
        && moduleDensity >= 1 && moduleDensity <= 3 && moduleRoute <= 3 && routeCycle <= 2 && float.IsFinite(NPC.ai[1])
        && NPC.ai[1] >= 0 && NPC.ai[1] <= ModuleWarningTicks && NPC.ai[1] == MathF.Truncate(NPC.ai[1])
        && float.IsFinite(NPC.ai[2]) && NPC.ai[2] >= -ModuleRecoveryTicks && NPC.ai[2] <= 270
        && NPC.ai[2] == MathF.Truncate(NPC.ai[2])
        && (NPC.ai[1] == 0 || (float.IsFinite(NPC.ai[0]) && float.IsFinite(NPC.ai[3])));
    private void ResetModuleClocks(bool synchronize = true)
    {
        NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0;
        moduleTarget = -1; modulePlayer = null; invalidModulePacket = false; if (synchronize) NPC.netUpdate = true;
    }
    private void CancelModule()
    {
        moduleFrame = true; NPC.velocity = Vector2.Zero;
        if (Main.netMode != NetmodeID.MultiplayerClient) ResetModuleClocks();
    }
    internal bool UpdateCoreModule(Player target, bool phaseTwo, bool finalPhase)
    {
        moduleFrame = false;
        if (NPC.ai[1] == 0) announcedRoute = 0;
        if (!ValidModuleState()) { CancelModule(); return true; }
        if ((NPC.ai[1] != 0 || NPC.ai[2] < 0) && Main.netMode != NetmodeID.MultiplayerClient
            && moduleTarget >= 0 && (NPC.target != moduleTarget || !ReferenceEquals(target, modulePlayer))) {
            CancelModule(); return true;
        }
        if (NPC.ai[2] < 0) {
            moduleFrame = true; NPC.velocity = Vector2.Zero;
            if (Main.netMode != NetmodeID.MultiplayerClient && ++NPC.ai[2] == 0) ResetModuleClocks();
            return true;
        }
        int interval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (NPC.ai[1] == 0 && NPC.ai[2] < interval - ModuleWarningTicks) {
            if (Main.netMode != NetmodeID.MultiplayerClient) NPC.ai[2]++;
            return false;
        }
        moduleFrame = true; NPC.velocity = Vector2.Zero;
        if (NPC.ai[1] == 0 && Main.netMode != NetmodeID.MultiplayerClient) {
            currentModule = nextModule; moduleDensity = (byte)(finalPhase ? 3 : phaseTwo ? 2 : 1);
            moduleRoute = phaseTwo && currentModule == 1 ? CaptureModuleRoute() : (byte)0;
            moduleTarget = NPC.target; modulePlayer = target;
            NPC.ai[0] = target.Center.X; NPC.ai[3] = target.Center.Y;
            NPC.ai[1] = ModuleWarningTicks; NPC.netUpdate = true;
        }
        if (NPC.ai[1] != 0 && currentModule == 1 && moduleRoute != 0 && announcedRoute != moduleRoute) {
            announcedRoute = moduleRoute;
            if (!Main.dedServ) CombatText.NewText(NPC.Hitbox, Color.Cyan,
                Language.GetTextValue("Mods.XianXia.CoreRoutes." + ((DownedBossSystem.EndgameRoute)moduleRoute)));
        }
        if (Main.netMode == NetmodeID.MultiplayerClient || NPC.ai[1] == 0) return true;
        if (--NPC.ai[1] != 0) {
            if (NPC.ai[1] % 15 == 0) NPC.netUpdate = true;
            return true;
        }
        ReleaseCoreModule();
        if (currentModule == 1 && moduleRoute != 0) routeCycle = (byte)((routeCycle + 1) % 3);
        nextModule = (byte)((currentModule + 1) % 3);
        NPC.ai[2] = -ModuleRecoveryTicks; NPC.netUpdate = true;
        return true;
    }
    private byte CaptureModuleRoute()
    {
        int chosen = (int)DownedBossSystem.ChosenRoute;
        // The ending is selected after the first victory. Before that, demonstrate every route.
        return chosen >= 1 && chosen <= 3 ? (byte)chosen : (byte)(routeCycle + 1);
    }
    private float RingGapHalfAngle => MathHelper.TwoPi / (moduleRoute == 1 ? 8 : 12);
    private bool InRingGap(float angle) => Math.Min(angle, MathHelper.TwoPi - angle) <= RingGapHalfAngle + 0.0001f
        || (moduleRoute == 3 && MathF.Abs(angle - MathHelper.TwoPi / 2) <= RingGapHalfAngle + 0.0001f);
    private Vector2 LockedModulePoint => new(NPC.ai[0], NPC.ai[3]);
    private float RingDirection => (LockedModulePoint - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation();
    private void ReleaseCoreModule()
    {
        int damage = Math.Max(18, NPC.damage / 3);
        if (currentModule == 0) {
            for (int row = 0; row < moduleDensity; row++) foreach (float side in new[] { -1f, 1f })
                Projectile.NewProjectile(NPC.GetSource_FromAI(), LockedModulePoint + new Vector2(side * (112 + row * 64), 0), Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.TabletJudgmentBeamProjectile>(), damage, 1.2f, Main.myPlayer);
        } else if (currentModule == 1 && moduleRoute == 2) {
            foreach (float side in new[] { -1f, 1f })
                Projectile.NewProjectile(NPC.GetSource_FromAI(), LockedModulePoint + new Vector2(0, side * 96), Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.CoreSeveranceBladeProjectile>(), damage, 1.4f, Main.myPlayer);
        } else if (currentModule == 1) {
            int spokes = 8 + moduleDensity * 4;
            // The captured route chooses wider refuge or two opposing gaps.
            for (int spoke = 0; spoke < spokes; spoke++) {
                float angle = MathHelper.TwoPi * spoke / spokes;
                if (InRingGap(angle)) continue;
                Vector2 velocity = Vector2.UnitY.RotatedBy(RingDirection - MathHelper.PiOver2 + angle) * (moduleDensity == 3 ? 8 : 6);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), damage, 1.4f, Main.myPlayer);
            }
        } else {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), LockedModulePoint, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), damage, 1.2f, Main.myPlayer);
        }
    }
    public override void SendExtraAI(BinaryWriter writer) { writer.Write(currentModule); writer.Write(nextModule); writer.Write(moduleDensity); writer.Write(moduleRoute); writer.Write(routeCycle); WriteArchiveState(writer); }
    public override void ReceiveExtraAI(BinaryReader reader) => ReadCoreState(reader);
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => BossTargeting.HasLivingTarget(NPC)
        && ValidModuleState() && archiveState != 1 && !(archiveState == 3 && archiveTimer > 0) && !archiveFrame && !moduleFrame && NPC.ai[1] == 0 && NPC.ai[2] >= 0
        && target.active && !target.dead && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawArchiveWarning(spriteBatch, screenPos);
        if (Main.dedServ || !ValidModuleState() || NPC.ai[1] == 0) return true;
        var pixel = TextureAssets.MagicPixel.Value;
        if (currentModule == 0) {
            for (int row = 0; row < moduleDensity; row++) foreach (float side in new[] { -1f, 1f })
                spriteBatch.Draw(pixel, LockedModulePoint - screenPos + new Vector2(side * (112 + row * 64) - 16, -240), null,
                    Color.Cyan * 0.3f, 0, Vector2.Zero, new Vector2(32, 480), SpriteEffects.None, 0);
            foreach (float side in new[] { -1f, 1f })
                spriteBatch.Draw(pixel, LockedModulePoint - screenPos + new Vector2(side * 80, -240), null,
                    Color.LightGreen * 0.8f, 0, Vector2.Zero, new Vector2(2, 480), SpriteEffects.None, 0);
        } else if (currentModule == 1 && moduleRoute == 2) {
            foreach (float side in new[] { -1f, 1f })
                spriteBatch.Draw(pixel, LockedModulePoint - screenPos + new Vector2(-240, side * 96 - 16), null,
                    Color.OrangeRed * 0.3f, 0, Vector2.Zero, new Vector2(480, 32), SpriteEffects.None, 0);
            foreach (float side in new[] { -1f, 1f })
                spriteBatch.Draw(pixel, LockedModulePoint - screenPos + new Vector2(-240, side * 64), null,
                    Color.LightGreen * 0.8f, 0, Vector2.Zero, new Vector2(480, 2), SpriteEffects.None, 0);
        } else if (currentModule == 1) {
            foreach (float side in new[] { -1f, 1f })
                spriteBatch.Draw(pixel, NPC.Center - screenPos, null, Color.LightGreen * 0.8f,
                    RingDirection + side * RingGapHalfAngle, Vector2.Zero, new Vector2(240, 3), SpriteEffects.None, 0);
            if (moduleRoute == 3) foreach (float side in new[] { -1f, 1f })
                spriteBatch.Draw(pixel, NPC.Center - screenPos, null, Color.LightGreen * 0.8f,
                    RingDirection + MathHelper.TwoPi / 2 + side * RingGapHalfAngle, Vector2.Zero, new Vector2(240, 3), SpriteEffects.None, 0);
        } else {
            spriteBatch.Draw(pixel, LockedModulePoint - screenPos + new Vector2(-48, -48), null,
                Color.OrangeRed * 0.3f, 0, Vector2.Zero, new Vector2(96, 96), SpriteEffects.None, 0);
        }
        spriteBatch.Draw(pixel, NPC.Center - screenPos + new Vector2(-50, -NPC.height / 2f - 12), null,
            Color.Cyan * 0.8f, 0, Vector2.Zero, new Vector2(100 * (ModuleWarningTicks - NPC.ai[1]) / ModuleWarningTicks, 5), SpriteEffects.None, 0);
        return true;
    }
}
