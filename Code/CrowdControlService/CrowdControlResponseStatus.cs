﻿namespace CrowdControlMod.CrowdControlService;

public enum CrowdControlResponseStatus
{
    /// <summary>
    ///     The effect executed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    ///     The effect failed to trigger, but is still available for use.
    ///     Viewer(s) will be refunded.
    /// </summary>
    Failure = 1,

    /// <summary>
    ///     Same as <see cref="Failure" /> but the effect is no longer available for use for the remainder of the game.
    /// </summary>
    Unavailable = 2,

    /// <summary>
    ///     The effect cannot be triggered right now, try again in a few seconds.
    ///     Will fail after a series of attempts.
    /// </summary>
    Retry = 3,

    /// <summary>
    ///     The timed effect has been paused and is now waiting.
    /// </summary>
    Paused = 6,

    /// <summary>
    ///     The timed effect has been resumed and is counting down again.
    /// </summary>
    Resumed = 7,

    /// <summary>
    ///     The timed effect has finished.
    /// </summary>
    Finished = 8
}