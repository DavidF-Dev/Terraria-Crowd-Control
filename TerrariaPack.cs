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
                // --- Player effects
                new Effect("Player Effects", "player_folder", ItemKind.Folder),
                new Effect("Kill player", "kill_player"),
                new Effect("Explode player", "explode_player"),
                new Effect("Heal player", "heal_player"),
                new Effect("Damage player", "damage_player"),
                new Effect("God mode", "god_mode_player"),
                new Effect("Increase max health", "increase_life"),
                new Effect("Decrease max health", "decrease_life"),
                new Effect("Increase max mana", "increase_mana"),
                new Effect("Decrease max mana", "decrease_mana"),
                new Effect("Increase spawn rate", "increase_spawn_rate"),
                
                // --- Time effects
                new Effect("Time Effects", "time_folder", ItemKind.Folder),
                new Effect("Set the time to noon", "time_noon"),
                new Effect("Set the time to midnight", "time_midnight"),
                new Effect("Set the time to sunrise", "time_sunrise"),
                new Effect("Set the time to sunset", "time_sunset"),
                
                // --- Buff effects
                new Effect("Buff Effects", "buff_folder", ItemKind.Folder),
                new Effect("Increase jump height", "jump_boost"),
                new Effect("Increase movement speed", "run_boost"),
                new Effect("Slippery boots", "icy_feet"),
                new Effect("Boost player survivability", "buff_survivability"),
                new Effect("Boost player health regeneration", "buff_regeneration"),
                
                // --- Inventory effects
                new Effect("Inventory Effects", "inventory_folder", ItemKind.Folder),
                new Effect("Drop item", "drop_item"),
                new Effect("Explode inventory", "explode_inventory"),
                new Effect("Reforge item", "item_prefix"),
                new Effect("Boost coin drops", "boost_money"),
                
                // --- World effects
                new Effect("World Effects", "world_folder", ItemKind.Folder),
                new Effect("Spawn structure", "spawn_structure"),
                new Effect("Random teleportation", "random_teleport"),
                
                // --- Screen effects
                new Effect("Screen Effects", "screen_folder", ItemKind.Folder),
                new Effect("Wall of Fish", "wall_of_fish")
            };
        }
    }
    
    #endregion
}