using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
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
        var aboveSurface = player.Player.position.ToTileCoordinates().Y < Main.worldSurface;
        player.Player.maxRunSpeed = aboveSurface ? SurfaceSpeed : CavernSpeed;
        player.Player.runAcceleration = aboveSurface ? SurfaceAcceleration : CavernAcceleration;
    }

    #endregion

    #region Constructors

    public RunBoostEffect(int duration) : base(EffectID.RunBoost, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    protected override int StartEmote => EmoteID.PartyCake;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        player.PostUpdateRunSpeedsHook += PostUpdateRunSpeeds;
        player.Player.SetHairDye(ItemID.SpeedHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook -= PostUpdateRunSpeeds;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.HermesBoots, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStopText(Id), EffectSeverity.Neutral);
    }

    #endregion
}