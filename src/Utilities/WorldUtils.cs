using System;
using CrowdControlMod.ID;
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
        return Main.IsItStorming ? Weather.Storm : Main.IsItRaining ? Weather.Rain : Main.IsItAHappyWindyDay ? Weather.Windy : Weather.Clear;
    }

    /// <summary>
    ///     Set the weather in the world (single-player or server-side).
    /// </summary>
    [PublicAPI]
    public static void SetWeather(Weather weather)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            TerrariaUtils.WriteDebug("Cannot set the weather when running as a client");
            return;
        }

        // Notes
        // - Main.UpdateWindyDayState()
        // - Main.StartRain()

        // Values from Terraria source
        // Slightly altered
        const float minWind = 0.34f - 0.01f;
        const float maxWind = 0.4f;
        const float minRain = 0.4f;
        const float maxRain = 0.5f;

        // Reset the wind counter so that the wind doesn't change for a while
        Main.ResetWindCounter(true);

        // Set cloud cover
        Main.cloudAlpha = weather is Weather.Clear or Weather.Windy
            ? 0f
            : weather == Weather.Storm
                ? maxRain * 2f * Main.rand.NextFloat(1f, 1.25f)
                : minRain;

        // Determine wind direction
        var windDir = Math.Sign(Main.windSpeedTarget);
        if (windDir == 0)
        {
            windDir = Main.rand.Next(100) < 50 ? -1 : 1;
        }

        // Set wind speed target
        Main.windSpeedTarget = windDir * weather switch
        {
            Weather.Clear or Weather.Rain => minWind * 0.5f,
            Weather.Storm => maxWind * 2 * Main.rand.NextFloat(1f, 1.25f),
            _ => maxWind
        };
        // Main.windSpeedCurrent = Main.windSpeedTarget;

        if (weather is Weather.Rain or Weather.Storm)
        {
            // Start the vanilla raining process
            Main.StartRain();
        }
        else
        {
            // Ensure it is not raining
            Main.StopRain();
        }

        if (Main.netMode != NetmodeID.Server)
        {
            return;
        }

        // Sync rain using vanilla syncing
        Main.SyncRain();

        // Sync cloud cover and wind speed using modded packet
        var packet = CrowdControlMod.GetInstance().GetPacket();
        packet.Write((byte)PacketID.SyncWeather);
        packet.Write(Main.cloudAlpha);
        packet.Write(Main.windSpeedTarget);
        packet.Write(Main.windCounter);
        packet.Write(Main.extremeWindCounter);
        packet.Send();
    }

    #endregion
}