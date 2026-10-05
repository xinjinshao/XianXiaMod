using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class GreenwoodArrayField : ModProjectile

{

    public override void SetDefaults()

    {

        Projectile.width = 96;

        Projectile.height = 96;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;
        Projectile.netImportant = true;



        Projectile.penetrate = -1;

        Projectile.timeLeft = 300;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;

        Projectile.localNPCHitCooldown = 30;

    }





    public override void AI()

    {

        Projectile.velocity = Vector2.Zero;

        Projectile.rotation += 0.02f;

        Player owner = Main.player[Projectile.owner];

        if (Main.netMode != NetmodeID.MultiplayerClient && owner.active && !owner.dead
            && owner.Hitbox.Intersects(Projectile.Hitbox) && Main.GameUpdateCount % 60 == 0
            && owner.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().TryArrayRecovery(Main.GameUpdateCount))

        {

            int healed = Math.Min(1, Math.Max(0, owner.statLifeMax2 - owner.statLife));
            if (healed > 0)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    owner.statLife += healed;
                    NetMessage.SendData(MessageID.SpiritHeal, owner.whoAmI, -1, null, owner.whoAmI, healed);
                }
                else owner.Heal(healed);
            }

            owner.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().RestoreSpiritualEnergy(1);

        }

        Lighting.AddLight(Projectile.Center, 0.05f, 0.24f, 0.12f);

    }



}
