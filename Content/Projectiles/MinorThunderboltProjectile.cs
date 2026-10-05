using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.Projectiles;

public class MinorThunderboltProjectile : ModProjectile

{
    public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
    {
        if (source is Terraria.DataStructures.EntitySource_Parent parent && parent.Entity is Projectile origin)
            Projectile.DamageType = origin.DamageType == DamageClass.Melee ? DamageClass.Melee : DamageClass.Magic;
    }
    public override void SendExtraAI(System.IO.BinaryWriter writer) => writer.Write(Projectile.DamageType == DamageClass.Melee);
    public override void ReceiveExtraAI(System.IO.BinaryReader reader) => Projectile.DamageType = reader.ReadBoolean() ? DamageClass.Melee : DamageClass.Magic;

    public override void SetDefaults()

    {

        Projectile.width = 24;

        Projectile.height = 80;

        Projectile.friendly = true;

        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = 1;

        Projectile.timeLeft = 180;

        Projectile.tileCollide = true;

        Projectile.ignoreWater = true;



    }



    public override void AI()

    {

        if (Projectile.velocity.LengthSquared() > 0.01f)

            Projectile.rotation = Projectile.velocity.ToRotation();

        Lighting.AddLight(Projectile.Center, 0.06f, 0.18f, 0.2f);

    }



}
