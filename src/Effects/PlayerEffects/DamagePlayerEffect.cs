using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.DataStructures;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Damage the player so they are left on a sliver of health. Will not kill the player.
/// </summary>
public sealed class DamagePlayerEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int Health = 10;
    private const int Buffer = 15;

    #endregion

    #region Constructors

    public DamagePlayerEffect() : base(EffectID.DamagePlayer, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.IsInvincible() || player.Player.statLife <= Health + Buffer)
        {
            // Retry if invincible or already severely damaged
            return CrowdControlResponseStatus.Retry;
        }

        // Damage the player so that they end up at the desired health
        var damage = Math.Abs(Health - player.Player.statLife);
        player.Player.Hurt(PlayerDeathReason.LegacyEmpty(), damage, 0);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.PsychoKnife, $"{viewerString} severely damaged {playerString}", Severity);
    }

    #endregion
}