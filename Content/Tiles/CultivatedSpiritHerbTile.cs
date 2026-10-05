using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using XianXia.Common.Systems;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Materials;

namespace XianXia.Content.Tiles;

// Separate from the solid SpiritHerbTile used by existing world generation.
public class CultivatedSpiritHerbTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.WaterDeath = false;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.AnchorValidTiles = new[] {
            TileID.Dirt, TileID.Grass, TileID.JungleGrass,
            ModContent.TileType<GreenwoodSoilTile>(), ModContent.TileType<SpiritMossTile>()
        };
        TileObjectData.newTile.AnchorAlternateTiles = new[] { (int)TileID.ClayPot, TileID.PlanterBox };
        TileObjectData.addTile(Type);
        HitSound = SoundID.Grass;
        DustType = DustID.Grass;
        AddMapEntry(new Color(90, 190, 130), CreateMapEntryName());
    }

    // Do not replace a crop or consume a seed merely to harvest it.
    public override bool CanPlace(int i, int j)
    {
        Tile tile = Framing.GetTileSafely(i, j);
        return !tile.HasTile && tile.LiquidAmount == 0;
    }

    public override void RandomUpdate(int i, int j)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        Tile tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type || tile.LiquidAmount > 0
            || SpiritHerbRules.Stage(tile.TileFrameX) == SpiritHerbRules.MatureStage
            || !WorldGen.genRand.NextBool(4)) return;
        tile.TileFrameX = SpiritHerbRules.NextFrame(tile.TileFrameX);
        if (Main.netMode == NetmodeID.Server) NetMessage.SendTileSquare(-1, i, j, 1);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        int frame = Framing.GetTileSafely(i, j).TileFrameX;
        int roots = SpiritHerbRules.RootYield(frame);
        if (roots > 0) yield return new Item(ModContent.ItemType<GreenwoodRoot>(), roots);
        // Immature destruction refunds exactly the planted seed, without roots.
        int roll = roots > 0 ? Main.rand.Next(3) : 0;
        yield return new Item(ModContent.ItemType<SpiritHerbSeeds>(), SpiritHerbRules.SeedYield(frame, roll));
    }

    public override bool IsTileSpelunkable(int i, int j) =>
        SpiritHerbRules.Stage(Framing.GetTileSafely(i, j).TileFrameX) == SpiritHerbRules.MatureStage;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (!IsTileSpelunkable(i, j)) return;
        r = 0.05f; g = 0.22f; b = 0.12f;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY,
        ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        // The original plant sprite is one 16x24 image, not a frame sheet.
        width = 16; height = 24; offsetY = -8; tileFrameX = 0; tileFrameY = 0;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Texture2D texture = TextureAssets.Tile[Type].Value;
        Tile tile = Framing.GetTileSafely(i, j);
        Vector2 offset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 position = new Vector2(i * 16 + 8, j * 16 + 16) - Main.screenPosition + offset;
        spriteBatch.Draw(texture, position, null, Lighting.GetColor(i, j), 0f,
            new Vector2(texture.Width / 2f, texture.Height), SpiritHerbRules.DrawScale(tile.TileFrameX),
            i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        return false;
    }
}
