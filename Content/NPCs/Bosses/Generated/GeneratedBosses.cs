using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Common.Systems;

namespace XianXia.Content.NPCs.Bosses.Generated;

[AutoloadBossHead]
public class GardenWarden : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 2800;
        NPC.damage = 28;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("garden_warden");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class BlackFurnaceIronGolem : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 3200;
        NPC.damage = 34;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("black_furnace_iron_golem");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class TribulationCloudAvatar : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 4200;
        NPC.damage = 30;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("tribulation_cloud_avatar");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class ThunderMarshJiao : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 18000;
        NPC.damage = 58;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("thunder_marsh_jiao");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class AbyssalStarWomb : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 21000;
        NPC.damage = 54;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("abyssal_star_womb");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class FormlessSwordSoul : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 48000;
        NPC.damage = 72;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("formless_sword_soul");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.SectTrialToken>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class GreenwoodMedicineKingEcho : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 52000;
        NPC.damage = 66;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("greenwood_medicine_king_echo");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class HeavenTabletGuardian : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 86000;
        NPC.damage = 82;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("heaven_tablet_guardian");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class BrokenHeavenInspector : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 96000;
        NPC.damage = 92;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("broken_heaven_inspector");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class MoonboneImmortal : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 420000;
        NPC.damage = 180;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("moonbone_immortal");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.Moonbone>(), 1, 12, 24));
    }
}


[AutoloadBossHead]
public class OldHeavenDaoCore : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 96;
        NPC.lifeMax = 650000;
        NPC.damage = 220;
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
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;
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

        if (finalPhase && NPC.ai[1]++ > 180f)
        {
            NPC.ai[1] = 0f;
            NPC.velocity = desired.SafeNormalize(Vector2.UnitY) * 14f;
        }
    }

    public override void OnKill() => DownedBossSystem.MarkDowned("old_heaven_dao_core");

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.DaoSeveringDust>(), 1, 12, 24));
    }
}
