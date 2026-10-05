using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class CelestialPuppet : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


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

        AIType = NPCID.Zombie;



        NPC.knockBackResist = 0.15f;

    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(CelestialPuppet), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.FallenHeavenPalaceBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers
            || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
            NPC.TargetClosest(false);
        if (NPC.target < 0 || NPC.target >= Main.maxPlayers
            || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
        {
            NPC.localAI[0] = 0f;
            return;
        }
        if (++NPC.localAI[0] < 130f) return;
        NPC.localAI[0] = 0f;
        int phase = (int)NPC.localAI[1];
        NPC.localAI[1] = (phase + 1) % 3;
        Player target = Main.player[NPC.target];
        switch (phase)
        {
            case 0:
                NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 7f;
                break;
            case 1:
                NPC.velocity.Y -= 8f;
                Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, aim,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    Math.Max(1, NPC.damage / 3), 0.5f);
                break;
            case 2:
                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 10f;
                break;
        }
        NPC.netUpdate = true;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenHeavenJade>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.HeavenDaoFragment>(), 3, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
