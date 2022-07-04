using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Artificially increase the player run-speed for a short duration.
/// </summary>
public sealed class RunBoostEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float SurfaceSpeed = 15f;
    private const float CavernSpeed = 9f;
    private const float SurfaceAcceleration = 2f;
    private const float CavernAcceleration = 1f;

    #endregion

    #region Static Methods

    private static void PostUpdateRunSpeeds()
    {
        var player = GetLocalPlayer();
        var aboveSurface = player.TileY < Main.worldSurface;
        player.Player.maxRunSpeed = aboveSurface ? SurfaceSpeed : CavernSpeed;
        player.Player.runAcceleration = aboveSurface ? SurfaceAcceleration : CavernAcceleration;
    }

    #endregion

    #region Constructors

    public RunBoostEffect(float duration) : base(EffectID.RunBoost, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        player.PostUpdateRunSpeedsHook += PostUpdateRunSpeeds;
        PlayerUtils.SetHairDye(player, ItemID.SpeedHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook -= PostUpdateRunSpeeds;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.HermesBoots, $"{viewerString} made {playerString} really, really fast for {durationString} seconds", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "Movement speed is back to normal", EffectSeverity.Neutral);
    }

    #endregion
}