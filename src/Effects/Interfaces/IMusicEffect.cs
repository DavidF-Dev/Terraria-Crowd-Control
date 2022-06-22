namespace CrowdControlMod.Effects.Interfaces;

/// <summary>
///     Any active effect that implements this interface can play music for the duration of the effect.
/// </summary>
public interface IMusicEffect
{
    #region Properties

    /// <summary>
    ///     Music to play when the effect is active.
    /// </summary>
    int MusicId { get; }

    /// <summary>
    ///     Priority when compared to other effects (higher = higher priority).
    /// </summary>
    int MusicPriority { get; }

    #endregion
}