using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using XianXia.Content.Tiles;

namespace XianXia.Common.Systems;

public class GeneratedBiomeWorldGenSystem : ModSystem
{
    private List<Rectangle> furnace = new(), greenwood = new(), thunder = new(), star = new(), sect = new(), heaven = new(), moonbone = new();
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        if (!ModContent.GetInstance<XianXiaConfig>().EnableWorldGeneration)
        {
            return;
        }

        int index = tasks.FindIndex(pass => pass.Name == "Shinies");
        if (index == -1)
        {
            return;
        }

        tasks.Insert(index + 2, new PassLegacy("XianXia Cultivation Biomes", GenerateCultivationBiomes));
        // Place on final terrain after native cleanup and structure passes.
        tasks.Add(new PassLegacy("XianXia Cultivation Objects", PlaceCultivationObjects));
    }

    private void GenerateCultivationBiomes(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.XianXia.WorldGeneration.CultivationDomains");

        greenwood = GenerateGroundPatches(ModContent.TileType<GreenwoodSoilTile>(), ModContent.TileType<SpiritHerbTile>(), 3, (int)Main.worldSurface - 80, (int)Main.worldSurface + 40, 34, 16);
        furnace = GenerateGroundPatches(ModContent.TileType<FurnaceSlagTile>(), ModContent.TileType<FurnaceSlagTile>(), 3, (int)Main.rockLayer, Main.maxTilesY - 260, 42, 22, ModContent.WallType<BlackFurnaceWall>());
        thunder = GenerateCloudFields(ModContent.TileType<ThunderCloudTile>(), 4, 160, (int)Main.worldSurface - 160, 46, 12);
        star = GenerateGroundPatches(ModContent.TileType<StarAbyssCrystalTile>(), ModContent.TileType<StarAbyssCrystalTile>(), 3, Main.maxTilesY - 420, Main.maxTilesY - 180, 38, 24);
        sect = GenerateGroundPatches(ModContent.TileType<SectRuinBrickTile>(), ModContent.TileType<SectRuinBrickTile>(), 2, (int)Main.rockLayer - 80, Main.maxTilesY - 360, 50, 18);
        heaven = GenerateGroundPatches(ModContent.TileType<FallenHeavenJadeTile>(), ModContent.TileType<FallenHeavenJadeTile>(), 2, Main.maxTilesY - 520, Main.maxTilesY - 260, 44, 24);
        moonbone = GenerateGroundPatches(ModContent.TileType<MoonboneTile>(), ModContent.TileType<MoonboneTile>(), 2, Main.maxTilesY - 360, Main.maxTilesY - 140, 54, 28);
    }

    private void PlaceCultivationObjects(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.XianXia.WorldGeneration.CultivationDomains");
        sect = ModContent.GetInstance<SectRuinHallSystem>().Generate(sect);
        PlaceObjects(ModContent.TileType<SwordTabletTile>(), 4, sect, ModContent.TileType<SectRuinBrickTile>(), 3);
        ModContent.GetInstance<ThunderCloudShrineSystem>().Generate(thunder);
        PlaceObjects(ModContent.TileType<SingingThunderStoneTile>(), 3, thunder, ModContent.TileType<ThunderCloudTile>(), 2);
        ModContent.GetInstance<StarAbyssBridgeSystem>().Generate(star);
        PlaceObjects(ModContent.TileType<RiftMembraneTile>(), 3, star, ModContent.TileType<StarAbyssCrystalTile>(), 2);
        ModContent.GetInstance<FallenHeavenCourtSystem>().Generate(heaven);
        PlaceObjects(ModContent.TileType<BrokenHeavenTabletTile>(), 2, heaven, ModContent.TileType<FallenHeavenJadeTile>(), 4);
        PlaceObjects(ModContent.TileType<ArchiveLightPillarTile>(), 2, heaven, ModContent.TileType<FallenHeavenJadeTile>(), 6);
        ModContent.GetInstance<GreenwoodGardenSystem>().Generate(greenwood);
        ModContent.GetInstance<FurnaceWorkshopSystem>().Generate(furnace);
        ModContent.GetInstance<MoonboneTerraceSystem>().Generate(moonbone);
    }

    private void PlaceObjects(int tileType, int count, List<Rectangle> regions, int groundType, int height)
    {
        int placed = 0;
        if (tileType == ModContent.TileType<SwordTabletTile>())
            foreach (Rectangle hall in ModContent.GetInstance<SectRuinHallSystem>().Halls)
                foreach (int offset in new[] { 10, 29 })
                    if (placed < count && TryPlaceObject(tileType, groundType, height, hall.Left + offset, hall.Bottom - 3)) placed++;
        if (tileType == ModContent.TileType<SingingThunderStoneTile>())
            foreach (Rectangle shrine in ModContent.GetInstance<ThunderCloudShrineSystem>().Shrines)
                if (placed < count && TryPlaceObject(tileType, groundType, height, shrine.Left + ThunderCloudShrineSystem.StoneOffset,
                    shrine.Bottom - ThunderCloudShrineSystem.FloorDepth)) placed++;
        if (tileType == ModContent.TileType<RiftMembraneTile>())
            foreach (Rectangle bridge in ModContent.GetInstance<StarAbyssBridgeSystem>().Bridges)
                if (placed < count && TryPlaceObject(tileType,groundType,height,bridge.X + StarAbyssBridgeSystem.MembraneOffset,
                    bridge.Y + StarAbyssBridgeSystem.BridgeRow)) placed++;
        if (tileType == ModContent.TileType<BrokenHeavenTabletTile>() || tileType == ModContent.TileType<ArchiveLightPillarTile>())
            foreach (Rectangle court in ModContent.GetInstance<FallenHeavenCourtSystem>().Courts)
                if (placed < count && TryPlaceObject(tileType,groundType,height,
                    court.Left + (tileType == ModContent.TileType<BrokenHeavenTabletTile>() ? FallenHeavenCourtSystem.TabletOffset : FallenHeavenCourtSystem.PillarOffset),
                    court.Bottom - FallenHeavenCourtSystem.FloorDepth)) placed++;
        for (int attempt = 0; attempt < count * 600 && placed < count && regions.Count > 0; attempt++)
        {
            Rectangle region = regions[WorldGen.genRand.Next(regions.Count)];
            int x = WorldGen.genRand.Next(region.Left, region.Right - 1);
            int groundY = WorldGen.genRand.Next(region.Top + height, region.Bottom);
            if (TryPlaceObject(tileType, groundType, height, x, groundY)) placed++;
        }
        // Random sampling can miss narrow legal ledges. Exhaust all region-local
        // positions before reporting shortage; this fallback changes no terrain.
        foreach (Rectangle region in regions)
        {
            for (int groundY = region.Top + height; groundY < region.Bottom && placed < count; groundY++)
                for (int x = region.Left; x < region.Right - 1 && placed < count; x++)
                    if (TryPlaceObject(tileType, groundType, height, x, groundY)) placed++;
        }
        // Final terrain may lack flat ledges. Add a small matching pedestal
        // only in completely empty, dry space; never excavate existing tiles.
        foreach (Rectangle region in regions)
            for (int groundY = region.Top + height; groundY < region.Bottom && placed < count; groundY++)
                for (int x = region.Left; x < region.Right - 1 && placed < count; x++)
                    if (TryPlacePedestal(tileType, groundType, height, x, groundY)) placed++;
        Mod.Logger.Info($"Cultivation object {tileType}: {placed}/{count} placed in {regions.Count} matching regions.");
        if (placed < count) Mod.Logger.Warn($"Cultivation object {tileType} placement incomplete; no suitable space in generated regions.");
    }

    private static bool TryPlacePedestal(int tileType, int groundType, int height, int x, int groundY) =>
        TryPlaceProtectedPedestal(tileType,groundType,height,x,groundY,GenVars.structures);

    internal static bool TryPlaceProtectedPedestal(int tileType,int groundType,int height,int x,int groundY,StructureMap structures)
    {
        if (!PlacementAllowed(tileType,groundType,height,x,groundY,structures)) return false;
        for (int column = 0; column < 2; column++) for (int row = 0; row <= height; row++) {
            Tile space = Main.tile[x + column,groundY - row];
            if (space.HasTile || space.LiquidAmount > 0) return false;
        }
        var original = CapturePlacement(x,groundY,height); bool placed = false;
        try {
            for (int column = 0; column < 2; column++) {
                Tile support = Main.tile[x + column,groundY];
                support.HasTile = true; support.TileType = (ushort)groundType; support.IsHalfBlock = false; support.Slope = 0;
            }
            if (!TryPlaceProtectedObject(tileType,groundType,height,x,groundY,structures)) return false;
            for (int column = 0; column < 2; column++) WorldGen.SquareTileFrame(x + column,groundY);
            placed = true;
            return true;
        } finally { if (!placed) RestorePlacement(original); }
    }

    private static bool TryPlaceObject(int tileType, int groundType, int height, int x, int groundY) =>
        TryPlaceProtectedObject(tileType,groundType,height,x,groundY,GenVars.structures);

    internal static bool TryPlaceProtectedObject(int tileType,int groundType,int height,int x,int groundY,StructureMap structures)
    {
        if (!PlacementAllowed(tileType,groundType,height,x,groundY,structures)) return false;
        for (int column = 0; column < 2; column++) {
            Tile support = Main.tile[x + column,groundY];
            if (!support.HasTile || support.TileType != groundType || support.IsHalfBlock || support.Slope != 0) return false;
            for (int row = 1; row <= height; row++) {
                Tile space = Main.tile[x + column,groundY - row];
                if (space.HasTile || space.LiquidAmount > 0) return false;
            }
        }
        var original = CapturePlacement(x,groundY,height); bool complete = false;
        try {
            WorldGen.PlaceObject(x,groundY - 1,tileType);
            bool valid = true;
            for (int column = 0; column < 2; column++) for (int row = 0; row < height; row++) {
                Tile part = Main.tile[x + column,groundY - height + row];
                if (!part.HasTile || part.TileType != tileType || part.TileFrameX != column * 16 || part.TileFrameY != row * 16) valid = false;
            }
            complete = valid;
            return complete;
        } finally { if (!complete) RestorePlacement(original); }
    }

    private static bool PlacementAllowed(int tileType,int groundType,int height,int x,int groundY,StructureMap structures)
    {
        if (structures == null || height < 1 || height >= Main.maxTilesY || tileType < 0 || tileType >= TileLoader.TileCount
            || groundType < 0 || groundType >= TileLoader.TileCount || !WorldGen.InWorld(x,groundY,10) || !WorldGen.InWorld(x + 1,groundY - height,10)) return false;
        var data = TileObjectData.GetTileData(tileType,0);
        if (data == null || data.Width != 2 || data.Height != height || data.Origin.X != 0 || data.Origin.Y != height - 1
            || data.CoordinateWidth != 16 || data.CoordinatePadding != 0) return false;
        bool ownedHall = false;
        if (tileType == ModContent.TileType<SwordTabletTile>() && groundType == ModContent.TileType<SectRuinBrickTile>() && height == 3)
            foreach (Rectangle hall in ModContent.GetInstance<SectRuinHallSystem>().Halls)
                if (x >= hall.Left + 2 && x + 1 < hall.Right - 2 && groundY == hall.Bottom - 3 && groundY - height >= hall.Top) { ownedHall = true; break; }
        if (tileType == ModContent.TileType<SingingThunderStoneTile>() && groundType == ModContent.TileType<ThunderCloudTile>() && height == 2)
            ownedHall |= ThunderCloudShrineSystem.IsPlannedStone(x, groundY);
        if (groundType == ModContent.TileType<FallenHeavenJadeTile>())
            ownedHall |= FallenHeavenCourtSystem.IsPlannedObject(tileType,height,x,groundY);
        if (tileType == ModContent.TileType<RiftMembraneTile>() && groundType == ModContent.TileType<StarAbyssCrystalTile>() && height == 2)
            ownedHall |= StarAbyssBridgeSystem.IsPlannedMembrane(x,groundY);
        for (int column = 0; column < 2; column++) for (int row = 0; row <= height; row++) {
            int tx = x + column, ty = groundY - row; Tile tile = Main.tile[tx,ty];
            if (Main.wallHouse[tile.WallType] || tile.HasActuator || tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire
                || (!ownedHall && !WorldGenerationProtection.CanChange(tx,ty,structures))) return false;
        }
        return true;
    }

    private readonly record struct PlacementCell(int X,int Y,bool Has,ushort Type,short FX,short FY,Terraria.ID.SlopeType Slope,bool Half);
    private static List<PlacementCell> CapturePlacement(int x,int groundY,int height)
    {
        var cells = new List<PlacementCell>();
        for (int column = 0; column < 2; column++) for (int row = 0; row <= height; row++) {
            Tile tile = Main.tile[x + column,groundY - row];
            cells.Add(new PlacementCell(x + column,groundY - row,tile.HasTile,tile.TileType,tile.TileFrameX,tile.TileFrameY,tile.Slope,tile.IsHalfBlock));
        }
        return cells;
    }
    private static void RestorePlacement(List<PlacementCell> cells)
    {
        foreach (var cell in cells) {
            Tile tile = Main.tile[cell.X,cell.Y]; tile.HasTile = cell.Has; tile.TileType = cell.Type;
            tile.TileFrameX = cell.FX; tile.TileFrameY = cell.FY; tile.Slope = cell.Slope; tile.IsHalfBlock = cell.Half;
        }
    }

    private static List<Rectangle> GenerateGroundPatches(int primaryTile, int accentTile, int patches, int minY, int maxY, int radiusX, int radiusY, int wallType = -1)
    {
        var regions = new List<Rectangle>();
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(160, Main.maxTilesX - 160);
            int y = NextWorldY(minY, maxY);
            int rx = radiusX + WorldGen.genRand.Next(-8, 9);
            int ry = radiusY + WorldGen.genRand.Next(-4, 5);
            if (PaintEllipse(x, y, rx, ry, primaryTile, accentTile, wallType) > 0)
                regions.Add(new Rectangle(x - rx, y - ry, rx * 2 + 1, ry * 2 + 1));
        }
        return regions;
    }

    private static List<Rectangle> GenerateCloudFields(int cloudTile, int patches, int minY, int maxY, int radiusX, int radiusY)
    {
        var regions = new List<Rectangle>();
        for (int i = 0; i < patches; i++)
        {
            int x = WorldGen.genRand.Next(180, Main.maxTilesX - 180);
            int y = NextWorldY(minY, maxY);
            if (PaintCloudEllipse(x,y,radiusX,radiusY,cloudTile,GenVars.structures) > 0)
                regions.Add(new Rectangle(x - radiusX, y - radiusY, radiusX * 2 + 1, radiusY * 2 + 1));
        }
        return regions;
    }

    internal static int PaintCloudEllipse(int x, int y, int radiusX, int radiusY, int cloudTile, StructureMap structures)
    {
        int painted = 0;
        for (int tx = x - radiusX; tx <= x + radiusX; tx++) for (int ty = y - radiusY; ty <= y + radiusY; ty++) {
            if (!WorldGen.InWorld(tx,ty,10)) continue;
            float nx = (tx - x) / (float)radiusX, ny = (ty - y) / (float)radiusY;
            if (nx * nx + ny * ny > 1f || !WorldGen.genRand.NextBool(2)) continue;
            Tile tile = Main.tile[tx,ty];
            if (tile.HasTile || tile.LiquidAmount > 0 || !WorldGenerationProtection.CanChange(tx,ty,structures)) continue;
            tile.HasTile = true; tile.TileType = (ushort)cloudTile; painted++;
            WorldGen.SquareTileFrame(tx,ty);
        }
        return painted;
    }

    private static int PaintEllipse(int x, int y, int radiusX, int radiusY, int primaryTile, int accentTile, int wallType) =>
        PaintProtectedEllipse(x,y,radiusX,radiusY,primaryTile,accentTile,wallType,GenVars.structures);

    internal static int PaintProtectedEllipse(int x, int y, int radiusX, int radiusY, int primaryTile, int accentTile, int wallType, StructureMap structures)
    {
        int painted = 0;
        for (int tx = x - radiusX; tx <= x + radiusX; tx++)
        {
            for (int ty = y - radiusY; ty <= y + radiusY; ty++)
            {
                if (!WorldGen.InWorld(tx, ty, 10))
                {
                    continue;
                }

                float nx = (tx - x) / (float)radiusX;
                float ny = (ty - y) / (float)radiusY;
                if (nx * nx + ny * ny > 1f || !Main.tile[tx, ty].HasTile)
                {
                    continue;
                }

                Tile tile = Main.tile[tx, ty];
                if (!CanReplace(tile.TileType) || !WorldGenerationProtection.CanChange(tx,ty,structures))
                {
                    continue;
                }

                tile.TileType = WorldGen.genRand.NextBool(5) ? (ushort)accentTile : (ushort)primaryTile;
                if (wallType >= 0) tile.WallType = (ushort)wallType;
                painted++;
                WorldGen.SquareTileFrame(tx, ty);
            }
        }
        return painted;
    }

    private static bool CanReplace(ushort tileType)
    {
        return tileType == TileID.Dirt
            || tileType == TileID.Stone
            || tileType == TileID.ClayBlock
            || tileType == TileID.Mud
            || tileType == TileID.SnowBlock
            || tileType == TileID.IceBlock
            || tileType == TileID.Ash
            || tileType == TileID.Ebonstone
            || tileType == TileID.Crimstone
            || tileType == TileID.Pearlstone;
    }

    private static int NextWorldY(int minY, int maxY)
    {
        int lower = Utils.Clamp(Math.Min(minY, maxY), 80, Main.maxTilesY - 200);
        int upper = Utils.Clamp(Math.Max(minY, maxY), lower + 1, Main.maxTilesY - 120);
        return WorldGen.genRand.Next(lower, upper);
    }
}
