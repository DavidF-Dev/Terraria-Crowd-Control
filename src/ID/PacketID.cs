using JetBrains.Annotations;

namespace CrowdControlMod.ID;

// ReSharper disable once InconsistentNaming
[PublicAPI]
public enum PacketID : byte
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
    ///     Notify the server about the status of an effect.<br />
    ///     Packet data: (bool)status
    /// </summary>
    EffectStatus,

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

    /// <summary>
    ///     Spawn an npc on the server.<br />
    ///     Packet data: (short)type (int)tileX (int)tileY
    /// </summary>
    SpawnNpc
}