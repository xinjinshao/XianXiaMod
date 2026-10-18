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
        for (int i = 0; Main.projectile != null && i < System.Math.Min(Main.maxProjectiles,Main.projectile.Length); i++)
        {
            Projectile ward = Main.projectile[i];
            if (ward == null || !ward.active || ward.type != type || ward.owner != Player.whoAmI
                || ward.ModProjectile is not HeavenTabletWardProjectile logic || !logic.CanProvideWard(Player)) continue;
            Player.statDefense += DefenseBonus;
            return; // More projectiles cannot multiply the bonus.
        }
    }
}
