using Terraria.ModLoader;
using XianXia.Common.Projectiles;
namespace XianXia.Content.Projectiles;
public class SmallTabletPetProjectile : OwnerBoundVanityPet {
    public override string Texture => "XianXia/Content/Items/HandGenerated/SmallTabletPet";
    protected override int PetBuff => ModContent.BuffType<global::XianXia.Content.Buffs.SmallTabletPetBuff>();
}
