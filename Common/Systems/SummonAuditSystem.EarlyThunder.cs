using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using XianXia.Content.Items.Construction;
using XianXia.Content.Tiles.Construction;
using XianXia.Common.Players;

namespace XianXia.Common.Systems;

public partial class SummonAuditSystem
{
    private static void AuditEarlyThunderBiome(Action<bool, string> check)
    {
        var recipe = Main.recipe.Take(Recipe.numRecipes).Single(r => !r.Disabled && r.createItem.type == ModContent.ItemType<ThunderBiomeBlock>());
        check(recipe.createItem.stack == 100 && recipe.Conditions.Count == 0 && recipe.requiredTile.Contains(TileID.WorkBenches)
            && recipe.requiredItem.Any(i => i.type == ItemID.Cloud && i.stack == 100),
            "native artificial thunder recipe is available before Hardmode with vanilla clouds");
        int mode = Main.netMode, local = Main.myPlayer; bool hard = Main.hardMode;
        int x = Main.maxTilesX / 2, y = Main.maxTilesY / 2;
        var saved = new List<(int X, int Y, bool HasTile, ushort Type)>();
        try {
            Main.netMode = NetmodeID.Server; Main.myPlayer = Main.maxPlayers; Main.hardMode = false;
            int tileType = ModContent.TileType<ThunderConstructedTile>();
            for (int i = 0; i < 100; i++) {
                Tile tile = Main.tile[x + i % 10, y + i / 10];
                saved.Add((x + i % 10, y + i / 10, tile.HasTile, tile.TileType));
                tile.HasTile = true; tile.TileType = (ushort)tileType;
            }
            var actor = new Player { active = true }; actor.Center = new Vector2((x + 5) * 16, (y + 5) * 16);
            LoaderManager.Get<BiomeLoader>().UpdateBiomes(actor);
            check(actor.InModBiome<Content.Biomes.ThunderMarshCloudsBiome>(), "native server scan recognizes artificial thunder before Hardmode");
            check(BossSummonRules.CanUseGeneratedBossSummon(actor, "tribulation_cloud_avatar"), "native artificial thunder meets pre-Hardmode cloud boss world and site requirements");
            check(!BossSummonRules.CanUseGeneratedBossSummon(actor, "thunder_marsh_jiao"), "native artificial thunder does not bypass Jiao Hardmode requirement");
            check(!EnemySpawnRules.Allows("ThunderPatternHawk", false, false, false, false)
                && !EnemySpawnRules.Allows("TribulationCloudling", false, false, false, false),
                "native early thunder construction keeps ordinary thunder enemies gated");
        }
        finally {
            foreach (var old in saved) { Tile tile = Main.tile[old.X, old.Y]; tile.HasTile = old.HasTile; tile.TileType = old.Type; }
            Main.netMode = mode; Main.myPlayer = local; Main.hardMode = hard;
        }
    }
}
