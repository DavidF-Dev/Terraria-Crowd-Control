using System;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Utilities;

public static class WorldUtils
{
    #region Enums

    public enum Weather
    {
        Clear,
        Rain,
        Storm,
        Windy
    }

    #endregion

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

    /// <summary>
    ///     Get the weather in the world.
    /// </summary>
    [PublicAPI] [Pure]
    public static Weather GetWeather()
    {
        return Main.IsItRaining ? Weather.Rain : Main.IsItStorming ? Weather.Storm : Main.IsItAHappyWindyDay ? Weather.Windy : Weather.Clear;
    }

    /// <summary>
    ///     Set the weather in the world.
    /// </summary>
    [PublicAPI]
    public static void SetWeather(Weather weather)
    {
        // TODO: Needs fixing - weather will change back after a few seconds

        // Based off Main.UpdateWindyDayState()

        // Values from Terraria source
        const float minWind = 0.34f;
        const float maxWind = 0.4f;
        const float minRain = 0.4f;
        const float maxRain = 0.5f;

        // Set cloud cover
        Main.cloudAlpha = weather is Weather.Clear or Weather.Windy
            ? 0f
            : weather == Weather.Storm
                ? maxRain + 0.2f
                : minRain - 0.01f;

        // Determine wind direction
        var windDir = Math.Sign(Main.windSpeedTarget);
        if (windDir == 0)
        {
            windDir = Main.rand.Next(100) < 50 ? -1 : 1;
        }

        // Set wind speed
        Main.windSpeedTarget = windDir * (weather is Weather.Clear or Weather.Rain ? minWind - 0.1f : maxWind + 0.2f);
        Main.windSpeedCurrent = Main.windSpeedTarget;

        if (Main.netMode == NetmodeID.Server)
        {
            // Update clients on the change
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    #endregion
}