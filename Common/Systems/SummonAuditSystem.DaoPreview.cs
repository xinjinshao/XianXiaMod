using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditDaoPreview(Action<bool,string> check)
    {
        byte[] Save(Item item) { using var stream = new MemoryStream(); TagIO.ToStream(ItemIO.Save(item),stream,false); return stream.ToArray(); }
        var weapons = ModLoader.GetMod("XianXia").GetContent<ModItem>().Select(sample => new Item(sample.Type)).Where(RefinedArtifact.SupportsRefinement).ToArray();
        check(weapons.Length == 19,"native Dao forge preview covers nineteen registered refinable weapons");
        foreach (var weapon in weapons) foreach (int growthState in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3,4 }) {
            Item target = weapon.Clone(); var growth = target.GetGlobalItem<RefinedArtifact>();
            target.GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
            if (growthState > 0) growth.SetLevel(3);
            if (growthState > 1) growth.TryAwaken();
            if (growthState > 2) growth.TryTransform(DownedBossSystem.EndgameRoute.SeverHeaven);
            byte[] before = Save(target); string preview = DaoArtifactPreview.Describe(target,(DownedBossSystem.EndgameRoute)route);
            bool valid = route >= 1 && route <= 3;
            string description = valid ? Language.GetTextValue(RefinedArtifact.DaoTooltipKey(target),Language.GetTextValue(EndgameRouteTransactions.NameKey((DownedBossSystem.EndgameRoute)route))) : "";
            check(valid ? preview.StartsWith(Language.GetTextValue("Mods.XianXia.DaoArtifacts.Preview") + "\n")
                && description.Contains('\n') && preview.EndsWith(description.Substring(description.IndexOf('\n') + 1))
                && !preview.Contains("Mods.XianXia.") && !preview.Contains("{0}") : preview.Length == 0,
                "native Dao preview resolves actual weapon benefits for valid planned routes and hides invalid route previews");
            check(before.SequenceEqual(Save(target)),"native Dao preview preserves complete serialized item for dormant awakened and previously transformed equipment");
        }
        check(DaoArtifactPreview.Describe(null,DownedBossSystem.EndgameRoute.SeverHeaven).Length == 0
            && DaoArtifactPreview.Describe(new Item(Terraria.ID.ItemID.DirtBlock),DownedBossSystem.EndgameRoute.SeverHeaven).Length == 0,
            "native Dao preview hides null or unsupported targets");
    }
}
