using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Biomes;
using XianXia.Content.Items.Fishing;

namespace XianXia.Common.Players;

public class CultivationFishingPlayer : ModPlayer
{
    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn,
        ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
    {
        if (!Player.active || Player.dead || itemDrop <= 0 || npcSpawn > 0 || attempt.inLava
            || attempt.inHoney || !attempt.crate || !attempt.rare || attempt.veryrare || attempt.legendary)
            return;
        // Prefer the highest unlocked overlapping biome, then replace only half its biome-crate rolls.
        int crate = 0;
        if (Allows(4) && Player.InModBiome<MoonboneAbyssBiome>()) crate = ModContent.ItemType<MoonboneFishingCrate>();
        else if (Allows(3) && Player.InModBiome<FallenHeavenPalaceBiome>()) crate = ModContent.ItemType<HeavenFishingCrate>();
        else if (Allows(2) && Player.InModBiome<TenThousandSectsRuinsBiome>()) crate = ModContent.ItemType<SectFishingCrate>();
        else if (Allows(1) && Player.InModBiome<ThunderMarshCloudsBiome>()) crate = ModContent.ItemType<ThunderFishingCrate>();
        else if (Allows(1) && Player.InModBiome<StarAbyssRiftBiome>()) crate = ModContent.ItemType<StarAbyssFishingCrate>();
        else if (Player.InModBiome<SunkenFurnaceVeinBiome>()) crate = ModContent.ItemType<FurnaceFishingCrate>();
        else if (Player.InModBiome<GreenwoodHerbGardenBiome>()) crate = ModContent.ItemType<GreenwoodFishingCrate>();
        else if (Player.InModBiome<ShallowSpiritVeinsBiome>()) crate = ModContent.ItemType<SpiritVeinFishingCrate>();
        if (crate != 0 && Main.rand.NextBool()) itemDrop = crate;
    }
    private static bool Allows(int tier) => CultivationFishingRules.Allows(tier,
        Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord);
}
