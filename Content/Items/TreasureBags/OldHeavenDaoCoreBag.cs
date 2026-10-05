using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Common.Systems;
using XianXia.Common.Items;

namespace XianXia.Content.Items.TreasureBags;

public class OldHeavenDaoCoreBag : CultivationBossBag
{
    public override string Texture => "XianXia/Content/Items/Materials/ArtifactBlankShard";
    protected override int BossType => ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.OldHeavenDaoCore>();
    protected override bool PreHardmode => false;
    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.RouteMaterial>(), 1));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 1, 16, 28));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.DaoSeveringDust>(), 1, 8, 16));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(), 1, 8, 16));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.SpiritGel>(), 4, 3, 8));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.Materials.ArtifactBlankShard>(), 8, 1, 3));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.SilentTabletDecoration>(), 1, 1, 1));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.ExpertRewards.OldHeavenResonator>()));
        itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(BossType));
    }
}
