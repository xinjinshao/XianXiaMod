using Terraria;

using Terraria.ID;

using Terraria.ModLoader;




namespace XianXia.Content.Items.BossSummons;

public class SummonHeavenTabletRubbing : global::XianXia.Common.Items.CultivationBossSummonItem

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



    public override int BossType => ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.HeavenTabletGuardian>();

    public override bool CanUseItem(Player player)

    {

        return player.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>()

            .CanUseBossSummon(

                ModContent.NPCType<global::XianXia.Content.NPCs.Bosses.HeavenTabletGuardian>(),

                global::XianXia.Common.Players.CultivationStage.NascentSoul,

                "formless_sword_soul")

            && global::XianXia.Common.Systems.BossSummonRules.CanUseGeneratedBossSummon(player, "heaven_tablet_guardian");

    }







    public override void AddRecipes()

    {

        CreateRecipe()

            .AddIngredient<global::XianXia.Content.Items.Materials.HeavenTabletRubbing>()

            .AddIngredient<global::XianXia.Content.Items.Materials.LowGradeSpiritStone>(31)

            .AddTile(TileID.DemonAltar)

            .Register();

    }

}
