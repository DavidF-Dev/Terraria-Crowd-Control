using System;
using CrowdControlMod.Config;
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
    private static bool? _isMagicMalaraith;
    private static bool? _isKaylaJayde;
    private static bool? _isOfficialConduit;
    private static bool? _isMoonlightFaye;
    private static bool? _isMakenbacon;

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

            // Try to get the Steam ID from Steamworks; otherwise we assume Steam isn't opened
            try
            {
                var steamId = SteamAPI.IsSteamRunning() && SteamUser.BLoggedOn() ? SteamUser.GetSteamID() : default;
                _steamId = steamId.IsValid() ? steamId.m_SteamID : 0UL;
            }
            catch (Exception e)
            {
                CrowdControlMod.GetInstance().Logger.Warn($"Failed to retrieve Steam ID due to an exception: {e}");
                _steamId = 0UL;
            }

            CrowdControlMod.GetInstance().Logger.Debug($"Loaded using Steam ID: {_steamId.Value}");
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

    /// <summary>
    ///     https://www.twitch.tv/magicmalaraith
    /// </summary>
    public static bool IsMagicMalaraith => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isMagicMalaraith ?? (_isMagicMalaraith = SteamId == 76561198136302414UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/kaylajayde
    /// </summary>
    public static bool IsKaylaJayde => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isKaylaJayde ?? (_isKaylaJayde = SteamId == 76561198105299229UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/official_conduit
    /// </summary>
    public static bool IsOfficialConduit => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isOfficialConduit ?? (_isOfficialConduit = SteamId == 76561198043497391UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/moonlit_faye
    /// </summary>
    public static bool IsMoonlitFaye => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isMoonlightFaye ?? (_isMoonlightFaye = SteamId == 76561198113754529UL).Value);

    /// <summary>
    ///     https://www.twitch.tv/makenbacon07
    /// </summary>
    public static bool IsMakenBacon => CrowdControlConfig.GetInstance().ForceEasterEggs || (_isMakenbacon ?? (_isMakenbacon = SteamId == 76561198080674794UL).Value);

    #endregion
}