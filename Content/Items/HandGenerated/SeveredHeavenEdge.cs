using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Common.Items;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;
using XianXia.Content.Tiles.Stations;

namespace XianXia.Content.Items.HandGenerated;

public class SeveredHeavenEdge : EndgameRouteReward
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/ImperialDecreeItem";
    protected override DownedBossSystem.EndgameRoute Route => DownedBossSystem.EndgameRoute.SeverHeaven;
    protected override int UniqueMaterial => ModContent.ItemType<ImperialDecreeItem>();
    protected override int Material => ModContent.ItemType<Moonbone>();
    protected override int MaterialCount => 8;
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!Active) return;
        player.GetDamage(DamageClass.Generic) += 0.12f; player.GetArmorPenetration(DamageClass.Generic) += 10;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyCostMultiplier *= 1.12f;
    }
}
