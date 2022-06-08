using CrowdControlMod.CrowdControlService;
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
    public CrowdControlResponseStatus Start([NotNull] string viewer)
    {
        if (IsActive)
        {
            return CrowdControlResponseStatus.Retry;
        }

        Viewer = viewer;
        TimeLeft = Duration;
        var responseStatus = OnStart();
        IsActive = responseStatus == CrowdControlResponseStatus.Success;

        if (IsActive && !TimeLeft.HasValue)
        {
            // Stop straight away if the effect does not have a duration
            Stop();
        } 
        else if (!IsActive)
        {
            // Ensure that the effect is stopped properly if not active
            Stop();
        }

        return responseStatus;
    }

    /// <summary>
    ///     Stop the effect instantly, without fail.
    /// </summary>
    [PublicAPI]
    public CrowdControlResponseStatus Stop()
    {
        if (!IsActive)
        {
            return CrowdControlResponseStatus.Failure;
        }

        IsActive = false;
        Viewer = string.Empty;
        TimeLeft = null;
        
        OnStop();

        return CrowdControlResponseStatus.Success;
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
            return;
        }
        
        OnUpdate(delta);
    }

    /// <summary>
    ///     Invoked when the effect is triggered.
    /// </summary>
    protected virtual CrowdControlResponseStatus OnStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    /// <summary>
    ///     Invoked when the effect is stopped. Stops without fail.
    /// </summary>
    protected virtual void OnStop()
    {
    }

    /// <summary>
    ///     Invoked each frame whilst the effect is active.
    /// </summary>
    protected virtual void OnUpdate(float delta)
    {
    }

    #endregion
}