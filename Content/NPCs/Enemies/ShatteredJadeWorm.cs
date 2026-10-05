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
    private byte dashSequence;
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }

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
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(ShatteredJadeWorm), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;

        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.2f : 0f;
    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }

    public override void AI()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NPC.rotation = NPC.velocity.LengthSquared() > 16f ? NPC.velocity.ToRotation() : NPC.velocity.X * 0.05f;
            return;
        }
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            if (NPC.ai[0] != 0f || NPC.velocity.X != 0f) NPC.netUpdate = true;
            NPC.ai[0] = 0f;
            NPC.velocity.X = 0f;
            return;
        }
        if (++NPC.ai[0] < 120f)
        {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(target.Center.X - NPC.Center.X) * 2.5f, 0.04f);
            NPC.rotation = NPC.velocity.X * 0.05f;
            if (NPC.ai[0] % 30f == 0f) NPC.netUpdate = true;
            return;
        }
        Vector2 dash = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 9f;
        dash.Y -= 2f;
        NPC.velocity = dash;
        NPC.rotation = NPC.velocity.ToRotation();
        NPC.ai[0] = 0f;
        dashSequence = unchecked((byte)(dashSequence + 1));
        NPC.netUpdate = true;
        DrawDash();
    }

    private void DrawDash()
    {
        if (Main.dedServ) return;
        for (int i = 0; i < 4; i++)
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, -NPC.velocity.X * 0.2f, -NPC.velocity.Y * 0.2f);
    }

    public override void SendExtraAI(System.IO.BinaryWriter writer) => writer.Write(dashSequence);

    public override void ReceiveExtraAI(System.IO.BinaryReader reader)
    {
        byte sequence = reader.ReadByte();
        if (sequence != dashSequence)
        {
            dashSequence = sequence;
            DrawDash();
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 2, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 3));
    }
}
