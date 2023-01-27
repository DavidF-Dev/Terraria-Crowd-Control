using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Artificially increase the jump-boost of the player.
/// </summary>
public sealed class JumpBoostEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float BoostAmount = 9f;

    #endregion

    #region Static Methods

    private static void PostUpdateEquips()
    {
        var player = GetLocalPlayer();
        player.Player.jumpSpeedBoost = BoostAmount;
        player.Player.jumpBoost = true;
    }

    #endregion

    public override EffectCategory Category => EffectCategory.Player;
    
    #region Constructors

    public JumpBoostEffect(float duration) : base(EffectID.JumpBoost, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().PostUpdateEquipsHook += PostUpdateEquips;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateEquipsHook -= PostUpdateEquips;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.FrogLeg, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStopText(Id), EffectSeverity.Neutral);
    }

    #endregion
}