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

public class ShatteredJadeWorm : ModNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ShatteredJadeWorm"));
    }

    public override void SetDefaults()
    {
        NPC.width = 44;
        NPC.height = 20;
        NPC.damage = 14;
        NPC.defense = 4;
        NPC.lifeMax = 60;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 60f;
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0.4f;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.2f : 0f;
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
                return;
        }

        NPC.ai[0]++;
        if (NPC.ai[0] < 120f)
        {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(target.Center.X - NPC.Center.X) * 2.5f, 0.04f);
            NPC.rotation = NPC.velocity.X * 0.05f;
        }
        else
        {
            Vector2 dash = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 9f;
            dash.Y -= 2f;
            NPC.velocity = dash;
            NPC.rotation = NPC.velocity.ToRotation();
            for (int i = 0; i < 4; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, -NPC.velocity.X * 0.2f, -NPC.velocity.Y * 0.2f);
            NPC.ai[0] = 0f;
            NPC.netUpdate = true;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 2, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 3));
    }
}
