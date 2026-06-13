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

        float distance = Vector2.Distance(NPC.Center, target.Center);

        if (target.active && !target.dead && distance < 160f)

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



        NPC.localAI[1]++;

        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead

            && NPC.localAI[1] >= 130f && distance > 160f && distance < 480f)

        {

            NPC.localAI[1] = 0f;

            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 6f;

            Projectile.NewProjectile(

                NPC.GetSource_FromAI(),

                NPC.Center,

                velocity,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.SpiritBoltProjectile>(),

                Math.Max(1, NPC.damage / 3),

                0.8f);

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.GreenwoodRoot>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.HerbDew>(), 3, 1, 2));

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

        if (NPC.localAI[0] >= 45f)

        {

            NPC.localAI[0] = 0f;

            foreach (Player player in Main.ActivePlayers)

            {

                if (Vector2.Distance(player.Center, NPC.Center) <= 128f)

                {

                    player.AddBuff(BuffID.Poisoned, 90);

                }

            }



            for (int i = 0; i < 10; i++)

            {

                float angle = MathHelper.TwoPi * i / 10f;

                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 48f;

                Dust.NewDust(NPC.Center + offset, 4, 4, DustID.Poisoned, offset.X * 0.03f, offset.Y * 0.03f, 100, default, 0.7f);

            }

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.HerbDew>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.CinnabarPowder>(), 3, 1, 2));

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



    public override void PostAI()

    {

        NPC.defense = NPC.velocity.LengthSquared() < 0.1f ? 22 : 14;

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

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.FurnaceCharcoal>(), 4, 1, 2));

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

            float swarmBonus = 1f;

            foreach (NPC other in Main.ActiveNPCs)

            {

                if (other.whoAmI != NPC.whoAmI && other.type == NPC.type && Vector2.Distance(NPC.Center, other.Center) < 200f)

                    swarmBonus += 0.25f;

            }

            Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);

            NPC.velocity = direction * (11f * swarmBonus);

            NPC.netUpdate = true;

        }



        NPC.rotation = NPC.velocity.X * 0.04f;

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(), 4, 1, 2));

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

            Vector2 predicted = target.Center + target.velocity * 30f;

            NPC.Center = predicted + new Vector2(Main.rand.NextFloat(-120f, 120f), Main.rand.NextFloat(-160f, -80f));

            Projectile.NewProjectile(

                NPC.GetSource_FromAI(),

                predicted + new Vector2(0f, -340f),

                Vector2.UnitY * 8f,

                ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),

                Math.Max(1, NPC.damage / 2),

                0f);

            NPC.netUpdate = true;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SingingThunderStoneItem>(), 5, 1, 2));

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

        bool diving = NPC.localAI[1] > 0f;

        if (target.active && !target.dead && NPC.localAI[0] >= (diving ? 30f : 140f))

        {

            NPC.localAI[0] = 0f;

            if (diving)

            {

                NPC.localAI[1] = 0f;

                NPC.velocity *= 0.3f;

            }

            else

            {

                NPC.localAI[1] = 1f;

                Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

                NPC.velocity = direction * 15f;

            }

            NPC.netUpdate = true;

        }



        if (NPC.velocity.LengthSquared() > 80f)

        {

            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, -NPC.velocity.X * 0.1f, -NPC.velocity.Y * 0.1f);

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ThunderPatternFeather>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 5, 1, 2));

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

        if (distance < 240f)

        {

            NPC.velocity += (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 0.12f;

        }



        if (NPC.life < NPC.lifeMax * 0.4f && NPC.localAI[1]++ > 180f)

        {

            NPC.localAI[1] = 0f;

            NPC.velocity += (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 6f;

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

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.StarEclipseCrystal>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenHeavenJade>(), 4, 1, 2));

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

        if (NPC.localAI[1] > 0f)

        {

            NPC.localAI[1]--;

            if (target.active && !target.dead && Vector2.Distance(target.Center, NPC.Center) < 40f)

            {

                target.velocity *= 0.6f;

            }

        }

        else if (target.active && !target.dead && NPC.localAI[0] >= 90f && Vector2.Distance(target.Center, NPC.Center) < 260f)

        {

            NPC.localAI[0] = 0f;

            Vector2 leap = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 8f;

            leap.Y -= 4f;

            NPC.velocity = leap;

            NPC.localAI[1] = 90f;

            NPC.netUpdate = true;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.AbyssDust>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.DarkBlueSpiritFluid>(), 5, 1, 2));

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



        bool guarding = target.active && !target.dead && Math.Abs(target.Center.X - NPC.Center.X) < 96f;

        if (guarding)

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

            if (guarding && NPC.localAI[1] > 0f)

            {

                NPC.localAI[1] = 0f;

                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 12f;

                NPC.damage = (int)(NPC.damage * 1.3f);

            }

            else

            {

                NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 9f;

            }

            NPC.netUpdate = true;

        }

    }



    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)

    {

        if (Math.Abs(Main.player[projectile.owner].Center.X - NPC.Center.X) < 96f)

        {

            NPC.localAI[1] = 1f;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenSwordIntent>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SectTrialToken>(), 3, 1, 2));

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

            NPC.localAI[1]++;

            if (NPC.localAI[1] % 3 == 0)

            {

                NPC.localAI[1] = 0f;

                NPC.defense = 72;

                for (int j = 0; j < 12; j++)

                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, -2f, 100, default, 0.6f);

            }

            else

            {

                NPC.defense = NPC.life < NPC.lifeMax / 2 ? 36 : 28;

            }

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



        if (NPC.defense == 72 && NPC.localAI[0] > 30f)

        {

            NPC.defense = NPC.life < NPC.lifeMax / 2 ? 36 : 28;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.TornScrollPage>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SectTrialToken>(), 4, 1, 2));

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

        int phase = (int)(NPC.localAI[0] / 130f) % 3;

        if (NPC.localAI[0] >= 130f)

        {

            NPC.localAI[0] = 0f;

            Player target = Main.player[NPC.target];

            switch (phase)

            {

                case 0:

                    NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 7f;

                    break;

                case 1:

                    NPC.velocity.Y -= 8f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)

                    {

                        Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, aim,

                            ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                            Math.Max(1, NPC.damage / 3), 0.5f);

                    }

                    break;

                case 2:

                    NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 10f;

                    break;

            }

            NPC.netUpdate = true;

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenHeavenJade>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.HeavenDaoFragment>(), 3, 1, 2));

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



        bool pushing = target.active && !target.dead && NPC.localAI[1] > 0f;

        if (pushing)

        {

            NPC.localAI[1]--;

            NPC.defense = 82;

            NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 3f;

            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[1] % 45 == 0)

            {

                Vector2 bolt = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 6f;

                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, bolt,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                    Math.Max(1, NPC.damage / 3), 1f);

            }

            if (Vector2.Distance(NPC.Center, target.Center) < 48f)

            {

                target.velocity += (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 2f;

                NPC.localAI[1] = 0f;

            }

        }

        else

        {

            NPC.defense = NPC.velocity.X == 0f ? 62 : 54;

            NPC.localAI[0]++;

            if (NPC.localAI[0] >= 160f)

            {

                NPC.localAI[0] = 0f;

                NPC.localAI[1] = 180f;

            }

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenDecreeItem>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenHeavenJade>(), 4, 1, 2));

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

            if (Main.netMode != NetmodeID.MultiplayerClient)

            {

                Vector2 predicted = target.Center + target.velocity * 18f;

                Vector2 velocity = (predicted - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;

                Projectile.NewProjectile(

                    NPC.GetSource_FromAI(),

                    NPC.Center,

                    velocity,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                    Math.Max(1, NPC.damage / 2),

                    1f);

            }

            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 12f;

            NPC.netUpdate = true;

        }



        Lighting.AddLight(NPC.Center, 0.08f, 0.18f, 0.24f);

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.Moonbone>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ColdMoonDust>(), 3, 1, 2));

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



    private Vector2[] recentPositions = new Vector2[20];

    private int positionIndex;



    public override void PostAI()

    {

        Player target = Main.player[NPC.target];

        if (!target.active || target.dead)

        {

            NPC.TargetClosest(false);

            target = Main.player[NPC.target];

        }



        recentPositions[positionIndex % recentPositions.Length] = target.Center;

        positionIndex++;



        NPC.localAI[0]++;

        if (Main.netMode != NetmodeID.MultiplayerClient && target.active && !target.dead && NPC.localAI[0] >= 95f)

        {

            NPC.localAI[0] = 0f;

            Vector2 oldPos = recentPositions[(positionIndex - 18 + recentPositions.Length) % recentPositions.Length];

            if (oldPos != Vector2.Zero)

            {

                Vector2 velocity = (target.Center - oldPos).SafeNormalize(Vector2.UnitY) * 7f;

                Projectile.NewProjectile(

                    NPC.GetSource_FromAI(),

                    NPC.Center,

                    velocity,

                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),

                    Math.Max(1, NPC.damage / 3),

                    1f);

            }

        }

    }



    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ArchiveRemnantLight>(), 4, 1, 2));

    }

}

