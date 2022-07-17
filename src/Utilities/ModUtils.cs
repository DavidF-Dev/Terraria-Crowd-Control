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
    public static bool IterateTypes<T>(Mod mod, IEnumerable<string> names, Action<T> action, Predicate<T>? predicate = null) where T : IModType
    {
        // Attempt to find the types in the mod and perform the provided action on them
        var foundAny = false;
        foreach (var name in names)
        {
            if (!mod.TryFind(name, out T value) || !(predicate?.Invoke(value) ?? true))
            {
                continue;
            }

            action(value);
            foundAny = true;
        }

        return foundAny;
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
                TerrariaUtils.WriteDebug($"[{buff.Type}] {buff.DisplayName.GetDefault()} ({buff.Name}): {buff.Description.GetDefault()}");
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
                TerrariaUtils.WriteDebug($"[{proj.Type}] {proj.DisplayName.GetDefault()} ({proj.Name})");
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
                TerrariaUtils.WriteDebug($"[{item.Type}] {item.DisplayName.GetDefault()} ({item.Name})");
            }
        }
    }

    /// <summary>
    ///     List NPCs in provided mod.
    /// </summary>
    // ReSharper disable once InconsistentNaming
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
                TerrariaUtils.WriteDebug($"[{npc.Type}] {npc.DisplayName.GetDefault()} ({npc.Name})");
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

        public const string NpcDesertScourge = "DesertScourgeHead";
        public const string NpcCrabulon = "Crabulon";
        public const string NpcTheHiveMind = "HiveMind";
        public const string NpcThePerforators = "PerforatorHive";
        public const string NpcTheSlimeGod = "SlimeGodCore";
        public const string NpcCryogen = "Cryogen";
        public const string NpcAquaticScourge = "AquaticScourgeHead";
        public const string NpcBrimstoneElemental = "BrimstoneElemental";
        public const string NpcCalamitas = "CalamitasClone";
        public const string NpcAstrumAureus = "AstrumAureus";
        public const string NpcThePlaguebringerGoliath = "PlaguebringerGoliath";
        public const string NpcRavager = "RavagerBody";
        public const string NpcAstrumDeus = "AstrumDeusHead";
        public const string NpcGiantClam = "GiantClam";
        public const string NpcEarthElemental = "Horse";
        public const string NpcCloudElemental = "ThiccWaifu";
        public const string NpcCragmawMire = "CragmawMire";
        public const string NpcGreatSandShark = "GreatSandShark";
        public const string NpcNuclearTerror = "NuclearTerror";

        #endregion
    }

    #endregion
}