using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public static class WeaponShotTransactions
{
    private static bool Finite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);

    public static void HandleRequest(Player player, int slot, int itemType, Vector2 aim)
    {
        var state = player.GetModPlayer<XianXiaPlayer>();
        if (Main.netMode != NetmodeID.Server || !player.active || player.dead || player.noItems || player.CCed
            || !state.NetworkInitialized || state.WeaponShotCooldown > 0
            || slot < 0 || slot >= player.inventory.Length || slot != player.selectedItem || !Finite(aim)) return;
        Item item = player.inventory[slot];
        if (item.type != itemType || item.stack <= 0 || item.ModItem is not CultivationWeaponItem weapon) return;
        state.WeaponShotCooldown = CombinedHooks.TotalUseTime(item.useTime, player, item);
        if (CombinedHooks.CanUseItem(player, item) && CombinedHooks.CanShoot(player, item))
        {
            Vector2 position = player.RotatedRelativePoint(player.MountedCenter);
            Vector2 delta = aim - position;
            if (!Finite(delta) || !float.IsFinite(delta.LengthSquared())) return;
            Vector2 velocity = delta.SafeNormalize(Vector2.UnitX * player.direction) * item.shootSpeed;
            int type = item.shoot;
            int damage = player.GetWeaponDamage(item);
            float knockback = player.GetWeaponKnockback(item, item.knockBack);
            CombinedHooks.ModifyShootStats(player, item, ref position, ref velocity, ref type, ref damage, ref knockback);
            FirePrepared(player, weapon, new EntitySource_ItemUse_WithAmmo(player, item, 0), position, velocity,
                type, damage, knockback, true);
        }
        state.SyncPlayer(player.whoAmI, -1, false);
    }

    public static bool FirePrepared(Player player, CultivationWeaponItem weapon, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback, bool invokeShootHooks)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || player.noItems || player.CCed
            || !Finite(position) || !Finite(velocity) || type <= 0 || type >= ProjectileLoader.ProjectileCount
            || CountActive() >= Main.maxProjectiles) return false;
        XianXiaPlayer state = player.GetModPlayer<XianXiaPlayer>();
        CultivationSnapshot before = state.CaptureSnapshot();
        int cost = weapon.GetSpiritCost(player);
        if (weapon.DeploysArray ? !state.TryDeployArray(type, cost) : !state.TryConsumeSpiritualEnergy(cost)) return false;
        int[] identities = new int[Main.maxProjectiles];
        for (int i = 0; i < Main.maxProjectiles; i++)
            identities[i] = Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI
                ? Main.projectile[i].identity : int.MinValue;
        bool spawnDefault = true;
        if (invokeShootHooks)
        {
            state.ApplyingWeaponShot = true;
            try { spawnDefault = CombinedHooks.Shoot(player, weapon.Item, source, position, velocity, type, damage, knockback); }
            finally { state.ApplyingWeaponShot = false; }
        }
        if (spawnDefault)
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        bool created = false;
        for (int i = 0; i < Main.maxProjectiles; i++)
            if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI
                && Main.projectile[i].identity != identities[i]) created = true;
        if (!created)
        {
            state.ApplySnapshot(before);
            return false;
        }
        state.AdvanceResourceRevision();
        return true;
    }

    private static int CountActive()
    {
        int count = 0;
        for (int i = 0; i < Main.maxProjectiles; i++) if (Main.projectile[i].active) count++;
        return count;
    }

}
