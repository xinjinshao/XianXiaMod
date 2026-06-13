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
        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.18f : 0f;
    }

    public override void PostAI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
            return;

        NPC.localAI[0]++;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] >= 120f && Main.rand.NextFloat() < 0.15f)
        {
            NPC.localAI[0] = 0f;
            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 5f;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.SpiritBoltProjectile>(),
                Math.Max(1, NPC.damage / 3),
                0.5f);
        }
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
