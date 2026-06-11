using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Buffs;

public class TribulationPressureBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetDamage(DamageClass.Generic) += 0.04f;
        player.endurance -= 0.04f;
    }
}
