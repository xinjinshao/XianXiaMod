using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

public class SummonOldFurnaceEmber : ModItem

{

    public override void SetDefaults()

    {

        Item.width = 28;

        Item.height = 28;

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

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.BlackFurnaceIronGolem>(),

                global::XianXia.Common.Players.CultivationStage.QiAwakening,

                "spirit_vein_wyrm")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "black_furnace_iron_golem");

    }



    public override bool? UseItem(Player player)

    {

        if (Main.netMode != NetmodeID.MultiplayerClient)

            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.BlackFurnaceIronGolem>());

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.OldFurnaceEmber>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(13)

            .AddTile(TileID.WorkBenches)

            .Register();

    }

}
