using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Forcefully kill the player instantly.
/// </summary>
public sealed class KillPlayerEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    [NotNull]
    private static readonly string[] KillVerbs =
    {
        "killed", "slapped really hard", "pulverised", "slain", "assassinated", "discombobulated",
        "vaporised", "force-choked", "disposed of", "stared violently", "yeeted out of existence",
        "friend-zoned", "zapped", "crushed", "imploded", "murdered", "executed", "slam dunked",
        "force-fed poison ivy", "smacked with a fish", "ripped to shreds", "attacked with a toothbrush",
        "spat on", "cancelled", "tormented", "led into a room of angry fans", "hugged too tightly",
        "subjected to a bad fun", "shot with a water gun", "poked", "removed from this plain of existence",
        // ReSharper disable once StringLiteralTypo
        "fed [c/FFFF00:ra][c/FF0000:in][c/0000FF:bo][c/8B00FF:ws]"
    };

    #endregion

    #region Constructors

    public KillPlayerEffect() : base(EffectID.KillPlayer, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Succeed if the player is not invincible (the player is killed when the start message is sent)
        return !PlayerUtils.IsInvincible(GetLocalPlayer()) ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Retry;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        // Kill the player here (choose a random verb to use)
        GetLocalPlayer().Player.KillMe(PlayerDeathReason.ByCustomReason($"{playerString} was {KillVerbs[Main.rand.Next(KillVerbs.Length)]} by {viewerString}"), 1000, 0);
    }

    #endregion
}