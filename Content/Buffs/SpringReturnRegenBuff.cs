// TODO: ART_PLACEHOLDER - see Docs/ART_TODO.md
using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class SpringReturnRegenBuff : ModBuff
{
    public override void SetStaticDefaults() => Main.buffNoSave[Type] = false;

    public override void Update(Player player, ref int buffIndex)
    {
        player.lifeRegen += 2;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 1;
    }
}
