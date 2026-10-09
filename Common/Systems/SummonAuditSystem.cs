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
        var previousItems = Main.item.Where(item => item?.active == true).ToHashSet();
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

            int beamType = ModContent.ProjectileType<global::XianXia.Content.Projectiles.TabletJudgmentBeamProjectile>();
            for (int phase = 0; phase < 3; phase++) {
                int tabletIndex = NPC.NewNPC(new EntitySource_Misc("XianXiaJudgmentAudit"), x, y + 48,
                    ModContent.NPCType<HeavenTabletGuardian>());
                Check(tabletIndex >= 0 && tabletIndex < Main.maxNPCs, $"registered tablet phase {phase}");
                NPC tabletNpc = Main.npc[tabletIndex]; owned.Add(tabletNpc); tabletNpc.target = 0;
                var tablet = (HeavenTabletGuardian)tabletNpc.ModNPC;
                tabletNpc.life = phase == 0 ? tabletNpc.lifeMax : tabletNpc.lifeMax / (phase == 1 ? 2 : 4);
                int interval = phase == 0 ? 300 : phase == 1 ? 240 : 180;
                tabletNpc.ai[2] = interval - HeavenTabletGuardian.JudgmentWarningTicks;
                tabletNpc.ai[0] = phase == 0 ? 150 : phase == 1 ? 110 : 72;
                var priorBeams = Main.projectile.Where(projectile => projectile.active).ToHashSet();
                var priorTabletNpcs = Main.npc.Where(npc => npc.active).ToHashSet();
                void TickTablet() {
                    try { tablet.AI(); }
                    finally {
                        owned.AddRange(Main.npc.Where(npc => npc.active && !priorTabletNpcs.Contains(npc) && !owned.Contains(npc)));
                        ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active
                            && !previousProjectiles.Contains(projectile) && !ownedProjectiles.Contains(projectile)));
                    }
                }
                if (phase == 1) {
                    for (int frame = 1; frame <= HeavenTabletGuardian.SealWarningTicks; frame++) {
                        TickTablet();
                        Check(!tablet.CanHitPlayer(Main.player[0], ref cooldownSlot) && tabletNpc.velocity == Vector2.Zero,
                            $"seal ritual harmless warning {frame}");
                        Check(tabletNpc.dontTakeDamage == (frame == 45), $"shield activates only after creation {frame}");
                        Check(Main.npc.Count(npc => npc.active && !priorTabletNpcs.Contains(npc)) == (frame < 45 ? 0 : 4),
                            $"four seal creation boundary {frame}");
                    }
                    NPC[] marks = Main.npc.Where(npc => npc.active && npc.ModNPC is HeavenTabletSealNPC)
                        .OrderBy(npc => npc.ai[1]).ToArray();
                    Check(marks.Length == 4, "four registered ordered seals");
                    int protectedLife = tabletNpc.life;
                    Main.player[0].ApplyDamageToNPC(tabletNpc, 100, 0, 1, false, DamageClass.Generic);
                    Check(tabletNpc.life == protectedLife, "native player damage respects boss shield");
                    int previousKillCount = NPC.killCount[marks[0].type];
                    foreach (NPC mark in marks) {
                        Check(mark.width == 40 && mark.height == 40 && mark.damage == 0 && mark.value == 0,
                            "registered seal body and no contact/coin rewards");
                        using var wire = new MemoryStream(); mark.ModNPC.SendExtraAI(new BinaryWriter(wire));
                        Check(wire.Length == 13, "registered seal binding and lifetime packet");
                    }
                    for (int order = 0; order < 4; order++) {
                        foreach (NPC mark in marks.Where(npc => npc.active)) {
                            mark.ModNPC.AI();
                            Check(mark.ModNPC.CanBeHitByItem(Main.player[0], new Item()) == (mark == marks[order]),
                                $"only seal {order} accepts hits");
                            Check(mark.dontTakeDamage == (mark != marks[order]), $"seal {order} native immunity flag");
                            Check(mark.immortal == (mark != marks[order]), $"seal {order} native low-level damage protection");
                            if (mark != marks[order]) {
                                int protectedMarkLife = mark.life;
                                Main.player[0].ApplyDamageToNPC(mark, 100, 0, 1, false, DamageClass.Generic);
                                Check(mark.life == protectedMarkLife, $"wrong seal protected from native player damage {order}");
                            }
                        }
                        NPC current = marks[order];
                        int readyLife = current.life;
                        Main.player[0].ApplyDamageToNPC(current, 100, 0, 1, false, DamageClass.Generic);
                        Check(current.life < readyLife && current.active, $"current seal {order} accepts native player damage");
                        int previousLife = current.life;
                        int applied = current.StrikeNPC(new NPC.HitInfo { Damage = previousLife + 100, HitDirection = 1 }, noPlayerInteraction: true);
                        Check(applied > 0 && !current.active, $"seal {order} native strike consumes ordered mark");
                        Check(NPC.killCount[current.type] == previousKillCount, "native seal strike does not increment kill count");
                        Check(!Main.item.Any(item => item?.active == true && !previousItems.Contains(item)),
                            "native seal strike creates no loot items");
                        Check(tabletNpc.dontTakeDamage == (order < 3), $"shield breaks after last ordered seal {order}");
                    }
                    int unlockedLife = tabletNpc.life;
                    Main.player[0].ApplyDamageToNPC(tabletNpc, 100, 0, 1, false, DamageClass.Generic);
                    Check(tabletNpc.life < unlockedLife && !tabletNpc.immortal, "native player damage resumes after shield breaks");
                    for (int frame = 1; frame <= HeavenTabletGuardian.SealRecoveryTicks; frame++) {
                        TickTablet(); Check(!tablet.CanHitPlayer(Main.player[0], ref cooldownSlot), $"seal recovery safe {frame}");
                    }
                    tabletNpc.ai[2] = interval - HeavenTabletGuardian.JudgmentWarningTicks;
                    tabletNpc.ai[0] = 110;
                }
                for (int frame = 1; frame <= HeavenTabletGuardian.JudgmentWarningTicks; frame++) {
                    TickTablet();
                    Check(tabletNpc.ai[1] == (frame == 60 ? 0 : 60 - frame), $"tablet {phase} warning clock {frame}");
                    Check(!tablet.CanHitPlayer(Main.player[0], ref cooldownSlot) && tabletNpc.velocity == Vector2.Zero,
                        $"tablet {phase} warning harmless and stationary {frame}");
                    var emitted = Main.projectile.Where(projectile => projectile.active && !priorBeams.Contains(projectile)).ToArray();
                    Check(emitted.Length == (frame < 60 ? 0 : 2 * (phase + 1)), $"tablet {phase} full warning before beams {frame}");
                    Check(emitted.All(projectile => projectile.type == beamType), $"tablet {phase} no overlapping spells {frame}");
                }
                var beams = Main.projectile.Where(projectile => projectile.active && !priorBeams.Contains(projectile)).ToArray();
                foreach (Projectile beam in beams) {
                    Check(beam.width == 32 && beam.height == 480 && beam.timeLeft == 30 && beam.netImportant,
                        "registered judgment beam body/lifetime/late join");
                    Check(beam.Center.Y == Main.player[0].Center.Y && MathF.Abs(beam.Center.X - Main.player[0].Center.X) >= 112
                        && beam.velocity == Vector2.Zero, "registered judgment beam locked outside safe lane");
                    Check(beam.ModProjectile.CanDamage() == true, "registered judgment beam living source accepts damage");
                }
                for (int frame = 1; frame <= HeavenTabletGuardian.JudgmentRecoveryTicks; frame++) {
                    TickTablet();
                    Check(tabletNpc.ai[2] == -45 + frame && !tablet.CanHitPlayer(Main.player[0], ref cooldownSlot),
                        $"tablet {phase} recovery including final frame {frame}");
                    Check(Main.projectile.Count(projectile => projectile.active && !priorBeams.Contains(projectile)) == beams.Length,
                        $"tablet {phase} recovery adds no spells {frame}");
                }
                TickTablet();
                Check(tablet.CanHitPlayer(Main.player[0], ref cooldownSlot), $"tablet {phase} contact resumes next frame");
                tabletNpc.active = false;
                foreach (Projectile beam in beams) {
                    beam.ModProjectile.AI();
                    Check(beam.ModProjectile.CanDamage() == false && beam.timeLeft <= 6,
                        "registered judgment beam source loss fades harmlessly");
                    beam.active = false;
                }
            }
            for (int phase = 0; phase < 3; phase++) {
                int inspectorSlot = NPC.NewNPC(new EntitySource_Misc("XianXiaDecreeAudit"), x, y + 48,
                    ModContent.NPCType<BrokenHeavenInspector>());
                Check(inspectorSlot >= 0 && inspectorSlot < Main.maxNPCs, "registered inspector created");
                NPC inspectorNpc = Main.npc[inspectorSlot]; owned.Add(inspectorNpc); inspectorNpc.target = 0;
                var inspector = (BrokenHeavenInspector)inspectorNpc.ModNPC;
                inspectorNpc.life = phase == 0 ? inspectorNpc.lifeMax : inspectorNpc.lifeMax / (phase == 1 ? 2 : 4);
                var beforeInspectorNpcs = Main.npc.Where(npc => npc.active).ToHashSet();
                void TickInspector() {
                    try { inspector.AI(); }
                    finally {
                        owned.AddRange(Main.npc.Where(npc => npc.active && !beforeInspectorNpcs.Contains(npc) && !owned.Contains(npc)));
                        ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active
                            && !previousProjectiles.Contains(projectile) && !ownedProjectiles.Contains(projectile)));
                    }
                }
                int decreeType = ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorDecreeBeamProjectile>();
                int interval = phase == 0 ? 300 : phase == 1 ? 240 : 180;
                for (int law = 0; law < 2; law++) {
                    inspectorNpc.ai[2] = interval - BrokenHeavenInspector.DecreeWarningTicks;
                    inspectorNpc.ai[0] = phase == 0 ? 150 : phase == 1 ? 110 : 72;
                    var priorDecreeBeams = Main.projectile.Where(projectile => projectile.active).ToHashSet();
                    for (int frame = 1; frame <= 60; frame++) {
                        TickInspector();
                        Check(inspectorNpc.ai[1] == (law == 0 ? 60 - frame : frame - 60), $"native decree {phase}/{law} clock {frame}");
                        Check(!inspector.CanHitPlayer(Main.player[0], ref cooldownSlot) && inspectorNpc.velocity == Vector2.Zero,
                            $"native decree {phase}/{law} warning stationary/harmless {frame}");
                        var emitted = Main.projectile.Where(projectile => projectile.active && !priorDecreeBeams.Contains(projectile)).ToArray();
                        Check(emitted.Length == (frame < 60 ? 0 : law + 1 + (phase == 2 ? 1 : 0)) && emitted.All(projectile => projectile.type == decreeType || (phase == 2 && projectile.type == ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorVerdictBladeProjectile>())),
                            $"native decree {phase}/{law} exact release/no overlap {frame}");
                    }
                    var decreeBeams = Main.projectile.Where(projectile => projectile.active && !priorDecreeBeams.Contains(projectile)).ToArray();
                    var bladeProjectiles = decreeBeams.Where(beam => beam.type != decreeType).ToArray();
                    Check(bladeProjectiles.Length == (phase == 2 ? 1 : 0) && bladeProjectiles.All(blade => blade.width == 160 && blade.height == 48 && blade.Center == inspectorNpc.Center && blade.ModProjectile.CanDamage() == true), "registered final-phase close blade body and location");
                    foreach (Projectile blade in bladeProjectiles) blade.active = false;
                    decreeBeams = decreeBeams.Where(beam => beam.type == decreeType).ToArray();
                    foreach (Projectile beam in decreeBeams)
                        Check(beam.width == 64 && beam.height == 480 && beam.timeLeft == 30 && beam.ModProjectile.CanDamage() == true,
                            "registered inspector wider beam and living source");
                    Check(decreeBeams.All(beam => beam.Center.Y == Main.player[0].Center.Y)
                        && (law == 0 ? decreeBeams.Single().Center.X == Main.player[0].Center.X
                            : decreeBeams.Select(beam => beam.Center.X).Order().SequenceEqual(new[] {
                                Main.player[0].Center.X - 112, Main.player[0].Center.X + 112 })), "native leave/return layout matches warning");
                    for (int frame = 1; frame <= 45; frame++) {
                        TickInspector();
                        Check(inspectorNpc.ai[2] == -45 + frame && !inspector.CanHitPlayer(Main.player[0], ref cooldownSlot),
                            $"native decree recovery {phase}/{law}/{frame}");
                    }
                    foreach (Projectile beam in decreeBeams) beam.active = false;
                }
                var puppets = Main.npc.Where(npc => npc.active && !beforeInspectorNpcs.Contains(npc)).ToArray();
                Check(puppets.Length == (phase == 0 ? 0 : 2), "registered puppet quota remains two across both decrees");
                foreach (NPC puppet in puppets) {
                    Check(puppet.ModNPC is CelestialPuppet && puppet.target == inspectorNpc.target && puppet.netUpdate,
                        "registered puppet type and synchronized target");
                    Check(puppet.ModNPC.PreAI(), "registered puppet captured live inspector session");
                    using var puppetPacket = new MemoryStream();
                    puppet.ModNPC.SendExtraAI(new BinaryWriter(puppetPacket));
                    Check(puppetPacket.Length == 13 && BitConverter.ToInt64(puppetPacket.ToArray(), 3) == inspector.SummonSession, "registered puppet source session packet");
                }
                inspectorNpc.active = false;
                foreach (NPC puppet in puppets)
                    Check(!puppet.ModNPC.PreAI() && !puppet.active && puppet.damage == 0, "registered puppet source loss despawns without death rewards");
            }
        }
        catch (Exception exception) { error = exception.ToString(); }
        finally {
            foreach (NPC npc in owned) { npc.damage = 0; npc.active = false; }
            foreach (Projectile projectile in ownedProjectiles) { projectile.damage = 0; projectile.active = false; }
            foreach (Item item in Main.item.Where(item => item?.active == true && !previousItems.Contains(item))) item.active = false;
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
            limitations = "Registered headless AI advanced manually for medicine, tablet and alternating inspector decrees; native player damage and NPC.StrikeNPC verify ordered seals and loot/kill-count suppression. No full engine ticks, graphics, clients, combat balance or complete playthrough."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Mod.Logger.Info($"Summon audit finished: {checks.Count} checks; passed={error == null}");
    }
}
