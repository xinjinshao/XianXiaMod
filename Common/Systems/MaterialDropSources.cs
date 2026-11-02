using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public readonly record struct MaterialDropSource(int NpcType,string Enemy,string Biome,int Tier);

public class MaterialDropSources : ModSystem
{
    private readonly Dictionary<int,List<MaterialDropSource>> sources = new();
    private bool built;
    public override void OnWorldLoad() { sources.Clear(); built = false; }
    public override void OnWorldUnload() { sources.Clear(); built = false; }
    public override void Unload() { sources.Clear(); built = false; }
    public IReadOnlyList<MaterialDropSource> ForItem(int type)
    {
        if (!built && Main.ItemDropsDB != null) {
            foreach (ModNPC enemy in Mod.GetContent<ModNPC>().Where(n => EnemySpawnRules.RequiredTier(n.Name) >= 0)) {
                string biome = EnemySpawnRules.BiomeName(enemy.Name); if (biome.Length == 0) continue;
                var drops = new List<DropRateInfo>();
                foreach (IItemDropRule rule in Main.ItemDropsDB.GetRulesForNPCID(enemy.Type,false)) rule.ReportDroprates(drops,new DropRateInfoChainFeed(1f));
                foreach (int item in drops.Where(d => d.dropRate > 0).Select(d => d.itemId).Distinct()) {
                    if (!sources.TryGetValue(item,out var list)) sources[item] = list = new();
                    list.Add(new MaterialDropSource(enemy.Type,enemy.Name,biome,EnemySpawnRules.RequiredTier(enemy.Name)));
                }
            }
            built = true;
        }
        return sources.TryGetValue(type,out var result) ? result : Array.Empty<MaterialDropSource>();
    }
    public string Description(MaterialDropSource source)
    {
        bool unlocked = EnemySpawnRules.Allows(source.Enemy,Main.hardMode,NPC.downedPlantBoss,NPC.downedGolemBoss,NPC.downedMoonlord);
        string biome = Mod.Find<ModBiome>(source.Biome).DisplayName.Value;
        return Language.GetTextValue("Mods.XianXia.MaterialSources.Source",Lang.GetNPCNameValue(source.NpcType),biome,
            Language.GetTextValue("Mods.XianXia.MaterialSources.Tier" + source.Tier),
            Language.GetTextValue("Mods.XianXia.MaterialSources." + (unlocked ? "Unlocked" : "Locked")));
    }
}
