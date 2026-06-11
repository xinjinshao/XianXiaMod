using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class TribulationResistanceBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.endurance += 0.08f;
        player.GetModPlayer<XianXiaPlayer>().ReduceSpiritPressure(1);
    }
}
