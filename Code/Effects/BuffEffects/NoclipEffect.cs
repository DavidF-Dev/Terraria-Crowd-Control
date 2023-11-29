using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;
using Terraria.ModLoader;

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
        if (player.Player.HasBuff(BuffID.Shimmer) || player.Player.shimmering || !player.Player.IsGrounded())
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
        player.Player.GetModPlayer<ShimmerPlayer>().Shimmering = true;
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private class ShimmerPlayer : ModPlayer
    {
        #region Fields

        public bool Shimmering;

        #endregion

        #region Methods

        public override void PostUpdateBuffs()
        {
            if (!Shimmering)
            {
                // Ignore
                return;
            }

            if (Player.HasBuff(BuffID.Shimmer) || Player.shimmering)
            {
                // Keep shimmering if in lava
                if (Player.IsInLiquid(LiquidID.Lava))
                {
                    var buffIndex = Player.FindBuffIndex(BuffID.Shimmer);
                    if (buffIndex == -1)
                    {
                        Player.AddBuff(BuffID.Shimmer, 30);
                    }
                    else if (Player.buffTime[buffIndex] < 30)
                    {
                        Player.buffTime[buffIndex] = 30;
                    }
                }

                // Prevent item use whilst shimmering
                Player.AddBuff(BuffID.Cursed, 2);
            }
            else
            {
                Shimmering = false;
            }
        }

        #endregion
    }

    #endregion
}