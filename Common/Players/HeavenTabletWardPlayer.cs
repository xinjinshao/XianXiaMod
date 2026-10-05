using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Projectiles;

namespace XianXia.Common.Players;

public class HeavenTabletWardPlayer : ModPlayer
{
    public const int DefenseBonus = 6;
    public override void PostUpdateEquips()
    {
        if (!Player.active || Player.dead || Player.HeldItem.ModItem is not HeavenTabletWardSeal) return;
        int type = ModContent.ProjectileType<HeavenTabletWardProjectile>();
        if (Player.ownedProjectileCounts[type] == 0) return;
        // Recomputed with normal equipment stats on every peer; no persistent resource or buff packet.
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile ward = Main.projectile[i];
            if (!ward.active || ward.type != type || ward.owner != Player.whoAmI
                || Vector2.DistanceSquared(Player.Center, ward.Center) > 160f * 160f
                || !Collision.CanHitLine(Player.Center, 1, 1, ward.Center, 1, 1)) continue;
            Player.statDefense += DefenseBonus;
            return; // More projectiles cannot multiply the bonus.
        }
    }
}
