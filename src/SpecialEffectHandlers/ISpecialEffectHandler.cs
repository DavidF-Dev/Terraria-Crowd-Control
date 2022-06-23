using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using JetBrains.Annotations;

namespace CrowdControlMod.SpecialEffectHandlers;

/// <summary>
///     Can be used to start/stop one or more regular effects in a specialised way.
/// </summary>
public interface ISpecialEffectHandler
{
    #region Methods

    /// <summary>
    ///     Get a collection of effect ids that should be started or stopped.
    /// </summary>
    [PublicAPI] [NotNull]
    public IEnumerable<string> GetEffectIds(CrowdControlRequestType requestType);

    #endregion
}