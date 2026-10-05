using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Systems;

namespace XianXia.Common.Players;

public class XianXiaPlayer : ModPlayer
{
    public const int BaseMaxSpiritualEnergy = 40;

    public int spiritualEnergy;
    public int maxSpiritualEnergy;
    public int spiritPressure;
    public int spiritualEnergyRegenBonus;
    public int tribulationTimer;
    public int tribulationIntensity;
    public int tribulationStage;
    public int tribulationComprehension;
    public float spiritualEnergyCostMultiplier = 1f;
    public bool discoveredSpiritualEnergy;
    public CultivationStage cultivationStage;
    public int arrayDeploymentCooldown;
    public int activeSkillCooldown, wardGuardTimer;
    public int skillRequestCooldown;

    public bool NetworkInitialized { get; private set; }
    public bool NetworkWasActive { get; set; }
    public bool ApplyingProgressionItem { get; set; }
    public int ProgressionItemCooldown { get; set; }
    public int BossSummonCooldown { get; set; }
    public int WeaponShotCooldown { get; set; }
    public bool ApplyingWeaponShot { get; set; }
    public bool IsResourceAuthority { get; private set; } = true;
    public int tribulationWeakness;
    public uint ResourceRevision { get; private set; }
    private CultivationSnapshot lastSentSnapshot;
    private int networkSyncTimer;
    private int regenTimer;
    private ulong lastArrayRecoveryTick = ulong.MaxValue;
    private readonly HashSet<int> clearedTribulationStages = new();

    public override void Initialize()
    {
        IsResourceAuthority = true;
        lastArrayRecoveryTick = ulong.MaxValue;
        maxSpiritualEnergy = BaseMaxSpiritualEnergy;
        spiritualEnergy = 0;
        spiritPressure = 0;
        tribulationTimer = 0;
        tribulationIntensity = 0;
        tribulationStage = 0;
        tribulationComprehension = 0;
        clearedTribulationStages.Clear();
        cultivationStage = CultivationStage.None;
        discoveredSpiritualEnergy = false;
        arrayDeploymentCooldown = 0;
        activeSkillCooldown = wardGuardTimer = 0;
        skillRequestCooldown = 0;
        tribulationKind = TribulationKind.None;
        tribulationAttempts = 0;
        regenTimer = 0;
        NetworkInitialized = false;
        NetworkWasActive = false;
        ResourceRevision = 0;
        ApplyingProgressionItem = false;
        ProgressionItemCooldown = 0;
        BossSummonCooldown = WeaponShotCooldown = 0;
        ApplyingWeaponShot = false;
        tribulationWeakness = 0;
        networkSyncTimer = 0;
        lastSentSnapshot = default;
    }

    public override void OnEnterWorld() => IsResourceAuthority = Main.netMode != NetmodeID.MultiplayerClient;

    public override void ResetEffects()
    {
        IsResourceAuthority = Main.netMode != NetmodeID.MultiplayerClient;
        maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage) + tribulationComprehension * 5;
        spiritualEnergyRegenBonus = 0;
        spiritualEnergyCostMultiplier = 1f;

        ApplyCultivationStageBonuses();

