using JetBrains.Annotations;

namespace CrowdControlMod;

public abstract class CrowdControlEffect
{
    #region Constructors

    protected CrowdControlEffect([NotNull] string id)
    {
        Id = id;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Unique id that correlates to the crowd control effect ids.
    /// </summary>
    [PublicAPI] [NotNull]
    public string Id { get; }

    /// <summary>
    ///     Effect is currently active.
    /// </summary>
    [PublicAPI]
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Name of the viewer that triggered the effect.
    /// </summary>
    [PublicAPI] [NotNull]
    protected string Viewer { get; private set; } = string.Empty;
    
    /// <summary>
    ///     Total time that the effect takes to complete.
    /// </summary>
    [PublicAPI] [CanBeNull]
    protected float? Duration { get; set; }
    
    /// <summary>
    ///     Current time remaining on the effect.
    /// </summary>
    [PublicAPI] [CanBeNull]
    protected float? TimeLeft { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Start the effect.
    /// </summary>
    [PublicAPI]
    public void Start([NotNull] string viewer)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Viewer = viewer;
        TimeLeft = Duration;
        
        OnStart();

        if (IsActive && !TimeLeft.HasValue)
        {
            // Stop straight away if the effect does not have a duration
            Stop();
        }
    }

    /// <summary>
    ///     Stop the effect instantly.
    /// </summary>
    [PublicAPI]
    public void Stop()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Viewer = string.Empty;
        TimeLeft = null;
        
        OnStop();
    }

    /// <summary>
    ///     Update the effect whilst active each frame so that the time remaining is reduced.
    /// </summary>
    [PublicAPI]
    public void Update(float delta)
    {
        if (!IsActive || !TimeLeft.HasValue)
        {
            return;
        }

        // Reduce the timer, stopping the effect if it reaches zero
        TimeLeft -= delta;
        if (TimeLeft <= 0)
        {
            Stop();
        }
    }

    /// <summary>
    ///     Invoked when the effect is triggered.
    /// </summary>
    protected virtual void OnStart()
    {
    }

    /// <summary>
    ///     Invoked when the effect is stopped.
    /// </summary>
    protected virtual void OnStop()
    {
    }

    #endregion
}