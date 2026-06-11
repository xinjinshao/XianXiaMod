using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Materials;
using XianXia.Content.NPCs.Bosses;

namespace XianXia.Content.Items.BossSummons;

public class SpiritVeinIncense : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;
    }

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
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.White;
    }

    public override bool CanUseItem(Player player)
    {
        return !NPC.AnyNPCs(ModContent.NPCType<SpiritVeinWyrm>());
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<SpiritVeinWyrm>());
        }
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<LowGradeSpiritStone>(8)
            .AddIngredient<SpiritGel>(6)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
