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
using Terraria.ModLoader.IO;
using XianXia.Content.NPCs.Bosses;
using XianXia.Content.NPCs.Enemies;

namespace XianXia.Common.Systems;

// Explicit opt-in for an isolated developer server. Never runs during ordinary play.
public partial class SummonAuditSystem : ModSystem
{
    // Captures output only inside the explicitly opted-in audit; it never connects or listens.
    private sealed class AuditOutputSocket : Terraria.Net.Sockets.ISocket
    {
        private bool connected;
        internal readonly List<byte[]> Sent = new();
        internal AuditOutputSocket(bool connected = false) => this.connected = connected;
        public void Close() => connected = false;
        public bool IsConnected() => connected;
        public void Connect(Terraria.Net.RemoteAddress address) => throw new InvalidOperationException("Audit socket cannot connect");
        public void AsyncSend(byte[] data, int offset, int size, Terraria.Net.Sockets.SocketSendCallback callback, object state = null) {
            Sent.Add(data.AsSpan(offset, size).ToArray()); callback?.Invoke(state);
        }
        public void AsyncReceive(byte[] data, int offset, int size, Terraria.Net.Sockets.SocketReceiveCallback callback, object state = null) { }
        public bool IsDataAvailable() => false;
        public void SendQueuedPackets() { }
        public bool StartListening(Terraria.Net.Sockets.SocketConnectionAccepted callback) => false;
        public void StopListening() { }
        public Terraria.Net.RemoteAddress GetRemoteAddress() => new Terraria.Net.TcpAddress(System.Net.IPAddress.Loopback, 0);
    }
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
        int originalGameMode = Main.GameMode;
        int[] originalKillCounts = (int[])NPC.killCount.Clone();
        var progressSystem = ModContent.GetInstance<DownedBossSystem>();
        var originalProgress = new TagCompound();
        progressSystem.SaveWorldData(originalProgress);
        int originalReputation = DownedBossSystem.SectReputation;

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
                    int[] previousKillCount = (int[])NPC.killCount.Clone();
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
                        Check(NPC.killCount.SequenceEqual(previousKillCount), "native seal strike does not increment kill count");
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
                    Check(beam.width == 32 && beam.height == 32 && beam.timeLeft == 30 && beam.netImportant,
                        "registered judgment beam body/lifetime/late join");
                    Check(beam.Center.Y == Main.player[0].Center.Y && MathF.Abs(beam.Center.X - Main.player[0].Center.X) >= 112
                        && beam.velocity == Vector2.Zero, "registered judgment beam locked outside safe lane");
                    Check(beam.ModProjectile.CanDamage() == true, "registered judgment beam living source accepts damage");
                    beam.Update(beam.whoAmI);Check(beam.active&&beam.timeLeft==29,"native tablet beam survives top-area first update");
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
                    Check(bladeProjectiles.Length == (phase == 2 ? 1 : 0) && bladeProjectiles.All(blade => blade.width == 32 && blade.height == 32 && blade.Center == inspectorNpc.Center && blade.ModProjectile.CanDamage() == true), "registered final-phase close blade body and location");
                    foreach (Projectile blade in bladeProjectiles) blade.active = false;
                    decreeBeams = decreeBeams.Where(beam => beam.type == decreeType).ToArray();
                    foreach (Projectile beam in decreeBeams) {
                        Check(beam.width == 32 && beam.height == 32 && beam.timeLeft == 30 && beam.ModProjectile.CanDamage() == true,
                            "registered inspector wider beam and living source");
                        beam.Update(beam.whoAmI);Check(beam.active&&beam.timeLeft==29,"native inspector beam survives top-area first update");
                    }
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
            byte[] CorePayload(byte[] prefix){using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);writer.Write(prefix);writer.Write(1L);writer.Write((byte)3);writer.Write((short)0);writer.Write((byte)15);for(int index=0;index<4;index++)writer.Write((short)-1);return stream.ToArray();}
            for (int corePhase = 0; corePhase < 3; corePhase++) {
                int coreSlot = NPC.NewNPC(new EntitySource_Misc("XianXiaCoreModuleAudit"), x, y + 48, ModContent.NPCType<OldHeavenDaoCore>());
                Check(coreSlot >= 0 && coreSlot < Main.maxNPCs, "registered old core created");
                NPC coreNpc = Main.npc[coreSlot]; owned.Add(coreNpc); coreNpc.target = 0;
                var core = (OldHeavenDaoCore)coreNpc.ModNPC;
                core.ReceiveExtraAI(new BinaryReader(new MemoryStream(CorePayload(new byte[]{0,0,1,0,0}))));
                coreNpc.life = corePhase == 0 ? coreNpc.lifeMax : coreNpc.lifeMax / (corePhase == 1 ? 2 : 4);
                void TickCore() {
                    try { core.AI(); }
                    finally { ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active && !previousProjectiles.Contains(projectile) && !ownedProjectiles.Contains(projectile))); }
                }
                for (int coreModule = 0; coreModule < 3; coreModule++) {
                    coreNpc.ai[2] = (corePhase == 0 ? 270 : corePhase == 1 ? 210 : 150) - OldHeavenDaoCore.ModuleWarningTicks;
                    coreNpc.ai[0] = 150; // An ordinary volley is due, but must remain paused during this attack.
                    Vector2 coreLock = Main.player[0].Center;
                    var beforeCoreProjectiles = Main.projectile.Where(projectile => projectile.active).ToHashSet();
                    for (int coreFrame = 1; coreFrame <= 60; coreFrame++) {
                        TickCore();
                        Check(coreNpc.ai[1] == 60 - coreFrame && coreNpc.ai[0] == coreLock.X && coreNpc.ai[3] == coreLock.Y, "registered core locked countdown/point");
                        Check(!core.CanHitPlayer(Main.player[0], ref cooldownSlot) && coreNpc.velocity == Vector2.Zero, "registered core warning freezes/contact blocked");
                        if (coreFrame < 60) Check(!Main.projectile.Any(projectile => projectile.active && !beforeCoreProjectiles.Contains(projectile)), "registered core no early or ordinary volley");
                    }
                    var coreProjectiles = Main.projectile.Where(projectile => projectile.active && !beforeCoreProjectiles.Contains(projectile)).ToArray();
                    int expectedCore = coreModule == 0 ? 2 * (corePhase + 1) : coreModule == 1 ? new[] { 9, 11, 15 }[corePhase] : 1;
                    int expectedCoreType = coreModule == 0 ? ModContent.ProjectileType<global::XianXia.Content.Projectiles.TabletJudgmentBeamProjectile>() : coreModule == 1 ? ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>() : ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>();
                    Check(coreProjectiles.Length == expectedCore && coreProjectiles.All(projectile => projectile.type == expectedCoreType), "registered core module sequence and phase density");
                    if (coreModule == 0) Check(coreProjectiles.All(projectile => projectile.width == 32 && projectile.height == 32 && projectile.Center.Y == coreLock.Y && MathF.Abs(projectile.Center.X - coreLock.X) >= 112), "registered core safe central column corridor");
                    if (coreModule == 1) {
                        float gapDirection = (coreLock - coreNpc.Center).SafeNormalize(Vector2.UnitY).ToRotation();
                        Check(coreProjectiles.All(projectile => MathF.Cos(projectile.velocity.ToRotation() - gapDirection) < MathF.Cos(MathHelper.TwoPi / 12)), "registered core ring sixty degree safe direction");
                    }
                    if (coreModule == 2) Check(coreProjectiles.Single().Center == coreLock && coreProjectiles.Single().ModProjectile.CanDamage() == false, "registered core field at warned point starts with its own harmless tell");
                    for (int coreFrame = 1; coreFrame <= 45; coreFrame++) {
                        TickCore();
                        Check(coreNpc.ai[2] == -45 + coreFrame && !core.CanHitPlayer(Main.player[0], ref cooldownSlot), "registered core exact harmless recovery");
                        Check(Main.projectile.Count(projectile => projectile.active && !beforeCoreProjectiles.Contains(projectile)) == expectedCore, "registered core recovery has no stacked attacks");
                    }
                    coreNpc.active = false;
                    foreach (Projectile coreProjectile in coreProjectiles) { coreProjectile.ModProjectile.AI(); Check(coreProjectile.ModProjectile.CanDamage() == false, "registered core source loss cancels module projectiles"); coreProjectile.active = false; }
                    coreNpc.active = true;
                }
                coreNpc.active = false;
            }
            for (byte testedCoreRoute = 1; testedCoreRoute <= 3; testedCoreRoute++) for (byte testedCoreDensity = 2; testedCoreDensity <= 3; testedCoreDensity++) {
                int routeCoreSlot = NPC.NewNPC(new EntitySource_Misc("XianXiaCoreRouteAudit"), x, y + 48, ModContent.NPCType<OldHeavenDaoCore>());
                Check(routeCoreSlot >= 0 && routeCoreSlot < Main.maxNPCs, "registered route core created");
                NPC routeCoreNpc = Main.npc[routeCoreSlot];owned.Add(routeCoreNpc);routeCoreNpc.target=0;routeCoreNpc.life=routeCoreNpc.lifeMax/(testedCoreDensity==2?2:4);
                var routeCore=(OldHeavenDaoCore)routeCoreNpc.ModNPC;
                // Exercise a complete synchronized warning snapshot without changing the world's permanent ending.
                routeCore.ReceiveExtraAI(new BinaryReader(new MemoryStream(CorePayload(new byte[]{1,1,testedCoreDensity,testedCoreRoute,0}))));
                Vector2 routeCorePoint=Main.player[0].Center;routeCoreNpc.ai[0]=routeCorePoint.X;routeCoreNpc.ai[3]=routeCorePoint.Y;routeCoreNpc.ai[1]=60;routeCoreNpc.ai[2]=150;
                var previousRouteCoreProjectiles=Main.projectile.Where(projectile=>projectile.active).ToHashSet();
                try {
                    for(int routeCoreFrame=1;routeCoreFrame<=60;routeCoreFrame++) {
                        routeCore.AI();Check(routeCoreNpc.ai[1]==60-routeCoreFrame&&!routeCore.CanHitPlayer(Main.player[0],ref cooldownSlot), "registered route core complete warning/contact");
                        if(routeCoreFrame<60)Check(!Main.projectile.Any(projectile=>projectile.active&&!previousRouteCoreProjectiles.Contains(projectile)),"registered route core no early casts");
                    }
                } finally {ownedProjectiles.AddRange(Main.projectile.Where(projectile=>projectile.active&&!previousProjectiles.Contains(projectile)&&!ownedProjectiles.Contains(projectile)));}
                var routeCoreProjectiles=Main.projectile.Where(projectile=>projectile.active&&!previousRouteCoreProjectiles.Contains(projectile)).ToArray();
                int expectedRouteCoreCount=testedCoreRoute==2?2:testedCoreRoute==1?(testedCoreDensity==2?11:15):(testedCoreDensity==2?10:14);
                Check(routeCoreProjectiles.Length==expectedRouteCoreCount,"registered route exact projectile count");
                if(testedCoreRoute==2)Check(routeCoreProjectiles.All(projectile=>projectile.type==ModContent.ProjectileType<global::XianXia.Content.Projectiles.CoreSeveranceBladeProjectile>()&&projectile.width==32&&projectile.height==32&&projectile.Center.X==routeCorePoint.X&&MathF.Abs(projectile.Center.Y-routeCorePoint.Y)==96),"registered sever route two horizontal blades safe center");
                else {
                    float routeCoreDirection=(routeCorePoint-routeCoreNpc.Center).SafeNormalize(Vector2.UnitY).ToRotation();float routeCoreGap=MathHelper.TwoPi/(testedCoreRoute==1?8:12);
                    Check(routeCoreProjectiles.All(projectile=>MathF.Cos(projectile.velocity.ToRotation()-routeCoreDirection)<MathF.Cos(routeCoreGap)),"registered route primary gap");
                    if(testedCoreRoute==3)Check(routeCoreProjectiles.All(projectile=>MathF.Cos(projectile.velocity.ToRotation()-routeCoreDirection-MathHelper.TwoPi/2)<MathF.Cos(routeCoreGap)),"registered abyss route opposing gap");
                }
                routeCoreNpc.active=false;foreach(Projectile routeCoreProjectile in routeCoreProjectiles){routeCoreProjectile.ModProjectile.AI();Check(routeCoreProjectile.ModProjectile.CanDamage()==false,"registered route source loss no damage");routeCoreProjectile.active=false;}
            }
            int archiveCoreSlot=NPC.NewNPC(new EntitySource_Misc("XianXiaArchiveAudit"),x,y+48,ModContent.NPCType<OldHeavenDaoCore>());
            Check(archiveCoreSlot>=0&&archiveCoreSlot<Main.maxNPCs,"registered archive core created");NPC archiveCoreNpc=Main.npc[archiveCoreSlot];owned.Add(archiveCoreNpc);archiveCoreNpc.target=0;archiveCoreNpc.life=archiveCoreNpc.lifeMax/4;
            var archiveCore=(OldHeavenDaoCore)archiveCoreNpc.ModNPC;var beforeArchiveNpcs=Main.npc.Where(npc=>npc.active).ToHashSet();var beforeArchiveProjectiles=Main.projectile.Where(projectile=>projectile.active).ToHashSet();
            void TickArchiveCore(){try{archiveCore.AI();}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeArchiveNpcs.Contains(npc)&&!owned.Contains(npc)));ownedProjectiles.AddRange(Main.projectile.Where(projectile=>projectile.active&&!previousProjectiles.Contains(projectile)&&!ownedProjectiles.Contains(projectile)));}}
            for(int archiveFrame=1;archiveFrame<=60;archiveFrame++) {
                TickArchiveCore();Check(!archiveCore.CanHitPlayer(Main.player[0],ref cooldownSlot)&&archiveCoreNpc.velocity==Vector2.Zero,"registered archive warning stationary/harmless");
                Check(archiveCoreNpc.dontTakeDamage==(archiveFrame==60)&&archiveCoreNpc.immortal==(archiveFrame==60),"registered archive shield only after four valid births");
                Check(Main.npc.Count(npc=>npc.active&&!beforeArchiveNpcs.Contains(npc))==(archiveFrame==60?4:0),"registered archives after full sixty frame tell");
                Check(Main.projectile.Count(projectile=>projectile.active&&!beforeArchiveProjectiles.Contains(projectile))==(archiveFrame==60?1:0),"registered archive warning compression only after locks, no stacked spell");
            }
            var archiveCompression=Main.projectile.Single(projectile=>projectile.active&&!beforeArchiveProjectiles.Contains(projectile));
            Check(archiveCompression.ModProjectile is global::XianXia.Content.Projectiles.CoreArchiveCompressionProjectile&&archiveCompression.ModProjectile.CanDamage()==false,"registered compression begins with its own harmless warning");
            archiveCompression.Update(archiveCompression.whoAmI);Check(archiveCompression.active,"registered compression survives a native projectile tick near upper world boundary");
            var typedCompression=(global::XianXia.Content.Projectiles.CoreArchiveCompressionProjectile)archiveCompression.ModProjectile;
            foreach((int Age,float Half) compressionSample in new[]{(59,480f),(60,480f),(360,400f),(660,320f)}) {
                archiveCompression.timeLeft=global::XianXia.Content.Projectiles.CoreArchiveCompressionProjectile.Lifetime-compressionSample.Age;typedCompression.AI();
                Check(MathF.Abs(typedCompression.SafeHalfSize-compressionSample.Half)<0.001f,"registered compression warning/mid/final radius");
                Check(!archiveCompression.Colliding(archiveCompression.Hitbox,new Rectangle((int)archiveCompression.Center.X-10,(int)archiveCompression.Center.Y-20,20,40)),"native projectile collision respects compression safe center");
                Check(archiveCompression.Colliding(archiveCompression.Hitbox,new Rectangle((int)archiveCompression.Center.X+600,(int)archiveCompression.Center.Y,20,40))==(compressionSample.Age>=60),"native projectile collision outside only after full compression warning");
            }
            Player compressionVictim=Main.player[0];Vector2 previousCompressionCenter=compressionVictim.Center,previousCompressionVelocity=compressionVictim.velocity;int previousCompressionLife=compressionVictim.statLife,previousCompressionMax=compressionVictim.statLifeMax2,previousCompressionMode=Main.netMode,previousCompressionOwner=archiveCompression.owner,previousLocalPlayer=Main.myPlayer,previousCompressionImmuneTime=compressionVictim.immuneTime;bool previousCompressionImmune=compressionVictim.immune;var previousCompressionCooldowns=(int[])compressionVictim.hurtCooldowns.Clone();
            var previousCompressionCombatText=Main.combatText;
            try {
                // Saturate the text pool: damage runs through the real engine without requiring server-side fonts.
                Main.combatText=Enumerable.Range(0,100).Select(_=>new CombatText{active=true}).ToArray();
                Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=0;archiveCompression.owner=0;compressionVictim.statLife=compressionVictim.statLifeMax2=10000;compressionVictim.velocity=Vector2.Zero;compressionVictim.immune=false;compressionVictim.immuneTime=0;Array.Clear(compressionVictim.hurtCooldowns);
                compressionVictim.Center=archiveCompression.Center;archiveCompression.Damage();Check(compressionVictim.statLife==10000,"native projectile damage leaves compression center unharmed");
                compressionVictim.Center=archiveCompression.Center+new Vector2(600,0);archiveCompression.Damage();Check(compressionVictim.statLife<10000,"native projectile damage hits outside compression despite small anchor hitbox");
            } finally {Main.combatText=previousCompressionCombatText;Main.netMode=previousCompressionMode;Main.myPlayer=previousLocalPlayer;archiveCompression.owner=previousCompressionOwner;compressionVictim.Center=previousCompressionCenter;compressionVictim.velocity=previousCompressionVelocity;compressionVictim.statLife=previousCompressionLife;compressionVictim.statLifeMax2=previousCompressionMax;compressionVictim.immune=previousCompressionImmune;compressionVictim.immuneTime=previousCompressionImmuneTime;previousCompressionCooldowns.CopyTo(compressionVictim.hurtCooldowns,0);}
            Player previousCompressionGuest=Main.player[1];
            try {
                Main.player[1]=new Player{whoAmI=1,active=true,dead=false,statLife=100,statLifeMax2=100};Main.player[1].Center=archiveCompression.Center+new Vector2(600,0);
                for(int compressionGuestFrame=1;compressionGuestFrame<=118;compressionGuestFrame++){typedCompression.AI();Check(typedCompression.CanHitPlayer(Main.player[1])==(compressionGuestFrame==118),"registered compression distant late entrant has approach time plus sixty frame grace");}
            } finally {Main.player[1]=previousCompressionGuest;}
            using(var nativeCompressionWire=new MemoryStream()){typedCompression.SendExtraAI(new BinaryWriter(nativeCompressionWire));Check(nativeCompressionWire.Length==17,"registered compression actual source packet");}
            var archiveLocks=Main.npc.Where(npc=>npc.active&&!beforeArchiveNpcs.Contains(npc)).OrderBy(npc=>npc.ai[1]).ToArray();Check(archiveLocks.Length==4,"registered four archive locks");
            TickArchiveCore();Check(archiveCoreNpc.velocity==Vector2.Zero&&archiveCoreNpc.immortal,"registered core remains stationary while archive shield active");
            int archiveProtectedLife=archiveCoreNpc.life;Main.player[0].ApplyDamageToNPC(archiveCoreNpc,100,0,1,false,DamageClass.Generic);Check(archiveCoreNpc.life==archiveProtectedLife,"registered archive shield rejects actual native player damage");
            int[] archiveKillCount=(int[])NPC.killCount.Clone();
            foreach(int archiveIndex in new[]{3,1,0,2}) {
                NPC archiveLock=archiveLocks[archiveIndex];archiveLock.ModNPC.AI();Check(archiveLock.ModNPC is CoreArchiveLockNPC&&archiveLock.ModNPC.CanBeHitByItem(Main.player[0],new Item())==true&&!archiveLock.immortal,"registered remaining archive is immediately vulnerable out of order");
                using(var archivePacket=new MemoryStream()){archiveLock.ModNPC.SendExtraAI(new BinaryWriter(archivePacket));Check(archivePacket.Length==13&&BitConverter.ToInt64(archivePacket.ToArray(),2)==archiveCore.ArchiveSession,"registered archive real OnSpawn source session");}
                int archiveApplied=archiveLock.StrikeNPC(new NPC.HitInfo{Damage=archiveLock.life+100,HitDirection=1},noPlayerInteraction:true);
                Check(archiveApplied>0&&!archiveLock.active,"registered native strike breaks unordered archive");Check(NPC.killCount.SequenceEqual(archiveKillCount)&&!Main.item.Any(item=>item?.active==true&&!previousItems.Contains(item)),"registered archive no kill progression or item rewards");
                Check(archiveCoreNpc.immortal==(archiveIndex!=2)&&archiveCoreNpc.dontTakeDamage==(archiveIndex!=2),"registered archive shield breaks only after final lock");
            }
            Check(!archiveCore.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered last archive break immediately safe before next AI");
            archiveCompression.ModProjectile.AI();Check(archiveCompression.ModProjectile.CanDamage()==false&&archiveCompression.timeLeft<=6,"registered archive completion cancels compression immediately");archiveCompression.active=false;
            int archiveUnlockedLife=archiveCoreNpc.life;Main.player[0].ApplyDamageToNPC(archiveCoreNpc,100,0,1,false,DamageClass.Generic);Check(archiveCoreNpc.life<archiveUnlockedLife,"registered core takes damage after archive shield broken");
            for(int archiveFrame=1;archiveFrame<=45;archiveFrame++){TickArchiveCore();Check(!archiveCore.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered archive recovery exact harmless duration");}
            archiveCoreNpc.active=false;
            int reusedArchiveSlot=NPC.NewNPC(new EntitySource_Misc("XianXiaArchiveReuseAudit"),x,y+48,ModContent.NPCType<OldHeavenDaoCore>());Check(reusedArchiveSlot>=0&&reusedArchiveSlot<Main.maxNPCs,"registered second archive core created");
            NPC reusedArchiveCoreNpc=Main.npc[reusedArchiveSlot];owned.Add(reusedArchiveCoreNpc);reusedArchiveCoreNpc.target=0;reusedArchiveCoreNpc.life=reusedArchiveCoreNpc.lifeMax/4;var reusedArchiveCore=(OldHeavenDaoCore)reusedArchiveCoreNpc.ModNPC;
            var beforeReusedArchiveNpcs=Main.npc.Where(npc=>npc.active).ToHashSet();var beforeReusedCompression=Main.projectile.Where(projectile=>projectile.active).ToHashSet();try{for(int frame=0;frame<60;frame++)reusedArchiveCore.AI();}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeReusedArchiveNpcs.Contains(npc)&&!owned.Contains(npc)));ownedProjectiles.AddRange(Main.projectile.Where(projectile=>projectile.active&&!previousProjectiles.Contains(projectile)&&!ownedProjectiles.Contains(projectile)));}
            var abandonedCompression=Main.projectile.Single(projectile=>projectile.active&&!beforeReusedCompression.Contains(projectile));
            var abandonedArchiveLocks=Main.npc.Where(npc=>npc.active&&!beforeReusedArchiveNpcs.Contains(npc)).ToArray();Check(abandonedArchiveLocks.Length==4,"registered source reuse scenario has four locks");long previousArchiveSession=reusedArchiveCore.ArchiveSession;reusedArchiveCoreNpc.active=false;
            int replacementArchiveSlot=NPC.NewNPC(new EntitySource_Misc("XianXiaArchiveReplacementAudit"),x,y+48,ModContent.NPCType<OldHeavenDaoCore>());Check(replacementArchiveSlot==reusedArchiveSlot,"registered archive parent slot actually reused");NPC replacementArchiveNpc=Main.npc[replacementArchiveSlot];owned.Add(replacementArchiveNpc);replacementArchiveNpc.target=0;Check(((OldHeavenDaoCore)replacementArchiveNpc.ModNPC).ArchiveSession!=previousArchiveSession,"registered replacement archive parent has new session");
            abandonedCompression.ModProjectile.AI();Check(abandonedCompression.ModProjectile.CanDamage()==false&&abandonedCompression.timeLeft<=6,"registered old compression does not follow same-type parent slot reuse");abandonedCompression.active=false;
            foreach(NPC abandonedArchive in abandonedArchiveLocks){abandonedArchive.ModNPC.AI();Check(!abandonedArchive.active&&abandonedArchive.damage==0,"registered old archive source cannot follow reused same-type parent slot");}
            Check(NPC.killCount.SequenceEqual(archiveKillCount)&&!Main.item.Any(item=>item?.active==true&&!previousItems.Contains(item)),"registered reused-source cleanup has no rewards");replacementArchiveNpc.active=false;
            foreach(bool finalSlamPhase in new[]{false,true}) {
                int slamParentIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaFurnaceSlamAudit"),x,y+48,ModContent.NPCType<BlackFurnaceIronGolem>());Check(slamParentIndex>=0&&slamParentIndex<Main.maxNPCs,"registered slam furnace created");NPC slamParent=Main.npc[slamParentIndex];owned.Add(slamParent);slamParent.target=0;slamParent.life=slamParent.lifeMax/(finalSlamPhase?4:2);slamParent.ai[2]=(finalSlamPhase?150:210)-1;var slamBoss=(BlackFurnaceIronGolem)slamParent.ModNPC;var beforeSlamNpcs=Main.npc.Where(npc=>npc.active).ToHashSet();var beforeSlamShots=Main.projectile.Where(projectile=>projectile.active).ToHashSet();float slamLockedY=Main.player[0].Center.Y;
                void TickSlam(){try{slamBoss.AI();}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeSlamNpcs.Contains(npc)&&!owned.Contains(npc)));ownedProjectiles.AddRange(Main.projectile.Where(projectile=>projectile.active&&!previousProjectiles.Contains(projectile)&&!ownedProjectiles.Contains(projectile)));}}
                for(int slamFrame=1;slamFrame<=105;slamFrame++){TickSlam();Check(slamParent.velocity==Vector2.Zero&&!slamBoss.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered slam exact harmless boss warning/recovery");Check(Main.projectile.Count(projectile=>projectile.active&&!beforeSlamShots.Contains(projectile))==(slamFrame<=60?0:1),"registered slam wave only after full sixty warning frames");Check(Main.npc.Count(npc=>npc.active&&!beforeSlamNpcs.Contains(npc))==(slamFrame<=60?0:2),"registered slam bounded adds only at release");}
                Projectile slamWave=Main.projectile.Single(projectile=>projectile.active&&!beforeSlamShots.Contains(projectile));Check(slamWave.ModProjectile is global::XianXia.Content.Projectiles.FurnaceSlamWaveProjectile&&slamWave.width==32&&slamWave.height==32&&slamWave.timeLeft==30&&slamWave.Center.Y==slamLockedY,"registered slam geometry lifetime and locked elevation");slamWave.Update(slamWave.whoAmI);Check(slamWave.active&&slamWave.timeLeft==29,"registered native slam projectile update preserves live wave");using(var slamWire=new MemoryStream()){slamWave.ModProjectile.SendExtraAI(new BinaryWriter(slamWire));Check(slamWire.Length==19,"registered slam wave existing source packet");}
                var slamVictim=Main.player[0];bool oldSlamStone=Main.tileSolid[TileID.Stone];var oldSlamText=Main.combatText;int oldSlamNetMode=Main.netMode,oldSlamLocal=Main.myPlayer,oldSlamOwner=slamWave.owner;Vector2 oldSlamPlayerCenter=slamVictim.Center,oldSlamPlayerVelocity=slamVictim.velocity;int oldSlamLife=slamVictim.statLife,oldSlamMaxLife=slamVictim.statLifeMax2,oldSlamImmuneTime=slamVictim.immuneTime;bool oldSlamImmune=slamVictim.immune;int[] oldSlamCooldowns=(int[])slamVictim.hurtCooldowns.Clone();
                bool slamWallRestored=true;var slamWallSnapshot=new List<(int X,int Y,bool HasTile,ushort Type,bool Actuated,Terraria.ID.SlopeType Slope,bool Half)>();
                void RestoreSlamWall(){foreach(var cell in slamWallSnapshot){Tile tile=Main.tile[cell.X,cell.Y];tile.HasTile=cell.HasTile;tile.TileType=cell.Type;tile.IsActuated=cell.Actuated;tile.Slope=cell.Slope;tile.IsHalfBlock=cell.Half;slamWallRestored &= tile.HasTile==cell.HasTile&&tile.TileType==cell.Type&&tile.IsActuated==cell.Actuated&&tile.Slope==cell.Slope&&tile.IsHalfBlock==cell.Half;}slamWallSnapshot.Clear();}
                try {
                    Main.tileSolid[TileID.Stone]=true;Main.combatText=Enumerable.Range(0,100).Select(_=>new CombatText{active=true}).ToArray();Main.netMode=NetmodeID.SinglePlayer;Main.myPlayer=slamWave.owner=0;slamVictim.statLife=slamVictim.statLifeMax2=10000;
                    void ClearSlamImmunity(){slamVictim.immune=false;slamVictim.immuneTime=0;Array.Clear(slamVictim.hurtCooldowns);slamVictim.velocity=Vector2.Zero;}
                    slamVictim.Center=slamWave.Center+new Vector2(200,0);ClearSlamImmunity();slamWave.Damage();Check(slamVictim.statLife==10000,"native slam damage outside horizontal zone leaves life unchanged");
                    slamVictim.Center=slamWave.Center+new Vector2(0,-60);ClearSlamImmunity();slamWave.Damage();Check(slamVictim.statLife==10000,"native slam damage above locked height leaves life unchanged");
                    slamVictim.Center=slamWave.Center+new Vector2(120,0);Check(slamWave.ModProjectile.CanHitPlayer(slamVictim),"native slam source valid before actual wall test");int slamWallX=(int)(slamWave.Center.X+64)/16;for(int wallY=(int)(slamWave.Center.Y-48)/16;wallY<=(int)(slamWave.Center.Y+48)/16;wallY++){Tile tile=Main.tile[slamWallX,wallY];slamWallSnapshot.Add((slamWallX,wallY,tile.HasTile,tile.TileType,tile.IsActuated,tile.Slope,tile.IsHalfBlock));tile.HasTile=true;tile.TileType=TileID.Stone;tile.IsActuated=false;tile.Slope=Terraria.ID.SlopeType.Solid;tile.IsHalfBlock=false;}
                    ClearSlamImmunity();slamWave.Damage();Check(slamVictim.statLife==10000&&!slamWave.ModProjectile.CanHitPlayer(slamVictim),"native slam solid wall blocks real player damage");RestoreSlamWall();slamVictim.Center=slamWave.Center;ClearSlamImmunity();slamWave.Damage();Check(slamVictim.statLife<10000,"native slam damage inside warned zone reduces player life");slamVictim.statLife=10000;slamVictim.Center=slamWave.Center+new Vector2(120,0);ClearSlamImmunity();slamWave.Damage();Check(slamVictim.statLife<10000,"native slam custom collision damages player far outside small anchor");
                    foreach(int fullHeightType in new[]{ModContent.ProjectileType<global::XianXia.Content.Projectiles.TabletJudgmentBeamProjectile>(),ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorDecreeBeamProjectile>()}) {int tallColumnIndex=Projectile.NewProjectile(slamParent.GetSource_FromAI(),slamWave.Center,Vector2.Zero,fullHeightType,18,0,0);Check(tallColumnIndex>=0&&tallColumnIndex<Main.maxProjectiles,"registered tall-column damage fixture created");Projectile tallColumn=Main.projectile[tallColumnIndex];ownedProjectiles.Add(tallColumn);slamVictim.statLife=10000;slamVictim.Center=tallColumn.Center+new Vector2(0,180);ClearSlamImmunity();tallColumn.Damage();Check(slamVictim.statLife<10000,"native full-height tablet column damages player beyond small anchor");slamVictim.statLife=10000;slamVictim.Center=tallColumn.Center+new Vector2(0,270);ClearSlamImmunity();tallColumn.Damage();Check(slamVictim.statLife==10000,"native tablet column outside full visual height harmless");if(fullHeightType==ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorDecreeBeamProjectile>()){slamVictim.statLife=10000;slamVictim.Center=tallColumn.Center+new Vector2(40,0);ClearSlamImmunity();tallColumn.Damage();Check(slamVictim.statLife<10000,"native inspector full-width fringe beyond anchor damages player");}tallColumn.active=false;}

                    foreach(var bladeGeometry in new[]{(ModContent.ProjectileType<global::XianXia.Content.Projectiles.CoreSeveranceBladeProjectile>(),220f,270f),(ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorVerdictBladeProjectile>(),60f,110f)}){int wideBladeIndex=Projectile.NewProjectile(slamParent.GetSource_FromAI(),slamWave.Center,Vector2.Zero,bladeGeometry.Item1,18,0,0);Check(wideBladeIndex>=0&&wideBladeIndex<Main.maxProjectiles,"registered wide blade damage fixture created");Projectile wideBlade=Main.projectile[wideBladeIndex];ownedProjectiles.Add(wideBlade);slamVictim.statLife=10000;slamVictim.Center=wideBlade.Center+new Vector2(bladeGeometry.Item2,0);ClearSlamImmunity();wideBlade.Damage();Check(slamVictim.statLife<10000,"native wide blade full range outside anchor damages player");slamVictim.statLife=10000;slamVictim.Center=wideBlade.Center+new Vector2(bladeGeometry.Item3,0);ClearSlamImmunity();wideBlade.Damage();Check(slamVictim.statLife==10000,"native wide blade beyond visual extent harmless");wideBlade.active=false;}
                } finally {RestoreSlamWall();Main.tileSolid[TileID.Stone]=oldSlamStone;Main.combatText=oldSlamText;Main.netMode=oldSlamNetMode;Main.myPlayer=oldSlamLocal;slamWave.owner=oldSlamOwner;slamVictim.Center=oldSlamPlayerCenter;slamVictim.velocity=oldSlamPlayerVelocity;slamVictim.statLife=oldSlamLife;slamVictim.statLifeMax2=oldSlamMaxLife;slamVictim.immune=oldSlamImmune;slamVictim.immuneTime=oldSlamImmuneTime;oldSlamCooldowns.CopyTo(slamVictim.hurtCooldowns,0);}
                Check(slamWallRestored,"temporary slam wall tile flags restored exactly");
                int lifetimeWaveIndex=Projectile.NewProjectile(slamParent.GetSource_FromAI(),slamWave.Center,Vector2.Zero,ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceSlamWaveProjectile>(),slamWave.damage,0,Main.myPlayer);Check(lifetimeWaveIndex>=0&&lifetimeWaveIndex<Main.maxProjectiles,"registered dedicated slam lifetime fixture created");Projectile lifetimeWave=Main.projectile[lifetimeWaveIndex];ownedProjectiles.Add(lifetimeWave);for(int lifeFrame=1;lifeFrame<=30;lifeFrame++){lifetimeWave.Update(lifetimeWave.whoAmI);Check(lifetimeWave.active==(lifeFrame<30)&&lifetimeWave.timeLeft==30-lifeFrame,"native slam full thirty-frame lifetime and expiry");}Check(lifetimeWave.ModProjectile.CanDamage()==false,"expired native slam wave cannot damage");
                foreach(int contactBeamType in new[]{ModContent.ProjectileType<global::XianXia.Content.Projectiles.TabletJudgmentBeamProjectile>(),ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorDecreeBeamProjectile>(),ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorVerdictBladeProjectile>(),ModContent.ProjectileType<global::XianXia.Content.Projectiles.CoreSeveranceBladeProjectile>(),ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceSlamWaveProjectile>()}){
                    int contactBeamIndex=Projectile.NewProjectile(slamParent.GetSource_FromAI(),slamWave.Center,Vector2.Zero,contactBeamType,18,0,Main.myPlayer);Check(contactBeamIndex>=0&&contactBeamIndex<Main.maxProjectiles,"registered beam contact fixture created");Projectile contactBeam=Main.projectile[contactBeamIndex];ownedProjectiles.Add(contactBeam);Check(contactBeam.ModProjectile.CanHitPlayer(null)==false,"registered valid-source beam null contact rejected without exception");var malformedBeamPlayer=new Player{active=true,dead=false,velocity=new Vector2(float.NaN,0)};malformedBeamPlayer.Center=contactBeam.Center;Check(!contactBeam.ModProjectile.CanHitPlayer(malformedBeamPlayer),"registered valid-source beam malformed velocity rejected");malformedBeamPlayer.velocity=Vector2.Zero;Check(contactBeam.ModProjectile.CanHitPlayer(malformedBeamPlayer),"registered valid-source beam ordinary contact preserved");contactBeam.active=false;
                }
                Vector2 beforeEdgeParent=slamParent.Center,beforeEdgePlayer=Main.player[0].Center;
                try{foreach(Vector2 edgePoint in new[]{new Vector2(50,slamWave.Center.Y),new Vector2(Main.maxTilesX*16f-50f,slamWave.Center.Y),new Vector2(slamWave.Center.X,50),new Vector2(slamWave.Center.X,Main.maxTilesY*16f-50f)}){slamParent.Center=edgePoint;Main.player[0].Center=slamParent.Center;int edgeSlamIndex=Projectile.NewProjectile(slamParent.GetSource_FromAI(),slamParent.Center,Vector2.Zero,ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceSlamWaveProjectile>(),slamWave.damage,0,Main.myPlayer);Check(edgeSlamIndex>=0&&edgeSlamIndex<Main.maxProjectiles,"registered edge slam fixture created");Projectile edgeSlam=Main.projectile[edgeSlamIndex];ownedProjectiles.Add(edgeSlam);edgeSlam.Update(edgeSlam.whoAmI);Check(edgeSlam.active&&edgeSlam.timeLeft==29,"native world-edge slam survives first update");edgeSlam.active=false;}}finally{slamParent.Center=beforeEdgeParent;Main.player[0].Center=beforeEdgePlayer;}
                var failedBladeEdges=new List<int>();Vector2 oldBladeParent=slamParent.Center,oldBladePlayer=Main.player[0].Center;
                try{foreach(Vector2 bladePoint in new[]{new Vector2(50,slamWave.Center.Y),new Vector2(Main.maxTilesX*16f-50,slamWave.Center.Y),new Vector2(slamWave.Center.X,50),new Vector2(slamWave.Center.X,Main.maxTilesY*16f-50)}){slamParent.Center=bladePoint;Main.player[0].Center=slamParent.Center;foreach(int bladeEdgeType in new[]{ModContent.ProjectileType<global::XianXia.Content.Projectiles.CoreSeveranceBladeProjectile>(),ModContent.ProjectileType<global::XianXia.Content.Projectiles.InspectorVerdictBladeProjectile>()}){int bladeEdgeIndex=Projectile.NewProjectile(slamParent.GetSource_FromAI(),slamParent.Center,Vector2.Zero,bladeEdgeType,18,0,Main.myPlayer);Check(bladeEdgeIndex>=0&&bladeEdgeIndex<Main.maxProjectiles,"registered horizontal blade edge fixture created");Projectile bladeEdge=Main.projectile[bladeEdgeIndex];ownedProjectiles.Add(bladeEdge);bladeEdge.Update(bladeEdge.whoAmI);if(!bladeEdge.active||bladeEdge.timeLeft!=29)failedBladeEdges.Add(bladeEdgeType);bladeEdge.active=false;}}}finally{slamParent.Center=oldBladeParent;Main.player[0].Center=oldBladePlayer;}Check(failedBladeEdges.Count==0,"native horizontal blades survive world edge; failed types: "+string.Join(",",failedBladeEdges));
                slamParent.active=false;Check(slamWave.ModProjectile.CanHitPlayer(Main.player[0])==false,"registered slam wave invalid source immediately harmless");slamWave.ModProjectile.AI();Check(slamWave.timeLeft<=6,"registered slam source cancellation fades promptly");slamWave.active=false;foreach(NPC slamAdd in Main.npc.Where(npc=>npc.active&&!beforeSlamNpcs.Contains(npc)).ToArray())slamAdd.active=false;
            }
            try {
                Main.GameMode=0;
                foreach(var fatalWorm in new[]{(ModContent.NPCType<SpiritVeinWyrm>(),ModContent.NPCType<SpiritVeinWyrmBody>(),ModContent.NPCType<SpiritVeinWyrmTail>(),"spirit_vein_wyrm",ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.LowGradeSpiritCore>(),1,1),(ModContent.NPCType<ThunderMarshJiao>(),ModContent.NPCType<ThunderMarshJiaoBody>(),ModContent.NPCType<ThunderMarshJiaoTail>(),"thunder_marsh_jiao",ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(),16,28)}) {
                    progressSystem.ClearWorld();var beforeFatalItems=Main.item.Where(item=>item?.active==true).ToHashSet();int fatalHeadIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaWormFatalRewardAudit"),x,y+48,fatalWorm.Item1);Check(fatalHeadIndex>=0&&fatalHeadIndex<Main.maxNPCs,"registered fatal reward worm head created");NPC fatalHead=Main.npc[fatalHeadIndex];owned.Add(fatalHead);fatalHead.target=0;fatalHead.playerInteraction[0]=true;fatalHead.lastInteraction=0;
                    var beforeFatalNpcs=Main.npc.Where(npc=>npc.active).ToHashSet();try{Check(SegmentedWormAI.TrySpawnChain(fatalHead,fatalWorm.Item2,fatalWorm.Item3,2),"registered fatal reward worm chain created");}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeFatalNpcs.Contains(npc)&&!owned.Contains(npc)));}NPC[] fatalSegments=Main.npc.Where(npc=>npc.active&&!beforeFatalNpcs.Contains(npc)).ToArray();Check(fatalSegments.Length==3,"registered fatal reward full short chain");
                    int[] originalHeadKills=(int[])NPC.killCount.Clone();fatalHead.life=1;Main.player[0].ApplyDamageToNPC(fatalSegments[0],100,0,1,false,DamageClass.Generic);Check(!fatalHead.active&&DownedBossSystem.DownedBosses.Contains(fatalWorm.Item4),"registered fatal segment strike kills head and advances correct boss");Check(DownedBossSystem.DownedBosses.Count==1&&DownedBossSystem.SectReputation==(fatalWorm.Item4=="spirit_vein_wyrm"?5:18)&&NPC.killCount.SequenceEqual(originalHeadKills),"registered fatal head advances progress once without unrelated banner tally");
                    int RewardCount()=>Main.item.Where(item=>item?.active==true&&!beforeFatalItems.Contains(item)&&item.type==fatalWorm.Item5).Sum(item=>item.stack);int actualReward=RewardCount();Check(actualReward>=fatalWorm.Item6&&actualReward<=fatalWorm.Item7,"registered fatal head gives one ordinary guaranteed material roll");int fatalReputation=DownedBossSystem.SectReputation;
                    foreach(NPC orphan in fatalSegments){orphan.NPCLoot();orphan.ModNPC.AI();Check(!orphan.active,"registered remaining fatal chain cleaned after head death");}Check(RewardCount()==actualReward&&DownedBossSystem.SectReputation==fatalReputation&&NPC.killCount.SequenceEqual(originalHeadKills),"registered dead body/tail cannot repeat head rewards or progress");
                    foreach(Item testReward in Main.item.Where(item=>item?.active==true&&!beforeFatalItems.Contains(item)))testReward.active=false;
                }
            } finally {Main.GameMode=originalGameMode;progressSystem.LoadWorldData(originalProgress);}
            for(int minionCase=0;minionCase<3;minionCase++) {
                int minionParentIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaMinionBindingAudit"),x,y+48,ModContent.NPCType<SpiritVeinWyrm>());Check(minionParentIndex>=0&&minionParentIndex<Main.maxNPCs,"registered minion protection parent created");NPC minionParent=Main.npc[minionParentIndex];owned.Add(minionParent);minionParent.target=0;
                int minionHeadIndex=NPC.NewNPC(minionParent.GetSource_FromAI(),x+64,y+48,ModContent.NPCType<ShatteredJadeWyrmMinion>());Check(minionHeadIndex>=0&&minionHeadIndex<Main.maxNPCs,"registered minion protection head created");NPC minionHead=Main.npc[minionHeadIndex];owned.Add(minionHead);minionHead.target=0;byte[] minionBirth;using(var minionWire=new MemoryStream()){minionHead.ModNPC.SendExtraAI(new BinaryWriter(minionWire));minionBirth=minionWire.ToArray();}Check(minionBirth.Length==34,"registered minion full source/link packet unchanged");
                minionHead.ai[3]=200;Check(minionHead.ModNPC.PreAI(),"registered minion observes advanced age");minionHead.ai[3]=0;Check(minionHead.ModNPC.PreAI()&&minionHead.ai[3]>=200,"registered minion old AI age cannot extend lifetime");
                if(minionCase<2){var switchedMinion=(byte[])minionBirth.Clone();if(minionCase==0)BitConverter.GetBytes((short)((minionParentIndex+1)%Main.maxNPCs)).CopyTo(switchedMinion,0);else BitConverter.GetBytes(BitConverter.ToInt64(minionBirth,2)+1).CopyTo(switchedMinion,2);minionHead.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(switchedMinion)));minionHead.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(minionBirth)));Check(!minionHead.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot)&&minionHead.ModNPC.CanBeHitByItem(Main.player[0],new Item())==false,"registered minion rejected source cannot reactivate");}
                else{minionHead.ai[3]=900;Check(!minionHead.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered minion observes expiry before AI");minionHead.ai[3]=0;Check(!minionHead.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered expired minion rejects old age state");}
                int[] minionKillCount=(int[])NPC.killCount.Clone();int minionFatal=minionHead.StrikeNPC(new NPC.HitInfo{Damage=minionHead.life+100,HitDirection=1},noPlayerInteraction:true);Check(minionFatal>0&&!minionHead.active&&NPC.killCount.SequenceEqual(minionKillCount)&&!Main.item.Any(item=>item?.active==true&&!previousItems.Contains(item)),"registered invalid minion native fatal strike has no rewards");minionParent.active=false;
            }
            foreach(var rewardWormTypes in new[]{(ModContent.NPCType<SpiritVeinWyrm>(),ModContent.NPCType<SpiritVeinWyrmBody>(),ModContent.NPCType<SpiritVeinWyrmTail>()),(ModContent.NPCType<ThunderMarshJiao>(),ModContent.NPCType<ThunderMarshJiaoBody>(),ModContent.NPCType<ThunderMarshJiaoTail>())}) {
                int rewardWormIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaWormRewardAudit"),x,y+48,rewardWormTypes.Item1);Check(rewardWormIndex>=0&&rewardWormIndex<Main.maxNPCs,"registered reward-test worm head created");NPC rewardWormHead=Main.npc[rewardWormIndex];owned.Add(rewardWormHead);rewardWormHead.target=0;var beforeRewardWorm=Main.npc.Where(npc=>npc.active).ToHashSet();
                try{Check(SegmentedWormAI.TrySpawnChain(rewardWormHead,rewardWormTypes.Item2,rewardWormTypes.Item3,2),"registered reward-test worm chain created");}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeRewardWorm.Contains(npc)&&!owned.Contains(npc)));}
                NPC[] rewardSegments=Main.npc.Where(npc=>npc.active&&!beforeRewardWorm.Contains(npc)).ToArray();Check(rewardSegments.Length==3&&rewardWormHead.ModNPC.PreKill()&&rewardSegments.All(npc=>!npc.ModNPC.PreKill()),"registered worm reward path belongs only to head");
                NPC sharedHitSegment=rewardSegments[0];Check(sharedHitSegment.ModNPC.CanBeHitByItem(Main.player[0],new Item())==null,"registered valid segment accepts shared-health attacks");int beforeSharedHeadLife=rewardWormHead.life;Main.player[0].ApplyDamageToNPC(sharedHitSegment,100,0,1,false,DamageClass.Generic);Check(rewardWormHead.active&&rewardWormHead.life<beforeSharedHeadLife,"registered native player strike through segment damages living head");rewardWormHead.active=false;
                foreach(NPC rewardSegment in rewardSegments){Check(rewardSegment.ModNPC.CanBeHitByItem(Main.player[0],new Item())==false&&rewardSegment.ModNPC.CanBeHitByProjectile(new Projectile())==false,"registered orphan segment rejects incoming hits before AI");rewardSegment.NPCLoot();Check(!Main.item.Any(item=>item?.active==true&&!previousItems.Contains(item)),"registered segment native loot path yields no independent rewards");rewardSegment.active=false;}
            }
            var bindingPairs=new[]{
                (ModContent.NPCType<MoonboneImmortal>(),ModContent.NPCType<ArchivedImmortalSoul>(),0),
                (ModContent.NPCType<TribulationCloudAvatar>(),ModContent.NPCType<TribulationCloudling>(),0),
                (ModContent.NPCType<FormlessSwordSoul>(),ModContent.NPCType<ObsessedSwordCultivator>(),5),
                (ModContent.NPCType<GreenwoodMedicineKingEcho>(),ModContent.NPCType<HerbGardenVineSpirit>(),0),
                (ModContent.NPCType<BrokenHeavenInspector>(),ModContent.NPCType<CelestialPuppet>(),0)};
            foreach(var deathPair in bindingPairs.Append((ModContent.NPCType<BlackFurnaceIronGolem>(),ModContent.NPCType<IronShardSpirit>(),0))) {
                int deathParentIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaCompanionDeathAudit"),x,y+48,deathPair.Item1);Check(deathParentIndex>=0&&deathParentIndex<Main.maxNPCs,"registered source-death parent created");NPC deathParent=Main.npc[deathParentIndex];owned.Add(deathParent);deathParent.target=0;
                int deathChildIndex=NPC.NewNPC(deathParent.GetSource_FromAI(),x+64,y+48,deathPair.Item2,ai0:deathParentIndex);Check(deathChildIndex>=0&&deathChildIndex<Main.maxNPCs,"registered source-death companion created");NPC deathChild=Main.npc[deathChildIndex];owned.Add(deathChild);deathChild.target=0;
                Check(deathChild.ModNPC.CanBeHitByItem(Main.player[0],new Item())==null&&deathChild.ModNPC.CanBeHitByProjectile(new Projectile())==null,"registered valid source retains incoming hit behavior");deathParent.active=false;
                Check(deathChild.ModNPC.CanBeHitByItem(Main.player[0],new Item())==false&&deathChild.ModNPC.CanBeHitByProjectile(new Projectile())==false,"registered source loss blocks incoming hit before AI");int[] previousCompanionKills=(int[])NPC.killCount.Clone();int deathResult=deathChild.StrikeNPC(new NPC.HitInfo{Damage=deathChild.life+100,HitDirection=1},noPlayerInteraction:true);
                Check(deathResult>0&&!deathChild.active,"registered native lethal strike cleans invalid source companion");Check(NPC.killCount.SequenceEqual(previousCompanionKills)&&!Main.item.Any(item=>item?.active==true&&!previousItems.Contains(item)),"registered invalid source fatal strike gives no items or kill progression");
            }
            foreach(var pair in bindingPairs)foreach(int switchKind in new[]{0,1,2}) {
                int bindingParentSlot=NPC.NewNPC(new EntitySource_Misc("XianXiaAllSummonBindingAudit"),x,y+48,pair.Item1);Check(bindingParentSlot>=0&&bindingParentSlot<Main.maxNPCs,"registered companion binding parent created");NPC bindingParent=Main.npc[bindingParentSlot];owned.Add(bindingParent);bindingParent.target=0;
                int bindingChildSlot=NPC.NewNPC(bindingParent.GetSource_FromAI(),x+64,y+48,pair.Item2,ai0:bindingParentSlot);Check(bindingChildSlot>=0&&bindingChildSlot<Main.maxNPCs,"registered companion binding child created");NPC bindingChild=Main.npc[bindingChildSlot];owned.Add(bindingChild);bindingChild.target=0;
                byte[] birthBindingWire;using(var bindStream=new MemoryStream()){bindingChild.ModNPC.SendExtraAI(new BinaryWriter(bindStream));birthBindingWire=bindStream.ToArray();}Check(birthBindingWire.Length==13+pair.Item3&&bindingChild.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered companion native source and wire size valid");
                var agedBindingWire=(byte[])birthBindingWire.Clone();BitConverter.GetBytes((short)200).CopyTo(agedBindingWire,pair.Item3+11);bindingChild.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(agedBindingWire)));bindingChild.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(birthBindingWire)));using(var ageStream=new MemoryStream()){bindingChild.ModNPC.SendExtraAI(new BinaryWriter(ageStream));Check(BitConverter.ToInt16(ageStream.ToArray(),pair.Item3+11)==200,"registered all companion types reject old lifetime extension");}
                var switchedBinding=(byte[])birthBindingWire.Clone();if(switchKind==0)switchedBinding[pair.Item3]=0;else if(switchKind==1)BitConverter.GetBytes((short)((bindingParentSlot+1)%Main.maxNPCs)).CopyTo(switchedBinding,pair.Item3+1);else BitConverter.GetBytes(BitConverter.ToInt64(birthBindingWire,pair.Item3+3)+1).CopyTo(switchedBinding,pair.Item3+3);
                bindingChild.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(switchedBinding)));Check(!bindingChild.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered all companion types immediately reject source identity switch");bindingChild.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(birthBindingWire)));Check(!bindingChild.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered all companion types cannot reactivate rejected identity");
                Check(!bindingChild.ModNPC.PreAI()&&!bindingChild.active,"registered rejected companion cleaned without death path");bindingParent.active=false;
            }
            int rewardFurnaceIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaReplenishedRewardAudit"),x,y+48,ModContent.NPCType<BlackFurnaceIronGolem>());Check(rewardFurnaceIndex>=0&&rewardFurnaceIndex<Main.maxNPCs,"registered reward-test furnace created");NPC rewardFurnace=Main.npc[rewardFurnaceIndex];owned.Add(rewardFurnace);rewardFurnace.target=0;
            for(int rewardTrial=0;rewardTrial<20;rewardTrial++){
                int rewardShardIndex=NPC.NewNPC(rewardFurnace.GetSource_FromAI(),x+64,y+48,ModContent.NPCType<IronShardSpirit>(),ai0:rewardFurnaceIndex);Check(rewardShardIndex>=0&&rewardShardIndex<Main.maxNPCs,"registered replenished reward-test shard created");NPC rewardShard=Main.npc[rewardShardIndex];owned.Add(rewardShard);rewardShard.target=0;
                Check(rewardShard.ModNPC.CanBeHitByItem(Main.player[0],new Item())==null&&!rewardShard.ModNPC.PreKill(),"registered valid summoned shard can be fought without loot path");int rewardDamage=rewardShard.StrikeNPC(new NPC.HitInfo{Damage=rewardShard.life+100,HitDirection=1},noPlayerInteraction:true);Check(rewardDamage>0&&!rewardShard.active&&rewardFurnace.active,"registered valid-source summoned shard can die normally");Check(!Main.item.Any(item=>item?.active==true&&!previousItems.Contains(item)),"registered repeated summoned shard kills produce no items or coins");
            }
            rewardFurnace.active=false;
            int furnaceSiteIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaFurnaceSiteAudit"),x,y+48,ModContent.NPCType<BlackFurnaceIronGolem>());
            Check(furnaceSiteIndex>=0&&furnaceSiteIndex<Main.maxNPCs,"registered furnace spawn-site parent created");NPC furnaceSiteNpc=Main.npc[furnaceSiteIndex];owned.Add(furnaceSiteNpc);furnaceSiteNpc.target=0;var furnaceSiteBoss=(BlackFurnaceIronGolem)furnaceSiteNpc.ModNPC;var beforeFurnaceSite=Main.npc.Where(npc=>npc.active).ToHashSet();
            try{for(int furnaceBatch=0;furnaceBatch<5;furnaceBatch++)furnaceSiteBoss.SpawnShardAdds();}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeFurnaceSite.Contains(npc)&&!owned.Contains(npc)));}
            NPC[] registeredShards=Main.npc.Where(npc=>npc.active&&!beforeFurnaceSite.Contains(npc)).ToArray();Check(registeredShards.Length==6,"registered safe furnace batches respect six-shard cap");
            foreach(NPC registeredShard in registeredShards){
                Check(registeredShard.ModNPC is IronShardSpirit&&registeredShard.width==48&&registeredShard.height==48,"registered actual shard body size");
                Check(Math.Abs(registeredShard.Center.X-furnaceSiteNpc.Center.X)<=60&&Math.Abs(registeredShard.Bottom.Y-furnaceSiteNpc.Center.Y)<=40,"registered NewNPC center/bottom fits sampled area");
                Check(!Collision.SolidCollision(registeredShard.position,48,48,true)&&!Collision.LavaCollision(registeredShard.position,48,48),"registered entire shard body is terrain-safe");
                Check(registeredShard.target==0&&registeredShard.netUpdate,"registered safe shard target synchronized");
                byte[] shardBirthPacket;
                using(var shardPacket=new MemoryStream()){registeredShard.ModNPC.SendExtraAI(new BinaryWriter(shardPacket));shardBirthPacket=shardPacket.ToArray();Check(shardPacket.Length==13&&BitConverter.ToInt64(shardBirthPacket,3)==furnaceSiteBoss.SummonSession,"registered terrain-safe shard captures furnace encounter source");}
                Check(registeredShard.ai[3]==-45&&!registeredShard.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered shard born in harmless warning");
                for(int birthFrame=1;birthFrame<=45;birthFrame++){
                    Check(!registeredShard.ModNPC.PreAI()&&registeredShard.active&&!registeredShard.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered shard exact forty-five frame warning");
                    registeredShard.ModNPC.PostAI();Check(registeredShard.velocity==Vector2.Zero&&registeredShard.localAI[0]==0,"registered shard warning suppresses post-AI swarm dash");
                }
                Check(registeredShard.ModNPC.PreAI()&&registeredShard.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered shard activates after full warning");
                registeredShard.ai[3]=-45;Check(registeredShard.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered old AI packet cannot refreeze active shard contact");
                Check(registeredShard.ModNPC.PreAI()&&registeredShard.ai[3]==0,"registered old warning restored to observed completion");
                using(var agedShardPacket=new MemoryStream()){registeredShard.ModNPC.SendExtraAI(new BinaryWriter(agedShardPacket));byte[] agedWire=agedShardPacket.ToArray();registeredShard.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(shardBirthPacket)));using var restoredWire=new MemoryStream();registeredShard.ModNPC.SendExtraAI(new BinaryWriter(restoredWire));Check(restoredWire.ToArray().SequenceEqual(agedWire),"registered old source packet cannot extend shard life");}
                var replacedShardSource=(byte[])shardBirthPacket.Clone();int replacementKind=registeredShard.whoAmI%3;
                if(replacementKind==0)replacedShardSource[0]=0;else if(replacementKind==1)BitConverter.GetBytes((short)((furnaceSiteIndex+1)%Main.maxNPCs)).CopyTo(replacedShardSource,1);else BitConverter.GetBytes(furnaceSiteBoss.SummonSession+1).CopyTo(replacedShardSource,3);
                registeredShard.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(replacedShardSource)));Check(!registeredShard.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered forged natural/parent/session switch immediately harmless");
                registeredShard.ModNPC.ReceiveExtraAI(new BinaryReader(new MemoryStream(shardBirthPacket)));Check(!registeredShard.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot),"registered later good source cannot revive rejected binding");
                Check(!registeredShard.ModNPC.PreAI()&&!registeredShard.active,"registered rejected summon cleared without death path");
                registeredShard.active=false;
            }
            furnaceSiteNpc.active=false;
            foreach(int jiaoStage in new[]{1,2}) {
                int jiaoIndex=NPC.NewNPC(new EntitySource_Misc("XianXiaThunderDiveAudit"),x,y+48,ModContent.NPCType<ThunderMarshJiao>());
                Check(jiaoIndex>=0&&jiaoIndex<Main.maxNPCs,"registered jiao head created");NPC jiaoNpc=Main.npc[jiaoIndex];owned.Add(jiaoNpc);jiaoNpc.target=0;jiaoNpc.life=jiaoNpc.lifeMax/(jiaoStage==1?2:4);jiaoNpc.ai[1]=(jiaoStage==1?190:150)-1;
                var jiao=(ThunderMarshJiao)jiaoNpc.ModNPC;var beforeJiaoNpcs=Main.npc.Where(npc=>npc.active).ToHashSet();var beforeJiaoProjectiles=Main.projectile.Where(projectile=>projectile.active).ToHashSet();
                void TickJiao(){try{jiao.AI();}finally{owned.AddRange(Main.npc.Where(npc=>npc.active&&!beforeJiaoNpcs.Contains(npc)&&!owned.Contains(npc)));ownedProjectiles.AddRange(Main.projectile.Where(projectile=>projectile.active&&!previousProjectiles.Contains(projectile)&&!ownedProjectiles.Contains(projectile)));}}
                TickJiao();NPC[] jiaoSegments=Main.npc.Where(npc=>npc.active&&!beforeJiaoNpcs.Contains(npc)).ToArray();Projectile[] jiaoZones=Main.projectile.Where(projectile=>projectile.active&&!beforeJiaoProjectiles.Contains(projectile)).ToArray();
                Check(jiaoSegments.Length==14,"registered jiao creates thirteen body segments and tail");Check(jiaoZones.Length==(jiaoStage==1?2:4),"registered jiao flank zone count by phase");Vector2 lockedJiaoPoint=Main.player[0].Center;
                foreach(Projectile zone in jiaoZones){float expectedOffset=zone.ModProjectile is global::XianXia.Content.Projectiles.TribulationWarningLineProjectile?160:256;
                    Check(Math.Abs(zone.Center.X-lockedJiaoPoint.X)==expectedOffset&&zone.Center.Y==lockedJiaoPoint.Y,"registered jiao fixed flank placement");Check(zone.ModProjectile.CanDamage()==false,"registered jiao zones start harmless");
                    using var packet=new MemoryStream();zone.ModProjectile.SendExtraAI(new BinaryWriter(packet));Check(packet.Length==19,"registered jiao zones carry existing source packet");}
                Main.player[0].position.X+=64;
                for(int jiaoFrame=1;jiaoFrame<=141;jiaoFrame++){
                    if(jiaoFrame>1)TickJiao();bool jiaoDanger=jiaoFrame>60&&jiaoFrame<=96;
                    Check(jiao.CanHitPlayer(Main.player[0],ref cooldownSlot)==jiaoDanger,"registered jiao exact head contact window");
                    foreach(NPC segment in jiaoSegments)Check(segment.ModNPC.CanHitPlayer(Main.player[0],ref cooldownSlot)==jiaoDanger,"registered jiao entire chain contact window before segment AI");
                    Check(Main.projectile.Count(projectile=>projectile.active&&!beforeJiaoProjectiles.Contains(projectile))==jiaoZones.Length,"registered jiao no overlapping ordinary births");
                }
                Check(jiaoNpc.ai.All(value=>value==0),"registered jiao full recovery clears attack clocks");
                foreach(Projectile zone in jiaoZones)zone.active=false;foreach(NPC segment in jiaoSegments)segment.active=false;jiaoNpc.active=false;Main.player[0].position.X-=64;
            }
            // Run actual registered item/skill updates after the existing boss scenarios.
            Player wheelActor = Main.player[0];
            var wheelState = wheelActor.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
            var wheelSnapshot = wheelState.CaptureSnapshot();
            int wheelOldSlot = wheelActor.selectedItem, wheelOldRequest = wheelState.skillRequestCooldown;
            int wheelOldMode = Main.netMode, wheelOldLocal = Main.myPlayer;
            Item wheelOldItem = wheelActor.inventory[0];
            Vector2 wheelOldCenter = wheelActor.Center, wheelOldVelocity = wheelActor.velocity;
            float wheelOldMultiplier = wheelState.spiritualEnergyCostMultiplier;
            int[] wheelOldCounts = (int[])wheelActor.ownedProjectileCounts.Clone();
            var wheelOldText = Main.combatText;
            bool wheelOldDead = wheelActor.dead;
            try {
                Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
                Main.combatText = Enumerable.Range(0, 100).Select(_ => new CombatText { active = true }).ToArray();
                wheelActor.Center = new Vector2(x, Main.maxTilesY * 8f); wheelActor.velocity = Vector2.Zero;
                wheelActor.dead = false;
                wheelActor.selectedItem = 0;
                var wheelItem = new Item(); wheelItem.SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.Weapons.FormlessSwordWheel>());
                wheelActor.inventory[0] = wheelItem;
                var wheelGrowth = wheelItem.GetGlobalItem<global::XianXia.Common.Items.RefinedArtifact>();
                wheelGrowth.SetLevel(3); Check(wheelGrowth.TryAwaken(), "registered wheel fixture awakened");
                wheelState.cultivationStage = global::XianXia.Common.Players.CultivationStage.NascentSoul;
                wheelState.spiritualEnergyCostMultiplier = 1;
                int wheelType = ModContent.ProjectileType<global::XianXia.Content.Projectiles.FormlessSwordWheelProjectile>();
                Projectile[] CastRegisteredWheels() {
                    wheelState.spiritualEnergy = 100; wheelState.activeSkillCooldown = 0; wheelState.skillRequestCooldown = 0;
                    var beforeWheels = Main.projectile.Where(projectile => projectile.active).ToHashSet();
                    try { ArtifactSkillTransactions.HandleRequest(wheelActor, ArtifactSkill.WheelFormation, 0, wheelItem.type, wheelActor.MountedCenter + new Vector2(1000, 0)); }
                    finally { ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active && !beforeWheels.Contains(projectile) && !ownedProjectiles.Contains(projectile))); }
                    Projectile[] result = Main.projectile.Where(projectile => projectile.active && !beforeWheels.Contains(projectile)).ToArray();
                    Check(result.Length == 5 && result.All(projectile => projectile.type == wheelType && projectile.owner == 0 && projectile.netUpdate), "registered skill creates exactly five owned dirty wheels");
                    Check(wheelState.spiritualEnergy == 64 && wheelState.activeSkillCooldown == 1200, "registered wheel skill actual energy and shared cooldown");
                    int wheelDamage = (int)(wheelActor.GetWeaponDamage(wheelItem) * 1.5f);
                    for (int index = 0; index < result.Length; index++) {
                        Check(result[index].damage == wheelDamage && Vector2.DistanceSquared(result[index].velocity, new Vector2(14, 0).RotatedBy((index - 2) * 0.3)) < 0.001f
                            && result[index].width == 80 && result[index].height == 80 && result[index].penetrate == 5 && result[index].timeLeft == 150, "registered wheel skill actual damage fan and defaults");
                    }
                    return result;
                }
                Projectile[] lifetimeWheels = CastRegisteredWheels();
                for (int frame = 1; frame <= 150; frame++) foreach (Projectile wheel in lifetimeWheels) {
                    wheel.Update(wheel.whoAmI);
                    Check(wheel.active == (frame < 150) && wheel.timeLeft == 150 - frame, "native five-wheel full lifetime and natural expiry");
                }
                Check(lifetimeWheels.All(wheel => wheel.ModProjectile.CanDamage() == false), "native expired wheels cannot damage");
                Projectile[] deathWheels = CastRegisteredWheels();
                wheelActor.dead = true;
                Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers;
                foreach (Projectile wheel in deathWheels) {
                    Check(wheel.ModProjectile.CanDamage() == false, "native owner death blocks wheel contact before update");
                    wheel.Update(wheel.whoAmI);
                    Check(wheel.active && !wheel.friendly && wheel.hide && wheel.velocity == Vector2.Zero && wheel.timeLeft == 5 && !wheel.netUpdate, "native server wheel cancellation stops and clears dirty state");
                    using var bytes = new MemoryStream(); wheel.ModProjectile.SendExtraAI(new BinaryWriter(bytes));
                    Check(bytes.ToArray().SequenceEqual(new byte[] { 1 }), "registered cancelled wheel serializes one-byte state");
                    wheel.ModProjectile.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 })));
                }
                wheelActor.dead = false;
                Check(deathWheels.All(wheel => wheel.ModProjectile.CanDamage() == false), "native old active packet and revived owner cannot restore cancelled wheels");
                for (int frame = 2; frame <= 6; frame++) foreach (Projectile wheel in deathWheels) {
                    wheel.Update(wheel.whoAmI);
                    Check(wheel.active == (frame < 6) && wheel.timeLeft == 6 - frame, "native cancelled five wheels expire within six updates");
                }
                // Keep the throw preview free of prior audit bosses; no NPC AI/world tick runs here.
                foreach (NPC auditNpc in owned) auditNpc.active = false;
                Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
                wheelActor.Center = new Vector2(x, 400); wheelActor.velocity = Vector2.Zero;
                Check(!Collision.SolidCollision(wheelActor.Center - new Vector2(64), 128, 128, true), "registered hammer preview has clear terrain");
                var hammerItem = new Item(); hammerItem.SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.Weapons.BlackFurnaceWarhammer>());
                wheelActor.inventory[0] = hammerItem;
                var hammerGrowth = hammerItem.GetGlobalItem<global::XianXia.Common.Items.RefinedArtifact>();
                hammerGrowth.SetLevel(3); Check(hammerGrowth.TryAwaken(), "registered hammer fixture awakened");
                wheelState.spiritualEnergy = 100; wheelState.activeSkillCooldown = 0; wheelState.skillRequestCooldown = 0;
                var beforeHammers = Main.projectile.Where(projectile => projectile.active).ToHashSet();
                try { ArtifactSkillTransactions.HandleRequest(wheelActor, ArtifactSkill.HammerVolley, 0, hammerItem.type, wheelActor.MountedCenter + new Vector2(1000, 0)); }
                finally { ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active && !beforeHammers.Contains(projectile) && !ownedProjectiles.Contains(projectile))); }
                int hammerType = ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceHammerProjectile>();
                int impactType = ModContent.ProjectileType<global::XianXia.Content.Projectiles.FurnaceImpactBurst>();
                Projectile[] hammers = Main.projectile.Where(projectile => projectile.active && !beforeHammers.Contains(projectile)).ToArray();
                Check(hammers.Length == 3 && hammers.All(projectile => projectile.type == hammerType && projectile.owner == 0 && projectile.netUpdate), "registered hammer skill creates three owned dirty hammers");
                Check(wheelState.spiritualEnergy == 70 && wheelState.activeSkillCooldown == 1200, "registered hammer actual cost and shared cooldown");
                int hammerDamage = (int)(wheelActor.GetWeaponDamage(hammerItem) * ArtifactSkillRules.HammerDamageMultiplier);
                var priorImpactIdentities = Main.projectile.Where(projectile => projectile.active && projectile.type == impactType).Select(projectile => (projectile.owner, projectile.identity)).ToHashSet();
                for (int index = 0; index < hammers.Length; index++) {
                    Projectile hammer = hammers[index];
                    Vector2 expectedVelocity = new Vector2(9, 0).RotatedBy((index - 1) * 0.24);
                    Check(hammer.damage == hammerDamage && Vector2.DistanceSquared(hammer.velocity, expectedVelocity) < 0.001f && hammer.width == 32 && hammer.height == 32
                        && hammer.penetrate == 1 && hammer.tileCollide && hammer.timeLeft == 90, "registered hammer skill damage fan and normal body");
                    hammer.Update(hammer.whoAmI);
                    Check(hammer.active && hammer.timeLeft == 89 && Math.Abs(hammer.velocity.Y - expectedVelocity.Y - 0.25f) < 0.001f, "native hammer first update retains gravity and lifetime");
                    var hammerMod = hammer.ModProjectile;
                    try { hammer.Kill(); hammerMod.OnKill(0); }
                    finally { ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active && !previousProjectiles.Contains(projectile) && !ownedProjectiles.Contains(projectile))); }
                    Check(Main.projectile.Count(projectile => projectile.active && projectile.type == impactType && !priorImpactIdentities.Contains((projectile.owner, projectile.identity))) == index + 1,
                        "native hammer Kill and repeated callback produce exactly one impact each");
                }
                Projectile[] impacts = Main.projectile.Where(projectile => projectile.active && projectile.type == impactType && !priorImpactIdentities.Contains((projectile.owner, projectile.identity))).ToArray();
                Check(impacts.Length == 3 && impacts.All(projectile => projectile.owner == 0 && projectile.damage == Math.Max(1, hammerDamage / 2)
                    && projectile.width == 96 && projectile.height == 96 && projectile.timeLeft == 10 && projectile.usesLocalNPCImmunity && projectile.localNPCHitCooldown == -1), "native three half-damage impact defaults");
                int impactDummyIndex = NPC.NewNPC(new EntitySource_Misc("XianXiaHammerImpactAudit"), x, 400, NPCID.BlueSlime);
                Check(impactDummyIndex >= 0 && impactDummyIndex < Main.maxNPCs, "native impact target created");
                NPC impactDummy = Main.npc[impactDummyIndex]; owned.Add(impactDummy);
                impactDummy.damage = 0; impactDummy.life = impactDummy.lifeMax = 10000; impactDummy.defense = 0;
                foreach (Projectile impact in impacts) {
                    impactDummy.life = 10000; impactDummy.Center = impact.Center; impactDummy.velocity = Vector2.Zero;
                    int firstImpactLife = 10000;
                    for (int frame = 1; frame <= 10; frame++) {
                        Array.Clear(impactDummy.immune);
                        if (frame >= 3) Array.Clear(impact.localNPCImmunity);
                        Check(frame <= 2 ? impact.ModProjectile.CanDamage() == null : impact.ModProjectile.CanDamage() == false, "native impact exact two-update damage window");
                        impact.Update(impact.whoAmI);
                        Check(impact.active == (frame < 10) && impact.timeLeft == 10 - frame, "native impact full ten-update lifetime");
                        if (frame == 1) {
                            firstImpactLife = impactDummy.life;
                            Check(firstImpactLife < 10000 && impact.localNPCImmunity[impactDummyIndex] == -1, "native impact damages target and installs per-projectile immunity");
                        } else Check(impactDummy.life == firstImpactLife, "native impact cannot hit same NPC twice or damage after active window");
                    }
                    Check(impactDummy.active && impactDummy.life > 0, "native impact target remains alive without kill rewards");
                }
                impactDummy.active = false;
                var occupiedForCapacity = Main.projectile.Take(Main.maxProjectiles).Where(projectile => !projectile.active).ToArray();
                try {
                    foreach (Projectile occupied in occupiedForCapacity.Take(Math.Max(0, occupiedForCapacity.Length - 2))) occupied.active = true;
                    int occupiedCount = Main.projectile.Take(Main.maxProjectiles).Count(projectile => projectile.active);
                    wheelState.spiritualEnergy = 100; wheelState.activeSkillCooldown = 0; wheelState.skillRequestCooldown = 0;
                    ArtifactSkillTransactions.HandleRequest(wheelActor, ArtifactSkill.HammerVolley, 0, hammerItem.type, wheelActor.MountedCenter + new Vector2(1000, 0));
                    Check(occupiedCount == Main.maxProjectiles - 2 && Main.projectile.Take(Main.maxProjectiles).Count(projectile => projectile.active) == occupiedCount
                        && wheelState.spiritualEnergy == 100 && wheelState.activeSkillCooldown == 0,
                        $"native hammer capacity rejection spends and creates nothing; occupied={occupiedCount}, allocatable={Main.maxProjectiles}, array={Main.projectile.Length}");
                } finally { foreach (Projectile occupied in occupiedForCapacity) occupied.active = false; }
                foreach (int sourceType in new[] { hammerType, impactType }) {
                    int cancellationIndex = Projectile.NewProjectile(new EntitySource_Misc("XianXiaFurnaceCancellationAudit"), wheelActor.Center, Vector2.Zero, sourceType, 59, 0, 0);
                    Check(cancellationIndex >= 0 && cancellationIndex < Main.maxProjectiles, "registered furnace cancellation fixture created");
                    Projectile cancelledShot = Main.projectile[cancellationIndex]; ownedProjectiles.Add(cancelledShot);
                    var previousImpacts = Main.projectile.Where(projectile => projectile.active && projectile.type == impactType).Select(projectile => (projectile.owner, projectile.identity)).ToHashSet();
                    wheelActor.dead = true;
                    Check(cancelledShot.ModProjectile.CanDamage() == false && cancelledShot.ModProjectile.CanHitNPC(null) == false, "native furnace owner death harmless before update");
                    cancelledShot.Update(cancelledShot.whoAmI);
                    Check(cancelledShot.active && cancelledShot.timeLeft == 5 && !cancelledShot.friendly && cancelledShot.hide && cancelledShot.velocity == Vector2.Zero, "native furnace cancellation starts bounded cleanup");
                    using var cancelledBytes = new MemoryStream(); cancelledShot.ModProjectile.SendExtraAI(new BinaryWriter(cancelledBytes));
                    Check(cancelledBytes.ToArray().SequenceEqual(new byte[] { (byte)(sourceType == hammerType ? 3 : 1) }), "native furnace parent and impact cancellation packet");
                    wheelActor.dead = false;
                    cancelledShot.ModProjectile.ReceiveExtraAI(new BinaryReader(new MemoryStream(new byte[] { 0 })));
                    Check(cancelledShot.ModProjectile.CanDamage() == false, "native furnace revived owner and old active packet cannot restore damage");
                    for (int frame = 2; frame <= 6; frame++) {
                        cancelledShot.Update(cancelledShot.whoAmI);
                        Check(cancelledShot.active == (frame < 6) && cancelledShot.timeLeft == 6 - frame, "native furnace cancelled entity expires within six updates");
                    }
                    Check(!Main.projectile.Any(projectile => projectile.active && projectile.type == impactType && !previousImpacts.Contains((projectile.owner, projectile.identity))), "native cancelled hammer cleanup cannot spawn free impacts");
                }
                // Find an actual clear vertical flight corridor; keep the normal terrain collision flag.
                Vector2 expiryPoint = Vector2.Zero;
                foreach (int offset in new[] { 0, 256, -256, 512, -512 }) {
                    Vector2 candidate = new Vector2(x + offset, 200), cursor = candidate; float verticalSpeed = 0; bool clear = true;
                    for (int frame = 1; frame <= 90 && clear; frame++) {
                        verticalSpeed = Math.Min(12, verticalSpeed + 0.25f); cursor.Y += verticalSpeed;
                        clear = !Collision.SolidCollision(cursor - new Vector2(16), 32, 32, true);
                    }
                    if (clear) { expiryPoint = candidate; break; }
                }
                Check(expiryPoint != Vector2.Zero, "native furnace expiry has clear real terrain corridor");
                int expiryIndex = Projectile.NewProjectile(new EntitySource_Misc("XianXiaFurnaceExpiryAudit"), expiryPoint, Vector2.Zero, hammerType, 59, 0, 0);
                Check(expiryIndex >= 0 && expiryIndex < Main.maxProjectiles, "registered natural-expiry hammer created");
                Projectile expiryHammer = Main.projectile[expiryIndex]; ownedProjectiles.Add(expiryHammer);
                var expiryMod = expiryHammer.ModProjectile;
                var previousExpiryImpacts = Main.projectile.Where(projectile => projectile.active && projectile.type == impactType).Select(projectile => (projectile.owner, projectile.identity)).ToHashSet();
                try {
                    for (int frame = 1; frame <= 90; frame++) {
                        expiryHammer.Update(expiryHammer.whoAmI);
                        Check(expiryHammer.active == (frame < 90) && expiryHammer.timeLeft == 90 - frame && expiryHammer.tileCollide, "native furnace full ninety-update flight and natural expiry");
                    }
                } finally { ownedProjectiles.AddRange(Main.projectile.Where(projectile => projectile.active && !previousProjectiles.Contains(projectile) && !ownedProjectiles.Contains(projectile))); }
                Projectile[] expiryImpacts = Main.projectile.Where(projectile => projectile.active && projectile.type == impactType && !previousExpiryImpacts.Contains((projectile.owner, projectile.identity))).ToArray();
                Check(expiryImpacts.Length == 1 && expiryImpacts[0].damage == 29, "native zero-lifetime hammer still releases one half-damage impact");
                expiryMod.OnKill(0);
                Check(Main.projectile.Count(projectile => projectile.active && projectile.type == impactType && !previousExpiryImpacts.Contains((projectile.owner, projectile.identity))) == 1, "native natural-expiry repeat callback cannot release second impact");
                using var expiryState = new MemoryStream(); expiryMod.SendExtraAI(new BinaryWriter(expiryState));
                Check(expiryState.ToArray().SequenceEqual(new byte[] { 2 }), "native normal expiry records release without cancellation");
                foreach (Projectile expiredImpact in expiryImpacts) expiredImpact.active = false;
                var daoSavedProgress = new TagCompound(); progressSystem.SaveWorldData(daoSavedProgress);
                Player daoSavedPlayer = Main.player[0];
                var daoSavedClients = (RemoteClient[])Netplay.Clients.Clone();
                bool daoSavedBroadcast = NetMessage.buffer[0].broadcast;
                var daoOutputSocket = new AuditOutputSocket(true);
                var daoTilePoint = wheelActor.Center.ToTileCoordinates();
                Tile daoTile = Main.tile[daoTilePoint.X + 2, daoTilePoint.Y];
                bool daoTileHadTile = daoTile.HasTile; ushort daoTileOldType = daoTile.TileType;
                try {
                    NetMessage.buffer[0].broadcast = true;
                    for (int clientIndex = 0; clientIndex < Netplay.Clients.Length; clientIndex++) {
                        Netplay.Clients[clientIndex] = new RemoteClient { Id = clientIndex, Socket = clientIndex == 0 ? daoOutputSocket : new AuditOutputSocket(), IsActive = clientIndex == 0, State = clientIndex == 0 ? 10 : 0 };
                    }
                    // Only patch the two tile fields used by NearAltar; never replace the loaded Tilemap.
                    daoTile.HasTile = true; daoTile.TileType = (ushort)ModContent.TileType<global::XianXia.Content.Tiles.Stations.DaoSeveringAltarTile>();
                    Player MakeDaoFixture(int itemType, int stones = 36) {
                        var actor = new Player { whoAmI = 0, active = true, dead = false, statLife = 100, statLifeMax2 = 100 };
                        actor.Center = wheelActor.Center; actor.selectedItem = 0; Main.player[0] = actor;
                        var daoState = actor.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>();
                        daoState.cultivationStage = global::XianXia.Common.Players.CultivationStage.DaoSevering;
                        daoState.discoveredSpiritualEnergy = true;
                        Check(daoState.TryInitializeNetwork(daoState.CaptureSnapshot()), "registered Dao fixture imports valid native player network state");
                        var seal = new Item(); seal.SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.DaoTransformationSeal>()); seal.stack = 2;
                        var target = new Item(); target.SetDefaults(itemType); Check(target.Prefix(PrefixID.Godly), "registered Dao fixture receives native combat prefix");
                        target.GetGlobalItem<global::XianXia.Common.Items.InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
                        var growth = target.GetGlobalItem<global::XianXia.Common.Items.RefinedArtifact>(); growth.SetLevel(3); Check(growth.TryAwaken(), "registered Dao fixture awakened");
                        var stone = new Item(); stone.SetDefaults(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>()); stone.stack = stones;
                        actor.inventory[0] = seal; actor.inventory[1] = target; actor.inventory[2] = stone;
                        return actor;
                    }
                    void SubmitDao(Player actor, byte route, byte prior = 0) => DaoArtifactTransactions.Handle(actor, 0, actor.inventory[0].type,
                        1, actor.inventory[1].type, actor.inventory[1].prefix, (byte)global::XianXia.Common.Items.InscribedEquipment.GetKind(actor.inventory[1]), 3, true, prior, route);
                    foreach (byte route in new byte[] { 1, 2, 3 }) {
                        Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
                        var routeProgress = new TagCompound(); progressSystem.SaveWorldData(routeProgress); routeProgress["chosenRoute"] = 0; progressSystem.LoadWorldData(routeProgress);
                        DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                        Check(DownedBossSystem.TryChooseRoute((DownedBossSystem.EndgameRoute)route), "registered Dao fixture chooses each real world route");
                        foreach (int artifactType in new[] { ModContent.ItemType<global::XianXia.Content.Items.Weapons.FormlessSwordWheel>(), ModContent.ItemType<global::XianXia.Content.Items.Weapons.BlackFurnaceWarhammer>() }) foreach (int transactionMode in new[] { NetmodeID.SinglePlayer, NetmodeID.Server }) {
                            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = 0;
                            Player actor = MakeDaoFixture(artifactType); Item target = actor.inventory[1]; int prefix = target.prefix;
                            Main.netMode = transactionMode; Main.myPlayer = transactionMode == NetmodeID.Server ? Main.maxPlayers : 0;
                            Check(EndgameRouteTransactions.NearAltar(actor), "registered Dao fixture detects station tile");
                            TagCompound[] inventoryBeforeDao = Enumerable.Range(0, 3).Select(slot => ItemIO.Save(actor.inventory[slot])).ToArray();
                            int outputBeforeDao = daoOutputSocket.Sent.Count;
                            SubmitDao(actor, route);
                            Check(transactionMode == NetmodeID.Server ? daoOutputSocket.Sent.Count > outputBeforeDao : daoOutputSocket.Sent.Count == outputBeforeDao,
                                "native Dao authority emits server reply output only in server mode");
                            var growth = target.GetGlobalItem<global::XianXia.Common.Items.RefinedArtifact>();
                            Check(actor.inventory[0].stack == 1 && actor.inventory[2].IsAir && (byte)growth.DaoRoute == route, "native Dao material transaction costs one seal and thirty-six stones");
                            Check(ReferenceEquals(actor.inventory[1], target) && target.prefix == prefix && growth.Level == 3 && growth.Awakened
                                && global::XianXia.Common.Items.InscribedEquipment.GetKind(target) == InscriptionKind.Greenwood, "native Dao preserves actual equipment prefix inscription and growth");
                            if (transactionMode == NetmodeID.Server) {
                                byte[][] inventoryPackets = daoOutputSocket.Sent.Skip(outputBeforeDao).Where(packet => packet.Length > 2 && packet[2] == MessageID.SyncEquipment).ToArray();
                                Check(inventoryPackets.Length == 3, "native Dao sends exactly three equipment packets for material and target slots; sent="
                                    + string.Join(",", daoOutputSocket.Sent.Skip(outputBeforeDao).Select(packet => packet.Length > 2 ? packet[2] : -1)));
                                var inventoryMod = ModContent.GetInstance<global::XianXia.XianXia>();
                                using var headerProbe = inventoryMod.GetPacket();
                                int modPayloadOffset = (int)headerProbe.BaseStream.Position;
                                byte[][] ownerPackets = daoOutputSocket.Sent.Skip(outputBeforeDao).Where(packet => packet.Length > modPayloadOffset && packet[modPayloadOffset] == 13).ToArray();
                                Check(ownerPackets.Length == 3, "native Dao produces three authoritative owner slot packets");
                                var oldConnection = Netplay.Connection; bool oldServerSideCharacter = Main.ServerSideCharacter;
                                try {
                                    Main.netMode = NetmodeID.MultiplayerClient; Main.ServerSideCharacter = false; Netplay.Connection = new RemoteServer();
                                    foreach (int receivingPlayer in new[] { 0, 1 }) {
                                        Main.myPlayer = receivingPlayer;
                                        var receiver = new Player { whoAmI = 0, active = true, dead = false };
                                        for (int slot = 0; slot < 3; slot++) receiver.inventory[slot] = ItemIO.Load(inventoryBeforeDao[slot]);
                                        // Favorite state is client-local: deliberately differ from the server item.
                                        if (receivingPlayer == 0) {
                                            receiver.inventory[0].favorited = true;
                                            receiver.inventory[1].favorited = true;
                                            receiver.inventory[2].favorited = true;
                                        }
                                        Main.player[0] = receiver;
                                        foreach (byte[] packet in inventoryPackets) {
                                            byte[] payload = packet.Skip(2).ToArray();
                                            var buffer = new MessageBuffer { whoAmI = 256, readBuffer = payload, reader = new BinaryReader(new MemoryStream(payload)) };
                                            try { buffer.GetData(0, payload.Length, out int messageType); Check(messageType == MessageID.SyncEquipment, "official receiver parses server equipment packet"); }
                                            finally { buffer.reader.Dispose(); }
                                        }
                                        if (receivingPlayer == 0) {
                                            Check(receiver.inventory[0].stack == 2 && receiver.inventory[2].stack == 36
                                                && global::XianXia.Common.Items.RefinedArtifact.GetDaoRoute(receiver.inventory[1]) == DownedBossSystem.EndgameRoute.None,
                                                "official normal client ignores its own vanilla equipment correction");
                                            foreach (byte[] packet in ownerPackets) {
                                                using var reader = new BinaryReader(new MemoryStream(packet.Skip(modPayloadOffset).ToArray()));
                                                inventoryMod.HandlePacket(reader, 256);
                                            }
                                            Check(receiver.inventory[0].favorited && receiver.inventory[1].favorited && !receiver.inventory[2].favorited,
                                                "native owner correction retains local favorites for surviving items and clears consumed air");
                                            Item firstAppliedTarget = receiver.inventory[1];
                                            foreach (byte[] packet in ownerPackets) {
                                                using var reader = new BinaryReader(new MemoryStream(packet.Skip(modPayloadOffset).ToArray()));
                                                inventoryMod.HandlePacket(reader, 256);
                                            }
                                            Check(ReferenceEquals(receiver.inventory[1], firstAppliedTarget) && receiver.inventory[0].stack == 1 && receiver.inventory[2].IsAir,
                                                "native owner slot correction replay is ignored without replacing canonical item");
                                        }
                                        Item receivedTarget = receiver.inventory[1];
                                        Check(receiver.inventory[0].stack == 1 && receiver.inventory[2].IsAir && receivedTarget.type == target.type && receivedTarget.prefix == prefix
                                            && global::XianXia.Common.Items.InscribedEquipment.GetKind(receivedTarget) == InscriptionKind.Greenwood
                                            && global::XianXia.Common.Items.RefinedArtifact.IsAwakened(receivedTarget) && global::XianXia.Common.Items.RefinedArtifact.GetLevel(receivedTarget) == 3
                                            && (byte)global::XianXia.Common.Items.RefinedArtifact.GetDaoRoute(receivedTarget) == route,
                                            $"official inventory receiver preserves owner/observer Dao state; receiver={receivingPlayer}");
                                    }
                                } finally { Main.player[0] = actor; Main.netMode = transactionMode; Main.myPlayer = Main.maxPlayers; Main.ServerSideCharacter = oldServerSideCharacter; Netplay.Connection = oldConnection; }
                            }
                            SubmitDao(actor, route, route);
                            Check(actor.inventory[0].stack == 1 && (byte)growth.DaoRoute == route, "native immediate Dao replay consumes nothing");
                            actor.GetModPlayer<global::XianXia.Common.Players.InscriptionPlayer>().Initialize(); SubmitDao(actor, route, route);
                            Check(actor.inventory[0].stack == 1 && (byte)growth.DaoRoute == route, "native Dao replay after request cooldown reset still consumes nothing");
                            Item loadedTarget = ItemIO.Load(ItemIO.Save(target));
                            Check(loadedTarget.type == target.type && loadedTarget.prefix == prefix && global::XianXia.Common.Items.RefinedArtifact.GetLevel(loadedTarget) == 3
                                && global::XianXia.Common.Items.RefinedArtifact.IsAwakened(loadedTarget) && (byte)global::XianXia.Common.Items.RefinedArtifact.GetDaoRoute(loadedTarget) == route
                                && global::XianXia.Common.Items.InscribedEquipment.GetKind(loadedTarget) == InscriptionKind.Greenwood, "native complete ItemIO save/load preserves Dao equipment state");
                            Item clonedTarget = target.Clone(); clonedTarget.GetGlobalItem<global::XianXia.Common.Items.RefinedArtifact>().SetLevel(2);
                            Check(growth.Level == 3 && growth.Awakened && (byte)growth.DaoRoute == route && !global::XianXia.Common.Items.RefinedArtifact.IsAwakened(clonedTarget)
                                && global::XianXia.Common.Items.RefinedArtifact.GetDaoRoute(clonedTarget) == DownedBossSystem.EndgameRoute.None, "native full item clone growth remains independent");
                            using var statePacket = new MemoryStream(); growth.NetSend(target, new BinaryWriter(statePacket));
                            Check(statePacket.ToArray().SequenceEqual(new byte[] { 3, 1, route }), "native transformed equipment retains three-byte growth wire state");
                            foreach (int invalidDao in new[] { 0, 1, 2, 3, 4 }) {
                                Player rejectedActor = MakeDaoFixture(artifactType, invalidDao == 0 ? 35 : 36); Item rejectedTarget = rejectedActor.inventory[1];
                                if (invalidDao == 1) daoTile.HasTile = false;
                                if (invalidDao == 2) DownedBossSystem.DownedBosses.Remove("old_heaven_dao_core");
                                if (invalidDao == 3) rejectedActor.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().cultivationStage = global::XianXia.Common.Players.CultivationStage.Tribulation;
                                if (invalidDao == 4) rejectedTarget.GetGlobalItem<global::XianXia.Common.Items.InscribedEquipment>().SetKind(InscriptionKind.None);
                                try {
                                    SubmitDao(rejectedActor, route);
                                    Check(rejectedActor.inventory[0].stack == 2 && rejectedActor.inventory[2].stack == (invalidDao == 0 ? 35 : 36)
                                        && global::XianXia.Common.Items.RefinedArtifact.GetDaoRoute(rejectedTarget) == DownedBossSystem.EndgameRoute.None, "native Dao invalid material station boss realm or inscription consumes nothing");
                                } finally { daoTile.HasTile = true; DownedBossSystem.DownedBosses.Add("old_heaven_dao_core"); }
                            }
                        }
                    }
                } finally { Main.player[0] = daoSavedPlayer; daoTile.HasTile = daoTileHadTile; daoTile.TileType = daoTileOldType; progressSystem.LoadWorldData(daoSavedProgress); daoSavedClients.CopyTo(Netplay.Clients, 0); NetMessage.buffer[0].broadcast = daoSavedBroadcast; }
                Check(ReferenceEquals(Main.player[0], daoSavedPlayer) && daoTile.HasTile == daoTileHadTile && daoTile.TileType == daoTileOldType
                    && Netplay.Clients.SequenceEqual(daoSavedClients)
                    && NetMessage.buffer[0].broadcast == daoSavedBroadcast
                    && (int)DownedBossSystem.ChosenRoute == daoSavedProgress.GetInt("chosenRoute") && DownedBossSystem.DownedBosses.SetEquals(daoSavedProgress.GetList<string>("downedBosses")),
                    "native Dao fixture restores player slot station fields and world route progression");
            } finally {
                wheelActor.dead = wheelOldDead; wheelActor.inventory[0] = wheelOldItem; wheelActor.selectedItem = wheelOldSlot;
                wheelActor.Center = wheelOldCenter; wheelActor.velocity = wheelOldVelocity;
                wheelState.ApplySnapshot(wheelSnapshot); wheelState.skillRequestCooldown = wheelOldRequest;
                wheelState.spiritualEnergyCostMultiplier = wheelOldMultiplier; wheelOldCounts.CopyTo(wheelActor.ownedProjectileCounts, 0);
                Main.netMode = wheelOldMode; Main.myPlayer = wheelOldLocal; Main.combatText = wheelOldText;
            }
            Check(wheelState.CaptureSnapshot() == wheelSnapshot && wheelActor.inventory[0] == wheelOldItem && wheelActor.Center == wheelOldCenter
                && wheelActor.dead == wheelOldDead && wheelActor.velocity == wheelOldVelocity && wheelActor.selectedItem == wheelOldSlot
                && wheelState.skillRequestCooldown == wheelOldRequest && wheelState.spiritualEnergyCostMultiplier == wheelOldMultiplier
                && wheelActor.ownedProjectileCounts.SequenceEqual(wheelOldCounts) && Main.netMode == wheelOldMode && Main.myPlayer == wheelOldLocal && Main.combatText == wheelOldText,
                "registered wheel fixture restores inventory resource snapshot owner counts and engine context");
            AuditBiomeScans(Check);
            AuditPillBuffs(Check);
            AuditFailureBuffs(Check);
            AuditEnemyBuffs(Check, owned);
        }
        catch (Exception exception) { error = exception.ToString(); }
        finally {
            foreach (NPC npc in owned) { npc.damage = 0; npc.active = false; }
            foreach (Projectile projectile in ownedProjectiles) { projectile.damage = 0; projectile.active = false; }
            foreach (Item item in Main.item.Where(item => item?.active == true && !previousItems.Contains(item))) item.active = false;
            Main.player[0] = original;
            Main.GameMode = originalGameMode;
            progressSystem.LoadWorldData(originalProgress);
            originalKillCounts.CopyTo(NPC.killCount, 0);
        }
        if (!ReferenceEquals(Main.player[0], original) || owned.Any(npc => npc.active)
            || ownedProjectiles.Any(projectile => projectile.active))
            error ??= "Audit cleanup did not restore the player slot and deactivate test NPCs.";
        else checks.Add("player slot restored and all test entities removed");
        if (Main.GameMode != originalGameMode || DownedBossSystem.DownedSpiritVeinWyrm != originalProgress.GetBool("downedSpiritVeinWyrm")
            || DownedBossSystem.SectReputation != originalReputation
            || DownedBossSystem.ChosenRoute != (DownedBossSystem.EndgameRoute)originalProgress.GetInt("chosenRoute")
            || !DownedBossSystem.DownedBosses.SetEquals(originalProgress.GetList<string>("downedBosses"))
            || !DownedBossSystem.ClaimedCommissions.SetEquals(originalProgress.GetList<string>("claimedCommissions"))
            || !NPC.killCount.SequenceEqual(originalKillCounts)) error ??= "Audit did not restore progress/difficulty/kill counts.";
        else checks.Add("world progression difficulty and kill counts restored");
        string directory = Path.Combine(Main.SavePath, "XianXia");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "summon-audit.json"), JsonSerializer.Serialize(new {
            schema = 1, passed = error == null, checks, error,
            limitations = "Registered AI advanced manually for medicine, tablet, inspector, core modules, archive locks and thunder jiao chains/zones; native compression and slam Projectile.Update/Projectile.Damage verify safe/unsafe regions, wall blocking and full thirty-frame slam expiry with hit-text slots occupied. Actual registered Five Wheels skill creation verifies damage, fan, cost and cooldown; all five projectiles run 150 native updates to natural expiry, and owner-death cancellation verifies server-update dirty-state handling and six-update cleanup. Registered Triple Fall creation verifies three hammers and half-damage impacts through native Kill plus duplicate callbacks; native impact updates verify a two-frame damage window, per-projectile immunity against a living vanilla NPC and ten-frame expiry, with unrelated owner immunity cleared. A full-capacity check uses only allocatable projectile slots. Furnace parent/impact owner-death cancellation runs six native updates without creating impacts; a normal hammer runs ninety updates in a clear real terrain corridor and still releases one half-damage impact at zero lifetime. These use a temporary local skill context and occupied hit-text slots, with inventory/resources/owner counts/engine context restored; no real client receives the cancellation. Actual Dao seal/stone/equipment transactions run both local and server authority modes for both recent artifacts and all three world routes, including full ItemIO save/load, full item cloning, duplicate refusal and five invalid-gate cases. Server reply output is captured by a non-networking audit ISocket; native packet creation and official vanilla client decoding run on isolated receiver copies. The ordinary non-SSC owner ignores vanilla equipment updates; the server-only mod correction applies full ItemIO data and rejects replay, while the observer applies vanilla updates. No live peer or real network transport is covered. Station qualification uses only two temporarily patched fields of a loaded world tile, with player/client slots and world progress restored; no physical multi-tile placement or real inventory transport is covered. Native loaded-world scans use six temporarily patched HasTile/TileType pairs, two nearby player copies and one distant copy: independent tile counts, non-stacking regen/pressure, registered sword resistance buff, removed-object cache invalidation through five lifecycle hooks, dead-player and client suppression. Tile fields and player/client contexts are restored. Sword resistance uses captured native message 55 and the official owner decoder, verifies the thirty-frame lease and fifteen-frame renewal threshold, avoids per-frame traffic, preserves longer buffs and immunity, stops renewal outside range, and executes registered buff updates on authority/client copies. These are direct hooks at the current world tick, not multi-tile placement, a complete buff-update/cadence cycle or real clients. Six registered pill types across all four stored grades execute real authority inventory transactions; captured message 55 packets use the official owner decoder and compare every primary effect, shared fatigue and quality regeneration duration. Duplicate and dead-player requests preserve inventory without new output. The buff delta helper synchronizes additions/duration changes, not removals or all possible authority-only buff paths. Native player damage/NPC.StrikeNPC verify shield breaks, reward suppression and ordinary worm head rewards through fatal segment hits. Progress/difficulty/banner tallies are restored; expert/master bag distribution is not covered. No full world/player ticks, rendered graphics, real clients, combat balance or complete playthrough."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Mod.Logger.Info($"Summon audit finished: {checks.Count} checks; passed={error == null}");
    }
}
