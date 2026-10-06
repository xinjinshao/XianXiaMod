using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace XianXia.Content.Buffs;
public class QiRecoveryCooldownBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Buffs/TribulationPressureBuff";
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}
