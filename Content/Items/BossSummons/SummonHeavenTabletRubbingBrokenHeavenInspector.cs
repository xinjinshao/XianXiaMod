using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

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



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanUseBossSummon(

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BrokenHeavenInspector>(),

                global::XianXia.Common.Players.CultivationStage.NascentSoul,

                "heaven_tablet_guardian")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "broken_heaven_inspector");

    }



    public override bool? UseItem(Player player)

    {

        if (Main.netMode != NetmodeID.MultiplayerClient)

            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.BrokenHeavenInspector>());

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.HeavenTabletRubbing>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(34)

            .AddTile(TileID.DemonAltar)

            .Register();

    }

}
