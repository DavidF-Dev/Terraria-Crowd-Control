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
    public static bool IsMrKaiga => _isMrKaiga ?? (_isMrKaiga = SteamId == 76561199164122300UL).Value;

    /// <summary>
    ///     https://www.twitch.tv/allfunngamez
    /// </summary>
    public static bool IsAllFunNGamez => _isAllFunNGamez ?? (_isAllFunNGamez = SteamId == 76561197963233461UL).Value;

    /// <summary>
    ///     https://www.twitch.tv/teebutv
    /// </summary>
    public static bool IsTeebu => _isTeebu ?? (_isTeebu = SteamId == 76561198066573407UL).Value;

    #endregion
}