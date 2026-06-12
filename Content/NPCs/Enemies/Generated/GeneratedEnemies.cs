using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies.Generated;

public class HerbGardenVineSpirit : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.HerbGardenVineSpirit.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 140;
        NPC.damage = 24;
        NPC.defense = 8;
        NPC.value = 70f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (target.active && !target.dead && Vector2.Distance(NPC.Center, target.Center) < 160f)
        {
            NPC.velocity *= 0.92f;
        }

        NPC.localAI[0]++;
        if (NPC.localAI[0] >= 90f)
        {
            NPC.localAI[0] = 0f;
            if (NPC.life < NPC.lifeMax)
            {
                NPC.life += Math.Min(4, NPC.lifeMax - NPC.life);
            }

            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, 0f, -0.6f);
            }
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>(), 2, 1, 2));
    }
}


public class MiasmaFlowerMoth : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.MiasmaFlowerMoth.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 90;
        NPC.damage = 20;
        NPC.defense = 4;
        NPC.value = 60f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        NPC.velocity *= 0.985f;
        NPC.localAI[0]++;
        if (NPC.localAI[0] < 45f)
        {
            return;
        }

        NPC.localAI[0] = 0f;
        foreach (Player player in Main.ActivePlayers)
        {
            if (Vector2.Distance(player.Center, NPC.Center) <= 128f)
            {
                player.AddBuff(BuffID.Poisoned, 90);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Poisoned, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>(), 2, 1, 2));
    }
}


public class FurnaceAshGolem : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.FurnaceAshGolem.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 180;
        NPC.damage = 28;
        NPC.defense = 14;
        NPC.value = 90f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

        NPC.knockBackResist = 0.2f;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        target.AddBuff(BuffID.OnFire3, 180);
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < 6; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection * 1.2f, -1.4f);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(), 2, 1, 2));
    }
}


public class IronShardSpirit : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.IronShardSpirit.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 70;
        NPC.damage = 22;
        NPC.defense = 6;
        NPC.value = 60f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (target.active && !target.dead && NPC.localAI[0] >= 75f)
        {
            NPC.localAI[0] = 0f;
            Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
            NPC.velocity = direction * 11f;
            NPC.netUpdate = true;
        }

        NPC.rotation = NPC.velocity.X * 0.04f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(), 2, 1, 2));
    }
}


public class TribulationCloudling : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.TribulationCloudling.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 240;
        NPC.damage = 42;
        NPC.defense = 16;
        NPC.value = 120f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 150f)
        {
            NPC.localAI[0] = 0f;
            NPC.Center = target.Center + new Vector2(Main.rand.NextFloat(-180f, 180f), Main.rand.NextFloat(-160f, -80f));
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                target.Center + new Vector2(0f, -340f),
                Vector2.UnitY * 8f,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),
                Math.Max(1, NPC.damage / 2),
                0f);
            NPC.netUpdate = true;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>(), 2, 1, 2));
    }
}


public class ThunderPatternHawk : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ThunderPatternHawk.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 300;
        NPC.damage = 48;
        NPC.defense = 18;
        NPC.value = 150f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (target.active && !target.dead && NPC.localAI[0] >= 110f)
        {
            NPC.localAI[0] = 0f;
            Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            NPC.velocity = direction * 13f;
            NPC.netUpdate = true;
        }

        if (NPC.velocity.LengthSquared() > 80f)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, -NPC.velocity.X * 0.1f, -NPC.velocity.Y * 0.1f);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>(), 2, 1, 2));
    }
}


public class StarEclipsedCultivator : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.StarEclipsedCultivator.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 360;
        NPC.damage = 50;
        NPC.defense = 20;
        NPC.value = 180f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.StarAbyssRiftBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        if (!target.active || target.dead)
        {
            return;
        }

        float distance = Vector2.Distance(target.Center, NPC.Center);
        if (distance < 180f)
        {
            NPC.velocity += (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 0.12f;
        }

        NPC.localAI[0]++;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] >= 135f)
        {
            NPC.localAI[0] = 0f;
            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7.5f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                Math.Max(1, NPC.damage / 3),
                1f);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(), 2, 1, 2));
    }
}


