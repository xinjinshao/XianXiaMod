using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class QiGatheringBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
        cultivation.spiritualEnergyRegenBonus += 1;
        cultivation.spiritualEnergyCostMultiplier = MathHelper.Min(cultivation.spiritualEnergyCostMultiplier, 0.9f);
    }
}
