using Terraria;
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
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.BlueSlime];
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
        AnimationType = NPCID.BlueSlime;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.28f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 1, 1, 3));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 4));
    }
}
