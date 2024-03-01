using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Increase the attack speed.
/// </summary>
public sealed class AttackSpeedEffect : CrowdControlEffect
{
    #region Static Methods

    private static void OnPostUpdateEquipsHook()
    {
        var player = GetLocalPlayer().Player;
        player.GetAttackSpeed(DamageClass.Generic) += 2f; // +200%
    }

    #endregion

    #region Constructors

    public AttackSpeedEffect(int duration) : base(EffectID.AttackSpeed, duration, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().PostUpdateEquipsHook += OnPostUpdateEquipsHook;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateEquipsHook -= OnPostUpdateEquipsHook;
    }

    #endregion
}