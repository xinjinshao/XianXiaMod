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

public class RebuiltHeavenHeart : EndgameRouteReward
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/HeavenTabletSeal";
    protected override DownedBossSystem.EndgameRoute Route => DownedBossSystem.EndgameRoute.RebuildHeaven;
    protected override int UniqueMaterial => ModContent.ItemType<HeavenTabletSeal>();
    protected override int Material => ModContent.ItemType<HeavenDaoFragment>();
    protected override int MaterialCount => 8;
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!Active) return;
        player.statDefense += 8; player.lifeRegen += 2;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 2;
    }
}
