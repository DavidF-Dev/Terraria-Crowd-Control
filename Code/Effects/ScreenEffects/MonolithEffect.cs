using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects.Interfaces;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Enable a monolith background temporarily.
/// </summary>
public sealed class MonolithEffect : CrowdControlEffect, IMusicEffect
{
    #region Enums

    public enum MonolithType
    {
        BloodMoon,
        Shimmer,
        Nebula,
        Solar,
        Vortex,
        Stardust,
        MoonLord
    }

    #endregion

    #region Static Fields and Constants

    private static string? _running;

    #endregion

    #region Static Methods

    private static short GetItemId(MonolithType type)
    {
        return type switch
        {
            MonolithType.BloodMoon => ItemID.BloodMoonMonolith,
            MonolithType.Shimmer => ItemID.ShimmerMonolith,
            MonolithType.Nebula => ItemID.NebulaMonolith,
            MonolithType.Solar => ItemID.SolarMonolith,
            MonolithType.Vortex => ItemID.VortexMonolith,
            MonolithType.Stardust => ItemID.StardustMonolith,
            MonolithType.MoonLord => ItemID.VoidMonolith,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static int GetMusicId(MonolithType type)
    {
        return type switch
        {
            MonolithType.BloodMoon => -1,
            MonolithType.Shimmer => MusicID.Shimmer,
            MonolithType.Nebula => -1,
            MonolithType.Solar => -1,
            MonolithType.Vortex => -1,
            MonolithType.Stardust => -1,
            MonolithType.MoonLord => MusicID.OtherworldlyInvasion,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    #endregion

    #region Fields

    private readonly MonolithType _type;

    #endregion

    #region Constructors

    public MonolithEffect(string id, float duration, MonolithType type) : base(id, duration, EffectSeverity.Neutral)
    {
        _type = type;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Screen;

    int IMusicEffect.MusicId => GetMusicId(_type);

    int IMusicEffect.MusicPriority => 0;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!string.IsNullOrEmpty(_running))
        {
            return CrowdControlResponseStatus.Retry;
        }

        var player = GetLocalPlayer();
        var result = _type switch
        {
            MonolithType.BloodMoon => !player.Player.bloodMoonMonolithShader,
            MonolithType.Shimmer => !player.Player.shimmerMonolithShader,
            MonolithType.Nebula => !player.Player.nebulaMonolithShader,
            MonolithType.Solar => !player.Player.solarMonolithShader,
            MonolithType.Vortex => !player.Player.vortexMonolithShader,
            MonolithType.Stardust => !player.Player.stardustMonolithShader,
            MonolithType.MoonLord => !player.Player.moonLordMonolithShader,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!result)
        {
            return CrowdControlResponseStatus.Retry;
        }

        _running = Id;
        player.PostUpdateEquipsHook += OnPostUpdateEquips;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        if (_running != Id)
        {
            return;
        }

        GetLocalPlayer().PostUpdateEquipsHook -= OnPostUpdateEquips;
        _running = null;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(GetItemId(_type), LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private void OnPostUpdateEquips()
    {
        var player = GetLocalPlayer();
        player.Player.bloodMoonMonolithShader = _type is MonolithType.BloodMoon;
        player.Player.shimmerMonolithShader = _type is MonolithType.Shimmer;
        player.Player.nebulaMonolithShader = _type is MonolithType.Nebula;
        player.Player.solarMonolithShader = _type is MonolithType.Solar;
        player.Player.vortexMonolithShader = _type is MonolithType.Vortex;
        player.Player.stardustMonolithShader = _type is MonolithType.Stardust;
        player.Player.moonLordMonolithShader = _type is MonolithType.MoonLord;
    }

    #endregion
}