using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

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



    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanUseBossSummon(

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.MoonboneImmortal>(),

                global::XianXia.Common.Players.CultivationStage.Tribulation,

                "broken_heaven_inspector")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "moonbone_immortal");

    }



    public override bool? UseItem(Player player)

    {

        if (Main.netMode != NetmodeID.MultiplayerClient)

            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.Generated.MoonboneImmortal>());

        return true;

    }



    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.MoonboneRitualTalisman>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(37)

            .AddTile(TileID.DemonAltar)

            .Register();

    }

}
