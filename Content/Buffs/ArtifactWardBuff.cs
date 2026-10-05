using Terraria;
using Terraria.ModLoader;

namespace XianXia.Content.Buffs;

public class ArtifactWardBuff : ModBuff
{
    public override string Texture => "XianXia/Content/Buffs/TribulationResistanceBuff";
    public override void SetStaticDefaults() { Main.buffNoSave[Type] = true; Main.buffNoTimeDisplay[Type] = true; }
    public override void Update(Player player, ref int buffIndex)
    {
        // The snapshot timer is authoritative; ordinary buff sync cannot extend it.
        if (player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().wardGuardTimer <= 0) return;
        player.endurance += 0.35f; player.statDefense += 12;
    }
}
