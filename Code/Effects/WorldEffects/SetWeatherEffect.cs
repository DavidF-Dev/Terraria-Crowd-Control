using System;
using System.Diagnostics.Contracts;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Set the weather in the world (either sunny, rainy, windy or stormy).
/// </summary>
public sealed class SetWeatherEffect : CrowdControlEffect
{
    #region Static Methods

    [Pure]
    private static string GetId(WorldUtils.Weather weather)
    {
        return weather switch
        {
            WorldUtils.Weather.Clear => EffectID.SetWeatherClear,
            WorldUtils.Weather.Rain => EffectID.SetWeatherRain,
            WorldUtils.Weather.Storm => EffectID.SetWeatherStorm,
            WorldUtils.Weather.Windy => EffectID.SetWeatherWindy,
            _ => throw new ArgumentOutOfRangeException(nameof(weather), weather, null)
        };
    }

    private static bool CanBeAWindyDay()
    {
        // Numbers extracted from the Terraria source code -> Main.UpdateWindyDayState()
        // Max time is reduced slightly so a windy day can at least last a little while
        const double reduce = 1500.0;
        return !(Main.time < 10800.0 - reduce || Main.time > 43200.0 || !Main.dayTime);
    }

    #endregion

    #region Fields

    private readonly WorldUtils.Weather _weather;

    #endregion

    #region Constructors

    public SetWeatherEffect(WorldUtils.Weather weather) : base(GetId(weather), null, EffectSeverity.Neutral)
    {
        _weather = weather;
        StartEmote = _weather switch
        {
            WorldUtils.Weather.Clear => EmoteID.WeatherSunny,
            WorldUtils.Weather.Rain => EmoteID.WeatherRain,
            WorldUtils.Weather.Storm => EmoteID.WeatherLightning,
            WorldUtils.Weather.Windy => EmoteID.MiscTree,
            _ => throw new ArgumentOutOfRangeException(nameof(weather), weather, null)
        };
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;
    
    protected override int StartEmote { get; }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (WorldUtils.GetWeather() == _weather)
        {
            return CrowdControlResponseStatus.Failure;
        }

        if (_weather == WorldUtils.Weather.Windy && !CanBeAWindyDay())
        {
            TerrariaUtils.WriteDebug("Failed to set the weather to windy because it isn't day time");
            return CrowdControlResponseStatus.Retry;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Change the weather in a single-player world
            WorldUtils.SetWeather(_weather);
        }
        else
        {
            // Notify server to change the weather
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var item = _weather switch
        {
            WorldUtils.Weather.Clear => ItemID.SunMask,
            WorldUtils.Weather.Rain => ItemID.RainCoat,
            WorldUtils.Weather.Storm => ItemID.NimbusRod,
            WorldUtils.Weather.Windy => Main.rand.Next(new[] {ItemID.KiteBlue, ItemID.KiteRed, ItemID.KiteYellow}),
            _ => (short)0
        };

        TerrariaUtils.WriteEffectMessage(item, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Set the weather
        WorldUtils.SetWeather(_weather);
    }

    #endregion
}