using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Players;

public class BiomeEffectsPlayer : ModPlayer
{
    public int starAbyssCorruption;
    public int thunderCloudTimer;

    public override void ResetEffects()
    {
    }

    public override void PostUpdate()
    {
        if (Main.myPlayer != Player.whoAmI)
            return;

        // Star Abyss Rift: accumulate corruption, faster pressure gain
        if (Player.InModBiome<Content.Biomes.StarAbyssRiftBiome>())
        {
            starAbyssCorruption = (int)MathHelper.Clamp(starAbyssCorruption + 1, 0, 3600);
            if (starAbyssCorruption >= 1800 && Main.GameUpdateCount % 120 == 0)
                Player.GetModPlayer<XianXiaPlayer>().spiritPressure = Math.Min(100, Player.GetModPlayer<XianXiaPlayer>().spiritPressure + 1);

            if (Main.rand.NextBool(300))
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.GemSapphire, 0f, -0.4f, 100, default, 0.6f);
        }
        else
        {
            starAbyssCorruption = (int)MathHelper.Clamp(starAbyssCorruption - 2, 0, 3600);
        }

        // Thunder Marsh Clouds: ambient lightning events
        if (Player.InModBiome<Content.Biomes.ThunderMarshCloudsBiome>())
        {
            thunderCloudTimer++;
            if (thunderCloudTimer >= 420 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                thunderCloudTimer = 0;
                Vector2 strikePos = Player.Center + new Vector2(Main.rand.NextFloat(-200f, 200f), -300f);
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    strikePos,
                    Vector2.Zero,
                    ModContent.ProjectileType<Content.Projectiles.TribulationWarningLineProjectile>(),
                    30,
                    0f,
                    Player.whoAmI);
            }
        }
        else
        {
            thunderCloudTimer = 0;
        }

        // Moonbone Abyss: increased pressure accumulation
        if (Player.InModBiome<Content.Biomes.MoonboneAbyssBiome>())
        {
            if (Main.GameUpdateCount % 90 == 0)
                Player.GetModPlayer<XianXiaPlayer>().spiritPressure = Math.Min(100, Player.GetModPlayer<XianXiaPlayer>().spiritPressure + 1);

            if (Main.rand.NextBool(200))
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.IceTorch, 0f, -1f, 100, default, 0.5f);
        }
    }
}
