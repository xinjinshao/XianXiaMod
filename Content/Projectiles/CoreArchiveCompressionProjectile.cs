using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using XianXia.Common.Systems;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Content.Projectiles;

public class CoreArchiveCompressionProjectile : ModProjectile
{
    public const int WarningTicks = 60, ShrinkTicks = 600, CancellationFade = 6;
    public const int Lifetime = OldHeavenDaoCore.ArchiveShieldTicks + WarningTicks;
    public const int MinimumHalfSize = 320, MaximumHalfSize = 4160, BodySize = 32;
    private short parentSlot = -1;
    private long parentSession;
    private float initialHalfSize = 480;
    private int seenRemaining = Lifetime;
    private bool cancelled, announced;
    private Player[] entrants;
    private int[] entrantAges, entryWaits;
    public override string Texture => "XianXia/Content/Projectiles/SpiritBolt";
    public override void SetDefaults()
    {
        // A large physical hitbox is killed by vanilla world-boundary checks. Colliding supplies the hazard region.
        Projectile.width = Projectile.height = BodySize;
        Projectile.hostile = true; Projectile.friendly = false; Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime; Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.netImportant = true; Projectile.aiStyle = -1;
        parentSlot = -1; parentSession = 0; initialHalfSize = 480; seenRemaining = Lifetime; cancelled = announced = false;
        entrants = new Player[Main.maxPlayers]; entrantAges = new int[Main.maxPlayers]; entryWaits = new int[Main.maxPlayers];
    }
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    private OldHeavenDaoCore Parent => parentSlot >= 0 && parentSlot < Main.maxNPCs
        && Main.npc[parentSlot]?.ModNPC is OldHeavenDaoCore core && parentSession > 0
        && core.ArchiveSession == parentSession && core.ArchiveShieldActive && BossTargeting.HasLivingTarget(Main.npc[parentSlot]) ? core : null;
    private bool ValidSource => Parent is OldHeavenDaoCore core && Vector2.DistanceSquared(Projectile.Center, core.NPC.Center) <= 4;
    private bool ObserveState()
    {
        if (!Finite(Projectile.Center) || Projectile.velocity != Vector2.Zero || Projectile.width != BodySize || Projectile.height != BodySize
            || !float.IsFinite(initialHalfSize) || initialHalfSize < 480 || initialHalfSize > MaximumHalfSize
            || Projectile.timeLeft <= 0 || Projectile.timeLeft > Lifetime) cancelled = true;
        seenRemaining = Math.Min(seenRemaining, Projectile.timeLeft);
        return !cancelled;
    }
    internal float SafeHalfSize => initialHalfSize + (MinimumHalfSize - initialHalfSize)
        * Math.Clamp((Lifetime - Math.Min(seenRemaining, Projectile.timeLeft) - WarningTicks) / (float)ShrinkTicks, 0, 1);
    public override void OnSpawn(IEntitySource source)
    {
        if (source is not EntitySource_Parent { Entity: NPC npc } || npc.ModNPC is not OldHeavenDaoCore core) return;
        parentSlot = npc.whoAmI >= 0 && npc.whoAmI < Main.maxNPCs ? (short)npc.whoAmI : (short)-1;
        parentSession = core.ArchiveSession;
        // Include every living participant already in the encounter, rather than trapping the current target alone.
        for (int index = 0; index < Main.maxPlayers; index++) {
            Player player = Main.player[index];
            if (player == null || !player.active || player.dead || !Finite(player.Center) || !Finite(player.velocity)
                || Vector2.DistanceSquared(player.Center, npc.Center) > BossTargeting.MaximumDistance * BossTargeting.MaximumDistance) continue;
            entrants[index] = player; entrantAges[index] = entryWaits[index] = WarningTicks;
            float extent = MathF.Max(MathF.Abs(player.Center.X - npc.Center.X), MathF.Abs(player.Center.Y - npc.Center.Y)) + 160;
            initialHalfSize = Math.Clamp(MathF.Max(initialHalfSize, extent), 480, MaximumHalfSize);
        }
    }
    private bool Outside(Rectangle box)
    {
        float half = SafeHalfSize;
        return box.Width > 0 && box.Height > 0 && (box.X < Projectile.Center.X - half || box.Y < Projectile.Center.Y - half
            || (long)box.X + box.Width > Projectile.Center.X + half || (long)box.Y + box.Height > Projectile.Center.Y + half);
    }
    private bool ReadyEntrant(Player target)
    {
        if (entrants == null) return false;
        for (int index = 0; index < entrants.Length; index++)
            if (ReferenceEquals(entrants[index], target) && entrantAges[index] >= entryWaits[index]) return true;
        return false;
    }
    private void TrackEntrants()
    {
        bool valid = ValidSource && !cancelled;
        for (int index = 0; index < entrants.Length; index++) {
            Player player = Main.player[index];
            if (!valid || player == null || !player.active || player.dead || !Finite(player.Center) || !Finite(player.velocity)
                || Vector2.DistanceSquared(player.Center, Projectile.Center) > BossTargeting.MaximumDistance * BossTargeting.MaximumDistance) {
                entrants[index] = null; entrantAges[index] = 0; continue;
            }
            if (!ReferenceEquals(entrants[index], player)) {
                entrants[index] = player; entrantAges[index] = 1;
                // Allow a distant late entrant to approach the final safe area at six pixels per tick, plus the full tell.
                float travel = MathF.Max(0, MathF.Max(MathF.Abs(player.Center.X - Projectile.Center.X), MathF.Abs(player.Center.Y - Projectile.Center.Y)) + 64 - MinimumHalfSize);
                entryWaits[index] = Math.Clamp(WarningTicks + (int)MathF.Ceiling(travel / 6), WarningTicks, 720);
                if (!Main.dedServ && ReferenceEquals(player, Main.LocalPlayer)) announced = false;
            } else entrantAges[index] = Math.Min(entryWaits[index], entrantAges[index] + 1);
        }
        if (!Main.dedServ && valid && !announced && Main.LocalPlayer is Player viewer && viewer.active && !viewer.dead
            && Finite(viewer.Center) && Vector2.DistanceSquared(viewer.Center, Projectile.Center) <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance) {
            announced = true; CombatText.NewText(viewer.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Archives.Compression"));
        }
    }
    public override bool? CanDamage() => ObserveState() && ValidSource && Lifetime - seenRemaining >= WarningTicks;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CanDamage() == true && Outside(targetHitbox);
    public override bool? CanHitNPC(NPC target) => false;
    public override bool CanHitPlayer(Player target) => CanDamage() == true && target.active && !target.dead
        && Finite(target.Center) && Finite(target.velocity) && ReadyEntrant(target) && Outside(target.Hitbox)
        && Vector2.DistanceSquared(target.Center, Projectile.Center) <= BossTargeting.MaximumDistance * BossTargeting.MaximumDistance
        && Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1);
    public override void AI()
    {
        ObserveState(); Projectile.velocity = Vector2.Zero; TrackEntrants();
        if (Main.netMode != NetmodeID.MultiplayerClient && (!ValidSource || cancelled)) {
            bool changed = !cancelled || Projectile.timeLeft > CancellationFade;
            cancelled = true; if (Projectile.timeLeft > CancellationFade) Projectile.timeLeft = CancellationFade;
            if (changed) Projectile.netUpdate = true;
        } else if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft % 60 == 0) Projectile.netUpdate = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        ObserveState(); writer.Write(parentSlot); writer.Write(parentSession); writer.Write(initialHalfSize);
        writer.Write((short)Math.Clamp(Math.Min(seenRemaining, Projectile.timeLeft), 0, Lifetime)); writer.Write(cancelled);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        short slot = reader.ReadInt16(); long session = reader.ReadInt64(); float half = reader.ReadSingle(); short remaining = reader.ReadInt16(); bool cancel = reader.ReadBoolean();
        bool valid = (parentSession == 0 || (session == parentSession && slot == parentSlot)) && slot >= 0 && slot < Main.maxNPCs && session > 0 && float.IsFinite(half) && half >= 480 && half <= MaximumHalfSize
            && remaining > 0 && remaining <= Lifetime;
        parentSlot = valid ? slot : (short)-1; parentSession = valid ? session : 0; initialHalfSize = valid ? half : 480;
        cancelled |= cancel || !valid; seenRemaining = Math.Min(seenRemaining, valid ? remaining : 0);
        Projectile.timeLeft = Math.Min(Projectile.timeLeft, seenRemaining);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ || !Finite(Projectile.Center) || Projectile.timeLeft <= 0) return false;
        int half = (int)MathF.Floor(SafeHalfSize), size = half * 2;
        int x = (int)(Projectile.Center.X - Main.screenPosition.X) - half, y = (int)(Projectile.Center.Y - Main.screenPosition.Y) - half;
        Color color = (CanDamage() == true && ReadyEntrant(Main.LocalPlayer) ? Color.OrangeRed : Color.Cyan) * (cancelled ? 0.15f : 0.7f);
        var pixel = TextureAssets.MagicPixel.Value;
        Main.spriteBatch.Draw(pixel, new Rectangle(x, y, size, 4), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(x, y + size - 4, size, 4), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(x, y, 4, size), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(x + size - 4, y, 4, size), color);
        return false;
    }
}
