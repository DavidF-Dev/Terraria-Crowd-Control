using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.Audio;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Play a sound effect at the player's position.
/// </summary>
public sealed class PlaySoundEffect : CrowdControlEffect
{
    #region Fields

    private readonly float? _overrideVolume;

    private readonly SoundStyle[] _soundStyles;

    #endregion

    #region Constructors

    public PlaySoundEffect(string id, float? overrideVolume, params SoundStyle[] soundStyles) : base(id, null, EffectSeverity.Neutral)
    {
        _overrideVolume = overrideVolume;
        _soundStyles = soundStyles;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!_soundStyles.Any())
        {
            TerrariaUtils.WriteDebug("Cannot choose a sound effect to play as there are no options");
            return CrowdControlResponseStatus.Failure;
        }

        // Play a random sound
        var slotId = SoundEngine.PlaySound(_soundStyles[Main.rand.Next(_soundStyles.Length)]);
        if (!SoundEngine.TryGetActiveSound(slotId, out var sound))
        {
            // Something went wrong
            return CrowdControlResponseStatus.Retry;
        }

        if (_overrideVolume.HasValue)
        {
            // Override the playback volume
            sound.Volume = _overrideVolume.Value;
        }

        return CrowdControlResponseStatus.Success;
    }

    #endregion
}