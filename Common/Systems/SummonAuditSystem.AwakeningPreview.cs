using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditAwakeningPreview(Action<bool,string> check)
    {
        byte[] Save(Item item) { using var stream = new MemoryStream(); TagIO.ToStream(ItemIO.Save(item),stream,false); return stream.ToArray(); }
        var weapons = ModLoader.GetMod("XianXia").GetContent<ModItem>().Select(sample => new Item(sample.Type)).Where(RefinedArtifact.SupportsRefinement).ToArray();
        check(weapons.Length == 19,"native awakening forge preview covers nineteen registered refinable weapons");
        foreach (var weapon in weapons) foreach (int growthState in new[] { 0,1,2,3 }) {
            Item target = weapon.Clone(); var growth = target.GetGlobalItem<RefinedArtifact>();
            target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
            if (growthState > 0) growth.SetLevel(3);
            if (growthState > 1) growth.TryAwaken();
            if (growthState > 2) growth.TryTransform(DownedBossSystem.EndgameRoute.SeverHeaven);
            byte[] before = Save(target); string preview = AwakeningArtifactPreview.Describe(target);
            Item awakened = weapon.Clone(); var after = awakened.GetGlobalItem<RefinedArtifact>(); after.SetLevel(3); after.TryAwaken();
            var lines = new List<TooltipLine>(); after.ModifyTooltips(awakened,lines);
            string actual = lines.Single(line => line.Name == "ArtifactAwakening").Text;
            check(preview == Language.GetTextValue("Mods.XianXia.Refinement.Preview",actual) && !preview.Contains("Mods.XianXia.") && !preview.Contains("{0}"),
                "native awakening preview matches actual awakened weapon tooltip and resolves localization for every growth state");
            check(before.SequenceEqual(Save(target)),"native awakening preview preserves complete serialized dormant awakened or transformed equipment");
        }
        check(AwakeningArtifactPreview.Describe(null).Length == 0 && AwakeningArtifactPreview.Describe(new Item(Terraria.ID.ItemID.DirtBlock)).Length == 0,
            "native awakening preview hides null and unsupported items");
    }
}
