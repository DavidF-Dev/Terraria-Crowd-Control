using JetBrains.Annotations;
using Terraria;

namespace CrowdControlMod.Utilities;

public static class WorldUtils
{
    #region Static Methods

    /// <summary>
    ///     Check if there is an active boss or invasion in the world.
    /// </summary>
    [PublicAPI] [Pure]
    public static bool ActiveBossEventOrInvasion(bool includeBloodMoon = true, bool includeEclipse = true)
    {
        // Check events
        if (Main.pumpkinMoon || Main.snowMoon || Main.invasionType > 0 ||
            (includeBloodMoon && Main.bloodMoon) || (includeEclipse && Main.eclipse))
        {
            return true;
        }

        // Check for any active boss npc
        for (var i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && Main.npc[i].boss)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}