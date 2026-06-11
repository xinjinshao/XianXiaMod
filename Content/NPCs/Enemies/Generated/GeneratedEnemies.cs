using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.NPCs.Enemies.Generated;

public class HerbGardenVineSpirit : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 140;
        NPC.damage = 24;
        NPC.defense = 8;
        NPC.value = 70f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>(), 2, 1, 2));
    }
}


public class MiasmaFlowerMoth : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 90;
        NPC.damage = 20;
        NPC.defense = 4;
        NPC.value = 60f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.GreenwoodHerbGardenBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.GreenwoodRoot>(), 2, 1, 2));
    }
}


public class FurnaceAshGolem : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 180;
        NPC.damage = 28;
        NPC.defense = 14;
        NPC.value = 90f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.FurnaceSlagIron>(), 2, 1, 2));
    }
}


public class IronShardSpirit : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 70;
        NPC.damage = 22;
        NPC.defense = 6;
        NPC.value = 60f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.SunkenFurnaceVeinBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.ArtifactBlankShard>(), 2, 1, 2));
    }
}


public class TribulationCloudling : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 240;
        NPC.damage = 42;
        NPC.defense = 16;
        NPC.value = 120f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>(), 2, 1, 2));
    }
}


public class ThunderPatternHawk : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 300;
        NPC.damage = 48;
        NPC.defense = 18;
        NPC.value = 150f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.ThunderMarshCloudsBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.TribulationCloudDew>(), 2, 1, 2));
    }
}


public class StarEclipsedCultivator : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 360;
        NPC.damage = 50;
        NPC.defense = 20;
        NPC.value = 180f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.StarAbyssRiftBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(), 2, 1, 2));
    }
}


public class StarAbyssLarva : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 260;
        NPC.damage = 46;
        NPC.defense = 18;
        NPC.value = 130f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.StarAbyssRiftBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.StarEclipseCrystal>(), 2, 1, 2));
    }
}


public class ObsessedSwordCultivator : ModNPC
{
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
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.SectTrialToken>(), 2, 1, 2));
    }
}


public class ScriptureArchiveEcho : ModNPC
{
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
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.TenThousandSectsRuinsBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.SectTrialToken>(), 2, 1, 2));
    }
}


public class CelestialPuppet : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 1350;
        NPC.damage = 88;
        NPC.defense = 46;
        NPC.value = 675f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.FallenHeavenPalaceBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(), 2, 1, 2));
    }
}


public class HeavenTabletGuard : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 1500;
        NPC.damage = 92;
        NPC.defense = 54;
        NPC.value = 750f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.FallenHeavenPalaceBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.HeavenDaoFragment>(), 2, 1, 2));
    }
}


public class MoonboneCultivator : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 4200;
        NPC.damage = 160;
        NPC.defense = 72;
        NPC.value = 2100f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Fighter;
        AIType = NPCID.CaveBat;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.MoonboneAbyssBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.Moonbone>(), 2, 1, 2));
    }
}


public class ArchivedImmortalSoul : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.lifeMax = 3600;
        NPC.damage = 150;
        NPC.defense = 64;
        NPC.value = 1800f;
        NPC.knockBackResist = 0.45f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = NPCAIStyleID.Bat;
        AIType = NPCID.CaveBat;
        NPC.noGravity = true;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<global::XianXia.Content.Biomes.MoonboneAbyssBiome>() ? 0.18f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Generated.DaoSeveringDust>(), 2, 1, 2));
    }
}
