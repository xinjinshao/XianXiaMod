using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Content.NPCs.Bosses;

public partial class HeavenTabletGuardian
{
    public const int SealWarningTicks = 45, SealShieldTicks = 1800, SealRecoveryTicks = 45;
    private static long nextSealSession;
    private long sealSession;
    private byte sealState, nextSeal;
    private short sealTimer;
    private readonly short[] sealSlots = { -1, -1, -1, -1 };
    private readonly HeavenTabletSealNPC[] sealEntities = new HeavenTabletSealNPC[4];
    private Player sealPlayer;
    private bool sealFrame;
    internal long SealSession {
        get {
            if (sealSession == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                sealSession = Interlocked.Increment(ref nextSealSession);
            return sealSession;
        }
    }
    internal bool IsCurrentSeal(HeavenTabletSealNPC seal) => sealState == 2 && nextSeal < 4
        && BossTargeting.HasLivingTarget(NPC) && seal.MatchesParent(this, nextSeal)
        && sealSlots[nextSeal] == seal.NPC.whoAmI
        && (Main.netMode == NetmodeID.MultiplayerClient || ReferenceEquals(sealEntities[nextSeal], seal));

    internal void BreakSeal(HeavenTabletSealNPC seal)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !IsCurrentSeal(seal)) return;
        nextSeal++; NPC.netUpdate = true;
        if (nextSeal == 4) FinishSeals();
    }
    private void RemoveSealEntities()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        foreach (HeavenTabletSealNPC seal in sealEntities) {
            if (seal == null || !ReferenceEquals(seal.NPC.ModNPC, seal) || !seal.NPC.active) continue;
            seal.NPC.damage = 0; seal.NPC.active = false;
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, number: seal.NPC.whoAmI);
        }
    }
    private void FinishSeals()
    {
        RemoveSealEntities(); sealState = 3; sealTimer = SealRecoveryTicks; sealPlayer = null;
        NPC.dontTakeDamage = false; NPC.velocity = Vector2.Zero;
        NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0; NPC.netUpdate = true;
        judgmentTarget = -1; judgmentPlayer = null;
    }
    private void AbandonSeals()
    {
        RemoveSealEntities(); sealState = 3; sealTimer = 0; sealPlayer = null;
        NPC.dontTakeDamage = false;
    }
    internal bool UpdateTabletSeals(Player target, bool phaseTwo, bool finalPhase)
    {
        sealFrame = false;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NPC.dontTakeDamage = sealState == 2;
            sealFrame = sealState == 1 || (sealState == 3 && sealTimer > 0);
            if (sealFrame) NPC.velocity = Vector2.Zero;
            return sealFrame;
        }
        if (sealState == 0 && phaseTwo && !finalPhase && NPC.ai[1] == 0 && NPC.ai[2] >= 0) {
            sealState = 1; sealTimer = SealWarningTicks; sealPlayer = target; sealFrame = true;
            NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0; NPC.netUpdate = true;
        }
        if (sealState == 1) {
            sealFrame = true; NPC.velocity = Vector2.Zero; NPC.dontTakeDamage = false;
            if (!ReferenceEquals(target, sealPlayer)) { FinishSeals(); return true; }
            if (--sealTimer > 0) { if (sealTimer % 15 == 0) NPC.netUpdate = true; return true; }
            if (!CreateSeals()) { FinishSeals(); return true; }
            sealState = 2; sealTimer = SealShieldTicks; NPC.dontTakeDamage = true; NPC.netUpdate = true;
            return true;
        }
        if (sealState == 2) {
            bool intact = nextSeal < 4;
            for (int order = nextSeal; order < 4 && intact; order++) {
                HeavenTabletSealNPC seal = sealEntities[order];
                intact = seal != null && seal.NPC.active && seal.NPC.life > 0
                    && ReferenceEquals(seal.NPC.ModNPC, seal) && seal.MatchesParent(this, order);
            }
            if (!intact || --sealTimer <= 0) { sealFrame = true; FinishSeals(); return true; }
            NPC.dontTakeDamage = true;
            if (sealTimer % 60 == 0) NPC.netUpdate = true;
            return false; // Judgment continues while the player breaks the ordered seals.
        }
        if (sealState == 3 && sealTimer > 0) {
            sealFrame = true; NPC.velocity = Vector2.Zero; NPC.dontTakeDamage = false;
            if (--sealTimer == 0) NPC.netUpdate = true;
            return true;
        }
        return false;
    }
    private bool CreateSeals()
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
                ModContent.NPCType<HeavenTabletSealNPC>(), ai0: NPC.whoAmI, ai1: order);
            if (slot < 0 || slot >= Main.maxNPCs || slot == NPC.whoAmI) return false;
            NPC created = Main.npc[slot];
            if (created?.ModNPC is not HeavenTabletSealNPC seal || !created.active || created.life <= 0
                || !seal.MatchesParent(this, order)) return false;
            sealSlots[order] = (short)slot; sealEntities[order] = seal;
            created.target = NPC.target; created.netUpdate = true;
        }
        return true;
    }
    private void WriteSealState(BinaryWriter writer)
    {
        writer.Write(SealSession); writer.Write(sealState); writer.Write(sealTimer); writer.Write(nextSeal);
        foreach (short slot in sealSlots) writer.Write(slot);
    }
    private void ReadTabletState(BinaryReader reader)
    {
        // Read the whole 21-byte packet before changing either judgment or seal state.
        byte rows = reader.ReadByte(); long session = reader.ReadInt64(); byte state = reader.ReadByte();
        short timer = reader.ReadInt16(); byte next = reader.ReadByte(); var slots = new short[4];
        for (int i = 0; i < 4; i++) slots[i] = reader.ReadInt16();
        bool valid = state <= 3 && next <= 4 && timer >= 0 && timer <= SealShieldTicks
            && (state != 1 || (timer > 0 && timer <= SealWarningTicks))
            && (state != 3 || timer <= SealRecoveryTicks);
        if (state == 2) {
            valid &= session > 0 && next < 4 && timer > 0;
            for (int i = 0; i < 4; i++) {
                valid &= slots[i] >= 0 && slots[i] < Main.maxNPCs && slots[i] != NPC.whoAmI;
                for (int j = 0; j < i; j++) valid &= slots[i] != slots[j];
            }
        }
        judgmentRows = rows; sealSession = session > 0 ? session : 0;
        sealState = valid ? state : (byte)3; sealTimer = valid ? timer : (short)0; nextSeal = valid ? next : (byte)4;
        for (int i = 0; i < 4; i++) sealSlots[i] = slots[i] >= 0 && slots[i] < Main.maxNPCs ? slots[i] : (short)-1;
        NPC.dontTakeDamage = valid && state == 2;
    }
    private void DrawSealWarning(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (Main.dedServ || sealState != 1 || sealTimer < 0 || sealTimer > SealWarningTicks
            || !float.IsFinite(NPC.Center.X) || !float.IsFinite(NPC.Center.Y)) return;
        var pixel = TextureAssets.MagicPixel.Value;
        foreach (Vector2 offset in new[] { new Vector2(0, -160), new Vector2(160, 0), new Vector2(0, 160), new Vector2(-160, 0) })
            spriteBatch.Draw(pixel, NPC.Center + offset - screenPos - new Vector2(20, 20), null,
                Color.Cyan * 0.3f, 0, Vector2.Zero, new Vector2(40, 40), SpriteEffects.None, 0);
        spriteBatch.Draw(pixel, NPC.Center - screenPos + new Vector2(-50, -NPC.height / 2f - 12), null,
            Color.Cyan * 0.8f, 0, Vector2.Zero, new Vector2(100 * (SealWarningTicks - sealTimer) / (float)SealWarningTicks, 5), SpriteEffects.None, 0);
    }
}
