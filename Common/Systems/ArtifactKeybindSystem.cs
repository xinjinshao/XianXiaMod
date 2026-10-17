using Terraria;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public class ArtifactKeybindSystem : ModSystem
{
    public static ModKeybind ArtifactSkillKey { get; private set; }
    public static ModKeybind WardSkillKey { get; private set; }
    public static ModKeybind PanelNextKey { get; private set; }
    public static ModKeybind PanelPreviousKey { get; private set; }
    public static ModKeybind PanelCastKey { get; private set; }
    public static ModKeybind PanelCancelKey { get; private set; }
    public static ModKeybind ForgeNextKey { get; private set; }
    public static ModKeybind ForgePreviousKey { get; private set; }
    public static ModKeybind ForgeConfirmKey { get; private set; }
    public static ModKeybind ForgeCancelKey { get; private set; }
    public static ModKeybind ForgeScrollUpKey { get; private set; }
    public static ModKeybind ForgeScrollDownKey { get; private set; }
    public static ModKeybind RouteNextKey { get; private set; }
    public static ModKeybind RoutePreviousKey { get; private set; }
    public static ModKeybind RouteConfirmKey { get; private set; }
    public static ModKeybind RouteCancelKey { get; private set; }
    public static ModKeybind RouteScrollUpKey { get; private set; }
    public static ModKeybind RouteScrollDownKey { get; private set; }
    public override void Load()
    {
        if (Main.dedServ) return;
        ArtifactSkillKey = KeybindLoader.RegisterKeybind(Mod, "ArtifactSkill", "Q");
        WardSkillKey = KeybindLoader.RegisterKeybind(Mod, "WardSkill", "V");
        RouteNextKey = KeybindLoader.RegisterKeybind(Mod,"RouteNext","None");
        RoutePreviousKey = KeybindLoader.RegisterKeybind(Mod,"RoutePrevious","None");
        RouteConfirmKey = KeybindLoader.RegisterKeybind(Mod,"RouteConfirm","None");
        RouteCancelKey = KeybindLoader.RegisterKeybind(Mod,"RouteCancel","None");
        RouteScrollUpKey = KeybindLoader.RegisterKeybind(Mod,"RouteScrollUp","None");
        RouteScrollDownKey = KeybindLoader.RegisterKeybind(Mod,"RouteScrollDown","None");
        ForgeNextKey = KeybindLoader.RegisterKeybind(Mod,"ForgePanelNext","None");
        ForgePreviousKey = KeybindLoader.RegisterKeybind(Mod,"ForgePanelPrevious","None");
        ForgeConfirmKey = KeybindLoader.RegisterKeybind(Mod,"ForgePanelConfirm","None");
        ForgeCancelKey = KeybindLoader.RegisterKeybind(Mod,"ForgePanelCancel","None");
        ForgeScrollUpKey = KeybindLoader.RegisterKeybind(Mod,"ForgeScrollUp","None");
        ForgeScrollDownKey = KeybindLoader.RegisterKeybind(Mod,"ForgeScrollDown","None");
        PanelNextKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelNext","None");
        PanelPreviousKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelPrevious","None");
        PanelCastKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelCast","None");
        PanelCancelKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelCancel","None");
    }
    public override void Unload() { ArtifactSkillKey = WardSkillKey = PanelNextKey = PanelPreviousKey = PanelCastKey = PanelCancelKey = ForgeNextKey = ForgePreviousKey = ForgeConfirmKey = ForgeCancelKey = ForgeScrollUpKey = ForgeScrollDownKey = RouteNextKey = RoutePreviousKey = RouteConfirmKey = RouteCancelKey = RouteScrollUpKey = RouteScrollDownKey = null; }
}
