using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.NPCs.Bosses;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Common.Systems;

// Explicit opt-in for an isolated developer server. Never runs during ordinary play.
public class SummonAuditSystem : ModSystem
{
    private bool attempted;
    public override void OnWorldLoad()
    {
        attempted = false;
        RunOnce();
    }

    internal void RunOnce()
    {
        if (attempted || !Main.dedServ || Main.netMode != NetmodeID.Server
            || Environment.GetEnvironmentVariable("XIANXIA_AUDIT_SUMMONS") != "1") return;
        attempted = true;
        var checks = new List<string>();
        var owned = new List<NPC>();
        var ownedProjectiles = new List<Projectile>();
        var previousProjectiles = Main.projectile.Where(projectile => projectile.active).ToHashSet();
        Player original = Main.player[0];
        string error = null;
        void Check(bool value, string name) {
            if (!value) throw new InvalidOperationException(name);
            checks.Add(name);
        }
        try {
            Check(!Main.player.Any(player => player?.active == true), "no connected players");
            Check(Main.npc.Count(npc => npc?.active != true) >= 4, "four free NPC slots");
            int x = Main.maxTilesX * 8;
            int y = 200;
            Check(!Collision.SolidCollision(new Vector2(x - 144, y - 108), 288, 48, true)
                && !Collision.LavaCollision(new Vector2(x - 144, y - 108), 288, 48), "empty preview area");
            Main.player[0] = new Player { whoAmI = 0, active = true, dead = false,
                position = new Vector2(x + 160, y), statLife = 100, statLifeMax2 = 100 };
            int parentIndex = NPC.NewNPC(new EntitySource_Misc("XianXiaSummonAudit"), x, y + 48,
                ModContent.NPCType<GreenwoodMedicineKingEcho>());
            Check(parentIndex >= 0 && parentIndex < Main.maxNPCs, "registered parent created");
            NPC parent = Main.npc[parentIndex];
            owned.Add(parent);
            Check(parent.ModNPC is GreenwoodMedicineKingEcho && parent.active, "parent mod instance");
            parent.target = 0;
            var medicine = (GreenwoodMedicineKingEcho)parent.ModNPC;
            long session = medicine.SummonSession;
            Check(session > 0, "authority encounter session");
            int type = ModContent.NPCType<HerbGardenVineSpirit>();
            var before = Main.npc.Where(npc => npc.active).ToHashSet();
            void TickMedicine() {
                // Capture every new entity even if the AI hook or an assertion fails.
                try { medicine.AI(); }
                finally {
                    owned.AddRange(Main.npc.Where(npc => npc.active && !before.Contains(npc) && !owned.Contains(npc)));
                    ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active
                        && !previousProjectiles.Contains(projectile) && !ownedProjectiles.Contains(projectile)));
                }
            }
            parent.life = parent.lifeMax / 2;
            parent.ai[0] = 110; // Ordinary shot is ready: summon must take priority.
            parent.ai[2] = 209; // Second-phase pattern boundary starts the warning.
            parent.velocity = new Vector2(4, 5);
            int cooldownSlot = 0;
            for (int frame = 1; frame <= GreenwoodMedicineKingEcho.SummonWarningTicks; frame++) {
                TickMedicine();
                Check(parent.ai[3] == (frame == 45 ? -30 : 45 - frame), $"warning clock frame {frame}");
                Check(!medicine.CanHitPlayer(Main.player[0], ref cooldownSlot), $"warning contact blocked frame {frame}");
                Check(parent.velocity == Vector2.Zero, $"warning movement stopped frame {frame}");
                Check(parent.ai[0] == 0 && parent.ai[2] == 0, $"other attack clocks paused frame {frame}");
                Check(parent.ai[1] == 1, $"healing ritual paused frame {frame}");
                Check(owned.Count(npc => npc != parent && npc.active) == (frame < 45 ? 0 : 3),
                    $"children appear only on release frame {frame}");
                Check(ownedProjectiles.Count == 0, $"no overlapping spell frame {frame}");
            }
            NPC[] children = owned.Where(npc => npc != parent).ToArray();
            Check(children.Length == 3 && children.All(npc => npc.type == type
                && npc.ModNPC is HerbGardenVineSpirit), "three registered vine children");
            foreach (NPC child in children) {
                Check(child.width == 48 && child.height == 48, "registered vine body size");
                Check(child.position.X >= parent.Center.X - 144 && child.position.X <= parent.Center.X + 96
                    && child.position.Y == parent.Center.Y - 108, "NewNPC body stays in preview area");
                Check(child.target == 0 && child.netUpdate, "child target synchronized");
                using var stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                    child.ModNPC.SendExtraAI(writer);
                Check(stream.Length == 13, "child binding packet length");
                stream.Position = 0;
                using var reader = new BinaryReader(stream);
                Check(reader.ReadBoolean() && reader.ReadInt16() == parentIndex
                    && reader.ReadInt64() == session && reader.ReadInt16() == 900,
                    "NewNPC OnSpawn captured parent session");
                Check(child.ModNPC.PreAI(), "bound child accepts living source");
            }
            for (int frame = 1; frame <= GreenwoodMedicineKingEcho.SummonRecoveryTicks; frame++) {
                TickMedicine();
                Check(parent.ai[3] == -30 + frame, $"recovery clock frame {frame}");
                Check(!medicine.CanHitPlayer(Main.player[0], ref cooldownSlot), $"recovery contact blocked frame {frame}");
                Check(parent.velocity == Vector2.Zero && parent.ai[0] == 0 && parent.ai[2] == 0
                    && parent.ai[1] == 1, $"recovery pauses other actions frame {frame}");
                Check(owned.Count(npc => npc != parent && npc.active) == 3 && ownedProjectiles.Count == 0,
                    $"recovery creates no extra entities frame {frame}");
            }
            TickMedicine();
            Check(parent.ai[3] == 0 && medicine.CanHitPlayer(Main.player[0], ref cooldownSlot),
                "contact resumes only after final recovery frame");
            Check(parent.ai[0] == 1 && parent.ai[2] == 1 && parent.ai[1] == 2,
                "normal clocks resume after recovery");
            try { medicine.SpawnVineAdds(); }
            finally { owned.AddRange(Main.npc.Where(npc => npc.active && !before.Contains(npc)
                && !owned.Contains(npc))); }
            Check(Main.npc.Count(npc => npc.active && !before.Contains(npc)) == 3, "quota prevents fourth child");
            parent.active = false;
            foreach (NPC child in children)
                Check(!child.ModNPC.PreAI() && !child.active && child.damage == 0,
                    "source loss despawns without death hooks");
        }
        catch (Exception exception) { error = exception.ToString(); }
        finally {
            foreach (NPC npc in owned) { npc.damage = 0; npc.active = false; }
            foreach (Projectile projectile in ownedProjectiles) { projectile.damage = 0; projectile.active = false; }
            Main.player[0] = original;
        }
        if (!ReferenceEquals(Main.player[0], original) || owned.Any(npc => npc.active)
            || ownedProjectiles.Any(projectile => projectile.active))
            error ??= "Audit cleanup did not restore the player slot and deactivate test NPCs.";
        else checks.Add("player slot restored and all test entities removed");
        string directory = Path.Combine(Main.SavePath, "XianXia");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "summon-audit.json"), JsonSerializer.Serialize(new {
            schema = 1, passed = error == null, checks, error,
            limitations = "Registered headless AI hooks advanced manually for 45 warning and 30 recovery frames; no full engine ticks, graphics, clients, combat balance or complete playthrough."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Mod.Logger.Info($"Summon audit finished: {checks.Count} checks; passed={error == null}");
    }
}
