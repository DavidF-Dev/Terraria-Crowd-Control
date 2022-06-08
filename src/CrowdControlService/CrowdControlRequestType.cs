using JetBrains.Annotations;

namespace CrowdControlMod.CrowdControlService;

[PublicAPI]
public enum CrowdControlRequestType
{
    Test,
    Start,
    Stop
}