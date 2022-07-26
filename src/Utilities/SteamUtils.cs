using System.Diagnostics.Contracts;
using Steamworks;

namespace CrowdControlMod.Utilities;

/// <summary>
///     Helper methods for checking Steam stuff.
/// </summary>
public static class SteamUtils
{
    #region Static Methods

    /// <summary>
    ///     Attempt to get the steam id of the current user playing the mod.
    /// </summary>
    [Pure]
    public static bool TryGetSteamID(out ulong uniqueId)
    {
        if (!SteamAPI.IsSteamRunning() || !SteamUser.BLoggedOn())
        {
            uniqueId = 0L;
            return false;
        }

        var steamId = SteamUser.GetSteamID();
        if (!steamId.IsValid())
        {
            uniqueId = 0L;
            return false;
        }

        uniqueId = steamId.m_SteamID;
        return true;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     https://www.twitch.tv/mrkaiga
    /// </summary>
    public static bool IsMrKaiga => TryGetSteamID(out var uniqueId) && uniqueId == 76561199164122300L;

    /// <summary>
    ///     https://www.twitch.tv/allfunngamez
    /// </summary>
    public static bool IsAllFunNGamez => TryGetSteamID(out var uniqueId) && uniqueId == 76561197963233461L;

    /// <summary>
    ///     https://www.twitch.tv/teebutv
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static bool IsTeebuTV => TryGetSteamID(out var uniqueId) && uniqueId == 76561198066573407L;

    #endregion
}