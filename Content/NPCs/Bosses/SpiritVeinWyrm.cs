using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
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
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.width = 96;
        NPC.height = 32;
        NPC.damage = 22;
        NPC.defense = 6;
        NPC.lifeMax = 1200;
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
        float speed = NPC.life < NPC.lifeMax / 2 ? 7.5f : 5.5f;
        Vector2 desiredVelocity = desired.SafeNormalize(Vector2.UnitY) * speed;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.035f);
        NPC.rotation = NPC.velocity.ToRotation();

        if (!spawnedChildren && NPC.life < NPC.lifeMax / 2 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            spawnedChildren = true;
            for (int i = 0; i < 3; i++)
            {
                int id = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + Main.rand.Next(-80, 81), (int)NPC.Center.Y + Main.rand.Next(-40, 41), ModContent.NPCType<ShatteredJadeWyrmMinion>(), ai0: NPC.whoAmI);
                Main.npc[id].velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            }
        }

        Lighting.AddLight(NPC.Center, 0.05f, 0.28f, 0.2f);
    }

    public override void OnKill()
    {
        DownedBossSystem.DownedSpiritVeinWyrm = true;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 1, 12, 18));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 1, 20, 35));
    }
}
