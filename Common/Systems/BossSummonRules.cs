using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public static class BossSummonRules
{
    public static bool CanUseGeneratedBossSummon(Player player, string bossId)
    {
        if (!IsAtRequiredSite(player, bossId, out string siteKey))
        {
            if (Main.myPlayer == player.whoAmI)
            {
                string siteName = Language.GetTextValue($"Mods.XianXia.Progression.SummonSites.{siteKey}");
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.BossSummonSiteRequired", siteName), 255, 210, 120);
            }

            return false;
        }

        if (RequiresNight(bossId) && Main.dayTime)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.BossSummonNightRequired"), 255, 210, 120);
            }

            return false;
        }

        return true;
    }

    private static bool IsAtRequiredSite(Player player, string bossId, out string siteKey)
    {
        siteKey = bossId switch
        {
            "garden_warden" or "greenwood_medicine_king_echo" => "GreenwoodHerbGarden",
            "black_furnace_iron_golem" => "SunkenFurnaceVein",
            "tribulation_cloud_avatar" or "thunder_marsh_jiao" => "ThunderMarshClouds",
            "abyssal_star_womb" => "StarAbyssRift",
            "formless_sword_soul" => "TenThousandSectsRuins",
            "heaven_tablet_guardian" or "broken_heaven_inspector" => "FallenHeavenPalace",
            "moonbone_immortal" or "old_heaven_dao_core" => "MoonboneAbyss",
            _ => "",
        };

        return bossId switch
        {
            "garden_warden" or "greenwood_medicine_king_echo" => player.InModBiome<Content.Biomes.GreenwoodHerbGardenBiome>(),
            "black_furnace_iron_golem" => player.InModBiome<Content.Biomes.SunkenFurnaceVeinBiome>(),
            "tribulation_cloud_avatar" or "thunder_marsh_jiao" => player.InModBiome<Content.Biomes.ThunderMarshCloudsBiome>(),
            "abyssal_star_womb" => player.InModBiome<Content.Biomes.StarAbyssRiftBiome>(),
            "formless_sword_soul" => player.InModBiome<Content.Biomes.TenThousandSectsRuinsBiome>(),
            "heaven_tablet_guardian" or "broken_heaven_inspector" => player.InModBiome<Content.Biomes.FallenHeavenPalaceBiome>(),
            "moonbone_immortal" or "old_heaven_dao_core" => player.InModBiome<Content.Biomes.MoonboneAbyssBiome>(),
            _ => true,
        };
    }

    private static bool RequiresNight(string bossId)
    {
        return bossId is "abyssal_star_womb" or "moonbone_immortal" or "old_heaven_dao_core";
    }
}
