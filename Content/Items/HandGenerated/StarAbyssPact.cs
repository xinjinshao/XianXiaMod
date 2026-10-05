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

public class StarAbyssPact : EndgameRouteReward
{
    public override string Texture => "XianXia/Content/Items/HandGenerated/StarCalamityCore";
    protected override DownedBossSystem.EndgameRoute Route => DownedBossSystem.EndgameRoute.AcceptStarAbyss;
    protected override int UniqueMaterial => ModContent.ItemType<StarCalamityCore>();
    protected override int Material => ModContent.ItemType<StarEclipseCrystal>();
    protected override int MaterialCount => 12;
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!Active) return;
        player.GetDamage(DamageClass.Generic) += 0.08f;
        player.GetModPlayer<XianXiaPlayer>().spiritualEnergyRegenBonus += 4;
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 120 == 0) {
            var state = player.GetModPlayer<XianXiaPlayer>();
            state.spiritPressure = System.Math.Min(100, state.spiritPressure + 2);
        }
    }
}
