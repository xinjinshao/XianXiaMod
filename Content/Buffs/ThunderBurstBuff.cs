using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;
namespace XianXia.Content.Buffs;
public class ThunderBurstBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Buffs/TribulationPressureBuff";
    public override void SetStaticDefaults() => Main.buffNoSave[Type] = false;
    public override void Update(Player player, ref int buffIndex)
    {
        player.GetDamage(DamageClass.Generic) += 0.12f;
        player.statDefense -= 6;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1.2f;
    }
}