        if (spiritualEnergy > maxSpiritualEnergy)
        {
            spiritualEnergy = maxSpiritualEnergy;
        }
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient && tribulationTimer > 0)
            HandleTribulationFailure();
        return true;
    }

    public override void UpdateDead()
    {
        if (activeSkillCooldown > 0) activeSkillCooldown--;
        wardGuardTimer = 0;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        HandleTribulationFailure();
        SyncServerChanges();
    }

    public override void PostUpdate()
    {
        if (skillRequestCooldown > 0) skillRequestCooldown--;
        if (activeSkillCooldown > 0) activeSkillCooldown--;
        if (wardGuardTimer > 0) {
            wardGuardTimer--;
            Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.ArtifactWardBuff>(), 2);
        }
        if (ProgressionItemCooldown > 0) ProgressionItemCooldown--;
        if (BossSummonCooldown > 0) BossSummonCooldown--;
        if (WeaponShotCooldown > 0) WeaponShotCooldown--;
        if (Main.netMode == NetmodeID.Server && !NetworkInitialized) return;
        if (tribulationWeakness > 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) tribulationWeakness--;
            Player.AddBuff(BuffID.Weak, 2);
        }
        SyncServerChanges();
        if (arrayDeploymentCooldown > 0)
            arrayDeploymentCooldown--;
        if (!discoveredSpiritualEnergy)
        {
            return;
        }

        if (spiritPressure >= 80)
        {
            Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 2);
        }

        UpdateTribulation();

        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        regenTimer++;
        int interval = cultivationStage >= CultivationStage.QiAwakening ? Math.Max(18, 60 - (int)cultivationStage * 5) : 60;
        if (regenTimer >= interval)
        {
            regenTimer = 0;
            spiritualEnergy = Math.Clamp(spiritualEnergy + 1 + spiritualEnergyRegenBonus, 0, maxSpiritualEnergy);
            if (Main.netMode != NetmodeID.MultiplayerClient && spiritPressure > 0)
            {
                spiritPressure--;
            }
        }
    }

    public bool CanConsumeSpiritualEnergy(int amount)
    {
        return spiritualEnergy >= GetSpiritualEnergyCost(amount);
    }

    public bool CanDeployArray(int projectileType, int energyCost)
    {
        return arrayDeploymentCooldown == 0
            && Player.ownedProjectileCounts[projectileType] == 0
            && CanConsumeSpiritualEnergy(energyCost);
    }

    public bool TryDeployArray(int projectileType, int energyCost)
    {
        if (!CanDeployArray(projectileType, energyCost) || !TryConsumeSpiritualEnergy(energyCost))
            return false;
        arrayDeploymentCooldown = 60 * 8;
        return true;
    }

    public bool TryArrayRecovery(ulong tick)
    {
        if (!IsResourceAuthority || tick == lastArrayRecoveryTick) return false;
        lastArrayRecoveryTick = tick;
        return true;
    }

    public int GetSpiritualEnergyCost(int amount)
    {
        if (amount <= 0)
            return 0;
        float multiplier = float.IsFinite(spiritualEnergyCostMultiplier)
            ? Math.Clamp(spiritualEnergyCostMultiplier, 0f, 10f) : 1f;
        return (int)MathF.Ceiling(amount * multiplier);
    }

    public bool TryConsumeSpiritualEnergy(int amount)
    {
        if (!IsResourceAuthority) return false;
        amount = GetSpiritualEnergyCost(amount);
        if (amount <= 0)
        {
            return true;
        }

        discoveredSpiritualEnergy = true;
        if (spiritualEnergy < amount)
        {
            return false;
        }

        spiritualEnergy -= amount;
        return true;
    }

    public void RestoreSpiritualEnergy(int amount)
    {
        if (!IsResourceAuthority) return;
        discoveredSpiritualEnergy = true;
        spiritualEnergy = Math.Clamp(spiritualEnergy + amount, 0, maxSpiritualEnergy);
    }

    public void UnlockQiAwakening()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        discoveredSpiritualEnergy = true;
        if (cultivationStage < CultivationStage.QiAwakening)
        {
            cultivationStage = CultivationStage.QiAwakening;
            maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage);
        }
        RestoreSpiritualEnergy(30);
    }

    public bool TryAdvanceCultivation(CultivationStage targetStage, float recoveryMultiplier = 1f)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !CanAdvanceCultivation(targetStage))
        {
            return false;
        }

        discoveredSpiritualEnergy = true;
        cultivationStage = targetStage;
        maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage);
        float recovery = float.IsFinite(recoveryMultiplier) ? Math.Clamp(recoveryMultiplier, 0.75f, 1.5f) : 1f;
        RestoreSpiritualEnergy((int)MathF.Ceiling((maxSpiritualEnergy / 3) * recovery));
        spiritPressure = Math.Clamp(spiritPressure + (int)targetStage * 8, 0, 100);
        BeginTribulation(targetStage);
        if (Main.myPlayer == Player.whoAmI)
        {
            Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.Advanced", CultivationStatusText.StageName(targetStage)), 120, 245, 220);
        }
        return true;
    }

    public bool CanAdvanceCultivation(CultivationStage targetStage)
    {
        return GetBreakthroughFailure(targetStage).Length == 0;
    }

    public string GetBreakthroughFailure(CultivationStage targetStage)
    {
        return CultivationRules.GetFailure(cultivationStage, targetStage, tribulationTimer,
            clearedTribulationStages.Contains((int)cultivationStage), Main.hardMode,
            NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord,
            DownedBossSystem.DownedBosses.Contains);
    }

    public bool CanRetryTribulation() => CultivationRules.CanRetry(cultivationStage,
        tribulationTimer, clearedTribulationStages.Contains((int)cultivationStage));

    public bool TryRetryTribulation()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !CanRetryTribulation() || Player.dead)
            return false;
        BeginTribulation(cultivationStage);
        return true;
    }

    public bool CanUseBreakthroughItem(CultivationStage targetStage)
    {
        string failure = GetBreakthroughFailure(targetStage);
        bool canAdvance = failure.Length == 0;
        if (!canAdvance && Main.myPlayer == Player.whoAmI)
        {
            string boss = CultivationRules.GetRequiredBoss(targetStage);
            string name = failure == "BreakthroughRequiresTrial"
                ? Language.GetTextValue($"Mods.XianXia.Progression.TrialNames.{boss}") : "";
            Main.NewText(Language.GetTextValue($"Mods.XianXia.Progression.{failure}", name), 255, 210, 120);
        }

        return canAdvance;
    }

    public bool CanUseBossSummon(int bossType, CultivationStage requiredStage = CultivationStage.None, string requiredDownedBoss = "")
    {
        if (NPC.AnyNPCs(bossType))
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.BossAlreadyPresent"), 255, 210, 120);
            }

            return false;
        }

        if (cultivationStage < requiredStage)
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.BossStageRequired", CultivationStatusText.StageName(requiredStage)), 255, 210, 120);
            }

            return false;
        }

        if (!string.IsNullOrWhiteSpace(requiredDownedBoss) && !DownedBossSystem.DownedBosses.Contains(requiredDownedBoss))
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.BossPrerequisiteRequired", requiredDownedBoss), 255, 210, 120);
            }

            return false;
        }

        return true;
    }

    public void ReduceSpiritPressure(int amount)
    {
        if (!IsResourceAuthority) return;
        spiritPressure = Math.Clamp(spiritPressure - amount, 0, 100);
    }

    public enum TribulationKind
    {
        None = 0,
        Minor = 1,
        HeartDemon = 2,
        HeavenTablet = 3,
        DaoSevering = 4,
    }

    public TribulationKind tribulationKind;
    public int tribulationAttempts;

    private void BeginTribulation(CultivationStage stage)
    {
        if (stage < CultivationStage.Foundation)
        {
            return;
        }

        tribulationKind = stage switch
        {
            CultivationStage.Foundation or CultivationStage.GoldenCore => TribulationKind.Minor,
            CultivationStage.NascentSoul => TribulationKind.HeartDemon,
            CultivationStage.SpiritSevering => TribulationKind.HeavenTablet,
            _ => TribulationKind.DaoSevering,
        };
        tribulationIntensity = Math.Clamp((int)stage - 1, 1, 8);
        tribulationStage = (int)stage;
        tribulationTimer = tribulationKind switch
        {
            TribulationKind.HeavenTablet => 60 * (40 + tribulationIntensity * 3),
            TribulationKind.DaoSevering => 60 * (50 + tribulationIntensity * 4),
            TribulationKind.HeartDemon => 60 * (30 + tribulationIntensity * 3),
            _ => 60 * (18 + tribulationIntensity * 4),
        };
        if (tribulationAttempts > 0)
        {
            tribulationTimer = (int)(tribulationTimer * Math.Max(0.7f, 1f - tribulationAttempts * 0.1f));
            if (Main.myPlayer == Player.whoAmI)
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationInsightActive", tribulationAttempts), 180, 220, 255);
        }
        if (Main.myPlayer == Player.whoAmI)
        {
            string msg = tribulationKind switch
            {
                TribulationKind.HeavenTablet => Language.GetTextValue("Mods.XianXia.Progression.TribulationStartedHeavenTablet", CultivationStatusText.StageName(stage)),
                TribulationKind.DaoSevering => Language.GetTextValue("Mods.XianXia.Progression.TribulationStartedDaoSevering", CultivationStatusText.StageName(stage)),
                TribulationKind.HeartDemon => Language.GetTextValue("Mods.XianXia.Progression.TribulationStartedHeartDemon", CultivationStatusText.StageName(stage)),
                _ => Language.GetTextValue("Mods.XianXia.Progression.TribulationStarted", CultivationStatusText.StageName(stage)),
            };
            Main.NewText(msg, 160, 210, 255);
        }
    }

    private void UpdateTribulation()
    {
        if (tribulationTimer <= 0)
        {
            tribulationIntensity = 0;
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationPressureBuff>(), 2);
            if (tribulationKind == TribulationKind.DaoSevering)
                Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.ArchiveLockBuff>(), 3);
            return;
        }

        tribulationTimer--;
        Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationPressureBuff>(), 2);

        if (Player.HasBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>()) && Main.GameUpdateCount % 30 == 0)
        {
            ReduceSpiritPressure(2);
        }

        if (tribulationKind == TribulationKind.HeavenTablet)
        {
            int htInterval = Math.Max(60, 200 - tribulationIntensity * 15);
            if (tribulationTimer % htInterval == 0)
            {
                SpawnHeavenTabletSeal();
            }
        }
        else if (tribulationKind == TribulationKind.DaoSevering)
        {
            {
                Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.ArchiveLockBuff>(), 3);
                if (tribulationTimer % Math.Max(70, 280 - tribulationIntensity * 20) == 0)
                    SpawnDaoSeveringField();
            }
        }
        else if (tribulationKind == TribulationKind.HeartDemon)
        {
            int hdInterval = Math.Max(90, 350 - tribulationIntensity * 25);
            if (tribulationTimer % hdInterval == 0)
            {
                SpawnHeartDemonEnemy();
            }
        }
        else
        {
            int interval = Math.Max(38, 110 - tribulationIntensity * 8);
            if (tribulationTimer % interval == 0)
            {
                SpawnTribulationLightning();
            }
        }

        if (tribulationTimer == 0)
        {
            CompleteTribulation();
        }
    }

    private void CompleteTribulation()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        AdvanceResourceRevision();
        int pressureReduced = 20 + tribulationIntensity * 2;
        int energyRestored = 20 + tribulationIntensity * 8;
        int completedStage = tribulationStage;
        ReduceSpiritPressure(pressureReduced);
        RestoreSpiritualEnergy(energyRestored);
        tribulationAttempts = 0;
        bool gainedComprehension = completedStage > 0 && clearedTribulationStages.Add(completedStage);
        if (gainedComprehension)
        {
            tribulationComprehension++;
            maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage) + tribulationComprehension * 5;
        }
        if (Main.myPlayer == Player.whoAmI)
        {
            Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationCompleted", energyRestored), 120, 245, 220);
            if (gainedComprehension)
            {
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationComprehensionGained", tribulationComprehension * 5), 160, 210, 255);
            }
        }
        tribulationIntensity = 0;
        tribulationStage = 0;
        tribulationKind = TribulationKind.None;
        if (Main.netMode == NetmodeID.Server) SyncPlayer(-1, -1, false);
    }

    public void HandleTribulationFailure()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || tribulationTimer <= 0)
            return;

        tribulationTimer = 0;
        tribulationStage = 0;
        AdvanceResourceRevision();
        tribulationIntensity = 0;
        tribulationKind = TribulationKind.None;
        tribulationAttempts = Math.Min(10, tribulationAttempts + 1);
        RestoreSpiritualEnergy(maxSpiritualEnergy / 4);
        ReduceSpiritPressure(10);
        tribulationWeakness = 60 * 60 * 3;
        Player.AddBuff(BuffID.Weak, tribulationWeakness);
        Player.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.SpiritualPressureDisorderBuff>(), 60 * 2);
        if (Main.netMode == NetmodeID.Server) SyncPlayer(-1, -1, false);

        if (Main.myPlayer == Player.whoAmI)
        {
            Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationFailed"), 255, 180, 140);
            if (tribulationAttempts >= 2)
            {
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationInsightHint"), 200, 220, 255);
            }
        }
    }

    private void SpawnHeartDemonEnemy()
    {
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            return;

        float offsetX = Main.rand.NextFloat(-200f, 200f);
        float offsetY = Main.rand.NextFloat(-200f, -120f);
        int npcType = tribulationStage >= 5
            ? ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.StarEclipsedCultivator>()
            : ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.TribulationCloudling>();
        int id = NPC.NewNPC(Player.GetSource_FromThis(), (int)(Player.Center.X + offsetX), (int)(Player.Center.Y + offsetY),
            npcType);
        if (id < Main.maxNPCs)
        {
            Main.npc[id].lifeMax = (int)(Main.npc[id].lifeMax * 0.5f);
            Main.npc[id].life = Main.npc[id].lifeMax;
            Main.npc[id].damage = Math.Max(1, Main.npc[id].damage * 2 / 3);
            Main.npc[id].netUpdate = true;
        }
    }

    private void SpawnHeavenTabletSeal()
    {
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i < 4; i++)
        {
            float angle = MathHelper.PiOver2 * i;
            Vector2 pos = Player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 160f;
            int dmg = 22 + tribulationIntensity * 8;
            Projectile.NewProjectile(Player.GetSource_FromThis(), pos, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(),
                dmg, 1f, Player.whoAmI);
        }
        if (tribulationIntensity >= 5)
            Projectile.NewProjectile(Player.GetSource_FromThis(),
                Player.Center + new Vector2(0f, -96f), Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),
                30 + tribulationIntensity * 6, 1.5f, Player.whoAmI);
    }

    private void SpawnDaoSeveringField()
    {
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = Player.Center + new Vector2(Main.rand.NextFloat(-200f, 200f), Main.rand.NextFloat(-160f, -80f));
            Projectile.NewProjectile(Player.GetSource_FromThis(), pos, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(),
                28 + tribulationIntensity * 6, 1f, Player.whoAmI);
        }
    }

    private void SpawnTribulationLightning()
    {
        if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
        {
            return;
        }

        float offsetX = Main.rand.NextFloat(-240f, 240f);
        Microsoft.Xna.Framework.Vector2 position = Player.Center + new Microsoft.Xna.Framework.Vector2(offsetX, -96f);
        Microsoft.Xna.Framework.Vector2 velocity = Microsoft.Xna.Framework.Vector2.Zero;
        int damage = 18 + tribulationIntensity * 7;
        Projectile.NewProjectile(
            Player.GetSource_FromThis(),
            position,
            velocity,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),
            damage,
            0f,
            Player.whoAmI);
    }

    public static int GetMaxSpiritualEnergy(CultivationStage stage)
    {
        return stage switch
        {
            CultivationStage.None => BaseMaxSpiritualEnergy,
            CultivationStage.QiAwakening => 40,
            CultivationStage.QiCondensation => 80,
            CultivationStage.Foundation => 120,
            CultivationStage.GoldenCore => 180,
            CultivationStage.NascentSoul => 240,
            CultivationStage.SpiritSevering => 320,
            CultivationStage.Tribulation => 420,
            CultivationStage.DaoSevering => 500,
            _ => BaseMaxSpiritualEnergy
        };
    }

    private void ApplyCultivationStageBonuses()
    {
        if (cultivationStage < CultivationStage.QiAwakening)
        {
            return;
        }

        int stage = (int)cultivationStage;
        float growthMultiplier = Math.Clamp(ModContent.GetInstance<XianXiaConfig>().PermanentGrowthMultiplier, 0f, 2f);
        Player.GetDamage(DamageClass.Generic) += 0.02f * stage * growthMultiplier;
        Player.statDefense += (int)MathF.Round(stage * growthMultiplier);
        Player.moveSpeed += 0.01f * stage * growthMultiplier;
        spiritualEnergyCostMultiplier *= MathF.Max(0.72f, 1f - stage * 0.025f * growthMultiplier);

        if (cultivationStage >= CultivationStage.GoldenCore)
        {
            Player.GetCritChance(DamageClass.Generic) += (int)MathF.Round(2f * growthMultiplier);
        }

        if (cultivationStage >= CultivationStage.NascentSoul)
        {
            Player.endurance += 0.03f * growthMultiplier;
            spiritualEnergyRegenBonus += Math.Max(0, (int)MathF.Round(growthMultiplier));
        }

        if (cultivationStage >= CultivationStage.Tribulation)
        {
            Player.GetCritChance(DamageClass.Generic) += (int)MathF.Round(3f * growthMultiplier);
            Player.endurance += 0.02f * growthMultiplier;
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag["spiritualEnergy"] = spiritualEnergy;
        tag["spiritPressure"] = spiritPressure;
        tag["tribulationTimer"] = tribulationTimer;
        tag["tribulationIntensity"] = tribulationIntensity;
        tag["tribulationStage"] = tribulationStage;
        tag["tribulationComprehension"] = tribulationComprehension;
        tag["tribulationKind"] = (int)tribulationKind;
        tag["tribulationAttempts"] = tribulationAttempts;
        tag["clearedTribulationStages"] = clearedTribulationStages.ToList();
        tag["discoveredSpiritualEnergy"] = discoveredSpiritualEnergy;
        tag["cultivationStage"] = (int)cultivationStage;
        tag["saveVersion"] = 3;
        tag["activeSkillCooldown"] = activeSkillCooldown;
        tag["tribulationWeakness"] = tribulationWeakness;
        tag["arrayDeploymentCooldown"] = arrayDeploymentCooldown;
    }

    public override void LoadData(TagCompound tag)
    {
        activeSkillCooldown = Math.Clamp(tag.GetInt("activeSkillCooldown"), 0, 1200);
        wardGuardTimer = 0; // Temporary protection does not resume after loading.
        tribulationWeakness = Math.Clamp(tag.GetInt("tribulationWeakness"), 0, 10800);
        int savedStage = tag.GetInt("cultivationStage");
        cultivationStage = savedStage >= 0 && savedStage <= 8 ? (CultivationStage)savedStage : CultivationStage.None;
        spiritPressure = Math.Clamp(tag.GetInt("spiritPressure"), 0, 100);
        tribulationTimer = Math.Clamp(tag.GetInt("tribulationTimer"), 0, 6000);
        tribulationIntensity = Math.Clamp(tag.GetInt("tribulationIntensity"), 0, 8);
        tribulationStage = tag.GetInt("tribulationStage");
        tribulationKind = (TribulationKind)Math.Clamp(tag.GetInt("tribulationKind"), 0, 4);
        tribulationAttempts = Math.Clamp(tag.GetInt("tribulationAttempts"), 0, 10);
        clearedTribulationStages.Clear();
        foreach (int stage in tag.GetList<int>("clearedTribulationStages"))
        {
            if (stage >= (int)CultivationStage.Foundation && stage <= (int)cultivationStage)
                clearedTribulationStages.Add(stage);
        }
        tribulationComprehension = clearedTribulationStages.Count;
        maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage) + tribulationComprehension * 5;
        spiritualEnergy = Math.Clamp(tag.GetInt("spiritualEnergy"), 0, maxSpiritualEnergy);
        discoveredSpiritualEnergy = tag.GetBool("discoveredSpiritualEnergy") || cultivationStage > CultivationStage.None;
        arrayDeploymentCooldown = Math.Clamp(tag.GetInt("arrayDeploymentCooldown"), 0, 60 * 8);
        if (tribulationStage != (int)cultivationStage || cultivationStage < CultivationStage.Foundation
            || clearedTribulationStages.Contains(tribulationStage))
            tribulationTimer = 0;
        if (tribulationTimer == 0)
        {
            tribulationKind = TribulationKind.None;
            tribulationIntensity = 0;
            tribulationStage = 0;
        }
        else
        {
            tribulationIntensity = Math.Clamp((int)cultivationStage - 1, 1, 8);
            tribulationKind = cultivationStage switch
            {
                CultivationStage.Foundation or CultivationStage.GoldenCore => TribulationKind.Minor,
                CultivationStage.NascentSoul => TribulationKind.HeartDemon,
                CultivationStage.SpiritSevering => TribulationKind.HeavenTablet,
                _ => TribulationKind.DaoSevering
            };
        }
    }

    public CultivationSnapshot CaptureSnapshot()
    {
        ushort mask = 0;
        foreach (int stage in clearedTribulationStages) mask |= (ushort)(1 << stage);
        return new(spiritualEnergy, (byte)cultivationStage, discoveredSpiritualEnergy,
            (byte)spiritPressure, (ushort)tribulationTimer, (byte)tribulationIntensity,
            (byte)tribulationStage, (byte)tribulationKind, (byte)tribulationAttempts,
            mask, (ushort)arrayDeploymentCooldown, (ushort)tribulationWeakness, ResourceRevision,
            (ushort)activeSkillCooldown, (ushort)wardGuardTimer);
    }

    public void NotifySnapshot(CultivationSnapshot state)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
        {
            if (state.Stage > (int)cultivationStage)
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.Advanced", CultivationStatusText.StageName((CultivationStage)state.Stage)), 120, 245, 220);
            if (state.Timer > 0 && tribulationTimer == 0)
            {
                string key = state.Kind switch
                {
                    2 => "TribulationStartedHeartDemon", 3 => "TribulationStartedHeavenTablet",
                    4 => "TribulationStartedDaoSevering", _ => "TribulationStarted"
                };
                Main.NewText(Language.GetTextValue($"Mods.XianXia.Progression.{key}", CultivationStatusText.StageName((CultivationStage)state.Stage)), 160, 210, 255);
            }
            if (state.Attempts > tribulationAttempts)
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationFailed"), 255, 180, 140);
            if (state.Comprehension > tribulationComprehension)
                Main.NewText(Language.GetTextValue("Mods.XianXia.Progression.TribulationComprehensionGained", state.Comprehension * 5), 160, 210, 255);
        }
    }

    public void ApplySnapshot(CultivationSnapshot state)
    {
        spiritualEnergy = state.Energy;
        ResourceRevision = state.Revision;
        cultivationStage = (CultivationStage)state.Stage;
        discoveredSpiritualEnergy = state.Discovered;
        spiritPressure = state.Pressure;
        tribulationTimer = state.Timer;
        tribulationIntensity = state.Intensity;
        tribulationStage = state.TrialStage;
        tribulationKind = (TribulationKind)state.Kind;
        tribulationAttempts = state.Attempts;
        tribulationWeakness = state.Weakness;
        arrayDeploymentCooldown = state.ArrayCooldown;
        activeSkillCooldown = state.SkillCooldown;
        wardGuardTimer = state.WardTimer;
        clearedTribulationStages.Clear();
        for (int stage = 3; stage <= 8; stage++)
            if ((state.ClearedStages & (1 << stage)) != 0) clearedTribulationStages.Add(stage);
        tribulationComprehension = state.Comprehension;
        maxSpiritualEnergy = GetMaxSpiritualEnergy(cultivationStage) + tribulationComprehension * 5;
    }

    // Terraria characters are client-owned. Import one bounded save snapshot on
    // joining; runtime advancement and trial results cannot replace it afterward.
    public bool TryInitializeNetwork(CultivationSnapshot state)
    {
        if (NetworkInitialized || !state.IsValid()) return false;
        ApplySnapshot(state with { WardTimer = 0 });
        ResourceRevision = 0;
        NetworkInitialized = true;
        regenTimer = 0;
        return true;
    }

    public void ResetNetworkSession()
    {
        NetworkInitialized = NetworkWasActive = false;
        ProgressionItemCooldown = networkSyncTimer = 0;
        BossSummonCooldown = WeaponShotCooldown = 0;
        ApplyingWeaponShot = false;
        lastSentSnapshot = default;
    }

    public void AdvanceResourceRevision() => ResourceRevision++;

    private void SyncServerChanges()
    {
        if (Main.netMode != NetmodeID.Server || !NetworkInitialized) return;
        if (++networkSyncTimer < 30) return;
        networkSyncTimer = 0;
        CultivationSnapshot state = CaptureSnapshot();
        if (state != lastSentSnapshot) SyncPlayer(-1, -1, false);
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        XianXiaPlayer clone = (XianXiaPlayer)targetCopy;
        clone.spiritualEnergy = spiritualEnergy;
        clone.cultivationStage = cultivationStage;
        clone.arrayDeploymentCooldown = arrayDeploymentCooldown;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        // Actions are requests; clients never submit resource balances.
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        if (Main.netMode == NetmodeID.Server && !NetworkInitialized) return;
        if (Main.netMode == NetmodeID.MultiplayerClient && !newPlayer) return;
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)(Main.netMode == NetmodeID.Server ? 4 : 3));
        packet.Write((byte)Player.whoAmI);
        CultivationSnapshot state = CaptureSnapshot();
        state.Write(packet);
        packet.Send(toWho, fromWho);
        if (toWho == -1) lastSentSnapshot = state;
    }
}
