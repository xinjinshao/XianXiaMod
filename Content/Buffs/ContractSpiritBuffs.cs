using Terraria;
using Terraria.ModLoader;
using XianXia.Content.Projectiles;

namespace XianXia.Content.Buffs;

public abstract class ContractSpiritBuff : ModBuff
{
    protected abstract int Spirit { get; }
    public override void SetStaticDefaults() { Main.buffNoSave[Type] = true; Main.buffNoTimeDisplay[Type] = true; }
    public override void Update(Player player, ref int buffIndex)
    {
        if (player.ownedProjectileCounts[Spirit] > 0 && !player.dead) player.buffTime[buffIndex] = 18000;
        else { player.DelBuff(buffIndex); buffIndex--; }
    }
}
public class FurnaceAshSpiritBuff : ContractSpiritBuff
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/FurnaceAshSpiritContract";
    protected override int Spirit => ModContent.ProjectileType<FurnaceAshSpirit>();
}
public class StarAbyssSpiritBuff : ContractSpiritBuff
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/StarAbyssLarvaContract";
    protected override int Spirit => ModContent.ProjectileType<StarAbyssSpirit>();
}
