namespace CrowdControlMod.Features;

/// <summary>
///     Feature that is tied to the crowd control session.
/// </summary>
public interface IFeature
{
    #region Methods

    /// <summary>
    ///     Initialise the feature when the session has started (client-side).
    /// </summary>
    void SessionStarted();

    /// <summary>
    ///     Clean up the feature when the session has ended (client-side).
    /// </summary>
    void SessionStopped();

    /// <summary>
    ///     Dispose the feature when the mod is unloaded.
    /// </summary>
    void Dispose();

    #endregion
}