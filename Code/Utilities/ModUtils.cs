using System;
using System.Collections.Generic;
using CrowdControlMod.Config;
using Terraria.ModLoader;

namespace CrowdControlMod.Utilities;

public static class ModUtils
{
    #region Static Methods

    /// <summary>
    ///     Try to get the enabled mod by name.
    /// </summary>
#nullable disable
    public static bool TryGetMod(string modId, out Mod mod)
    {
        if (IsSupported(modId))
        {
            return ModLoader.TryGetMod(modId, out mod);
        }

        // Mod not supported
        mod = null;
        return false;
    }
#nullable restore

    /// <summary>
    ///     Perform actions on multiple mod types, only if the type exists.
    /// </summary>
    public static void IterateTypes<T>(Mod mod, IEnumerable<string> names, Action<T> action, Predicate<T>? predicate = null) where T : IModType
    {
        // Attempt to find the types in the mod and perform the provided action on them
        foreach (var name in names)
        {
            if (!mod.TryFind(name, out T value) || !(predicate?.Invoke(value) ?? true))
            {
                continue;
            }

            action(value);
        }
    }

    /// <summary>
    ///     List installed and enabled mods.
    /// </summary>
    public static void ListMods()
    {
        foreach (var mod in ModLoader.Mods)
        {
            TerrariaUtils.WriteDebug($"{mod.DisplayName} ({mod.Name}): {mod.Version}");
        }
    }

    /// <summary>
    ///     List buffs in provided mod.
    /// </summary>
    public static void ListBuffs(string modId, Predicate<ModBuff>? predicate = null)
    {
        if (!ModLoader.TryGetMod(modId, out var mod))
        {
            TerrariaUtils.WriteDebug($"Could not find mod: '{modId}'");
            return;
        }

        for (var i = 0; i < BuffLoader.BuffCount; i++)
        {
            var buff = BuffLoader.GetBuff(i);
            if (buff != null && buff.Mod == mod && (predicate?.Invoke(buff) ?? true))
            {
                TerrariaUtils.WriteDebug($"[{buff.Type}] {buff.DisplayName.Value} ({buff.Name}): {buff.Description.Value}");
            }
        }
    }

    /// <summary>
    ///     List projectiles in provided mod.
    /// </summary>
    public static void ListProjectiles(string modId, Predicate<ModProjectile>? predicate = null)
    {
        if (!ModLoader.TryGetMod(modId, out var mod))
        {
            TerrariaUtils.WriteDebug($"Could not find mod: '{modId}'");
            return;
        }

        for (var i = 0; i < ProjectileLoader.ProjectileCount; i++)
        {
            var proj = ProjectileLoader.GetProjectile(i);
            if (proj != null && proj.Mod == mod && (predicate?.Invoke(proj) ?? true))
            {
                TerrariaUtils.WriteDebug($"[{proj.Type}] {proj.DisplayName.Value} ({proj.Name})");
            }
        }
    }

    /// <summary>
    ///     List items in provided mod.
    /// </summary>
    public static void ListItems(string modId, Predicate<ModItem>? predicate = null)
    {
        if (!ModLoader.TryGetMod(modId, out var mod))
        {
            TerrariaUtils.WriteDebug($"Could not find mod: '{modId}'");
            return;
        }

        for (var i = 0; i < ItemLoader.ItemCount; i++)
        {
            var item = ItemLoader.GetItem(i);
            if (item != null && item.Mod == mod && (predicate?.Invoke(item) ?? true))
            {
                TerrariaUtils.WriteDebug($"[{item.Type}] {item.DisplayName.Value} ({item.Name})");
            }
        }
    }

    /// <summary>
    ///     List NPCs in provided mod.
    /// </summary>
    public static void ListNPCs(string modId, Predicate<ModNPC>? predicate = null)
    {
        if (!ModLoader.TryGetMod(modId, out var mod))
        {
            TerrariaUtils.WriteDebug($"Could not find mod: '{modId}'");
            return;
        }

        for (var i = 0; i < NPCLoader.NPCCount; i++)
        {
            var npc = NPCLoader.GetNPC(i);
            if (npc != null && npc.Mod == mod && (predicate?.Invoke(npc) ?? true))
            {
                TerrariaUtils.WriteDebug($"[{npc.Type}] {npc.DisplayName.Value} ({npc.Name})");
            }
        }
    }

    private static bool IsSupported(string modId)
    {
        return modId switch
        {
            Calamity.Name => CrowdControlConfig.GetInstance().AllowCalamity,
            _ => true
        };
    }

    #endregion

    #region Nested Types

    public static class Calamity
    {
        #region Static Fields and Constants

        public const string Name = "CalamityMod";

        public const string DesertScourgeNPC = "DesertScourgeHead";
        public const string CrabulonNPC = "Crabulon";
        public const string TheHiveMindNPC = "HiveMind";
        public const string ThePerforatorsNPC = "PerforatorHive";
        public const string TheSlimeGodNPC = "SlimeGodCore";
        public const string CryogenNPC = "Cryogen";
        public const string AquaticScourgeNPC = "AquaticScourgeHead";
        public const string BrimstoneElementalNPC = "BrimstoneElemental";
        public const string CalamitasNPC = "CalamitasClone";
        public const string AstrumAureusNPC = "AstrumAureus";
        public const string ThePlaguebringerGoliathNPC = "PlaguebringerGoliath";
        public const string RavagerNPC = "RavagerBody";
        public const string AstrumDeusNPC = "AstrumDeusHead";
        public const string GiantClamNPC = "GiantClam";
        public const string EarthElementalNPC = "Horse";
        public const string CloudElementalNPC = "ThiccWaifu";
        public const string CragmawMireNPC = "CragmawMire";
        public const string GreatSandSharkNPC = "GreatSandShark";
        public const string NuclearTerrorNPC = "NuclearTerror";

        #endregion
    }

    #endregion
}