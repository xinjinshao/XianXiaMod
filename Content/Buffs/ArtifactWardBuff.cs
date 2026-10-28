using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Buffs;

public class ArtifactWardBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Buffs/TribulationResistanceBuff";
    public override void SetStaticDefaults() { Main.buffNoSave[Type] = true; Main.buffNoTimeDisplay[Type] = true; }
    // The icon is only a visual indicator; the authoritative timer applies protection.
}
