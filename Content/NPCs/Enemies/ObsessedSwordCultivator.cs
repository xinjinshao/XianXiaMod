using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

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
