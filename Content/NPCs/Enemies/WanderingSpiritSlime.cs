using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Biomes;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.NPCs.Enemies;

public class WanderingSpiritSlime : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = global::XianXia.Common.Animation.NpcFrameAnimator.EnemyFrameCount;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("Mods.XianXia.Bestiary.WanderingSpiritSlime"));
    }

    public override void SetDefaults()
    {
        NPC.width = 36;
        NPC.height = 30;
        NPC.damage = 12;
        NPC.defense = 2;
        NPC.lifeMax = 45;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = 45f;
        NPC.aiStyle = NPCAIStyleID.Slime;
        AIType = NPCID.BlueSlime;
    }

    public override void FindFrame(int frameHeight)
    {
        global::XianXia.Common.Animation.NpcFrameAnimator.Animate(NPC, frameHeight, Main.npcFrameCount[Type], 7);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.28f : 0f;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < 6; i++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MagicMirror, hit.HitDirection * 0.8f, -1.2f, 100, default, 0.8f);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 1, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 4));
    }
}
