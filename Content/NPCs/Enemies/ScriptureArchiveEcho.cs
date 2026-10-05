using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class ScriptureArchiveEcho : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ScriptureArchiveEcho.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 720;

        NPC.damage = 66;

        NPC.defense = 28;

        NPC.value = 360f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Bat;

        AIType = NPCID.CaveBat;

        NPC.noGravity = true;



    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(ScriptureArchiveEcho), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (NPC.localAI[2] > 0f) NPC.localAI[2]--;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            UpdateDefense();
            return;
        }
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            NPC.localAI[0] = NPC.localAI[1] = NPC.localAI[2] = 0f;
            UpdateDefense();
            return;
        }
        UpdateDefense();
        if (++NPC.localAI[0] < 105f) return;
        NPC.localAI[0] = 0f;
        if (++NPC.localAI[1] >= 3f)
        {
            NPC.localAI[1] = 0f;
            NPC.localAI[2] = 30f;
            UpdateDefense();
            DrawShield();
        }
        for (int i = -1; i <= 1; i++)
        {
            Vector2 velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(12f * i)) * 6.5f;
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                Math.Max(1, NPC.damage / 4), 0.5f);
        }
    }

    private void UpdateDefense()
    {
        int defense = NPC.localAI[2] > 0f ? 72 : NPC.life < NPC.lifeMax / 2 ? 36 : 28;
        if (NPC.defense != defense && Main.netMode != NetmodeID.MultiplayerClient) NPC.netUpdate = true;
        NPC.defense = defense;
    }

    private void DrawShield()
    {
        if (Main.dedServ) return;
        for (int j = 0; j < 12; j++)
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, -2f, 100, default, 0.6f);
    }

    public override void SendExtraAI(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)Math.Clamp((int)NPC.localAI[2], 0, 30));
    }

    public override void ReceiveExtraAI(System.IO.BinaryReader reader)
    {
        bool wasShielded = NPC.localAI[2] > 0f;
        NPC.localAI[2] = Math.Min(30, (int)reader.ReadByte());
        UpdateDefense();
        if (!wasShielded && NPC.localAI[2] > 0f) DrawShield();
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.TornScrollPage>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SectTrialToken>(), 4, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
