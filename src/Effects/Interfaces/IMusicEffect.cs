namespace CrowdControlMod.Effects.Interfaces;

public interface IMusicEffect
{
    /// <summary>
    ///     Music to play when the effect is active.
    /// </summary>
    int MusicId { get; }
    
    /// <summary>
    ///     Priority when compared to other effects (higher = higher priority).
    /// </summary>
    int MusicPriority { get; }
}