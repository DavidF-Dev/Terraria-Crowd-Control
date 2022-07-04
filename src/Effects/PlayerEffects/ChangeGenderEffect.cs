using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.Audio;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Change the player's gender between male and female.
/// </summary>
public sealed class ChangeGenderEffect : CrowdControlEffect
{
    #region Constructors

    public ChangeGenderEffect() : base(EffectID.ChangeGender, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Flip the player's gender
        var player = GetLocalPlayer();
        player.Player.Male = !player.Player.Male;
        SoundEngine.PlaySound(SoundID.Item6, player.Player.position);
        PlayerUtils.SetHairDye(player, ItemID.PartyHairDye);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Confetti, $"{viewerString} changed {playerString} to a {(GetLocalPlayer().Player.Male ? "boy" : "girl")}", Severity);
    }

    #endregion
}