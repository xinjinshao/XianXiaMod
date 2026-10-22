using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditExposureLifecycle(Action<bool, string> check)
    {
        Player previous = Main.player[0];
        int previousMode = Main.netMode, previousLocal = Main.myPlayer;
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
                check(effects.starAbyssCorruption == 0 && effects.thunderCloudTimer == 0, "native dead-update guard discards within-world environmental exposure");
            }
            foreach (int mode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server, NetmodeID.MultiplayerClient }) {
                var actor = new Player { whoAmI = 0, active = true, dead = true }; Main.player[0] = actor;
                Main.netMode = mode; Main.myPlayer = mode == NetmodeID.Server ? Main.maxPlayers : 0;
                var effects = actor.GetModPlayer<BiomeEffectsPlayer>();
                effects.starAbyssCorruption = 3600; effects.thunderCloudTimer = 419;
                PlayerLoader.UpdateDead(actor);
                check(effects.starAbyssCorruption == 0 && effects.thunderCloudTimer == 0,
                    "native official death-hook dispatch clears exposure in each mode: " + mode);
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.spiritPressure = 25;
                effects.starAbyssCorruption = 3600; effects.thunderCloudTimer = 419; effects.PostUpdate();
                check(effects.starAbyssCorruption == 0 && effects.thunderCloudTimer == 0 && state.spiritPressure == 25,
                    "native dead-update fallback clears timers without changing pressure in each mode: " + mode);
            }
        } finally { Main.player[0] = previous; Main.netMode = previousMode; Main.myPlayer = previousLocal; }
        check(ReferenceEquals(Main.player[0], previous) && Main.netMode == previousMode && Main.myPlayer == previousLocal,
            "native exposure lifecycle restores original player slot and engine mode");
    }
}
