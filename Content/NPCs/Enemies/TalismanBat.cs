using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Biomes;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.NPCs.Enemies;

public class TalismanBat : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.TalismanBat"));
    }

    public override void SetDefaults()
    {
        NPC.width = 38;
        NPC.height = 26;
        NPC.damage = 13;
        NPC.defense = 2;
        NPC.lifeMax = 38;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath4;
        NPC.value = 50f;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(TalismanBat), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;

        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.localAI[0] = 0f;
            return;
        }
        if (++NPC.localAI[0] < 120f) return;
        NPC.localAI[0] = 0f;
        if (Main.rand.NextFloat() >= 0.15f) return;
        Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 5f;
        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
            ModContent.ProjectileType<global::XianXia.Content.Projectiles.EnemySpiritBoltProjectile>(),
            Math.Max(1, NPC.damage / 3), 0.5f);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 2, 1, 2));
    }
    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
