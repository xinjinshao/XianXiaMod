using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

public partial class BrokenHeavenInspector
{
    public const int DecreeWarningTicks = 60, DecreeRecoveryTicks = 45, PuppetSummonQuota = 2;
    private bool decreeFrame, nextReturnDecree;
    private bool decreePuppets, decreeBlade;
    private int decreeTarget = -1, announcedDecree, puppetSummonsCreated;
    private Player decreePlayer;
    private bool ValidDecreeState() => float.IsFinite(NPC.ai[1]) && MathF.Abs(NPC.ai[1]) <= DecreeWarningTicks
        && NPC.ai[1] == MathF.Truncate(NPC.ai[1]) && float.IsFinite(NPC.ai[2])
        && NPC.ai[2] >= -DecreeRecoveryTicks && NPC.ai[2] <= 300 && NPC.ai[2] == MathF.Truncate(NPC.ai[2])
        && (NPC.ai[1] == 0 || (float.IsFinite(NPC.ai[0]) && float.IsFinite(NPC.ai[3])));
    private void CancelDecree()
    {
        decreeFrame = true; NPC.velocity = Vector2.Zero;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        NPC.ai[0] = NPC.ai[1] = NPC.ai[2] = NPC.ai[3] = 0;
        decreeTarget = -1; decreePlayer = null; announcedDecree = 0; decreePuppets = decreeBlade = false; NPC.netUpdate = true;
    }
    private void AnnounceDecree(bool returnToCenter)
    {
        int code = returnToCenter ? -1 : 1;
        if (announcedDecree == code) return;
        announcedDecree = code;
        if (!Main.dedServ && decreeBlade) CombatText.NewText(NPC.Hitbox, Color.OrangeRed,
            Language.GetTextValue("Mods.XianXia.Decrees.VerdictBlade"));
        if (!Main.dedServ) CombatText.NewText(NPC.Hitbox, Color.OrangeRed,
            Language.GetTextValue(returnToCenter ? "Mods.XianXia.Decrees.ReturnToCenter" : "Mods.XianXia.Decrees.LeaveCenter"));
    }
    internal bool UpdateInspectorDecree(Player target, bool phaseTwo, bool finalPhase)
    {
        decreeFrame = false;
        if (!ValidDecreeState()) { CancelDecree(); return true; }
        if ((NPC.ai[1] != 0 || NPC.ai[2] < 0) && Main.netMode != NetmodeID.MultiplayerClient
            && decreeTarget >= 0 && (NPC.target != decreeTarget || !ReferenceEquals(target, decreePlayer))) {
            CancelDecree(); return true;
        }
        if (NPC.ai[2] < 0) {
            decreeFrame = true; NPC.velocity = Vector2.Zero; announcedDecree = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient && ++NPC.ai[2] == 0) {
                NPC.ai[0] = NPC.ai[3] = 0; decreeTarget = -1; decreePlayer = null; decreePuppets = decreeBlade = false; NPC.netUpdate = true;
            }
            return true;
        }
        int interval = finalPhase ? 180 : phaseTwo ? 240 : 300;
        if (NPC.ai[1] == 0 && NPC.ai[2] < interval - DecreeWarningTicks) {
            announcedDecree = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient) NPC.ai[2]++;
            return false;
        }
        decreeFrame = true; NPC.velocity = Vector2.Zero;
        if (NPC.ai[1] == 0 && Main.netMode != NetmodeID.MultiplayerClient) {
            decreeTarget = NPC.target; decreePlayer = target;
            NPC.ai[0] = target.Center.X; NPC.ai[3] = target.Center.Y;
            NPC.ai[1] = nextReturnDecree ? -DecreeWarningTicks : DecreeWarningTicks;
            decreePuppets = phaseTwo && puppetSummonsCreated < PuppetSummonQuota;
            decreeBlade = finalPhase;
            NPC.netUpdate = true;
        }
        if (NPC.ai[1] == 0) return true; // Client waits for the authoritative signed countdown.
        bool returnToCenter = NPC.ai[1] < 0;
        AnnounceDecree(returnToCenter);
        if (Main.netMode == NetmodeID.MultiplayerClient) return true;
        NPC.ai[1] += returnToCenter ? 1 : -1;
        if (NPC.ai[1] != 0) {
            if (NPC.ai[1] % 15 == 0) NPC.netUpdate = true;
            return true;
        }
        foreach (float offset in returnToCenter ? new[] { -112f, 112f } : new[] { 0f })
            Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.ai[0] + offset, NPC.ai[3]), Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorDecreeBeamProjectile>(),
                Math.Max(18, NPC.damage / 3), 1.2f, Main.myPlayer);
        if (decreeBlade)
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorVerdictBladeProjectile>(),
                Math.Max(18, NPC.damage / 3), 1.2f, Main.myPlayer);
        if (decreePuppets) SpawnInspectorPuppets();
        nextReturnDecree = !nextReturnDecree; NPC.ai[2] = -DecreeRecoveryTicks; NPC.netUpdate = true;
        return true;
    }
    internal void SpawnInspectorPuppets()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !BossTargeting.HasLivingTarget(NPC)) return;
        int type = ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.CelestialPuppet>();
        while (puppetSummonsCreated < PuppetSummonQuota) {
            if (!TryPuppetSpawn(out int x, out int y)) break;
            int slot = Terraria.NPC.NewNPC(NPC.GetSource_FromAI(), x, y, type, ai0: NPC.whoAmI);
            if (slot < 0 || slot >= Main.maxNPCs || slot == NPC.whoAmI) break;
            NPC created = Main.npc[slot];
            if (created == null || !created.active || created.life <= 0 || created.type != type) break;
            created.target = NPC.target; created.netUpdate = true; puppetSummonsCreated++;
        }
    }
    private bool TryPuppetSpawn(out int x, out int y)
    {
        x = y = 0;
        float bottom = NPC.Center.Y - 60;
        if (bottom < 64 || bottom > Main.maxTilesY * 16f - 16) return false;
        for (int attempt = 0; attempt < 12; attempt++) {
            float center = NPC.Center.X + Main.rand.Next(-80, 81);
            if (center < 40 || center > Main.maxTilesX * 16f - 40) continue;
            int candidateX = (int)center, candidateY = (int)bottom;
            Vector2 topLeft = new(candidateX - 24, candidateY - 48);
            if (Collision.SolidCollision(topLeft, 48, 48, true) || Collision.LavaCollision(topLeft, 48, 48)) continue;
            x = candidateX; y = candidateY; return true;
        }
        return false;
    }
    public override void SendExtraAI(BinaryWriter writer) { writer.Write((byte)puppetSummonsCreated); writer.Write(decreePuppets); writer.Write(SummonSession); writer.Write(decreeBlade); }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        byte created = reader.ReadByte(); bool warned = reader.ReadBoolean(); long session = reader.ReadInt64(); bool blade = reader.ReadBoolean();
        decreeBlade = blade;
        summonSession = session > 0 ? session : 0;
        puppetSummonsCreated = Math.Min((int)created, PuppetSummonQuota);
        decreePuppets = warned && puppetSummonsCreated < PuppetSummonQuota;
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => BossTargeting.HasLivingTarget(NPC)
        && ValidDecreeState() && !decreeFrame && NPC.ai[1] == 0 && NPC.ai[2] >= 0
        && target.active && !target.dead && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || !ValidDecreeState() || NPC.ai[1] == 0) return true;
        var pixel = TextureAssets.MagicPixel.Value;
        if (decreeBlade)
            spriteBatch.Draw(pixel, NPC.Center - screenPos + new Vector2(-80, -24), null, Color.OrangeRed * 0.4f,
                0, Vector2.Zero, new Vector2(160, 48), SpriteEffects.None, 0);
        if (decreePuppets)
            spriteBatch.Draw(pixel, NPC.Center - screenPos + new Vector2(-104, -108), null, Color.Cyan * 0.3f,
                0, Vector2.Zero, new Vector2(208, 48), SpriteEffects.None, 0);
        foreach (float offset in NPC.ai[1] < 0 ? new[] { -112f, 112f } : new[] { 0f })
            spriteBatch.Draw(pixel, new Vector2(NPC.ai[0] + offset - 32, NPC.ai[3] - 240) - screenPos, null,
                Color.OrangeRed * 0.3f, 0, Vector2.Zero, new Vector2(64, 480), SpriteEffects.None, 0);
        foreach (float side in new[] { -1f, 1f })
            spriteBatch.Draw(pixel, new Vector2(NPC.ai[0] + side * 64, NPC.ai[3] - 240) - screenPos, null,
                Color.LightGreen * 0.8f, 0, Vector2.Zero, new Vector2(2, 480), SpriteEffects.None, 0);
        spriteBatch.Draw(pixel, NPC.Center - screenPos + new Vector2(-50, -NPC.height / 2f - 12), null,
            Color.OrangeRed * 0.8f, 0, Vector2.Zero,
            new Vector2(100 * (DecreeWarningTicks - MathF.Abs(NPC.ai[1])) / DecreeWarningTicks, 5), SpriteEffects.None, 0);
        return true;
    }
}
