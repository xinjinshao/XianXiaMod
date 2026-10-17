using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Items;
using XianXia.Common.UI;
using XianXia.Content.Items.HandGenerated;
using XianXia.Content.Items.Weapons;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditInscriptionNavigation(Action<bool,string> check)
    {
        Player previous = Main.player[0]; int local = Main.myPlayer;
        bool menu = Main.gameMenu, blocked = Main.blockInput, chat = Main.drawingPlayerChat, inventory = Main.playerInventory;
        Item mouse = Main.mouseItem;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        try {
            Main.myPlayer = 0; Main.gameMenu = Main.blockInput = Main.drawingPlayerChat = false; Main.playerInventory = true; Main.mouseItem = new Item();
            var actor = new Player { whoAmI = 0, active = true, selectedItem = 1 }; Main.player[0] = actor;
            actor.inventory[1].SetDefaults(ModContent.ItemType<ArtifactAwakeningSeal>());
            actor.inventory[2].SetDefaults(ModContent.ItemType<ThunderPatternSwordCase>());
            var growth = actor.inventory[2].GetGlobalItem<RefinedArtifact>(); growth.SetLevel(3); growth.TryAwaken();
            actor.inventory[2].GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.Greenwood);
            Item target = actor.inventory[2].Clone(), tool = actor.inventory[1].Clone();
            var ui = new InscriptionUIState(); const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            void Set(string field,object value) => typeof(InscriptionUIState).GetField(field,flags).SetValue(ui,value);
            bool Read(string method) => (bool)typeof(InscriptionUIState).GetMethod(method,flags).Invoke(ui,new object[] { actor });
            Set("toolSlot",1); Set("toolType",tool.type); Set("targetSlot",2); Set("targetType",target.type); Set("targetPrefix",target.prefix);
            Set("previousKind",(byte)InscriptionKind.Greenwood); Set("previousLevel",(byte)3); Set("previousAwakened",true);
            Set("previousDaoRoute",(byte)0); Set("previousWorldRoute",(byte)DownedBossSystem.ChosenRoute);
            check(Read("ToolStillMatches") && Read("TargetStillMatches"),"native forge navigation shares valid tool and equipment preview predicates without activating graphics");
            for (int invalid = 0; invalid < 7; invalid++) {
                actor.inventory[2] = target.Clone();
                switch (invalid) {
                    case 0: actor.inventory[2].TurnToAir(); break;
                    case 1: actor.inventory[2].prefix++; break;
                    case 2: actor.inventory[2].GetGlobalItem<InscribedEquipment>().SetKind(InscriptionKind.None); break;
                    case 3: actor.inventory[2].GetGlobalItem<RefinedArtifact>().SetLevel(2); break;
                    case 4: actor.inventory[2].GetGlobalItem<RefinedArtifact>().SetLevel(2); actor.inventory[2].GetGlobalItem<RefinedArtifact>().SetLevel(3); break;
                    case 5: actor.inventory[2].GetGlobalItem<RefinedArtifact>().TryTransform(DownedBossSystem.EndgameRoute.RebuildHeaven); break;
                    case 6: var changed = new TagCompound(); world.SaveWorldData(changed); changed["chosenRoute"] = DownedBossSystem.ChosenRoute == DownedBossSystem.EndgameRoute.RebuildHeaven ? 2 : 1; world.LoadWorldData(changed); break;
                }
                check(!Read("TargetStillMatches"),"native forge confirmation rejects stale item prefix inscription refinement awakening Dao or world route");
                world.LoadWorldData(saved);
            }
            actor.inventory[2] = target.Clone();
            for (int invalid = 0; invalid < 8; invalid++) {
                switch (invalid) {
                    case 0: Main.playerInventory = false; break;
                    case 1: Main.blockInput = true; break;
                    case 2: Main.drawingPlayerChat = true; break;
                    case 3: actor.dead = true; break;
                    case 4: actor.noItems = true; break;
                    case 5: actor.selectedItem = 2; break;
                    case 6: Main.mouseItem = target.Clone(); break;
                    case 7: actor.inventory[1].TurnToAir(); break;
                }
                check(!Read("ToolStillMatches"),"native forge confirmation rejects closed inventory blocked input chat death disabled use switched tool or occupied cursor");
                Main.playerInventory = true; Main.blockInput = Main.drawingPlayerChat = false; actor.dead = actor.noItems = false; actor.selectedItem = 1; Main.mouseItem = new Item(); actor.inventory[1] = tool.Clone();
            }
            check(Read("ToolStillMatches") && Read("TargetStillMatches"),"native forge predicate fixture restores its valid local preview");
        } finally { world.LoadWorldData(saved); Main.player[0] = previous; Main.myPlayer = local; Main.gameMenu = menu; Main.blockInput = blocked; Main.drawingPlayerChat = chat; Main.playerInventory = inventory; Main.mouseItem = mouse; }
        check(ReferenceEquals(Main.player[0],previous) && ReferenceEquals(Main.mouseItem,mouse),"native forge navigation fixture restores player and cursor item context");
    }
}
