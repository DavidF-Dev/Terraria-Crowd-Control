using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

public sealed class SetWeatherEffect : CrowdControlEffect
{
    #region Static Methods

    [NotNull] [Pure]
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

    #endregion

    #region Fields

    private readonly WorldUtils.Weather _weather;

    #endregion

    #region Constructors

    public SetWeatherEffect(WorldUtils.Weather weather) : base(GetId(weather), null, EffectSeverity.Neutral)
    {
        _weather = weather;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (WorldUtils.GetWeather() == _weather)
        {
            return CrowdControlResponseStatus.Failure;
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Change the weather in a single-player world
            WorldUtils.SetWeather(_weather);
        }
        else
        {
            // Notify server to change the weather
            SendPacket(PacketID.SetWeather, (int)_weather);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        var itemId = _weather switch
        {
            WorldUtils.Weather.Clear => ItemID.SunMask,
            WorldUtils.Weather.Rain => ItemID.RainCoat,
            WorldUtils.Weather.Storm => ItemID.NimbusRod,
            WorldUtils.Weather.Windy => Main.rand.Next(new[] {ItemID.KiteBlue, ItemID.KiteRed, ItemID.KiteYellow}),
            _ => throw new ArgumentOutOfRangeException()
        };

        var message = _weather switch
        {
            WorldUtils.Weather.Clear => $"{viewerString} summoned a clear sky above {playerString}'s head",
            WorldUtils.Weather.Rain => $"{viewerString} made it rain",
            WorldUtils.Weather.Storm => $"{viewerString} bad-mouthed Thor and summoned a raging storm",
            WorldUtils.Weather.Windy => $"{viewerString} brought upon a windy {(Main.dayTime ? "day" : "night")}",
            _ => throw new ArgumentOutOfRangeException()
        };

        TerrariaUtils.WriteEffectMessage(itemId, message, Severity);
    }

    protected override void OnReceivePacket(PacketID packetId, CrowdControlPlayer player, BinaryReader reader)
    {
        if (packetId != PacketID.SetWeather)
        {
            // Ignore
            return;
        }

        var weather = (WorldUtils.Weather)reader.ReadInt32();
        if (weather != _weather)
        {
            // Should be the same (technically don't even need to send the weather but oh well)
            return;
        }

        // Set the weather
        WorldUtils.SetWeather(weather);
    }

    #endregion
}