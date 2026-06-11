using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Content.Buffs;

public class ArtifactResonanceBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
        cultivation.spiritualEnergyCostMultiplier = MathHelper.Min(cultivation.spiritualEnergyCostMultiplier, 0.92f);
    }
}
