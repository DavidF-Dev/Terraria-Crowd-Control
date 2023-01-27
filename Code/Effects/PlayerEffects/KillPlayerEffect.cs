using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.DataStructures;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Forcefully kill the player instantly.
/// </summary>
public sealed class KillPlayerEffect : CrowdControlEffect
{
    #region Constructors

    public KillPlayerEffect() : base(EffectID.KillPlayer, null, EffectSeverity.Negative)
    {
    }

    #endregion

    public override EffectCategory Category => EffectCategory.Player;
    
    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Succeed if the player is not invincible (the player is killed when the start message is sent)
        return !GetLocalPlayer().Player.IsInvincible() ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Get kill verbs from localisation
        var killVerbs = LangUtils.GetEffectMiscText(Id, "KillVerbs").Split('|');
        var chosenKillVerb = killVerbs[Main.rand.Next(killVerbs.Length)].Trim('\n');

        // Get kill reason from localisation, substituting in kill verb
        var killReason = LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, chosenKillVerb);

        // Kill the player here
        GetLocalPlayer().Player.KillMe(PlayerDeathReason.ByCustomReason(killReason), 1000, 0);
    }

    #endregion
}