public class StarAbyssLarva : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.StarAbyssLarva.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 260;
        NPC.damage = 46;
        NPC.defense = 18;
        NPC.value = 130f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.StarAbyssRiftBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (target.active && !target.dead && NPC.localAI[0] >= 90f && Vector2.Distance(target.Center, NPC.Center) < 260f)
        {
            NPC.localAI[0] = 0f;
            Vector2 leap = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 8f;
            leap.Y -= 4f;
            NPC.velocity = leap;
            NPC.netUpdate = true;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(), 2, 1, 2));
    }
}


public class ObsessedSwordCultivator : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ObsessedSwordCultivator.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 850;
        NPC.damage = 72;
        NPC.defense = 34;
        NPC.value = 425f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

        NPC.knockBackResist = 0.25f;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        if (target.active && !target.dead && Math.Abs(target.Center.X - NPC.Center.X) < 96f)
        {
            NPC.velocity.X *= 0.65f;
            NPC.defense = 42;
        }
        else
        {
            NPC.defense = 34;
        }

        NPC.localAI[0]++;
        if (target.active && !target.dead && NPC.localAI[0] >= 120f)
        {
            NPC.localAI[0] = 0f;
            NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 9f;
            NPC.netUpdate = true;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.SectTrialToken>(), 2, 1, 2));
    }
}


public class ScriptureArchiveEcho : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ScriptureArchiveEcho.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 720;
        NPC.damage = 66;
        NPC.defense = 28;
        NPC.value = 360f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 105f)
        {
            NPC.localAI[0] = 0f;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(12f * i)) * 6.5f;
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    Math.Max(1, NPC.damage / 4),
                    0.5f);
            }
        }

        if (NPC.life < NPC.lifeMax / 2)
        {
            NPC.defense = 36;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.SectTrialToken>(), 2, 1, 2));
    }
}


public class CelestialPuppet : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.CelestialPuppet.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 1350;
        NPC.damage = 88;
        NPC.defense = 46;
        NPC.value = 675f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

        NPC.knockBackResist = 0.15f;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.FallenHeavenPalaceBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        NPC.localAI[0]++;
        if (NPC.localAI[0] >= 80f)
        {
            NPC.localAI[0] = 0f;
            NPC.velocity.Y -= 5f;
            NPC.velocity.X *= -0.65f;
            NPC.netUpdate = true;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(), 2, 1, 2));
    }
}


public class HeavenTabletGuard : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.HeavenTabletGuard.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 1500;
        NPC.damage = 92;
        NPC.defense = 54;
        NPC.value = 750f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

        NPC.knockBackResist = 0.1f;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.FallenHeavenPalaceBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.defense = NPC.velocity.X == 0f ? 62 : 54;
        NPC.localAI[0]++;
        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 150f)
        {
            NPC.localAI[0] = 0f;
            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 8f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                Math.Max(1, NPC.damage / 3),
                1f);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(), 2, 1, 2));
    }
}


public class MoonboneCultivator : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.MoonboneCultivator.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 4200;
        NPC.damage = 160;
        NPC.defense = 72;
        NPC.value = 2100f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.MoonboneAbyssBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (target.active && !target.dead && NPC.localAI[0] >= 70f)
        {
            NPC.localAI[0] = 0f;
            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 12f;
            NPC.netUpdate = true;
        }

        Lighting.AddLight(NPC.Center, 0.08f, 0.18f, 0.24f);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.Moonbone>(), 2, 1, 2));
    }
}


public class ArchivedImmortalSoul : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ArchivedImmortalSoul.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 3600;
        NPC.damage = 150;
        NPC.defense = 64;
        NPC.value = 1800f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;

    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.MoonboneAbyssBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
        }

        NPC.localAI[0]++;
        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 95f)
        {
            NPC.localAI[0] = 0f;
            Vector2 mirrored = new Vector2(-target.velocity.X, target.velocity.Y).SafeNormalize(Vector2.UnitY) * 7f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                mirrored,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                Math.Max(1, NPC.damage / 3),
                1f);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.DaoSeveringDust>(), 2, 1, 2));
    }
}
