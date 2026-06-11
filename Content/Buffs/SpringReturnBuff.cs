using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class SpringReturnBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.lifeRegen += 4;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 1;
    }
}
