using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Content.Items.Materials;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditPillQualityStacks(Action<bool, string> check)
    {
        foreach (int type in new[] { ModContent.ItemType<QiCondensingPill>(), ModContent.ItemType<FoundationPill>(),
            ModContent.ItemType<QiRecoveryPill>(), ModContent.ItemType<SpringReturnPill>(), ModContent.ItemType<TribulationResistingPill>(),
            ModContent.ItemType<FurnaceGuardPill>(), ModContent.ItemType<WindStepPill>(), ModContent.ItemType<ThunderBurstPill>() })
        foreach (PillQuality destinationGrade in Enum.GetValues<PillQuality>())
        foreach (PillQuality sourceGrade in Enum.GetValues<PillQuality>())
        foreach (bool destinationCrafted in new[] { false, true }) foreach (bool sourceCrafted in new[] { false, true }) {
            Item Pill(PillQuality grade, bool crafted, int stack) {
                var result = new Item(type, stack);
                result.GetGlobalItem<PillQualitySystem>().LoadData(result, new TagCompound { ["quality"] = (int)grade, ["crafted"] = crafted });
                return result;
            }
            var destination = Pill(destinationGrade, destinationCrafted, 28);
            var source = Pill(sourceGrade, sourceCrafted, 5);
            bool same = destinationGrade == sourceGrade;
            check(ItemLoader.CanStack(destination, source) == same && ItemLoader.CanStackInWorld(destination, source) == same,
                "native eight-pill stack gates preserve grade boundaries in inventory and world: " + destination.ModItem.Name + "/" + destinationGrade + "/" + sourceGrade + "/" + destinationCrafted + "/" + sourceCrafted);
            ItemLoader.TryStackItems(destination, source, out int transferred, false);
            var targetQuality = destination.GetGlobalItem<PillQualitySystem>();
            var originalQuality = source.GetGlobalItem<PillQualitySystem>();
            check(transferred == (same ? 2 : 0) && destination.stack == (same ? 30 : 28) && source.stack == (same ? 3 : 5)
                && targetQuality.Quality == destinationGrade && originalQuality.Quality == sourceGrade
                && targetQuality.Crafted == (destinationCrafted || same && sourceCrafted) && originalQuality.Crafted == sourceCrafted,
                "native actual item loader transfers only fitting equal-grade pills and merges provenance");
            Item clone = destination.Clone();
            check(clone.stack == destination.stack && clone.GetGlobalItem<PillQualitySystem>().Quality == destinationGrade
                && clone.GetGlobalItem<PillQualitySystem>().Crafted == targetQuality.Crafted
                && !ReferenceEquals(clone.GetGlobalItem<PillQualitySystem>(), targetQuality),
                "native pill clone keeps independent quality metadata after stack transfer");
            Item loaded = ItemIO.Load(ItemIO.Save(destination));
            check(loaded.type == type && loaded.stack == destination.stack && loaded.GetGlobalItem<PillQualitySystem>().Quality == destinationGrade
                && loaded.GetGlobalItem<PillQualitySystem>().Crafted == targetQuality.Crafted,
                "native full ItemIO roundtrip preserves stacked pill grade amount and provenance");
            var split = new Item(type, 1);
            ItemLoader.SplitStack(split, destination, 1);
            check(split.GetGlobalItem<PillQualitySystem>().Quality == destinationGrade
                && split.GetGlobalItem<PillQualitySystem>().Crafted == targetQuality.Crafted,
                "native split hook copies source pill quality onto independent destination");
        }
    }
}
