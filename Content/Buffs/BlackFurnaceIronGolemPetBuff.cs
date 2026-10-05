using Terraria;
using Terraria.ModLoader;
namespace XianXia.Content.Buffs;
public class BlackFurnaceIronGolemPetBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/BlackFurnaceIronGolemPet";
    public override void SetStaticDefaults() { Main.buffNoTimeDisplay[Type] = true; Main.vanityPet[Type] = true; }
    public override void Update(Player player, ref int buffIndex)
    {
        player.buffTime[buffIndex] = 18000;
        int projectileType = ModContent.ProjectileType<global::XianXia.Content.Projectiles.BlackFurnaceIronGolemPetProjectile>();
        if (player.whoAmI == Main.myPlayer && !player.dead && player.ownedProjectileCounts[projectileType] == 0)
            Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Microsoft.Xna.Framework.Vector2.Zero, projectileType, 0, 0, player.whoAmI);
    }
}
