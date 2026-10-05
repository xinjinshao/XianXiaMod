using Terraria;
using Terraria.ModLoader;

namespace XianXia.Common.Systems;

public class ArtifactKeybindSystem : ModSystem
{
    public static ModKeybind ArtifactSkillKey { get; private set; }
    public static ModKeybind WardSkillKey { get; private set; }
    public override void Load()
    {
        if (Main.dedServ) return;
        ArtifactSkillKey = KeybindLoader.RegisterKeybind(Mod, "ArtifactSkill", "Q");
        WardSkillKey = KeybindLoader.RegisterKeybind(Mod, "WardSkill", "V");
    }
    public override void Unload() { ArtifactSkillKey = WardSkillKey = null; }
}
