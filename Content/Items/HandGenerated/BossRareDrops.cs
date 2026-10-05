using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.HandGenerated;

public class GardenWardenMask : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 1); Item.rare = ItemRarityID.Green; Item.vanity = true; } }
public class FormlessSwordSoulCostume : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 3); Item.rare = ItemRarityID.Lime; Item.vanity = true; } }
public class InspectorMask : ModItem { public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1; public override void SetDefaults() { Item.width = 32; Item.height = 32; Item.maxStack = 1; Item.value = Item.buyPrice(gold: 4); Item.rare = ItemRarityID.Yellow; Item.vanity = true; } }
