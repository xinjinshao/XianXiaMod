using System;
using Terraria;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditExposureLifecycle(Action<bool, string> check)
    {
        Player previous = Main.player[0];
        try {
            foreach (string hook in new[] { "initialize", "enter", "load", "unload", "disconnect", "inactive" }) {
                var actor = new Player { whoAmI = 0, active = true }; Main.player[0] = actor;
                var effects = actor.GetModPlayer<BiomeEffectsPlayer>(); effects.starAbyssCorruption = 3600; effects.thunderCloudTimer = 419;
                var lifecycle = new BiomeScanCacheSystem();
                switch (hook) {
                    case "initialize": effects.Initialize(); break;
                    case "enter": effects.OnEnterWorld(); break;
                    case "load": lifecycle.OnWorldLoad(); break;
                    case "unload": lifecycle.OnWorldUnload(); break;
                    case "disconnect": actor.active = false; lifecycle.PostUpdatePlayers(); break;
                    case "inactive": actor.active = false; effects.PostUpdate(); break;
                }
                check(effects.starAbyssCorruption == 0 && effects.thunderCloudTimer == 0, "native environment lifecycle clears previous world/occupant exposure: " + hook);
                effects.starAbyssCorruption = 1800; effects.thunderCloudTimer = 200; actor.active = true; actor.dead = true; effects.PostUpdate();
                check(effects.starAbyssCorruption == 1800 && effects.thunderCloudTimer == 200, "native environment reset preserves existing within-world death behavior");
            }
        } finally { Main.player[0] = previous; }
        check(ReferenceEquals(Main.player[0], previous), "native exposure lifecycle restores original player slot");
    }
}
