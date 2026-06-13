using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

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
