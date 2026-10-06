using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Content.NPCs.Bosses;
using XianXia.Content.Items.BossSummons;
using XianXia.Content.Items.MasterRewards;
using XianXia.Content.Items.HandGenerated;

namespace XianXia.Common.Systems;

public class BossChecklistIntegrationSystem : ModSystem
{
    public static int RegisteredBossCount { get; private set; }
    public override void PostSetupContent()
    {
        RegisteredBossCount = 0;
        if (!ModContent.GetInstance<XianXiaConfig>().EnableSoftCompatibilityHooks
            || !ModLoader.TryGetMod("BossChecklist", out Mod checklist)) return;
        if (checklist.Version < new Version(1, 6))
        {
            Mod.Logger.Warn("Boss Checklist integration requires version 1.6 or newer; registration skipped.");
            return;
        }
        Register<SpiritVeinWyrm, SpiritVeinIncense, SpiritVeinWyrmMonument>(checklist, 0.8f, "spirit_vein_wyrm", ModContent.ItemType<SpiritVeinWyrmTrophy>());
        Register<GardenWarden, SummonGardenBrokenKey, GardenWardenMonument>(checklist, 4.2f, "garden_warden", ModContent.ItemType<GardenWardenMask>());
        Register<BlackFurnaceIronGolem, SummonOldFurnaceEmber, BlackFurnaceIronGolemMonument>(checklist, 4.4f, "black_furnace_iron_golem", ModContent.ItemType<BlackFurnaceIronGolemPet>());
        Register<TribulationCloudAvatar, SummonThunderCallingJade, TribulationCloudAvatarMonument>(checklist, 6.5f, "tribulation_cloud_avatar");
        Register<ThunderMarshJiao, SummonThunderCallingJadeThunderMarshJiao, ThunderMarshJiaoMonument>(checklist, 8.2f, "thunder_marsh_jiao");
        Register<AbyssalStarWomb, SummonStarAbyssMembrane, AbyssalStarWombMonument>(checklist, 8.4f, "abyssal_star_womb", ModContent.ItemType<AbyssalStarWombLamp>());
        Register<FormlessSwordSoul, SummonSectTrialToken, FormlessSwordSoulMonument>(checklist, 12.2f, "formless_sword_soul", ModContent.ItemType<FormlessSwordSoulCostume>());
        Register<GreenwoodMedicineKingEcho, SummonSectTrialTokenGreenwoodMedicineKingEcho, GreenwoodMedicineKingEchoMonument>(checklist, 12.4f, "greenwood_medicine_king_echo");
        Register<HeavenTabletGuardian, SummonHeavenTabletRubbing, HeavenTabletGuardianMonument>(checklist, 13.1f, "heaven_tablet_guardian", ModContent.ItemType<SmallTabletPet>());
        Register<BrokenHeavenInspector, SummonHeavenTabletRubbingBrokenHeavenInspector, BrokenHeavenInspectorMonument>(checklist, 13.2f, "broken_heaven_inspector", ModContent.ItemType<InspectorMask>());
        Register<MoonboneImmortal, SummonMoonboneRitualTalisman, MoonboneImmortalMonument>(checklist, 18.2f, "moonbone_immortal");
        Register<OldHeavenDaoCore, SummonMoonboneRitualTalismanOldHeavenDaoCore, OldHeavenDaoCoreMonument>(checklist, 18.4f, "old_heaven_dao_core");
        Mod.Logger.Info($"Boss Checklist registered {RegisteredBossCount}/12 XianXia bosses.");
    }
    private void Register<TBoss, TSummon, TMonument>(Mod checklist, float order, string downedId, params int[] extraCollectibles)
        where TBoss : ModNPC where TSummon : ModItem where TMonument : ModItem
    {
        string name = typeof(TBoss).Name;
        var collectibles = new List<int> { ModContent.ItemType<TMonument>() };
        collectibles.AddRange(extraCollectibles);
        var data = new Dictionary<string, object> {
            ["spawnItems"] = ModContent.ItemType<TSummon>(),
            ["spawnInfo"] = Language.GetText($"Mods.XianXia.BossChecklistIntegration.{name}.SpawnInfo"),
            ["collectibles"] = collectibles,
        };
        try
        {
            object result = checklist.Call("LogBoss", Mod, name, order,
                (Func<bool>)(() => DownedBossSystem.DownedBosses.Contains(downedId)), ModContent.NPCType<TBoss>(), data);
            if (result is string message && message == "Success") RegisteredBossCount++;
            else Mod.Logger.Warn($"Boss Checklist rejected XianXia entry {name}: {result}");
        }
        catch (Exception error)
        {
            Mod.Logger.Warn($"Boss Checklist failed to register XianXia entry {name}: {error}");
        }
    }
    public override void Unload() => RegisteredBossCount = 0;
}
