using JetBrains.Annotations;

namespace CrowdControlMod;

[PublicAPI]
public enum CrowdControlPacket : byte
{
    /// <summary>
    ///     Send a debug message to all clients.<br />
    ///     Packet data: (string)message (uint)colourPackedValue
    /// </summary>
    DebugMessage,
    
    /// <summary>
    ///     Send an effect message to all clients.<br />
    ///     Packet data: (short)itemId (string)message (int)severity
    /// </summary>
    EffectMessage,
    
    /// <summary>
    ///     Set the in-game time on the server.<br />
    ///     Packet data: (int)time (bool)isDay
    /// </summary>
    SetTime,
    
    /// <summary>
    ///     Spawn a structure on the server.<br />
    ///     Packet data: (int)structure (int)tileX (int)tileY
    /// </summary>
    SpawnStructure,
}