using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Damage the player so they are left on a sliver of health. Will not kill the player.
/// </summary>
public sealed class DamagePlayerEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int Health = 12;
    private const int Buffer = 18;

    #endregion

    #region Constructors

    public DamagePlayerEffect() : base(EffectID.DamagePlayer, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.EmotionAnger;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.IsInvincible() || player.Player.statLife <= Health + Buffer)
        {
            // Retry if invincible or already severely damaged
            return CrowdControlResponseStatus.Retry;
        }

        // Determine amount to damage the player
        var damage = Math.Abs(Health - player.Player.statLife);

        // Damage the player so that they end up at the desired health
        player.Player.HurtDirect(damage);

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.PsychoKnife, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}