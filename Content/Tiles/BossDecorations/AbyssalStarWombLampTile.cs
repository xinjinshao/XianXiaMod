using Terraria;
using Terraria.ModLoader;
using XianXia.Common.Items;

namespace XianXia.Content.Tiles.BossDecorations;

public class AbyssalStarWombLampTile : MasterBossMonumentTile
{
    public override string Texture => "XianXia/Content/Tiles/SwordTabletTile";
    protected override int MonumentItem => ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.AbyssalStarWombLamp>();

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileLighted[Type] = true;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.18f;
        g = 0.10f;
        b = 0.35f;
    }
}
