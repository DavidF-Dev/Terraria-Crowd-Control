namespace CrowdControlMod.ID;

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
    ///     Sync the weather on the client (from the server).<br />
    ///     Packet data: (float)cloudAlpha (float)windSpeedTarget (int)windCounter (int)extremeWindCounter
    /// </summary>
    SyncWeather,

    /// <summary>
    ///     Sync an npc in non-vanilla ways (from the server).<br />
    ///     Packet data: (int)whoAmI (int)lifeMax (int)life
    /// </summary>
    SyncNPCSpecial,

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
    ///     Packet data: (bool)disableTombstones, (bool)despawnBosses
    /// </summary>
    ConfigState
}