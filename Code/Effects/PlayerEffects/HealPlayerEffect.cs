using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Fully heal the player's health.
/// </summary>
public sealed class HealPlayerEffect : CrowdControlEffect
{
    #region Constructors

    public HealPlayerEffect() : base(EffectID.HealPlayer, 0, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    protected override int StartEmote => EmoteID.EmotionLove;

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
        player.Player.AddBuff(BuffID.Lovestruck, 60 * 5);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Heart, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}