using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Teleport the player to their previous death position, if there is one.
/// </summary>
public sealed class TeleportToDeathEffect : CrowdControlEffect
{
    #region Fields

    private Vector2? _deathPos;

    #endregion

    #region Constructors

    public TeleportToDeathEffect() : base(EffectID.DeathTeleport, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override void OnSessionStarted()
    {
        GetLocalPlayer().KillHook += OnKill;
    }

    protected override void OnSessionStopped()
    {
        GetLocalPlayer().KillHook -= OnKill;
        _deathPos = null;
    }

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!_deathPos.HasValue)
        {
            // No death point
            return CrowdControlResponseStatus.Failure;
        }

        // Teleport the player
        GetLocalPlayer().Player.Teleport(_deathPos.Value, TeleportationStyleID.RecallPotion);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.PotionOfReturn, $"{viewerString} send {playerString} back to where they last died", Severity);
    }

    private void OnKill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        _deathPos = GetLocalPlayer().Player.position;
    }

    #endregion
}