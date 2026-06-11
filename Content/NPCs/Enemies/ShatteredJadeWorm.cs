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
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.SandSlime;
        AnimationType = NPCID.SandSlime;
        NPC.knockBackResist = 0.4f;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.2f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 2, 1, 2));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 3));
    }
}
