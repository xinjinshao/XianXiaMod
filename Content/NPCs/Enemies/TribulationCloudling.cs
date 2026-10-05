using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class TribulationCloudling : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


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
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(TribulationCloudling), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.localAI[0] = 0f;
            return;
        }
        if (++NPC.localAI[0] < 150f) return;
        NPC.localAI[0] = 0f;
        Vector2 predicted = target.Center + target.velocity * 30f;
        if (!float.IsFinite(predicted.X) || !float.IsFinite(predicted.Y)) predicted = target.Center;
        predicted.X = Math.Clamp(predicted.X, 32f, Main.maxTilesX * 16f - 32f);
        predicted.Y = Math.Clamp(predicted.Y, 32f, Main.maxTilesY * 16f - 32f);
        for (int attempt = 0; attempt < 12; attempt++)
        {
            Vector2 destination = predicted + new Vector2(Main.rand.NextFloat(-120f, 120f), Main.rand.NextFloat(-160f, -80f));
            destination.X = Math.Clamp(destination.X, NPC.width / 2f + 16f, Main.maxTilesX * 16f - NPC.width / 2f - 16f);
            destination.Y = Math.Clamp(destination.Y, NPC.height / 2f + 16f, Main.maxTilesY * 16f - NPC.height / 2f - 16f);
            Vector2 topLeft = destination - new Vector2(NPC.width / 2f, NPC.height / 2f);
            if (Collision.SolidCollision(topLeft, NPC.width, NPC.height)) continue;
            NPC.Center = destination;
            NPC.velocity = Vector2.Zero;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), predicted, Vector2.Zero,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.TribulationWarningLineProjectile>(),
                Math.Max(1, NPC.damage / 2), 0f);
            NPC.netUpdate = true;
            return;
        }
        // No safe destination: skip this attack rather than teleport into terrain.
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.TribulationCloudDew>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SingingThunderStoneItem>(), 5, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
