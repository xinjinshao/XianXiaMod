using System;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.UI;
namespace XianXia.Common.Systems;
public partial class SummonAuditSystem
{
    private static void AuditPanelFocus(Action<bool,string> check)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var forge = ModContent.GetInstance<InscriptionUISystem>(); var route = ModContent.GetInstance<EndgameRouteUISystem>();
        var forgeField = typeof(InscriptionUISystem).GetField("userInterface",flags); var routeField = typeof(EndgameRouteUISystem).GetField("userInterface",flags);
        object oldForge = forgeField.GetValue(forge), oldRoute = routeField.GetValue(route);
        // Set only the unactivated state backing field: no fonts, UI events or rendering are executed.
        var stateField = typeof(UserInterface).GetFields(flags).Single(field => typeof(UIState).IsAssignableFrom(field.FieldType));
        var interfaceCopy = new UserInterface();
        try {
            forgeField.SetValue(forge,null); routeField.SetValue(route,null);
            check(!CultivationPanelFocus.MaterialPanelOpen && !forge.IsOpen && !route.IsOpen,"native null material interfaces do not capture combat focus");
            forgeField.SetValue(forge,interfaceCopy);
            check(!CultivationPanelFocus.MaterialPanelOpen,"native closed material interface does not capture combat focus");
            stateField.SetValue(interfaceCopy,new UIState());
            check(CultivationPanelFocus.MaterialPanelOpen && forge.IsOpen,"native actual inscription current state captures combat focus");
            forgeField.SetValue(forge,null); routeField.SetValue(route,interfaceCopy);
            check(CultivationPanelFocus.MaterialPanelOpen && route.IsOpen,"native actual route current state captures combat focus");
            forgeField.SetValue(forge,interfaceCopy);
            check(CultivationPanelFocus.MaterialPanelOpen,"native either material panel retains focus when both are present");
            stateField.SetValue(interfaceCopy,null);
            check(!CultivationPanelFocus.MaterialPanelOpen && !forge.IsOpen && !route.IsOpen,"native clearing current states releases material focus");
        } finally { forgeField.SetValue(forge,oldForge); routeField.SetValue(route,oldRoute); }
        check(ReferenceEquals(forgeField.GetValue(forge),oldForge) && ReferenceEquals(routeField.GetValue(route),oldRoute),"native focus fixture restores original user interfaces");
    }
}
