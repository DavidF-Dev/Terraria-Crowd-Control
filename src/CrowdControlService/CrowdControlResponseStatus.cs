using JetBrains.Annotations;

namespace CrowdControlMod.CrowdControlService;

[PublicAPI]
public enum CrowdControlResponseStatus
{
    Success,
    Failure,
    Unavailable,
    Retry
}