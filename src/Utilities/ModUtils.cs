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
    public static void ListBuffs(string modId)
    {
        if (!ModLoader.TryGetMod(modId, out var mod))
        {
            TerrariaUtils.WriteDebug($"Could not find mod: '{modId}'");
            return;
        }

        for (var i = 0; i < BuffLoader.BuffCount; i++)
        {
            var buff = BuffLoader.GetBuff(i);
            if (buff != null && buff.Mod == mod)
            {
                TerrariaUtils.WriteDebug($"[{buff.Type}] {buff.DisplayName.GetDefault()} ({buff.Name}): {buff.Description.GetDefault()}");
            }
        }
    }

    #endregion
}