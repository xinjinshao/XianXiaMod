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
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.CaveBat];
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
        AnimationType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<ShallowSpiritVeinsBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 2, 1, 2));
    }
}
