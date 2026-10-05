using System;

using Microsoft.Xna.Framework;

using Terraria;

using Terraria.GameContent.Bestiary;

using Terraria.GameContent.ItemDropRules;

using Terraria.ID;

using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies;

public class ObsessedSwordCultivator : ModNPC

{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }


    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)

    {

        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.ObsessedSwordCultivator.Text"));

    }



    public override void SetDefaults()

    {

        NPC.width = 48;

        NPC.height = 48;

        NPC.lifeMax = 850;

        NPC.damage = 72;

        NPC.defense = 34;

        NPC.value = 425f;

        NPC.knockBackResist = 0.45f;

        NPC.HitSound = SoundID.NPCHit1;

        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.aiStyle = NPCAIStyleID.Fighter;

        AIType = NPCID.Zombie;



        NPC.knockBackResist = 0.25f;

    }



    public override float SpawnChance(NPCSpawnInfo spawnInfo)

    {
        if (!global::XianXia.Common.Systems.EnemySpawnRules.Allows(nameof(ObsessedSwordCultivator), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)) return 0f;


        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;

    }



    public override void PostAI()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            if (NPC.localAI[2] > 0f && --NPC.localAI[2] == 0f) NPC.damage = NPC.defDamage;
            return;
        }
        if (NPC.localAI[2] == 1f) NPC.netUpdate = true;
        if (NPC.localAI[2] > 0f) NPC.localAI[2]--;
        NPC.damage = NPC.localAI[2] > 0f ? (int)(NPC.defDamage * 1.3f) : NPC.defDamage;
        NPC.defense = 34;
        if (!global::XianXia.Common.Systems.EnemyTargeting.TryGetLivingTarget(NPC, out Player target))
        {
            if (NPC.localAI[2] > 0f) NPC.netUpdate = true;
            NPC.localAI[0] = NPC.localAI[1] = NPC.localAI[2] = 0f;
            NPC.damage = NPC.defDamage;
            return;
        }
        bool guarding = Math.Abs(target.Center.X - NPC.Center.X) < 96f;
        if (guarding)
        {
            NPC.velocity.X *= 0.65f;
            NPC.defense = 42;
        }
        if (++NPC.localAI[0] < 120f) return;
        NPC.localAI[0] = 0f;
        if (guarding && NPC.localAI[1] > 0f)
        {
            NPC.localAI[1] = 0f;
            NPC.localAI[2] = 30f;
            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 12f;
            NPC.damage = (int)(NPC.defDamage * 1.3f);
        }
        else NPC.velocity.X = Math.Sign(target.Center.X - NPC.Center.X) * 9f;
        NPC.netUpdate = true;
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || projectile.owner < 0 || projectile.owner >= Main.maxPlayers) return;
        Player attacker = Main.player[projectile.owner];
        if (attacker.active && !attacker.dead && Math.Abs(attacker.Center.X - NPC.Center.X) < 96f)
            NPC.localAI[1] = 1f;
    }

    public override void SendExtraAI(System.IO.BinaryWriter writer)
    {
        writer.Write((byte)Math.Clamp((int)NPC.localAI[2], 0, 30));
        writer.Write(NPC.damage);
    }

    public override void ReceiveExtraAI(System.IO.BinaryReader reader)
    {
        NPC.localAI[2] = Math.Min(30, (int)reader.ReadByte());
        NPC.damage = reader.ReadInt32();
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)

    {

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.BrokenSwordIntent>(), 2, 1, 2));

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SectTrialToken>(), 3, 1, 2));

    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }
}
