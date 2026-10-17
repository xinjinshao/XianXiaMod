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
    public override void Load()
    {
        if (Main.dedServ) return;
        ArtifactSkillKey = KeybindLoader.RegisterKeybind(Mod, "ArtifactSkill", "Q");
        WardSkillKey = KeybindLoader.RegisterKeybind(Mod, "WardSkill", "V");
        PanelNextKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelNext","None");
        PanelPreviousKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelPrevious","None");
        PanelCastKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelCast","None");
        PanelCancelKey = KeybindLoader.RegisterKeybind(Mod,"SkillPanelCancel","None");
    }
    public override void Unload() { ArtifactSkillKey = WardSkillKey = PanelNextKey = PanelPreviousKey = PanelCastKey = PanelCancelKey = null; }
}
