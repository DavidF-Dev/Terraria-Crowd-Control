using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.BuffEffects;

/// <summary>
///     Provide the player with the shimmer buff temporarily so that they fall through blocks.
/// </summary>
public sealed class NoclipEffect : CrowdControlEffect
{
    #region Constructors

    public NoclipEffect(int duration) : base(EffectID.BuffShimmer, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Buff;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.HasBuff(BuffID.Shimmer) || !player.Player.IsGrounded())
        {
            // Retry if the player is already shimmered or not grounded
            return CrowdControlResponseStatus.Retry;
        }

        player.PreUpdateBuffsHook += PreUpdateBuffs;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.PreUpdateBuffsHook -= PreUpdateBuffs;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? _)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GalaxyPearl, LangUtils.GetEffectStartText(Id, viewerString, playerString, _), Severity);
    }

    private void PreUpdateBuffs()
    {
        var player = GetLocalPlayer();
        player.Player.buffImmune[BuffID.Shimmer] = false;
        var buffIndex = player.Player.FindBuffIndex(BuffID.Shimmer);
        if (buffIndex != -1 && player.Player.buffTime[buffIndex] < TimeLeft)
        {
            player.Player.DelBuff(buffIndex);
        }

        player.Player.AddBuff(BuffID.Shimmer, (int)Math.Ceiling(60 * TimeLeft));
    }

    #endregion
}