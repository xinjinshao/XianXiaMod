using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class StarAbyssCorrosionBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense -= 8;
        player.GetModPlayer<XianXiaPlayer>().spiritPressure = System.Math.Min(100,
            player.GetModPlayer<XianXiaPlayer>().spiritPressure + 1);
    }
}
