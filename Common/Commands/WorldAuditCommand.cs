using System.Collections.Generic;
using System.Text.Json;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using XianXia.Content.Tiles;

namespace XianXia.Common.Commands;

// Console-only, explicit read-only scan. No work is done during normal play.
public class WorldAuditCommand : ModCommand
{
    public override string Command => "xianxia-world-audit";
    public override CommandType Type => CommandType.Console;
    public override string Description => Language.GetTextValue("Mods.XianXia.WorldAudit.Description");
    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var objects = new Dictionary<int, ObjectCount>();
        void Add<T>(int height) where T : ModTile => objects.Add(ModContent.TileType<T>(), new ObjectCount { Name = typeof(T).Name, Height = height });
        Add<SwordTabletTile>(3);
        Add<SingingThunderStoneTile>(2);
        Add<RiftMembraneTile>(2);
        Add<BrokenHeavenTabletTile>(4);
        Add<ArchiveLightPillarTile>(6);
        for (int x = 0; x < Main.maxTilesX; x++)
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (!tile.HasTile || !objects.TryGetValue(tile.TileType, out ObjectCount count)) continue;
                count.Cells++;
                if (tile.TileFrameX != 0 || tile.TileFrameY != 0) continue;
                count.Origins++;
                bool complete = x + 1 < Main.maxTilesX && y + count.Height <= Main.maxTilesY;
                if (complete)
                    for (int column = 0; column < 2; column++)
                        for (int row = 0; row < count.Height; row++)
                        {
                            Tile part = Main.tile[x + column, y + row];
                            if (!part.HasTile || part.TileType != tile.TileType
                                || part.TileFrameX != column * 16 || part.TileFrameY != row * 16) complete = false;
                        }
                if (complete) count.Complete++;
            }
        string report = JsonSerializer.Serialize(new { Width = Main.maxTilesX, Height = Main.maxTilesY, Objects = objects.Values });
        Mod.Logger.Info("SavedWorldAudit: " + report);
        caller.Reply(report);
    }

    private sealed class ObjectCount
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Cells { get; set; }
        public int Origins { get; set; }
        public int Complete { get; set; }
    }
}
