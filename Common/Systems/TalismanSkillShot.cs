using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Items;

namespace XianXia.Common.Systems;

public static class TalismanSkillShot
{
    public static bool TryFire(Player player, Item weapon, Vector2 aim)
    {
        Vector2 position = player.RotatedRelativePoint(player.MountedCenter), delta = aim - position;
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(delta.LengthSquared())) return false;
        Item[] before = player.inventory.Select(item => item.Clone()).ToArray();
        var state = player.GetModPlayer<XianXiaPlayer>();
        bool picked; int type, damage, ammo; float speed, knockback;
        state.ApplyingArtifactAmmo = true;
        try { picked = player.PickAmmo(weapon,out type,out speed,out damage,out knockback,out ammo,false); }
        finally { state.ApplyingArtifactAmmo = false; }
        bool valid = picked && type > 0 && type < ProjectileLoader.ProjectileCount && float.IsFinite(speed) && speed > 0 && float.IsFinite(knockback);
        if (valid) {
            Vector2 velocity = delta.SafeNormalize(Vector2.UnitX * player.direction) * (speed * 1.25f);
            valid = float.IsFinite(velocity.LengthSquared());
            if (valid) {
                int boosted = Math.Max(1,(int)(damage * DaoArtifactRules.TalismanShotMultiplier(RefinedArtifact.ActiveDaoRoute(weapon))));
                int index = Projectile.NewProjectile(new EntitySource_ItemUse_WithAmmo(player,weapon,ammo),position,velocity,type,boosted,knockback,player.whoAmI);
                valid = index >= 0 && index < Main.maxProjectiles;
                if (valid) {
                    Main.projectile[index].netUpdate = true;
                    if (Main.netMode == Terraria.ID.NetmodeID.Server) Terraria.NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile,-1,-1,null,index);
                }
            }
        }
        for (int slot = 0; slot < before.Length; slot++) {
            Item current = player.inventory[slot], original = before[slot];
            if (current.type == original.type && current.stack == original.stack && current.prefix == original.prefix) continue;
            if (!valid) player.inventory[slot] = original;
            AuthoritativeInventorySync.Send(player,slot);
        }
        return valid;
    }
}
