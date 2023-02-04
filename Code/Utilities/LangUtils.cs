using System;
using Terraria.Localization;

namespace CrowdControlMod.Utilities;

/// <summary>
///     Localisation utils for this mod.<br />
///     All localisation requests should happen through this class (except for config).
///     TODO: tModLoader automatically formats localisation file; I want to format it manually.
/// </summary>
public static class LangUtils
{
    #region Static Fields and Constants

    /// <summary>
    ///     Path in localisation file to this mod's content.
    /// </summary>
    public const string ModPath = "Mods.CrowdControlMod.";

    #endregion

    #region Static Methods

    /// <summary>
    ///     Get the localised effect start text for the provided effect id.
    /// </summary>
    public static string GetEffectStartText(string effectId, string viewerString, string playerString, string? durationString, params object[] arguments)
    {
        var text = GetText($"Effect.Start.{effectId}", arguments);
        text = text.Replace("[Viewer]", viewerString);
        text = text.Replace("[Player]", playerString);
        if (durationString != null)
        {
            text = text.Replace("[Duration]", durationString);
        }

        return text;
    }

    /// <summary>
    ///     Get the localised effect stop text for the provided effect id.
    /// </summary>
    public static string GetEffectStopText(string effectId, params object[] arguments)
    {
        return GetText($"Effect.Stop.{effectId}", arguments);
    }

    /// <summary>
    ///     Get a localised misc text for the provided effect id.<br />
    ///     E.g. "Dungeon Guardian was a phony."
    /// </summary>
    public static string GetEffectMiscText(string effectId, string relativeKey, params object[] arguments)
    {
        return GetText($"Effect.Misc.{effectId}.{relativeKey}", arguments);
    }

    private static string GetText(string relativeKey, params object[] arguments)
    {
        var key = $"{ModPath}{relativeKey}";
        string text;
        try
        {
            text = Language.GetTextValue(key, arguments);
        }
        catch (Exception)
        {
            TerrariaUtils.WriteDebug($"An exception occured when localising key '{key}' for language '{Language.ActiveCulture.Name}'");
            return key;
        }

        if (text.Equals(key))
        {
            TerrariaUtils.WriteDebug($"Unable to locate localisation key '{key}' for language '{Language.ActiveCulture.Name}'");
        }

        return text;
    }

    #endregion

    #region Properties

    public static string ConnectingText => GetText("Chat.Connecting");

    public static string ConnectedText => GetText("Chat.Connected");

    public static string DisconnectedText => GetText("Chat.Disconnected");

    public static string FirstTimeStartText => GetText("Chat.FirstTimeStart");

    public static string FirstTimeStopText => GetText("Chat.FirstTimeStop");

    #endregion
}