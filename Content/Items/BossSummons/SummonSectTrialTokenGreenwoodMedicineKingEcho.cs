using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

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



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanUseBossSummon(

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GreenwoodMedicineKingEcho>(),

                global::XianXia.Common.Players.CultivationStage.GoldenCore,

                "garden_warden")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "greenwood_medicine_king_echo");

    }



    public override bool? UseItem(Player player)

    {

        if (Main.netMode != NetmodeID.MultiplayerClient)

            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.GreenwoodMedicineKingEcho>());

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.SectTrialToken>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(28)

            .AddTile(TileID.DemonAltar)

            .Register();

    }

}
