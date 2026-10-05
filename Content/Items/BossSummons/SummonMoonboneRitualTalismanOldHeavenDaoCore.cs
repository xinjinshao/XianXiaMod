using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

public class SummonMoonboneRitualTalismanOldHeavenDaoCore : global::XianXia.Common.Items.CultivationBossSummonItem

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



    public override int BossType => ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.OldHeavenDaoCore>();

    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanUseBossSummon(

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.OldHeavenDaoCore>(),

                global::XianXia.Common.Players.CultivationStage.Tribulation,

                "moonbone_immortal")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "old_heaven_dao_core");

    }







    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.MoonboneRitualTalisman>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(40)

            .AddTile(TileID.DemonAltar)

            .Register();

    }

}
