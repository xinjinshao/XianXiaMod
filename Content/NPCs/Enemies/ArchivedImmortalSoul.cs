using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public partial class ArchivedImmortalSoul : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


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
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(ArchivedImmortalSoul), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.MoonboneAbyssBiome>() ? 0.18f : 0f;

    }



    private readonly Vector2[] recentPositions = new Vector2[20];
    private int positionIndex, positionCount, historyTarget = -1;

    public override void PostAI()
    {
        if (!NPC.active || Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!SynchronizeSummonTarget()) return;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            ResetHistory();
            return;
        }
        if (historyTarget != NPC.target)
        {
            ResetHistory();
            historyTarget = NPC.target;
        }
        recentPositions[positionIndex] = target.Center;
        positionIndex = (positionIndex + 1) % recentPositions.Length;
        positionCount = Math.Min(positionCount + 1, recentPositions.Length);
        if (++NPC.localAI[0] < 95f) return;
        NPC.localAI[0] = 0f;
        if (positionCount < 18) return;
        Vector2 oldPos = recentPositions[(positionIndex - 18 + recentPositions.Length) % recentPositions.Length];
        Vector2 velocity = (target.Center - oldPos).SafeNormalize(Vector2.UnitY) * 7f;
        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
            Math.Max(1, NPC.damage / 3), 1f);
    }

    private void ResetHistory()
    {
        positionIndex = positionCount = 0;
        historyTarget = -1;
        NPC.localAI[0] = 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.ArchiveRemnantLight>(), 4, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
