using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class StarEclipsedCultivator : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


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

        AIType = NPCID.Zombie;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(StarEclipsedCultivator), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.StarAbyssRiftBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.localAI[0] = NPC.localAI[1] = 0f;
            return;
        }
        Vector2 away = (NPC.Center - target.Center).SafeNormalize(Vector2.Zero);
        if (Vector2.Distance(target.Center, NPC.Center) < 240f)
        {
            NPC.velocity += away * 0.12f;
            if (NPC.localAI[0] % 30f == 0f) NPC.netUpdate = true;
        }
        if (NPC.life < NPC.lifeMax * 0.4f)
        {
            if (++NPC.localAI[1] >= 182f)
            {
                NPC.localAI[1] = 0f;
                NPC.velocity += away * 6f;
                NPC.netUpdate = true;
            }
        }
        else NPC.localAI[1] = 0f;
        if (++NPC.localAI[0] < 135f) return;
        NPC.localAI[0] = 0f;
        Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7.5f;
        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
            Math.Max(1, NPC.damage / 3), 1f);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.StarEclipseCrystal>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenHeavenJade>(), 4, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
