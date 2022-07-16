using System;
using Terraria.ModLoader;

namespace CrowdControlMod.Utilities;

public static class ModUtils
{
    #region Static Methods

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
    
    #endregion
}