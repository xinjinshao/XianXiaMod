using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using XianXia.Common.Systems;

namespace XianXia.Common.Items;

public class RefinedArtifact : GlobalItem
{
    public override bool InstancePerEntity => true;
    public byte Level { get; private set; }
    public bool Awakened { get; private set; }
    public DownedBossSystem.EndgameRoute DaoRoute { get; private set; }
    public static bool IsSample(Item item) => item != null && InscribedEquipment.IsEligible(item)
        && item.ModItem.Name is "CloudpiercerFlyingSword" or "GreenwoodArrayPlate";
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => IsSample(entity);
    public static byte GetLevel(Item item) => IsSample(item) ? item.GetGlobalItem<RefinedArtifact>().Level : (byte)0;
    public static bool IsAwakened(Item item) => IsSample(item) && item.GetGlobalItem<RefinedArtifact>().Awakened;
    public void SetLevel(int level) { Level = RefinementRules.Normalize(level); if (Level != 3) { Awakened = false; DaoRoute = DownedBossSystem.EndgameRoute.None; } }
    public static DownedBossSystem.EndgameRoute GetDaoRoute(Item item) => IsSample(item) ? item.GetGlobalItem<RefinedArtifact>().DaoRoute : DownedBossSystem.EndgameRoute.None;
    public static DownedBossSystem.EndgameRoute ActiveDaoRoute(Item item) => GetDaoRoute(item) == DownedBossSystem.ChosenRoute
        && DownedBossSystem.DownedBosses.Contains("old_heaven_dao_core") ? GetDaoRoute(item) : DownedBossSystem.EndgameRoute.None;
    public bool TryTransform(DownedBossSystem.EndgameRoute route)
    {
        if (Level != 3 || !Awakened || DaoRoute != DownedBossSystem.EndgameRoute.None || DaoArtifactRules.Normalize((int)route) == DownedBossSystem.EndgameRoute.None) return false;
        DaoRoute = route; return true;
    }
    public bool TryAwaken() { if (Level != 3 || Awakened) return false; Awakened = true; return true; }
    public override void SaveData(Item item, TagCompound tag)
    {
        if (Level == 0) return;
        tag["refinementVersion"] = 3; tag["refinement"] = (int)Level;
        tag["awakened"] = Awakened;
        tag["daoRoute"] = (int)DaoRoute;
    }
    public override void LoadData(Item item, TagCompound tag) { SetLevel(tag.GetInt("refinement")); Awakened = Level == 3 && tag.GetBool("awakened"); DaoRoute = Awakened ? DaoArtifactRules.Normalize(tag.GetInt("daoRoute")) : DownedBossSystem.EndgameRoute.None; }
    public override void NetSend(Item item, BinaryWriter writer) { writer.Write(Level); writer.Write(Awakened); writer.Write((byte)DaoRoute); }
    public override void NetReceive(Item item, BinaryReader reader)
    {
        byte level = reader.ReadByte(); bool awakened = reader.ReadBoolean(); byte route = reader.ReadByte();
        SetLevel(level); Awakened = Level == 3 && awakened;
        DaoRoute = Awakened ? DaoArtifactRules.Normalize(route) : DownedBossSystem.EndgameRoute.None;
    }
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        damage += RefinementRules.DamageBonus(Level);
        var route = ActiveDaoRoute(item);
        if (route != DownedBossSystem.EndgameRoute.None) {
            damage.Base += item.damage * (DaoArtifactRules.DamageScale(item.ModItem.Name) - 1f);
            damage += DaoArtifactRules.ExtraDamage(route);
        }
    }
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Refinement", Language.GetTextValue("Mods.XianXia.Refinement.Tooltip", Level, RefinementRules.MaximumLevel, Level * 4)));
        tooltips.Add(new TooltipLine(Mod, "ArtifactAwakening", Language.GetTextValue(Awakened ? "Mods.XianXia.Refinement.Awakened" : "Mods.XianXia.Refinement.Unawakened")));
        if (DaoRoute != DownedBossSystem.EndgameRoute.None)
            tooltips.Add(new TooltipLine(Mod, "DaoTransformation", Language.GetTextValue("Mods.XianXia.DaoArtifacts.Tooltip", Language.GetTextValue(EndgameRouteTransactions.NameKey(DaoRoute)))));
    }
}
