namespace CrowdControlMod.CrowdControlService;

public enum CrowdControlRequestType
{
    Test = 0,

    /// <summary>
    ///     Trigger the effect.
    /// </summary>
    Start = 1,

    /// <summary>
    ///     Stop the effect, if it is timed.
    /// </summary>
    Stop = 2
}