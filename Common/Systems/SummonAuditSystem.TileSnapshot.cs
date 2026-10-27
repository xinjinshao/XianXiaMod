using System;
using Terraria;
using Terraria.ID;

namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    // Tile is an index-backed handle. Copy the pinned SDK's five data values, not the handle.
    private readonly record struct AuditTileSnapshot(TileTypeData Type,WallTypeData Wall,LiquidData Liquid,
        TileWallWireStateData State,TileWallBrightnessInvisibilityData Coating)
    {
        public static AuditTileSnapshot Capture(Tile tile) => new(tile.Get<TileTypeData>(),tile.Get<WallTypeData>(),
            tile.Get<LiquidData>(),tile.Get<TileWallWireStateData>(),tile.Get<TileWallBrightnessInvisibilityData>());
        public void Restore(Tile tile) {
            tile.Get<TileTypeData>() = Type; tile.Get<WallTypeData>() = Wall; tile.Get<LiquidData>() = Liquid;
            tile.Get<TileWallWireStateData>() = State; tile.Get<TileWallBrightnessInvisibilityData>() = Coating;
        }
        public bool Matches(Tile tile) => this == Capture(tile);
    }
    private static void AuditTileSnapshotRestoration(Action<bool,string> check)
    {
        Tile tile = Main.tile[Main.maxTilesX / 2,15]; var original = AuditTileSnapshot.Capture(tile);
        try {
            for (int scenario = 0; scenario < 3; scenario++) {
                tile.ClearEverything(); tile.HasTile = true; tile.TileType = TileID.Stone; tile.WallType = WallID.DirtUnsafe;
                tile.IsHalfBlock = scenario == 0; if (scenario > 0) tile.Slope = (SlopeType)scenario;
                tile.IsActuated = scenario == 1; tile.HasActuator = true;
                tile.TileFrameX = (short)(18 + scenario * 18); tile.TileFrameY = 36; tile.WallFrameX = 36; tile.WallFrameY = 54;
                tile.TileFrameNumber = scenario; tile.WallFrameNumber = scenario;
                tile.TileColor = (byte)(4 + scenario); tile.WallColor = (byte)(8 + scenario);
                tile.LiquidAmount = (byte)(120 + scenario); tile.LiquidType = LiquidID.Lava; tile.SkipLiquid = true; tile.CheckingLiquid = true;
                tile.RedWire = tile.GreenWire = tile.BlueWire = tile.YellowWire = true;
                tile.IsTileInvisible = tile.IsWallInvisible = tile.IsTileFullbright = tile.IsWallFullbright = true;
                var saved = AuditTileSnapshot.Capture(tile); tile.ClearEverything();
                check(!saved.Matches(tile),"native Tile data snapshot is independent of ClearEverything including rich frames paint wires liquid and coating state");
                saved.Restore(tile);
                check(saved.Matches(tile) && tile.HasTile && tile.TileType == TileID.Stone && tile.WallType == WallID.DirtUnsafe
                    && tile.LiquidAmount == 120 + scenario && tile.LiquidType == LiquidID.Lava && tile.RedWire && tile.IsTileInvisible && tile.IsWallFullbright,
                    "native Tile data snapshot restores five SDK data structures without losing private packed fields");
            }
        } finally { original.Restore(tile); }
        check(original.Matches(tile),"native Tile snapshot audit restores original loaded world tile contents");
    }
}
