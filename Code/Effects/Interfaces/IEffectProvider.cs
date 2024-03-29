﻿using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;

namespace CrowdControlMod.Effects.Interfaces;

/// <summary>
///     Can be used to start/stop one or more regular effects in a specialised way.
/// </summary>
public interface IEffectProvider
{
    #region Methods

    /// <summary>
    ///     Get a collection of effect ids that should be started or stopped.<br />
    ///     - If starting; only return one id.<br />
    ///     - If stopping; return all possible ids.
    /// </summary>
    public IReadOnlyCollection<string> GetEffectIds(CrowdControlRequestType requestType);

    #endregion
}