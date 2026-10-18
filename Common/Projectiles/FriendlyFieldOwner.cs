using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace XianXia.Common.Projectiles;

internal static class FriendlyFieldOwner
{
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
    internal static bool HasValidState(Projectile projectile, int lifetime) => projectile != null
        && projectile.timeLeft > 0 && projectile.timeLeft <= lifetime
        && Finite(projectile.Center) && Finite(projectile.velocity);
    internal static Player Find(Projectile projectile, int lifetime, ArtifactProjectileOwnerBinding binding)
    {
        if (!HasValidState(projectile, lifetime) || projectile.owner < 0 || Main.player == null || projectile.owner >= Main.maxPlayers || projectile.owner >= Main.player.Length) return null;
        Player owner = Main.player[projectile.owner];
        if (owner == null || !owner.active || owner.dead || !Finite(owner.Center) || !binding.Matches(projectile,owner)) return null;
        float distance = Vector2.DistanceSquared(owner.Center, projectile.Center);
        return float.IsFinite(distance) && distance <= 1600f * 1600f ? owner : null;
    }
}
