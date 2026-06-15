using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

public class SummonThunderCallingJade : ModItem

{

    public override void SetDefaults()

    {

        Item.width = 36;

        Item.height = 36;

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

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.TribulationCloudAvatar>(),

                global::XianXia.Common.Players.CultivationStage.QiCondensation,

                "garden_warden")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "tribulation_cloud_avatar");

    }



    public override bool? UseItem(Player player)

    {

        if (Main.netMode != NetmodeID.MultiplayerClient)

            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.TribulationCloudAvatar>());

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.ThunderCallingJade>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(16)

            .AddTile(TileID.WorkBenches)

            .Register();

    }

}
