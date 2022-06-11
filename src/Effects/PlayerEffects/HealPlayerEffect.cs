using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

public sealed class HealPlayerEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int LovestruckSeconds = 6;

    #endregion

    #region Constructors

    public HealPlayerEffect() : base(EffectID.HealPlayer, null, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.statLife == player.Player.statLifeMax2)
        {
            // Already full healed
            return CrowdControlResponseStatus.Failure;
        }

        player.Player.statLife = player.Player.statLifeMax2;
        player.Player.AddBuff(BuffID.Lovestruck, 60 * LovestruckSeconds);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Heart, $"{viewerString} healed {playerString}", Severity);
    }

    #endregion
}