using JetBrains.Annotations;

namespace CrowdControlMod.Effects;

[PublicAPI]
public enum EffectSeverity
{
    Neutral,

    /// <summary>
    ///     Provides positive buffs or rewards for the streamer.
    /// </summary>
    Positive,

    /// <summary>
    ///     Provides negative buffs of punishments for the streamer.
    /// </summary>
    Negative
}