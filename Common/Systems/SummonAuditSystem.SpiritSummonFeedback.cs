using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditSpiritSummonFeedback(Action<bool, string> check)
    {
        var previous = Main.player[0]; int mode = Main.netMode, local = Main.myPlayer;
        bool hard = Main.hardMode, plant = NPC.downedPlantBoss, golem = NPC.downedGolemBoss, moon = NPC.downedMoonlord;
        var shots = (Projectile[])Main.projectile.Clone();
        try {
            foreach (int type in new[] { ModContent.ItemType<SmallArtifactPendant>(), ModContent.ItemType<FurnaceAshSpiritContract>(),
                ModContent.ItemType<StarAbyssLarvaContract>(), ModContent.ItemType<NascentSoulCloneTalisman>(),
                ModContent.ItemType<CelestialPuppetToken>(), ModContent.ItemType<ArchivedImmortalSoulContract>() })
            foreach (int clientMode in new[] { NetmodeID.SinglePlayer, NetmodeID.MultiplayerClient })
            foreach (int scenario in Enumerable.Range(0, 9)) {
                Main.netMode = clientMode; Main.myPlayer = 0;
                Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = NPC.downedMoonlord = true;
                for (int i = 0; i < Main.maxProjectiles; i++) Main.projectile[i] = new Projectile { whoAmI = i };
                var actor = new Player { whoAmI = 0, active = true, maxMinions = 2 }; Main.player[0] = actor;
                var item = new Item(type); string expected = "";
                switch (scenario) {
                    case 1: actor.dead = true; expected = "Unavailable"; break;
                    case 2: actor.active = false; expected = "Unavailable"; break;
                    case 3: actor.noItems = true; expected = "Unavailable"; break;
                    case 4: actor.frozen = true; expected = "Unavailable"; break;
                    case 5: actor.slotsMinions = 2; expected = "SlotsFull"; break;
                    case 6: Main.projectile[0].active = true; Main.projectile[0].owner = 0; Main.projectile[0].type = item.shoot; expected = "AlreadySummoned"; break;
                    case 7: foreach (var shot in Main.projectile.Take(Main.maxProjectiles)) { shot.active = true; shot.owner = -1; shot.type = 0; } expected = "ProjectilesFull"; break;
                    case 8: Main.hardMode = NPC.downedPlantBoss = NPC.downedGolemBoss = NPC.downedMoonlord = false;
                        if (type != ModContent.ItemType<SmallArtifactPendant>() && type != ModContent.ItemType<FurnaceAshSpiritContract>()) expected = "WorldLocked";
                        break;
                }
                var before = actor.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().CaptureSnapshot();
                check(item.ModItem.CanUseItem(actor) == (expected.Length == 0),
                    "native six-spirit item eligibility follows shared actual requirements: " + item.ModItem.Name + "/" + clientMode + "/" + scenario);
                var tips = new System.Collections.Generic.List<TooltipLine>(); item.ModItem.ModifyTooltips(tips);
                string key = "Mods.XianXia.SpiritSummon." + (expected.Length == 0 ? "Ready" : expected);
                check(Terraria.Localization.Language.GetTextValue(key) != key && tips.Single(tip => tip.Name == "SpiritSummonRequirement").Text == Terraria.Localization.Language.GetTextValue(key),
                    "native six-spirit tooltip resolves actual local eligibility reason");
                check(actor.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().CaptureSnapshot().Equals(before)
                    && item.stack == 1, "native spirit eligibility preview does not spend items or cultivation resources");
            }
        } finally {
            Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local;
            Main.hardMode = hard; NPC.downedPlantBoss = plant; NPC.downedGolemBoss = golem; NPC.downedMoonlord = moon;
            shots.CopyTo(Main.projectile, 0);
        }
    }
}
