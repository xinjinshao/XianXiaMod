using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.Players;
using XianXia.Common.UI;
using XianXia.Content.Items.Weapons;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditEarlySkillHints(Action<bool,string> check)
    {
        var previous = Main.player[0]; int mode = Main.netMode,local = Main.myPlayer;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        var tooltip = typeof(ArtifactSkillUIState).GetMethod("SkillTooltip",BindingFlags.Instance | BindingFlags.NonPublic)!;
        try {
            Main.myPlayer = 0; var ui = new ArtifactSkillUIState();
            foreach (int netMode in new[] { 0,1,2 }) foreach (bool sword in new[] { false,true })
            foreach (int record in new[] { 0,1,2,3 }) foreach (int route in new[] { 0,1,2,3 }) foreach (bool core in new[] { false,true }) {
                Main.netMode = netMode; world.LoadWorldData(new TagCompound { ["chosenRoute"] = route }); if (core) DownedBossSystem.DownedBosses.Add("old_heaven_dao_core");
                var actor = new Player { active = true,whoAmI = 0,selectedItem = 0 }; Main.player[0] = actor;
                var state = actor.GetModPlayer<XianXiaPlayer>(); state.cultivationStage = CultivationStage.DaoSevering; state.spiritualEnergy = 100;
                actor.inventory[0] = new Item(sword ? ModContent.ItemType<CloudpiercerFlyingSword>() : ModContent.ItemType<GreenwoodArrayPlate>());
                var growth = actor.HeldItem.GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken(); if (record > 0) growth.TryTransform((DownedBossSystem.EndgameRoute)record);
                var before = state.CaptureSnapshot(); var skill = sword ? ArtifactSkill.SwordBurst : ArtifactSkill.ArrayPulse;
                string text = (string)tooltip.Invoke(ui,new object[] { skill,state })!;
                bool active = record > 0 && record == route && core;
                string key = "Mods.XianXia.Skills." + (sword ? "Sword" : "Array") + (active ? "DaoRequirement" : "Requirement");
                check(text.Contains(Language.GetTextValue(key)) && !text.Contains("Mods.XianXia.") && !text.Contains("{0}"),
                    "native early artifact actual skill panel tooltip selects active Dao or ordinary requirement for every world and network mode");
                check(state.CaptureSnapshot().Equals(before) && (int)RefinedArtifact.GetDaoRoute(actor.HeldItem) == record && RefinedArtifact.IsAwakened(actor.HeldItem),
                    "native early artifact skill panel description preserves resources and item growth");
            }
        } finally { Main.player[0] = previous; Main.netMode = mode; Main.myPlayer = local; world.LoadWorldData(saved); }
    }
}
