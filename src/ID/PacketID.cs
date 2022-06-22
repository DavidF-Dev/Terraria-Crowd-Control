using JetBrains.Annotations;

namespace CrowdControlMod.ID;

// ReSharper disable once InconsistentNaming
[PublicAPI]
public enum PacketID : byte
{
    /// <summary>
    ///     Send a debug message to all clients (from the server).<br />
    ///     Packet data: (string)message (uint)colourPackedValue
    /// </summary>
    DebugMessage,

    /// <summary>
    ///     Send an effect message to all clients (from the server).<br />
    ///     Packet data: (short)itemId (string)message (int)severity
    /// </summary>
    EffectMessage,

    /// <summary>
    ///     Notify the server about the status of an effect (from the client).<br />
    ///     Packet data: (bool)status
    /// </summary>
    EffectStatus,

    /// <summary>
    ///     Notify the server about an effect that wants to run on the server (from the client).<br />
    ///     Packet data: depends on the effect.
    /// </summary>
    HandleEffect,

    /// <summary>
    ///     Notify the server about the client's config settings (from client).<br />
    ///     Packet data: (bool)disableTombstones
    /// </summary>
    ConfigState
}