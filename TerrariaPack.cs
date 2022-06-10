using System;
using System.Collections.Generic;
using CrowdControl.Common;
using CrowdControl.Games.Packs;

public sealed class Terraria : SimpleTCPPack
{
    #region Constructors
    
    public Terraria(IPlayer player, Func<CrowdControlBlock, bool> responseHandler, Action<object> statusUpdateHandler) : base(player, responseHandler, statusUpdateHandler)
    {
    }

    #endregion
    
    #region Properties

    /// <summary>
    ///     Host ip address. Internal server.
    /// </summary>
    public override string Host { get; } = "127.0.0.1";

    /// <summary>
    ///     Host port that matches the listening port in the mod.
    /// </summary>
    public override ushort Port { get; } = 58430;
    
    /// <summary>
    ///     Game details.
    /// </summary>
    public override Game Game { get; } = new Game(uint.MaxValue, "Terraria Crowd Control", "Terraria", "PC", CrowdControl.Common.ConnectorType.SimpleTCPConnector);

    /// <summary>
    ///     List of the supported effects and their corresponding ids.
    /// </summary>
    public override List<Effect> Effects
    {
        get
        {
            return new List<Effect>()
            {
                // --- Time effects
                new Effect("Time Effects", "time_folder", ItemKind.Folder),
                new Effect("Set the time to noon", "time_noon"),
                new Effect("Set the time to midnight", "time_midnight"),
                new Effect("Set the time to sunrise", "time_sunrise"),
                new Effect("Set the time to sunset", "time_sunset"),
                
                // --- Positive buff effects
                new Effect("Boost player survivability", "buff_survivability"),
                new Effect("Boost player health regeneration", "buff_regeneration"),
                
                new Effect("Spawn structure", "spawn_structure"),
                new Effect("Increase spawn rate", "increase_spawn_rate")
            };
        }
    }
    
    #endregion
}