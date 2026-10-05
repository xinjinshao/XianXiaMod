using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;

namespace XianXia.Content.Buffs;

public class SmallArtifactSpiritBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/SmallArtifactPendant";
    public override void SetStaticDefaults() { Main.buffNoSave[Type] = true; Main.buffNoTimeDisplay[Type] = true; }
    public override void Update(Player player, ref int buffIndex)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<SmallArtifactSpirit>()] > 0 && !player.dead)
            player.buffTime[buffIndex] = 18000;
        else { player.DelBuff(buffIndex); buffIndex--; }
    }
}
