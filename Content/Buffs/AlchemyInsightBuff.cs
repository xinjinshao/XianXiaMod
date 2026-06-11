using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class AlchemyInsightBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 1;
    }
}
