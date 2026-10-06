using Terraria;
using Terraria.ModLoader;
namespace XianXia.Content.Buffs;
public class FurnaceGuardBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Buffs/TribulationResistanceBuff";
    public override void SetStaticDefaults() => Main.buffNoSave[Type] = false;
    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense += 8;
        player.moveSpeed -= 0.1f;
    }
}
