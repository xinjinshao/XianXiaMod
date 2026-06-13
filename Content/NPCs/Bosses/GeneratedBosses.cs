using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses;

[AutoloadBossHead]
public class GardenWarden : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.GardenWarden.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 2800;
        int baseDamage = 28;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 10;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.65f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int dmg = Math.Max(18, NPC.damage / 4);
            if (phaseTwo) {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 16f, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), dmg, 1.2f, Main.myPlayer);
            }
            if (finalPhase) {
                Vector2 perp = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                for (int j = -1; j <= 1; j += 2)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + perp * j * 96f, Vector2.Zero,
                        ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), dmg, 1.2f, Main.myPlayer);
            }

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("garden_warden");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.GardenWardenMask>(), 7, 1, 1));
        
    }
}


[AutoloadBossHead]
public class BlackFurnaceIronGolem : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.BlackFurnaceIronGolem.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 3200;
        int baseDamage = 34;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 18;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.6f;
        float p3 = 0.3f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int dmg = Math.Max(18, NPC.damage / 4);
            if (phaseTwo) {
                for (int j = 0; j < 2; j++)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-60, 61), (int)NPC.Center.Y + Main.rand.Next(-40, 41),
                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.IronShardSpirit>(), ai0: NPC.whoAmI);
            }
            Vector2 side = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = (target.Center - (NPC.Center + side * i * 72f)).SafeNormalize(Vector2.UnitY) * (finalPhase ? 9f : 7f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + side * i * 72f, velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), dmg, 1.4f, Main.myPlayer);
            }

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("black_furnace_iron_golem");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BlackFurnaceIronGolemPet>(), 20, 1, 1));
        
    }
}


[AutoloadBossHead]
public class TribulationCloudAvatar : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.TribulationCloudAvatar.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 4200;
        int baseDamage = 30;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 12;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.7f;
        float p3 = 0.4f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int wDmg = Math.Max(18, NPC.damage / 3);
            int lanes = finalPhase ? 5 : phaseTwo ? 3 : 1;
            for (int i = 0; i < lanes; i++)
            {
                float offset = (i - (lanes - 1) / 2f) * 112f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + new Vector2(offset, 0f), Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(), wDmg, 1.2f, Main.myPlayer);
            }
            if (phaseTwo && NPC.ai[3]++ == 0) {
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
                    ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.TribulationCloudling>(), ai0: NPC.whoAmI);
            }
            if (finalPhase)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), Math.Max(18, NPC.damage / 4), 1.2f, Main.myPlayer);

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("tribulation_cloud_avatar");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.TribulationCloudBottle>(), 10, 1, 1));
        npcloot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.FoundationSeal>(), 1, 1, 1));
    }
}


[AutoloadBossHead]
public class ThunderMarshJiao : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ThunderMarshJiao.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 18000;
        int baseDamage = 58;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 26;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.7f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int wDmg = Math.Max(18, NPC.damage / 3);
            int lanes = finalPhase ? 5 : phaseTwo ? 3 : 1;
            for (int i = 0; i < lanes; i++)
            {
                float offset = (i - (lanes - 1) / 2f) * 112f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + new Vector2(offset, 0f), Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(), wDmg, 1.2f, Main.myPlayer);
            }
            if (finalPhase) {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), Math.Max(18, NPC.damage / 4), 1.2f, Main.myPlayer);
            }

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("thunder_marsh_jiao");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ThunderMarshJiaoWing>(), 12, 1, 1));
        
    }
}


[AutoloadBossHead]
public class AbyssalStarWomb : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.AbyssalStarWomb.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 21000;
        int baseDamage = 54;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 30;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.65f;
        float p3 = 0.3f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int ringDmg = Math.Max(18, NPC.damage / 4);
            int spokes = finalPhase ? 12 : phaseTwo ? 8 : 6;
            float rot = Main.GameUpdateCount * 0.03f;
            for (int i = 0; i < spokes; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / spokes + rot).ToRotationVector2() * (finalPhase ? 8f : 6f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, v,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), ringDmg, 1.4f, Main.myPlayer);
            }
            if (phaseTwo && Main.GameUpdateCount % 540 < 30)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 18f, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), ringDmg, 1.2f, Main.myPlayer);

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("abyssal_star_womb");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.StarEclipseCrystal>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.StarEclipseCrystal>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.AbyssalStarWombLamp>(), 12, 1, 1));
        
    }
}


