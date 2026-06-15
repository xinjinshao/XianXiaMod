// Boss unique guaranteed drops matching wiki Boss_Stats.md
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

public class FoundationSeal : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 24; Item.height = 24; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 2); Item.rare = ItemRarityID.LightRed; } }
public class MedicineKingWoodHeart : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 24; Item.height = 24; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 3); Item.rare = ItemRarityID.Lime; } }
public class StarCalamityCore : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 24; Item.height = 24; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 5); Item.rare = ItemRarityID.Red; } }
public class ImperialDecreeItem : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 24; Item.height = 24; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 4); Item.rare = ItemRarityID.Yellow; } }
public class HeavenTabletSeal : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 24; Item.height = 24; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 3); Item.rare = ItemRarityID.Yellow; } }
public class RouteMaterial : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 24; Item.height = 24; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 8); Item.rare = ItemRarityID.Purple; } }
