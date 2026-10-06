using Terraria;
using Terraria.ModLoader;
namespace XianXia.Content.Buffs;
public class WindStepBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Buffs/SpringReturnBuff";
    public override void SetStaticDefaults() => Main.buffNoSave[Type] = false;
    public override void Update(Player player, ref int buffIndex)
    {
        player.moveSpeed += 0.15f;
        player.statDefense -= 4;
    }
}