[AutoloadBossHead]
public class FormlessSwordSoul : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.FormlessSwordSoul.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 48000;
        int baseDamage = 72;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 38;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.75f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int ringDmg = Math.Max(18, NPC.damage / 4);
            if (phaseTwo && NPC.ai[3]++ == 0) {
                for (int s = 0; s < 3; s++)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-80, 81), (int)NPC.Center.Y + Main.rand.Next(-40, 41),
                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.ObsessedSwordCultivator>(), ai0: NPC.whoAmI);
            }
            int spokes = finalPhase ? 10 : phaseTwo ? 8 : 6;
            float rot = Main.GameUpdateCount * 0.025f;
            for (int i = 0; i < spokes; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / spokes + rot).ToRotationVector2() * (finalPhase ? 8f : 6f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, v,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), ringDmg, 1.4f, Main.myPlayer);
            }
            if (phaseTwo)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 18f, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), ringDmg, 1.2f, Main.myPlayer);

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("formless_sword_soul");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SectTrialToken>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.FormlessSwordSoulCostume>(), 10, 1, 1));
        
    }
}


[AutoloadBossHead]
public class GreenwoodMedicineKingEcho : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.GreenwoodMedicineKingEcho.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 52000;
        int baseDamage = 66;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.7f;
        float p3 = 0.4f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int fDmg = Math.Max(18, NPC.damage / 4);
            if (phaseTwo && NPC.ai[3]++ == 0) {
                for (int f = 0; f < 3; f++)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)target.Center.X + Main.rand.Next(-120, 121), (int)target.Center.Y - 60,
                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.HerbGardenVineSpirit>(), ai0: NPC.whoAmI);
            }
            Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 16f, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), fDmg, 1.2f, Main.myPlayer);
            if (finalPhase) {
                Vector2 up = new Vector2(0, -1);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + up * 80f, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), fDmg, 1.2f, Main.myPlayer);
            }

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("greenwood_medicine_king_echo");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpringReturnPill>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.MedicineKingCauldronDecoration>(), 10, 1, 1));
        npcloot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.MedicineKingWoodHeart>(), 1, 1, 1));
    }
}


[AutoloadBossHead]
public class HeavenTabletGuardian : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.HeavenTabletGuardian.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 86000;
        int baseDamage = 82;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 48;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.75f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            if (phaseTwo && NPC.localAI[1] == 0) { NPC.localAI[1] = 1f; }
            int sDmg = Math.Max(18, NPC.damage / 3);
            int lanes = finalPhase ? 5 : phaseTwo ? 3 : 1;
            for (int i = 0; i < lanes; i++)
            {
                float offset = (i - (lanes - 1) / 2f) * 112f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + new Vector2(offset, 0f), Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(), sDmg, 1.2f, Main.myPlayer);
            }
            if (finalPhase)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), Math.Max(18, NPC.damage / 4), 1.2f, Main.myPlayer);

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("heaven_tablet_guardian");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.HeavenDaoFragment>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.HeavenDaoFragment>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SmallTabletPet>(), 20, 1, 1));
        npcloot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.HeavenTabletSeal>(), 1, 1, 1));
    }
}


[AutoloadBossHead]
public class BrokenHeavenInspector : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.BrokenHeavenInspector.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 96000;
        int baseDamage = 92;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 42;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.7f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            if (phaseTwo && NPC.localAI[1] == 0) {
                NPC.localAI[1] = 1f;
                for (int p = 0; p < 2; p++)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-80, 81), (int)NPC.Center.Y + Main.rand.Next(-40, 41),
                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.CelestialPuppet>(), ai0: NPC.whoAmI);
            }
            int sDmg = Math.Max(18, NPC.damage / 3);
            int lanes = finalPhase ? 5 : phaseTwo ? 3 : 1;
            for (int i = 0; i < lanes; i++)
            {
                float offset = (i - (lanes - 1) / 2f) * 112f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + new Vector2(offset, 0f), Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(), sDmg, 1.2f, Main.myPlayer);
            }
            if (finalPhase)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), Math.Max(18, NPC.damage / 4), 1.2f, Main.myPlayer);

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("broken_heaven_inspector");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.HeavenDaoFragment>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.HeavenDaoFragment>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.InspectorMask>(), 7, 1, 1));
        npcloot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ImperialDecreeItem>(), 1, 1, 1));
    }
}


