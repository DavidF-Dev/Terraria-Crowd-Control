using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Terraria;

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
    [Pure]
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
    [Pure]
    public static Weather GetWeather()
    {
        return Main.IsItStorming ? Weather.Storm : Main.IsItRaining ? Weather.Rain : Main.IsItAHappyWindyDay ? Weather.Windy : Weather.Clear;
    }

    /// <summary>
    ///     Set the weather in the world (single-player or server-side).
    /// </summary>
    public static void SetWeather(Weather weather)
    {
        if (NetUtils.IsClient)
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
                ? maxRain * 2f
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

        if (!NetUtils.IsServer)
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

    /// <summary>
    ///     Get the tiles in a radial area around the given center position.
    /// </summary>
    [Pure]
    public static IEnumerable<(int x, int y)> GetTilesAround(int centerX, int centerY, int radius)
    {
        List<(int, int)> result = new(radius * radius);
        var radiusSquared = radius * radius;
        for (var x = centerX - radius; x < centerX + radius; x++)
        {
            for (var y = centerY - radius; y < centerY + radius; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY || Vector2.DistanceSquared(new Vector2(centerX, centerY), new Vector2(x, y)) > radiusSquared)
                {
                    // Ignore if out of bounds or not within the radius
                    continue;
                }

                result.Add((x, y));
            }
        }

        return result;
    }

    /// <summary>
    ///     Get the tiles in a rectangular area around the given center position.
    /// </summary>
    [Pure]
    public static IEnumerable<(int x, int y)> GetTilesAround(int centerX, int centerY, int halfWidth, int halfHeight)
    {
        List<(int, int)> result = new(halfWidth * halfHeight);
        for (var x = centerX - halfWidth; x < centerX + halfWidth; x++)
        {
            for (var y = centerY - halfHeight; y < centerY + halfHeight; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                {
                    continue;
                }

                result.Add((x, y));
            }
        }

        return result;
    }

    /// <summary>
    ///     Check if the provided tile is solid.
    /// </summary>
    [Pure]
    public static bool IsTileSolid(int x, int y)
    {
        return x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY &&
               Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType] && !Main.tile[x, y].IsActuated;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     'For the Worthy' mode is enabled in the world.
    /// </summary>
    public static bool IsForTheWorthy
    {
        get => Main.getGoodWorld;
        set => Main.getGoodWorld = value;
    }

    /// <summary>
    ///     'Don't Starve' mode is enabled in the world.
    /// </summary>
    public static bool IsDontStarve
    {
        get => Main.dontStarveWorld;
        set => Main.dontStarveWorld = value;
    }

    /// <summary>
    ///     'Drunk mode' is enabled in the world.
    /// </summary>
    public static bool IsDrunkWorld
    {
        get => Main.drunkWorld;
        set => Main.drunkWorld = value;
    }

    #endregion
}