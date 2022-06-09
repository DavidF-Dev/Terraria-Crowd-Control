using JetBrains.Annotations;

namespace CrowdControlMod;

[PublicAPI]
public enum CrowdControlPacket : byte
{
    /// <summary>
    ///     Broadcast a debug message to be received by any clients that have developer mode enabled.<br />
    ///     Packet data: (string)message
    /// </summary>
    BroadcastDebug,
    
    /// <summary>
    ///     Set the in-game time on the server.<br />
    ///     Packet data: (int)time (bool)isDay
    /// </summary>
    SetTime
}