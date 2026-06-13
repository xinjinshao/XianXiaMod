using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class HeavenTabletGuard : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


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

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
