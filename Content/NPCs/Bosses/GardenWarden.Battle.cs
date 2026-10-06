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
using XianXia.Content.Projectiles;

namespace XianXia.Content.NPCs.Bosses;

// Hand-maintained battle hooks; the generated defaults and loot stay in GardenWarden.cs.
public partial class GardenWarden
{
    public const int BriarCastTicks = 60, RecoveryTicks = 50, DashWarningTicks = 36, DashTicks = 24;
    public const float DashLineLength = DashTicks * 14f + 48f;
    public const int MaximumSession = 0x00ffffff; // Exactly representable in projectile float AI.
    private static int nextSession;
    public int HazardSession { get; private set; }
    public int BattleTarget { get; private set; } = -1;
    public bool HasLivingBattleTarget => HazardSession > 0 && NPC.active && NPC.life > 0
        && NPC.target >= 0 && NPC.target < Main.maxPlayers && BattleTarget == NPC.target
        && Main.player[NPC.target].active && !Main.player[NPC.target].dead
        && Finite(NPC.Center) && Finite(Main.player[NPC.target].Center)
        && Vector2.DistanceSquared(NPC.Center, Main.player[NPC.target].Center) <= 4000f * 4000f;
    private const int Chase = 0, Briars = 1, Recovery = 2, DashWarning = 3, Dash = 4, DashRecovery = 5;
    private bool FinalPhase => NPC.life < NPC.lifeMax * 0.35f;
    private bool PhaseTwo => NPC.life < NPC.lifeMax * 0.65f;
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    private void Enter(int state)
    {
        NPC.ai[0] = state; NPC.ai[1] = 0; NPC.netUpdate = true;
    }
    private void RenewSession()
    {
        nextSession = nextSession >= MaximumSession ? 1 : nextSession + 1;
        HazardSession = nextSession; BattleTarget = NPC.target; NPC.netUpdate = true;
    }
    private void CancelBattle()
    {
        NPC.velocity *= 0.9f;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        HazardSession = 0; BattleTarget = -1; Enter(Chase); NPC.EncourageDespawn(30);
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(HazardSession); writer.Write(BattleTarget);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        int session = reader.ReadInt32(), target = reader.ReadInt32();
        HazardSession = session > 0 && session <= MaximumSession ? session : 0;
        BattleTarget = target >= 0 && target < Main.maxPlayers ? target : -1;
    }
    public override void AI()
    {
        if (!EnemyTargeting.TryGetLivingTarget(NPC, out Player target) || !Finite(NPC.Center)
            || !Finite(target.Center) || !Finite(target.velocity)
            || Vector2.DistanceSquared(NPC.Center, target.Center) > 4000f * 4000f)
        { CancelBattle(); return; }
        bool server = Main.netMode != NetmodeID.MultiplayerClient;
        if (server && (HazardSession == 0 || BattleTarget != NPC.target))
        { RenewSession(); Enter(Chase); }
        if (!float.IsFinite(NPC.ai[0]) || NPC.ai[0] != MathF.Truncate(NPC.ai[0])
            || !float.IsFinite(NPC.ai[1]) || NPC.ai[0] < Chase || NPC.ai[0] > DashRecovery)
        { if (server) Enter(Chase); return; }
        NPC.ai[1]++;
        switch ((int)NPC.ai[0])
        {
            case Chase:
                Vector2 perch = target.Center + new Vector2(target.direction >= 0 ? -220 : 220, -120);
                NPC.velocity = Vector2.Lerp(NPC.velocity, (perch - NPC.Center).SafeNormalize(Vector2.UnitY)
                    * (FinalPhase ? 8f : PhaseTwo ? 6f : 4.5f), 0.06f);
                if (!server) break;
                if (NPC.ai[1] % 90 == 0) FireSeeds(target);
                if (NPC.ai[1] >= (FinalPhase ? 110 : PhaseTwo ? 140 : 180)) BeginBriars(target);
                break;
            case Briars:
                NPC.velocity *= 0.88f;
                if (server && NPC.ai[1] >= BriarCastTicks) Enter(Recovery);
                break;
            case Recovery:
                NPC.velocity *= 0.92f;
                if (server && NPC.ai[1] >= RecoveryTicks)
                {
                    if (!FinalPhase) { Enter(Chase); break; }
                    Vector2 aim = target.Center + target.velocity * 8f - NPC.Center;
                    Vector2 locked = NPC.Center + aim.SafeNormalize(Vector2.UnitY) * DashLineLength;
                    NPC.ai[2] = locked.X; NPC.ai[3] = locked.Y; Enter(DashWarning);
                }
                break;
            case DashWarning:
                NPC.velocity *= 0.8f;
                if (server && NPC.ai[1] >= DashWarningTicks)
                {
                    NPC.velocity = (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).SafeNormalize(Vector2.UnitY) * 14f;
                    Enter(Dash);
                }
                break;
            case Dash:
                if (server && NPC.ai[1] >= DashTicks) Enter(DashRecovery);
                break;
            case DashRecovery:
                NPC.velocity *= 0.9f;
                if (server && NPC.ai[1] >= RecoveryTicks) Enter(Chase);
                break;
        }
        NPC.rotation = NPC.velocity.ToRotation();
        if (Main.dedServ) return;
        Lighting.AddLight(NPC.Center, 0.12f, 0.22f, 0.06f);
        if (NPC.localAI[3] != NPC.ai[0])
        {
            NPC.localAI[3] = NPC.ai[0];
            if (NPC.ai[0] == Briars || NPC.ai[0] == DashWarning)
                CombatText.NewText(NPC.Hitbox, Color.LightGreen, Language.GetTextValue(
                    NPC.ai[0] == Briars ? "Mods.XianXia.GardenBattle.Briars" : "Mods.XianXia.GardenBattle.Dash"));
        }
    }
    private void FireSeeds(Player target)
    {
        Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
        for (int i = -1; i <= 1; i++)
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, aim.RotatedBy(i * 0.21f) * 7.5f,
                ModContent.ProjectileType<BossSpiritBoltProjectile>(), Math.Max(8, NPC.damage / 3), 1f, Main.myPlayer);
    }
    private void BeginBriars(Player target)
    {
        // Locked at cast time; two strips leave a 144-pixel central lane. The final
        // phase adds outer strips without filling that lane or following the player.
        Vector2 anchor = target.Bottom + new Vector2(Math.Clamp(target.velocity.X * 8f, -96f, 96f), -24f);
        NPC.ai[2] = anchor.X; NPC.ai[3] = anchor.Y;
        int pairs = FinalPhase ? 2 : 1;
        for (int pair = 1; pair <= pairs; pair++)
            for (int sign = -1; sign <= 1; sign += 2)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), anchor + new Vector2(sign * pair * 112f, 0), Vector2.Zero,
                    ModContent.ProjectileType<GardenBriarPatch>(), Math.Max(8, NPC.damage / 3), 0f, Main.myPlayer,
                    NPC.whoAmI, HazardSession);
        Enter(Briars);
    }
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => HasLivingBattleTarget
        && (NPC.ai[0] == Chase || NPC.ai[0] == Dash)
        && Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || NPC.ai[0] != DashWarning) return true;
        Vector2 delta = new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center;
        if (!Finite(delta)) return true;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, NPC.Center - screenPos, null,
            Color.LightGreen * 0.75f, delta.ToRotation(), Vector2.Zero,
            new Vector2(Math.Min(DashLineLength, delta.Length()), 3f), SpriteEffects.None, 0f);
        return true;
    }
}
