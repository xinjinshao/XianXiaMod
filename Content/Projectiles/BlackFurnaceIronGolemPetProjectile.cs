using Terraria.ModLoader;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;
public class BlackFurnaceIronGolemPetProjectile : OwnerBoundVanityPet {
    public override string Texture => "XianXia/Content/Items/HandGenerated/BlackFurnaceIronGolemPet";
    protected override int PetBuff => ModContent.BuffType<global::XianXia.Content.Buffs.BlackFurnaceIronGolemPetBuff>();
}
