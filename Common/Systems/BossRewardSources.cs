using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public readonly record struct BossRewardSource(int BossType,int BagType,IItemDropRuleCondition[] Conditions);

public class BossRewardSources : ModSystem
{
    private readonly Dictionary<int,List<BossRewardSource>> sources = new();
    private bool built;
    public override void OnWorldLoad() { sources.Clear(); built = false; }
    public override void OnWorldUnload() { sources.Clear(); built = false; }
    public override void Unload() { sources.Clear(); built = false; }
    private void Add(IEnumerable<DropRateInfo> drops,int boss,int bag)
    {
        foreach (var drop in drops.Where(d => d.dropRate > 0)) {
            if (!sources.TryGetValue(drop.itemId,out var list)) sources[drop.itemId] = list = new();
            // Multiple branches can report the same item with different conditions.
            var conditions = drop.conditions?.ToArray() ?? Array.Empty<IItemDropRuleCondition>();
            if (!list.Any(s => s.BossType == boss && s.BagType == bag && s.Conditions.SequenceEqual(conditions)))
                list.Add(new BossRewardSource(boss,bag,conditions));
        }
    }
    public IReadOnlyList<BossRewardSource> ForItem(int type)
    {
        if (!built && Main.ItemDropsDB != null) {
            foreach (var boss in Mod.GetContent<ModNPC>().Where(n => n.NPC.boss)) {
                var drops = new List<DropRateInfo>();
                foreach (var rule in Main.ItemDropsDB.GetRulesForNPCID(boss.Type,false)) rule.ReportDroprates(drops,new DropRateInfoChainFeed(1f));
                Add(drops,boss.Type,0);
                foreach (int bag in drops.Select(d => d.itemId).Distinct().Where(id => ContentSamples.ItemsByType.TryGetValue(id,out var item)
                    && item.ModItem is Common.Items.CultivationBossBag)) {
                    var contents = new List<DropRateInfo>();
                    foreach (var rule in Main.ItemDropsDB.GetRulesForItemID(bag)) rule.ReportDroprates(contents,new DropRateInfoChainFeed(1f));
                    Add(contents,boss.Type,bag);
                }
            }
            built = true;
        }
        return sources.TryGetValue(type,out var result) ? result : Array.Empty<BossRewardSource>();
    }
    public string Description(BossRewardSource source)
    {
        string boss = Lang.GetNPCNameValue(source.BossType);
        string text = source.BagType > 0
            ? Language.GetTextValue("Mods.XianXia.BossSources.Bag",boss,Lang.GetItemNameValue(source.BagType))
            : Language.GetTextValue("Mods.XianXia.BossSources.Direct",boss);
        string conditions = string.Join(", ",source.Conditions.Select(c => c.GetConditionDescription()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
        return conditions.Length == 0 ? text : text + "\n" + Language.GetTextValue("Mods.XianXia.BossSources.Conditions",conditions);
    }
}
