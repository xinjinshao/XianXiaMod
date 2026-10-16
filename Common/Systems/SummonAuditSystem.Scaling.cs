using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Terraria;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private void AuditBossScaling(Action<bool, string> check)
    {
        int originalMode = Main.GameMode;
        var rows = new List<object>();
        string[] bosses = { "SpiritVeinWyrm", "GardenWarden", "BlackFurnaceIronGolem", "TribulationCloudAvatar",
            "ThunderMarshJiao", "AbyssalStarWomb", "FormlessSwordSoul", "GreenwoodMedicineKingEcho",
            "HeavenTabletGuardian", "BrokenHeavenInspector", "MoonboneImmortal", "OldHeavenDaoCore" };
        try {
            foreach (string name in bosses) {
                int previousModeLife = 0;
                foreach (int mode in new[] { 0, 1, 2 }) {
                    Main.GameMode = mode;
                    int previousLife = 0, singleDamage = 0;
                    foreach (int players in new[] { 1, 2, 4 }) {
                        var npc = new NPC();
                        npc.SetDefaults(Mod.Find<ModNPC>(name).Type, new NPCSpawnParams {
                            playerCountForMultiplayerDifficultyOverride = players });
                        check(npc.ModNPC?.Name == name && npc.boss && npc.lifeMax > 0 && npc.damage > 0,
                            "native scaling creates registered boss with positive final combat values");
                        check(npc.damage == BossStatRules.Get(name).Damage * (mode + 1),
                            "native contact damage receives exactly one normal expert or master difficulty multiplier");
                        if (players == 1) {
                            singleDamage = npc.damage;
                            check(npc.lifeMax > previousModeLife, "native boss single-player life increases across difficulty modes");
                            previousModeLife = npc.lifeMax;
                        } else {
                            check(mode == 0 ? npc.lifeMax == previousLife : npc.lifeMax > previousLife,
                                "native normal life stays fixed and expert/master life grows with player count");
                            check(npc.damage == singleDamage, "native multiplayer life scaling does not also multiply contact damage");
                        }
                        if (mode == 0) {
                            var basis = BossStatRules.Get(name);
                            check(npc.lifeMax == basis.Life && npc.damage == basis.Damage,
                                "native normal defaults match the centralized boss baseline");
                        }
                        rows.Add(new { boss = name, mode, players, life = npc.lifeMax, damage = npc.damage, defense = npc.defense });
                        previousLife = npc.lifeMax;
                    }
                }
            }
        } finally { Main.GameMode = originalMode; }
        check(Main.GameMode == originalMode && rows.Count == 108, "native scaling audit restores mode and covers all 108 boss/difficulty/player cases");
        string directory = Path.Combine(Main.SavePath, "XianXia");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "boss-scaling-audit.json"), JsonSerializer.Serialize(new {
            schema = 1, rows, limitations = "Registered NPC.SetDefaults with native player-count override; no combat duration, live players, seed variants or optional mods."
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
