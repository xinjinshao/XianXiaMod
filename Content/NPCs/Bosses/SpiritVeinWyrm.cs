using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.NPCs.Bosses;

[AutoloadBossHead]
public class SpiritVeinWyrm : ModNPC
{
    private bool spawnedChildren;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.BossFrameCount;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.SpiritVeinWyrm"));
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 32;
        int baseLife = 1200; int baseDamage = 22;
        if (Main.expertMode) { baseLife = (int)(baseLife * 1.45f); baseDamage = (int)(baseDamage * 1.25f); }
        if (Main.masterMode) { baseLife = (int)(baseLife * 1.85f); baseDamage = (int)(baseDamage * 1.45f); }
        NPC.lifeMax = baseLife;
        NPC.damage = baseDamage;
        NPC.defense = 6;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 80);
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.aiStyle = -1;
        Music = MusicID.Boss1;
    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 8);
    }

    public override void AI()
    {
        Player target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest(false);
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.15f;
                NPC.EncourageDespawn(30);
                return;
            }
        }

        Vector2 desired = target.Center - NPC.Center;
        bool phaseTwo = NPC.life < NPC.lifeMax / 2;
        bool finalPhase = NPC.life < NPC.lifeMax / 4;

        if (phaseTwo && NPC.localAI[0] < 1f)
        {
            NPC.localAI[0] = 1f;
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.Cyan, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.SpiritVeinTremor"));
            }
        }

        if (finalPhase && NPC.localAI[0] < 2f)
        {
            NPC.localAI[0] = 2f;
            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(NPC.Hitbox, Color.OrangeRed, Language.GetTextValue("Mods.XianXia.Progression.BossPhase.ShatteredJadeRampage"));
            }
        }

        float speed = finalPhase ? 9.2f : phaseTwo ? 7.5f : 5.5f;
        Vector2 desiredVelocity = desired.SafeNormalize(Vector2.UnitY) * speed;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, finalPhase ? 0.055f : 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();

        if (!spawnedChildren && phaseTwo && Main.netMode != NetmodeID.MultiplayerClient)
        {
            spawnedChildren = true;
            for (int i = 0; i < 3; i++)
            {
                int id = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-80, 81), (int)NPC.Center.Y + Main.rand.Next(-40, 41), ModContent.NPCType<ShatteredJadeWyrmMinion>(), ai0: NPC.whoAmI);
                Main.npc[id].velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            }
        }

        NPC.ai[0]++;
        int shotInterval = finalPhase ? 90 : phaseTwo ? 130 : 180;
        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] >= shotInterval)
        {
            NPC.ai[0] = 0f;
            Vector2 aim = desired.SafeNormalize(Vector2.UnitY);
            int damage = NPC.damage / 4;
            if (damage < 12)
            {
                damage = 12;
            }

            int spread = finalPhase ? 2 : phaseTwo ? 1 : 0;
            for (int i = -spread; i <= spread; i++)
            {
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(10f * i)) * (finalPhase ? 8.5f : 7f);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<global::XianXia.Content.Projectiles.BossSpiritBoltProjectile>(),
                    damage,
                    1.2f,
                    Main.myPlayer);
            }
        }

        Lighting.AddLight(NPC.Center, 0.05f, 0.28f, 0.2f);
    }

    public override void OnKill()
    {
        DownedBossSystem.MarkDowned("spirit_vein_wyrm");
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 1, 12, 18));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 1, 20, 35));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritVeinWyrmTrophy>(), 10, 1, 1));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.LowGradeSpiritCore>(), 1, 1, 1));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritVeinScale>(), 1, 12, 18));
    }
}