[AutoloadBossHead]
public class MoonboneImmortal : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.MoonboneImmortal.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 420000;
        int baseDamage = 180;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 80;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.7f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int ringDmg = Math.Max(18, NPC.damage / 4);
            if (phaseTwo && NPC.localAI[1] == 0) {
                NPC.localAI[1] = 1f;
                for (int a = 0; a < 2; a++)
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-80, 81), (int)NPC.Center.Y + Main.rand.Next(-40, 41),
                        ModContent.NPCType<global::XianXia.Content.NPCs.Enemies.ArchivedImmortalSoul>(), ai0: NPC.whoAmI);
            }
            int spokes = finalPhase ? 12 : phaseTwo ? 8 : 6;
            float rot = Main.GameUpdateCount * 0.025f;
            for (int i = 0; i < spokes; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / spokes + rot).ToRotationVector2() * (finalPhase ? 8f : 6f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, v,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), ringDmg, 1.4f, Main.myPlayer);
            }
            if (phaseTwo)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 18f, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), ringDmg, 1.2f, Main.myPlayer);

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("moonbone_immortal");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.Moonbone>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.MoonboneImmortalWingAccessory>(), 16, 1, 1));
        npcloot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.StarCalamityCore>(), 1, 1, 1));
    }
}


[AutoloadBossHead]
public class OldHeavenDaoCore : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.OldHeavenDaoCore.Text"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        int baseLife = 650000;
        int baseDamage = 220;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 100;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(gold: 1);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss2;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.EncourageDespawn(30);
                return;
            }
        }
        Vector2 desired = target.Center - NPC.Center;
        float p2 = 0.75f;
        float p3 = 0.35f;
        bool phaseTwo = NPC.life < (int)(NPC.lifeMax * p2);
        bool finalPhase = NPC.life < (int)(NPC.lifeMax * p3);
        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritPressureSurge"));
        }
        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.DaoScarUnstable"));
        }
        float speed = finalPhase ? 10.5f : phaseTwo ? 8f : 5.5f;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desired.SafeNormalize(Vector2.UnitY) * speed, phaseTwo ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();
        Lighting.AddLight(NPC.Center, 0.15f, 0.12f, 0.22f);

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 72 : phaseTwo ? 110 : 150;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            int damage = Math.Max(18, NPC.damage / 3);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(12f * i)) * (phaseTwo ? 9.5f : 7.5f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.5f,
                    Main.myPlayer);
            }
        }

        NPC.ai[2]++;
        int patternInterval = finalPhase ? 150 : phaseTwo ? 210 : 270;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= patternInterval)
        {
            NPC.ai[2] = 0f;

            int module = (int)(NPC.localAI[2]++ / 180f) % 3;
            int sDmg = Math.Max(18, NPC.damage / 3);
            if (module == 0) {
                int lanes = finalPhase ? 5 : phaseTwo ? 3 : 1;
                for (int i = 0; i < lanes; i++)
                {
                    float offset = (i - (lanes - 1) / 2f) * 112f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + new Vector2(offset, 0f), Vector2.Zero,
                        ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(), sDmg, 1.2f, Main.myPlayer);
                }
            } else if (module == 1) {
                int spokes = finalPhase ? 10 : phaseTwo ? 8 : 6;
                float rot = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < spokes; i++)
                {
                    Vector2 v = (MathHelper.TwoPi * i / spokes + rot).ToRotationVector2() * (finalPhase ? 8f : 6f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, v,
                        ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(), sDmg, 1.4f, Main.myPlayer);
                }
            } else {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center + target.velocity * 16f, Vector2.Zero,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossArrayFieldProjectile>(), sDmg, 1.2f, Main.myPlayer);
            }

        }

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("old_heaven_dao_core");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 1, 16, 28));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SilentTabletDecoration>(), 1, 1, 1));
        npcloot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.RouteMaterial>(), 1, 1, 1));
    }
}
