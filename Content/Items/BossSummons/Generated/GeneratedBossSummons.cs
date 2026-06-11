using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Items.BossSummons.Generated;

public class SummonGardenBrokenKey : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GardenWarden>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GardenWarden>());
        return true;
    }
}


public class SummonOldFurnaceEmber : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BlackFurnaceIronGolem>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BlackFurnaceIronGolem>());
        return true;
    }
}


public class SummonThunderCallingJade : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.TribulationCloudAvatar>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.TribulationCloudAvatar>());
        return true;
    }
}


public class SummonThunderCallingJadeThunderMarshJiao : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.ThunderMarshJiao>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.ThunderMarshJiao>());
        return true;
    }
}


public class SummonStarAbyssMembrane : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.AbyssalStarWomb>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.AbyssalStarWomb>());
        return true;
    }
}


public class SummonSectTrialToken : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.FormlessSwordSoul>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.FormlessSwordSoul>());
        return true;
    }
}


public class SummonSectTrialTokenGreenwoodMedicineKingEcho : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GreenwoodMedicineKingEcho>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.GreenwoodMedicineKingEcho>());
        return true;
    }
}


public class SummonHeavenTabletRubbing : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.HeavenTabletGuardian>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.HeavenTabletGuardian>());
        return true;
    }
}


public class SummonHeavenTabletRubbingBrokenHeavenInspector : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BrokenHeavenInspector>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BrokenHeavenInspector>());
        return true;
    }
}


public class SummonMoonboneRitualTalisman : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.MoonboneImmortal>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.MoonboneImmortal>());
        return true;
    }
}


public class SummonMoonboneRitualTalismanOldHeavenDaoCore : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = ItemRarityID.Green;
    }

    public override bool CanUseItem(Player player) => !NPC.AnyNPCs(ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.OldHeavenDaoCore>());

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.OldHeavenDaoCore>());
        return true;
    }
}
