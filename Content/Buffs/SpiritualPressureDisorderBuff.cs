using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Buffs;

public class SpiritualPressureDisorderBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense -= 6;
        player.moveSpeed -= 0.08f;
    }
}
