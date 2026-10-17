using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.UI;
using XianXia.Content.Items.HandGenerated;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditRouteNavigation(Action<bool,string> check)
    {
        Player previous = Main.player[0]; int local = Main.myPlayer;
        bool menu = Main.gameMenu, blocked = Main.blockInput, chat = Main.drawingPlayerChat, inventory = Main.playerInventory; Item mouse = Main.mouseItem;
        var world = ModContent.GetInstance<DownedBossSystem>(); var saved = new TagCompound(); world.SaveWorldData(saved);
        try {
            Main.myPlayer = 0; Main.gameMenu = Main.blockInput = Main.drawingPlayerChat = false; Main.playerInventory = true; Main.mouseItem = new Item();
            var actor = new Player { whoAmI = 0, active = true, selectedItem = 1 }; Main.player[0] = actor;
            actor.inventory[1].SetDefaults(ModContent.ItemType<RouteMaterial>()); actor.inventory[1].stack = 2; Item material = actor.inventory[1];
            var ui = new EndgameRouteUIState(); const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            void Set(string field,object value) => typeof(EndgameRouteUIState).GetField(field,flags).SetValue(ui,value);
            bool Valid() => (bool)typeof(EndgameRouteUIState).GetMethod("ValidSelection",flags).Invoke(ui,null);
            void Confirm() => typeof(EndgameRouteUIState).GetMethod("ConfirmSelection",flags).Invoke(ui,null);
            Set("slot",1); Set("selectedItem",material); Set("selected",DownedBossSystem.EndgameRoute.None);
            check(Valid(),"native route navigation validates actual held material without constructing graphics");
            Confirm(); check(material.stack == 2,"native route confirmation with no preview consumes nothing");
            Set("selected",DownedBossSystem.EndgameRoute.SeverHeaven);
            for (int invalid = 0; invalid < 12; invalid++) {
                switch (invalid) {
                    case 0: Main.playerInventory = false; break;
                    case 1: Main.blockInput = true; break;
                    case 2: Main.drawingPlayerChat = true; break;
                    case 3: actor.dead = true; break;
                    case 4: actor.noItems = true; break;
                    case 5: actor.frozen = true; break;
                    case 6: actor.selectedItem = 2; break;
                    case 7: Main.mouseItem = material.Clone(); break;
                    case 8: actor.inventory[1] = material.Clone(); break;
                    case 9: material.stack = 0; break;
                    case 10: Set("slot",58); break;
                    case 11: Set("selectedItem",null); break;
                }
                check(!Valid(),"native route preview rejects input inventory lifecycle cursor replacement empty stack and slot failures");
                Confirm(); check(material.stack == (invalid == 9 ? 0 : 2),"native invalid route confirmation does not consume material");
                Main.playerInventory = true; Main.blockInput = Main.drawingPlayerChat = false; actor.dead = actor.noItems = actor.frozen = false; actor.selectedItem = 1;
                Main.mouseItem = new Item(); material.stack = 2; actor.inventory[1] = material; Set("slot",1); Set("selectedItem",material);
            }
            var locked = new TagCompound(); world.SaveWorldData(locked); locked["chosenRoute"] = 1; world.LoadWorldData(locked);
            Confirm();
            typeof(EndgameRouteUIState).GetMethod("SelectRoute",flags).Invoke(ui,new object[] { DownedBossSystem.EndgameRoute.AcceptStarAbyss });
            check(material.stack == 2 && DownedBossSystem.ChosenRoute == DownedBossSystem.EndgameRoute.RebuildHeaven
                && (DownedBossSystem.EndgameRoute)typeof(EndgameRouteUIState).GetField("selected",flags).GetValue(ui) == DownedBossSystem.EndgameRoute.SeverHeaven,
                "native locked world cannot change route preview or submit old confirmation");
        } finally { world.LoadWorldData(saved); Main.player[0] = previous; Main.myPlayer = local; Main.gameMenu = menu; Main.blockInput = blocked; Main.drawingPlayerChat = chat; Main.playerInventory = inventory; Main.mouseItem = mouse; }
        check(ReferenceEquals(Main.player[0],previous) && ReferenceEquals(Main.mouseItem,mouse),"native route navigation fixture restores player and cursor context");
    }
}
