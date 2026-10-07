using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public partial class IronShardSpirit : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


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
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(IronShardSpirit), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (!NPC.active) return;
        NPC.rotation = NPC.velocity.X * 0.04f;
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!SynchronizeSummonTarget()) return;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.localAI[0] = 0f;
            return;
        }
        if (++NPC.localAI[0] < 75f) return;
        NPC.localAI[0] = 0f;
        float swarmBonus = 1f;
        foreach (NPC other in Main.ActiveNPCs)
        {
            if (other.whoAmI == NPC.whoAmI || other.type != NPC.type
                || Vector2.Distance(NPC.Center, other.Center) >= 200f) continue;
            swarmBonus += 0.25f;
            if (swarmBonus >= 2f) break;
        }
        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * (11f * swarmBonus);
        NPC.rotation = NPC.velocity.X * 0.04f;
        NPC.netUpdate = true;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.FurnaceSlagIron>(), 4, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
