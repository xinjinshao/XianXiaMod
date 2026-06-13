using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace XianXia.Content.Tiles;

public class SwordTabletTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; AddMapEntry(new Color(160, 200, 220), CreateMapEntryName());
        DustType = DustID.Stone; MineResist = 3f; MinPick = 150;
    }
    public override bool CanExplode(int i, int j) => false;
    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (closer) Main.LocalPlayer.AddBuff(ModContent.BuffType<global::XianXia.Content.Buffs.TribulationResistanceBuff>(), 3);
    }
}

public class BrokenHeavenTabletTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(220, 210, 160), CreateMapEntryName());
        DustType = DustID.GoldCoin; MineResist = 4f; MinPick = 200;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.3f; g = 0.28f; b = 0.15f; }
}

public class ArchiveLightPillarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(220, 220, 240), CreateMapEntryName());
        DustType = DustID.IceTorch; MineResist = 5f; MinPick = 250;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.2f; g = 0.22f; b = 0.35f; }
}

public class SingingThunderStoneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(140, 130, 220), CreateMapEntryName());
        DustType = DustID.Electric; MineResist = 2f; MinPick = 110;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.15f; g = 0.12f; b = 0.3f; }
    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (closer && Main.GameUpdateCount % 180 == 0)
            Main.LocalPlayer.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().spiritualEnergyRegenBonus += 1;
    }
}

public class RiftMembraneTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false; Main.tileFrameImportant[Type] = true; Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileLighted[Type] = true;
        AddMapEntry(new Color(40, 40, 100), CreateMapEntryName());
        DustType = DustID.GemSapphire; MineResist = 2f; MinPick = 110;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = 0.05f; g = 0.05f; b = 0.2f; }
    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (closer && Main.GameUpdateCount % 120 == 0)
            Main.LocalPlayer.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().spiritPressure = System.Math.Min(100,
                Main.LocalPlayer.GetModPlayer<global::XianXia.Common.Players.XianXiaPlayer>().spiritPressure + 1);
    }
}
