using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Players;
using XianXia.Content.Items.Weapons;
using XianXia.Content.Items.Accessories;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Items.Guides;

public class TribulationGauge : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.UseSound = SoundID.Item4;
        Item.value = Item.buyPrice(gold: 1, silver: 50);
        Item.rare = ItemRarityID.LightRed;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            Main.NewText(GetStatus(player), 160, 210, 255);
        }

        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<TribulationCloudDew>(3)
            .AddIngredient<LowGradeSpiritStone>(8)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    private static string GetStatus(Player player)
    {
        XianXiaPlayer cultivation = player.GetModPlayer<XianXiaPlayer>();
        string pressure = Gauge("Pressure").Format(cultivation.spiritPressure);
        string comprehension = Gauge("Comprehension").Format(cultivation.tribulationComprehension * 5);

        if (cultivation.tribulationTimer > 0)
        {
            int seconds = (int)MathF.Ceiling(cultivation.tribulationTimer / 60f);
            return $"{pressure} {Gauge("Active").Format(seconds, cultivation.tribulationIntensity)} {comprehension}";
        }

        if (cultivation.spiritPressure >= 80)
        {
            return $"{pressure} {GaugeValue("Danger")} {comprehension}";
        }

        if (cultivation.spiritPressure >= 50)
        {
            return $"{pressure} {GaugeValue("Warning")} {comprehension}";
        }

        return $"{pressure} {GaugeValue("Stable")} {comprehension}";
    }

    private static LocalizedText Gauge(string key)
    {
        return ModContent.GetInstance<TribulationGauge>().GetLocalization($"Gauge.{key}");
    }

    private static string GaugeValue(string key)
    {
        return Gauge(key).Value;
    }
}
