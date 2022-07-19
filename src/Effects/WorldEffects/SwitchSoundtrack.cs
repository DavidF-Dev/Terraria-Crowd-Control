using System.Reflection;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Switch soundtrack between Vanilla and Otherworld.
/// </summary>
public sealed class SwitchSoundtrack : CrowdControlEffect
{
    #region Fields

    private readonly FieldInfo? _swapMusicField;

    #endregion

    #region Constructors

    public SwitchSoundtrack() : base(EffectID.SwitchSoundtrack, null, EffectSeverity.Neutral)
    {
        _swapMusicField = typeof(Main).GetField("swapMusic", BindingFlags.Static | BindingFlags.NonPublic);
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.EmoteNote;

    private bool SwapMusic
    {
        get => (bool)_swapMusicField?.GetValue(null)!;
        set => _swapMusicField?.SetValue(null, value);
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Simply toggle Main.swapMusic
        SwapMusic = !SwapMusic;
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        // Check which soundtrack is playing (flipped on the drunk world seed)
        var playingOtherworld = (!WorldUtils.IsDrunkWorld && SwapMusic) || (WorldUtils.IsDrunkWorld && !SwapMusic);

        if (playingOtherworld)
        {
            TerrariaUtils.WriteEffectMessage(ItemID.MusicBoxOWDay, $"{viewerString} started playing the Terraria Otherworld soundtrack", Severity);
        }
        else
        {
            TerrariaUtils.WriteEffectMessage(ItemID.MusicBoxOverworldDay, $"{viewerString} started playing the Terraria soundtrack", Severity);
        }
    }

    #endregion
}