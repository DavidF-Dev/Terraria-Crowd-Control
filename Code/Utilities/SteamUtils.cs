﻿using CrowdControlMod.Config;
using Steamworks;

namespace CrowdControlMod.Utilities;

/// <summary>
///     Helper methods for checking Steam stuff.
/// </summary>
public static class SteamUtils
{
    #region Static Fields and Constants

    private static ulong? _steamId;
    private static bool? _isMrKaiga;
    private static bool? _isAllFunNGamez;
    private static bool? _isTeebu;
    private static bool? _isLunadabintu;
    private static bool? _isThatGrayson;
    private static bool? _isTheJayrBayr;

    #endregion

    #region Properties

    /// <summary>
    ///     Logged in unique steam identifier (0 if not logged in).
    /// </summary>
    public static ulong SteamId
    {
        get
        {
            if (_steamId.HasValue)
            {
                return _steamId.Value;
            }

            var steamId = SteamAPI.IsSteamRunning() && SteamUser.BLoggedOn() ? SteamUser.GetSteamID() : default;
            _steamId = steamId.IsValid() ? steamId.m_SteamID : 0UL;
            return _steamId.Value;
        }
    }

    /// <summary>
    ///     https://www.twitch.tv/mrkaiga
    /// </summary>
    public static bool IsMrKaiga => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isMrKaiga ?? (_isMrKaiga = SteamId == 76561199164122300UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/allfunngamez
    /// </summary>
    public static bool IsAllFunNGamez => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isAllFunNGamez ?? (_isAllFunNGamez = SteamId == 76561197963233461UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/teebutv
    /// </summary>
    public static bool IsTeebu => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isTeebu ?? (_isTeebu = SteamId == 76561198066573407UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/lunadabintu
    /// </summary>
    public static bool IsLunadabintu => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isLunadabintu ?? (_isLunadabintu = SteamId == 76561198254317966UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/that_grayson
    /// </summary>
    public static bool IsThatGrayson => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isThatGrayson ?? (_isThatGrayson = SteamId == 76561198042877752UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/thejayrbayr
    /// </summary>
    public static bool IsTheJayrBayr => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isTheJayrBayr ?? (_isTheJayrBayr = SteamId == 76561198884016816UL).Value);
    
    #endregion
}