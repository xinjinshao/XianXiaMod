using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Buffs;

public class ArchiveLockBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.moveSpeed -= 0.15f;
        player.runAcceleration -= 0.1f;
        player.jumpSpeedBoost -= 0.5f;
    }
}
