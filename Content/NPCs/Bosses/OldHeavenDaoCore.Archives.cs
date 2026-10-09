using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Content.NPCs.Bosses;

public partial class OldHeavenDaoCore
{
    public const int ArchiveWarningTicks = 60, ArchiveShieldTicks = 1800, ArchiveRecoveryTicks = 45;
    private static long nextArchiveSession;
    private long archiveSession;
    private byte archiveState, brokenArchiveMask;
    private short archiveTimer;
    private readonly short[] archiveSlots = { -1, -1, -1, -1 };
    private readonly CoreArchiveLockNPC[] archiveEntities = new CoreArchiveLockNPC[4];
    private Player archivePlayer;
    private int archiveTarget = -1;
    private bool archiveFrame, archiveAnnounced;
    internal long ArchiveSession {
        get {
            if (archiveSession == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                archiveSession = Interlocked.Increment(ref nextArchiveSession);
            return archiveSession;
        }
    }
    internal bool IsActiveArchive(CoreArchiveLockNPC archive) => archiveState == 2 && archive.Index < 4
        && (brokenArchiveMask & (1 << archive.Index)) == 0 && BossTargeting.HasLivingTarget(NPC)
        && archive.MatchesParent(this, archive.Index) && archiveSlots[archive.Index] == archive.NPC.whoAmI
        && (Main.netMode == NetmodeID.MultiplayerClient || ReferenceEquals(archiveEntities[archive.Index], archive));
    internal void BreakArchive(CoreArchiveLockNPC archive)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !IsActiveArchive(archive)) return;
        brokenArchiveMask |= (byte)(1 << archive.Index); NPC.netUpdate = true;
        if (brokenArchiveMask == 15) FinishArchives();
    }
    private void RemoveArchiveEntities()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        foreach (CoreArchiveLockNPC archive in archiveEntities) {
            if (archive == null || !ReferenceEquals(archive.NPC.ModNPC, archive) || !archive.NPC.active) continue;
            archive.NPC.damage = 0; archive.NPC.active = false;
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: archive.NPC.whoAmI);
        }
    }
    private void FinishArchives()
    {
        RemoveArchiveEntities(); archiveState = 3; archiveTimer = ArchiveRecoveryTicks; archivePlayer = null;
        NPC.dontTakeDamage = NPC.immortal = false; NPC.velocity = Vector2.Zero;
        NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0; NPC.netUpdate = true;
        moduleTarget = -1; modulePlayer = null;
    }
    private void AbandonArchives()
    {
        RemoveArchiveEntities(); archiveState = 3; archiveTimer = 0; archivePlayer = null;
        NPC.dontTakeDamage = NPC.immortal = false;
    }
    private void AnnounceArchives()
    {
        if (archiveAnnounced) return;
        archiveAnnounced = true;
        if (!Main.dedServ) CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Archives.BreakLocks"));
    }
    internal bool UpdateCoreArchives(Player target, bool phaseTwo, bool finalPhase)
    {
        archiveFrame = false;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            if (archiveState == 1) AnnounceArchives();
            NPC.dontTakeDamage = NPC.immortal = archiveState == 2;
            archiveFrame = archiveState == 1 || (archiveState == 3 && archiveTimer > 0);
            if (archiveFrame) NPC.velocity = Vector2.Zero;
            return archiveFrame;
        }
        if (archiveState == 0 && finalPhase && NPC.ai[1] == 0 && NPC.ai[2] >= 0) {
            archiveState = 1; archiveTimer = ArchiveWarningTicks; archivePlayer = target; archiveTarget = NPC.target; archiveFrame = true;
            NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0; NPC.netUpdate = true;
        }
        if (archiveState == 1) {
            AnnounceArchives();
            archiveFrame = true; NPC.velocity = Vector2.Zero; NPC.dontTakeDamage = NPC.immortal = false;
            if (NPC.target != archiveTarget || !ReferenceEquals(target, archivePlayer)) { FinishArchives(); return true; }
            if (--archiveTimer > 0) { if (archiveTimer % 15 == 0) NPC.netUpdate = true; return true; }
            if (!CreateArchives()) { FinishArchives(); return true; }
            archiveState = 2; archiveTimer = ArchiveShieldTicks; NPC.dontTakeDamage = NPC.immortal = true; NPC.netUpdate = true;
            return true;
        }
        if (archiveState == 2) {
            bool intact = brokenArchiveMask < 15;
            for (int order = 0; order < 4 && intact; order++) {
                if ((brokenArchiveMask & (1 << order)) != 0) continue;
                CoreArchiveLockNPC archive = archiveEntities[order];
                intact = archive != null && archive.NPC.active && archive.NPC.life > 0
                    && ReferenceEquals(archive.NPC.ModNPC, archive) && archive.MatchesParent(this, order);
            }
            if (!intact || --archiveTimer <= 0) { archiveFrame = true; FinishArchives(); return true; }
            NPC.dontTakeDamage = NPC.immortal = true;
            if (archiveTimer % 60 == 0) NPC.netUpdate = true;
            return false; // Route modules continue while the player breaks any remaining archive lock.
        }
        if (archiveState == 3 && archiveTimer > 0) {
            archiveFrame = true; NPC.velocity = Vector2.Zero; NPC.dontTakeDamage = NPC.immortal = false;
            if (--archiveTimer == 0) NPC.netUpdate = true;
            return true;
        }
        return false;
    }
    private bool CreateArchives()
    {
        // Check all four full bodies first; never enable a shield with inaccessible spawn points.
        var centers = new Vector2[4];
        for (int order = 0; order < 4; order++) {
            Vector2 offset = order switch { 0 => new(0, -160), 1 => new(160, 0), 2 => new(0, 160), _ => new(-160, 0) };
            Vector2 center = NPC.Center + offset; centers[order] = center;
            if (!float.IsFinite(center.X) || !float.IsFinite(center.Y) || center.X < 36 || center.Y < 36
                || center.X > Main.maxTilesX * 16f - 36 || center.Y > Main.maxTilesY * 16f - 36
                || Collision.SolidCollision(center - new Vector2(20, 20), 40, 40, true)
                || Collision.LavaCollision(center - new Vector2(20, 20), 40, 40)) return false;
        }
        for (int order = 0; order < 4; order++) {
            int slot = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(), (int)centers[order].X, (int)centers[order].Y + 20,
                ModContent.NPCType<CoreArchiveLockNPC>(), ai0: NPC.whoAmI, ai1: order);
            if (slot < 0 || slot >= Main.maxNPCs || slot == NPC.whoAmI) return false;
            NPC created = Main.npc[slot];
            if (created?.ModNPC is not CoreArchiveLockNPC archive || !created.active || created.life <= 0
                || !archive.MatchesParent(this, order)) return false;
            archiveSlots[order] = (short)slot; archiveEntities[order] = archive;
            created.target = NPC.target; created.netUpdate = true;
        }
        return true;
    }
    private void WriteArchiveState(BinaryWriter writer)
    {
        writer.Write(ArchiveSession); writer.Write(archiveState); writer.Write(archiveTimer); writer.Write(brokenArchiveMask);
        foreach (short slot in archiveSlots) writer.Write(slot);
    }
    private void ReadCoreState(BinaryReader reader)
    {
        // Atomically read all five module bytes and twenty archive bytes.
        byte current = reader.ReadByte(), next = reader.ReadByte(), density = reader.ReadByte(), route = reader.ReadByte(), cycle = reader.ReadByte();
        long session = reader.ReadInt64(); byte state = reader.ReadByte(); short timer = reader.ReadInt16(); byte mask = reader.ReadByte();
        var slots = new short[4]; for (int i = 0; i < 4; i++) slots[i] = reader.ReadInt16();
        bool valid = current <= 2 && next <= 2 && density >= 1 && density <= 3 && route <= 3 && cycle <= 2
            && state <= 3 && mask <= 15 && timer >= 0 && timer <= ArchiveShieldTicks
            && (state != 1 || (timer > 0 && timer <= ArchiveWarningTicks))
            && (state != 3 || timer <= ArchiveRecoveryTicks);
        if (state == 2) {
            valid &= session > 0 && mask < 15 && timer > 0;
            for (int i = 0; i < 4; i++) {
                valid &= slots[i] >= 0 && slots[i] < Main.maxNPCs && (NPC == null || slots[i] != NPC.whoAmI);
                for (int j = 0; j < i; j++) valid &= slots[i] != slots[j];
            }
        }
        invalidModulePacket = current > 2 || next > 2 || density < 1 || density > 3 || route > 3 || cycle > 2;
        currentModule = current <= 2 ? current : (byte)0; nextModule = next <= 2 ? next : (byte)0;
        moduleDensity = density >= 1 && density <= 3 ? density : (byte)1; moduleRoute = route <= 3 ? route : (byte)0; routeCycle = cycle <= 2 ? cycle : (byte)0;
        archiveSession = session > 0 ? session : 0; archiveState = valid ? state : (byte)3;
        archiveTimer = valid ? timer : (short)0; brokenArchiveMask = valid ? mask : (byte)15;
        for (int i = 0; i < 4; i++) archiveSlots[i] = slots[i] >= 0 && slots[i] < Main.maxNPCs ? slots[i] : (short)-1;
        if (NPC != null) NPC.dontTakeDamage = NPC.immortal = valid && state == 2;
    }
    private void DrawArchiveWarning(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (Main.dedServ || archiveState != 1 || archiveTimer < 0 || archiveTimer > ArchiveWarningTicks
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return;
        var pixel = TextureAssets.MagicPixel.Value;
        foreach (Vector2 offset in new[] { new Vector2(0, -160), new Vector2(160, 0), new Vector2(0, 160), new Vector2(-160, 0) })
            spriteBatch.Draw(pixel, NPC.Center + offset - screenPos - new Vector2(20, 20), null,
                Color.Cyan * 0.3f, 0, Vector2.Zero, new Vector2(40, 40), SpriteEffects.None, 0);
        spriteBatch.Draw(pixel, NPC.Center - screenPos + new Vector2(-50, -NPC.height / 2f - 12), null,
            Color.Cyan * 0.8f, 0, Vector2.Zero, new Vector2(100 * (ArchiveWarningTicks - archiveTimer) / (float)ArchiveWarningTicks, 5), SpriteEffects.None, 0);
    }
}
