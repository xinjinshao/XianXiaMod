using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Items;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditBossSources(Action<bool,string> check)
    {
        var catalog = ModContent.GetInstance<BossRewardSources>(); var mod = ModLoader.GetMod("XianXia"); int difficulty = Main.GameMode; var keys = Main.keyState;
        var items = Main.item.Where(i => i.active).ToHashSet(); var shots = Main.projectile.Where(p => p.active).ToHashSet();
        try {
            var bosses = mod.GetContent<ModNPC>().Where(n => n.NPC.boss).ToArray();
            check(bosses.Length == 12,"native reward source catalog covers twelve registered bosses");
            foreach (var boss in bosses) {
                var drops = new List<DropRateInfo>();
                foreach (var rule in Main.ItemDropsDB.GetRulesForNPCID(boss.Type,false)) rule.ReportDroprates(drops,new DropRateInfoChainFeed(1f));
                foreach (int type in drops.Where(d => d.dropRate > 0).Select(d => d.itemId).Distinct())
                    check(catalog.ForItem(type).Any(s => s.BossType == boss.Type && s.BagType == 0),"native reward source preserves actual direct boss drops");
                var bags = drops.Select(d => d.itemId).Distinct().Where(type => ContentSamples.ItemsByType.TryGetValue(type,out var item) && item.ModItem is CultivationBossBag).ToArray();
                check(bags.Length == 1,"native reward source links exactly one actual treasure bag to each boss");
                foreach (int bag in bags) {
                    var contents = new List<DropRateInfo>();
                    foreach (var rule in Main.ItemDropsDB.GetRulesForItemID(bag)) rule.ReportDroprates(contents,new DropRateInfoChainFeed(1f));
                    foreach (int type in contents.Where(d => d.dropRate > 0).Select(d => d.itemId).Distinct())
                        check(catalog.ForItem(type).Any(s => s.BossType == boss.Type && s.BagType == bag),"native reward source follows actual bag content rules back to its boss");
                }
            }
            var rewards = mod.GetContent<ModItem>().Select(i => new Item(i.Type)).Where(i => catalog.ForItem(i.type).Count > 0).ToArray();
            foreach (int gameMode in new[] { 0,1,2 }) foreach (var item in rewards) {
                Main.GameMode = gameMode; Main.keyState = new KeyboardState(Keys.LeftShift); var sources = catalog.ForItem(item.type); var lines = new List<TooltipLine>();
                check(item.TryGetGlobalItem<BossRewardSourceHelp>(out var help),"native boss source tooltip attaches to actual reward items"); help.ModifyTooltips(item,lines);
                check(lines.Count == sources.Count + 2 && lines.All(l => !l.Text.Contains("Mods.XianXia.") && !l.Text.Contains("{0}")),
                    "native boss source tooltip resolves source scope reward lines and availability caution in every difficulty");
                for (int i = 0; i < sources.Count; i++) {
                    var source = sources[i]; string line = lines.Single(l => l.Name == "BossSource" + i).Text;
                    check(line.Contains(Lang.GetNPCNameValue(source.BossType)) && (source.BagType == 0 || line.Contains(Lang.GetItemNameValue(source.BagType))),
                        "native boss source names the actual boss and bag without substituting ordinary enemy provenance");
                    var descriptions = source.Conditions.Select(c => c.GetConditionDescription()).Where(s => !string.IsNullOrWhiteSpace(s));
                    check(descriptions.All(line.Contains),"native boss source retains every nonempty native drop condition description");
                }
                Main.keyState = new KeyboardState(); lines.Clear(); help.ModifyTooltips(item,lines);
                check(lines.Count == Math.Min(4,sources.Count) + 2 + (sources.Count > 4 ? 1 : 0)
                    && lines.Any(l => l.Name == "BossSourceMore") == (sources.Count > 4),
                    "native boss source defaults to four provenance rows and advertises Shift expansion only when needed");
            }
            int moonbone = ModContent.ItemType<Moonbone>();
            check(catalog.ForItem(moonbone).Any(s => s.BagType == 0) && catalog.ForItem(moonbone).Any(s => s.BagType > 0)
                && ModContent.GetInstance<MaterialDropSources>().ForItem(moonbone).Count > 0,"native moonbone retains ordinary enemy direct boss and bag acquisition hints together");
            check(catalog.ForItem(ItemID.DirtBlock).Count == 0,"native boss source catalog does not invent unrelated drops");
            var before = catalog.ForItem(moonbone).Select(s => (s.BossType,s.BagType,s.Conditions.Length)).ToArray();
            catalog.OnWorldUnload(); check(catalog.ForItem(moonbone).Select(s => (s.BossType,s.BagType,s.Conditions.Length)).SequenceEqual(before),"native boss source cache rebuilds after unload");
            catalog.OnWorldLoad(); check(catalog.ForItem(moonbone).Select(s => (s.BossType,s.BagType,s.Conditions.Length)).SequenceEqual(before),"native boss source cache rebuilds after load");
            check(Main.item.Where(i => i.active).ToHashSet().SetEquals(items) && Main.projectile.Where(p => p.active).ToHashSet().SetEquals(shots),
                "native boss source reporting creates no loot or projectiles");
        } finally { Main.GameMode = difficulty; Main.keyState = keys; }
    }
}
