using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Content.Projectiles;

public class GardenBriarPatch : ModProjectile
{
    public override string Texture => "XianXia/Content/Projectiles/GreenwoodArrayField";
    public const int Lifetime = 150, WarningTicks = 45, FadeTicks = 15;
    private bool invalidState;
    private bool ValidGeometry() => float.IsFinite(Projectile.Center.X) && float.IsFinite(Projectile.Center.Y)
        && float.IsFinite(Projectile.position.X) && float.IsFinite(Projectile.position.Y);
    public override void SetDefaults()
    {
        Projectile.width = 80; Projectile.height = 48;
        Projectile.hostile = true; Projectile.friendly = false;
        Projectile.penetrate = -1; Projectile.timeLeft = Lifetime;
        Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.netImportant = true;
    }
    private bool SourceAlive()
    {
        if (invalidState || !ValidGeometry()) return false;
        int index = (int)Projectile.ai[0], session = (int)Projectile.ai[1];
        if (index < 0 || index >= Main.maxNPCs || index != Projectile.ai[0] || session <= 0 || session != Projectile.ai[1]) return false;
        NPC npc = Main.npc[index];
        return npc != null && npc.ModNPC is GardenWarden warden && warden.HazardSession == session && warden.HasLivingBattleTarget;
    }
    public override void AI()
    {
        invalidState |= Projectile.timeLeft < 0 || Projectile.timeLeft > Lifetime || !ValidGeometry();
        Projectile.velocity = Vector2.Zero;
        if (SourceAlive()) {
            if (Main.netMode != NetmodeID.MultiplayerClient
                && (Projectile.timeLeft == Lifetime - WarningTicks || Projectile.timeLeft == FadeTicks)) Projectile.netUpdate = true;
            return;
        }
        if (Main.netMode != NetmodeID.MultiplayerClient) Projectile.Kill();
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        invalidState |= Projectile.timeLeft < 0 || Projectile.timeLeft > Lifetime || !ValidGeometry();
        writer.Write((short)(invalidState ? -1 : Projectile.timeLeft));
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        int remaining = reader.ReadInt16();
        invalidState |= remaining < 0 || remaining > Lifetime;
        Projectile.timeLeft = remaining >= 0 && remaining <= Lifetime ? remaining : 0;
    }
    public override bool? CanDamage() => SourceAlive() && Projectile.timeLeft > FadeTicks
        && Projectile.timeLeft <= Lifetime - WarningTicks;
    public override bool CanHitPlayer(Player target) => CanDamage() == true && target.active && !target.dead
        && float.IsFinite(target.Center.X) && float.IsFinite(target.Center.Y)
        && Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1);
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        // Follow the native player-hit/buff path; clients resolve their own hurt events.
        if (CanDamage() == true && CanHitPlayer(target)) target.AddBuff(BuffID.Poisoned, 120);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ || !SourceAlive()) return false;
        bool active = CanDamage() == true;
        Color color = Color.LightGreen * (Projectile.timeLeft <= FadeTicks ? 0.15f : active ? 0.8f : 0.5f);
        Rectangle box = new((int)(Projectile.position.X - Main.screenPosition.X),
            (int)(Projectile.position.Y - Main.screenPosition.Y), Projectile.width, Projectile.height);
        // The border matches the real hitbox, with a translucent fill only while active.
        var pixel = TextureAssets.MagicPixel.Value;
        if (active) Main.spriteBatch.Draw(pixel, box, color * 0.25f);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y, box.Width, 2), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y + box.Height - 2, box.Width, 2), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X, box.Y, 2, box.Height), color);
        Main.spriteBatch.Draw(pixel, new Rectangle(box.X + box.Width - 2, box.Y, 2, box.Height), color);
        return false;
    }
}
