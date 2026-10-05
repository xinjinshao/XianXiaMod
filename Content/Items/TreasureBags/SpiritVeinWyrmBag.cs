using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Content.Items.Materials;
using XianXia.Common.Items;

namespace XianXia.Content.Items.TreasureBags;

public class SpiritVeinWyrmBag : CultivationBossBag
{
    public override string Texture => "XianXia/Content/Items/Materials/ArtifactBlankShard";
    protected override int BossType => ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.SpiritVeinWyrm>();
    protected override bool PreHardmode => true;
    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<LowGradeSpiritStone>(), 1, 12, 18));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpiritGel>(), 1, 20, 35));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritVeinWyrmTrophy>(), 10, 1, 1));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.LowGradeSpiritCore>(), 1, 1, 1));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SpiritVeinScale>(), 1, 12, 18));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.ExpertRewards.VeinCoilCharm>()));
        itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(BossType));
    }
}
