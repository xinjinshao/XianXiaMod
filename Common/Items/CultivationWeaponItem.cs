using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

public abstract class CultivationWeaponItem : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public abstract int GetSpiritCost(Player player);
    public virtual bool DeploysArray => false;
    public override bool AltFunctionUse(Player player) => Common.Items.RefinedArtifact.IsAwakened(Item);

    public sealed override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (Main.netMode != NetmodeID.Server && player.altFunctionUse == 2
            && ArtifactSkillTransactions.HeldSkill(Item) is ArtifactSkill skill) {
            ArtifactSkillTransactions.Request(player, skill); return false;
        }
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            if (player.whoAmI == Main.myPlayer && player.HeldItem == Item)
            {
                var packet = Mod.GetPacket();
                packet.Write((byte)8);
                packet.Write((byte)player.selectedItem);
                packet.Write(Item.type);
                packet.Write(Main.MouseWorld.X);
                packet.Write(Main.MouseWorld.Y);
                packet.Send();
            }
            return false;
        }
        if (Main.netMode == NetmodeID.Server)
            return player.GetModPlayer<Common.Players.XianXiaPlayer>().ApplyingWeaponShot;
        WeaponShotTransactions.FirePrepared(player, this, source, position, velocity, type, damage, knockback, false);
        return false;
    }
}
