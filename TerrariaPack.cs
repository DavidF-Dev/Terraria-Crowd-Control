using System;
using System.Collections.Generic;
using CrowdControl.Common;
using CrowdControl.Games.Packs;

public sealed class Terraria : SimpleTCPPack
{
    #region Static Fields and Constants
    
    private const string PlayerFolder = "player_folder";
    private const string BuffFolder = "buff_folder";
    private const string InventoryFolder = "inventory_folder";
    private const string WorldFolder = "world_folder";
    private const string BossFolder = "boss_folder";
    private const string ScreenFolder = "screen_folder";
    private const string ChallengesFolder = "challenges_folder";
    
    #endregion
    
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
    public override Game Game { get; } = new Game(66, "Terraria Crowd Control", "Terraria", "PC", CrowdControl.Common.ConnectorType.SimpleTCPConnector);

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
                new Effect("Player effects", PlayerFolder, ItemKind.Folder),
                new Effect("Kill", "kill_player", PlayerFolder),
                new Effect("Explode", "explode_player", PlayerFolder),
                new Effect("Heal", "heal_player", PlayerFolder),
                new Effect("Damage", "damage_player", PlayerFolder),
                new Effect("God-mode", "god_mode_player", PlayerFolder),
                new Effect("Increase max health", "increase_life", PlayerFolder),
                new Effect("Decrease max health", "decrease_life", PlayerFolder),
                new Effect("Increase max mana", "increase_mana", PlayerFolder),
                new Effect("Decrease max mana", "decrease_mana", PlayerFolder),
                new Effect("Increase spawn rate", "increase_spawn_rate", PlayerFolder),
                new Effect("Infinite ammo & mana", "infinite_ammo", PlayerFolder),
                new Effect("Teleport to death point", "death_tp", PlayerFolder),
                new Effect("Give pet", "give_pet", PlayerFolder),
                new Effect("Give light pet", "give_light_pet", PlayerFolder),
                new Effect("Change gender", "change_gender", PlayerFolder),
                new Effect("Force mount", "force_mount", PlayerFolder),
                new Effect("Shoot bombs", "shoot_bombs", PlayerFolder),
                new Effect("Shoot grenades", "shoot_grenades", PlayerFolder),
                new Effect("Increase jump height", "jump_boost", PlayerFolder),
                new Effect("Increase movement speed", "run_boost", PlayerFolder),
                new Effect("Slippery boots", "icy_feet", PlayerFolder),
                new Effect("Cannot pickup items", "no_item_pickup", PlayerFolder),

                // --- Buff effects (positive)
                new Effect("Buff effects", BuffFolder, ItemKind.Folder),
                new Effect("Boost survivability", "buff_survivability", BuffFolder),
                new Effect("Boost health regeneration", "buff_regen", BuffFolder),
                new Effect("Provide light buffs", "buff_light", BuffFolder),
                new Effect("Help search for treasure", "buff_treasure", BuffFolder),
                new Effect("Boost movement speed", "buff_movement", BuffFolder),
                
                // --- Buff effects (negative)
                new Effect("Freeze", "buff_freeze", BuffFolder),
                new Effect("Set on fire", "buff_fire", BuffFolder),
                new Effect("Daze", "buff_daze", BuffFolder),
                new Effect("Levitate", "buff_levitate", BuffFolder),
                new Effect("Confuse", "buff_confuse", BuffFolder),
                new Effect("Make invisible", "buff_invisible", BuffFolder),
                
                // --- Inventory effects
                new Effect("Inventory effects", "inventory_folder", ItemKind.Folder),
                new Effect("Drop item", "drop_item", InventoryFolder),
                new Effect("Explode inventory", "explode_inventory", InventoryFolder),
                new Effect("Reforge item", "item_prefix", InventoryFolder),
                new Effect("Boost coin drops", "boost_money", InventoryFolder),
                new Effect("Give pickaxe", "give_pickaxe", InventoryFolder),
                new Effect("Give sword", "give_sword", InventoryFolder),
                new Effect("Give armour", "give_armour", InventoryFolder),
                new Effect("Give healing potion", "give_healing_potion", InventoryFolder),
                new Effect("Give random potion", "give_potion", InventoryFolder),
                new Effect("Give random kite", "give_kite", InventoryFolder),
                
                // --- World effects
                new Effect("World effects", WorldFolder, ItemKind.Folder),
                new Effect("Use a sun dial", "sun_dial", WorldFolder),
                new Effect("Set to noon", "time_noon", WorldFolder),
                new Effect("Set to midnight", "time_midnight", WorldFolder),
                new Effect("Set to sunrise", "time_sunrise", WorldFolder),
                new Effect("Set to sunset", "time_sunset", WorldFolder),
                new Effect("Spawn structure", "spawn_structure", WorldFolder),
                new Effect("Random teleport", "random_teleport", WorldFolder),
                new Effect("Rainbow feet", "rainbow_feet", WorldFolder),
                new Effect("Spawn a Dungeon Guardian", "spawn_guardian", WorldFolder),
                new Effect("Spawn a fake Dungeon Guardian", "spawn_fake_guardian", WorldFolder),
                new Effect("Spawn critters", "spawn_critters", WorldFolder),
                // new Effect("Set the weather to Clear", "weather_clear", WorldFolder),
                // new Effect("Set the weather to Rainy", "weather_rain", WorldFolder),
                // new Effect("Set the weather to Stormy", "weather_storm", WorldFolder),
                // new Effect("Set the weather to Windy", "weather_windy", WorldFolder),
                
                // --- Boss effects
                new Effect("Boss effects", BossFolder, ItemKind.Folder),
                new Effect("Spawn a boss", "random_boss", BossFolder),
                new Effect("Spawn King Slime", "spawn_king_slime", BossFolder),
                new Effect("Spawn Eye of Cthulhu (Night)", "spawn_eye_of_cthulhu", BossFolder),
                new Effect("Spawn Eater of Worlds (Corruption)", "spawn_eater_of_worlds", BossFolder),
                new Effect("Spawn Brain of Cthulhu (Crimson)", "spawn_brain_of_cthulhu", BossFolder),
                new Effect("Spawn Queen Bee", "spawn_queen_bee", BossFolder),
                new Effect("Spawn Skeletron (Night)", "spawn_skeletron", BossFolder),
                new Effect("Spawn Deerclops", "spawn_deerclops", BossFolder),
                new Effect("Spawn Wall of Flesh (Hell)", "spawn_wall_of_flesh", BossFolder),
                new Effect("Spawn Queen Slime", "spawn_queen_slime", BossFolder),
                new Effect("Spawn Twins (Night)", "spawn_twins", BossFolder),
                new Effect("Spawn Destroyer (Night)", "spawn_destroyer", BossFolder),
                new Effect("Spawn Skeletron Prime (Night)", "spawn_skeletron_prime", BossFolder),
                new Effect("Spawn Plantera", "spawn_plantera", BossFolder),
                new Effect("Spawn Golem", "spawn_golem", BossFolder),
                new Effect("Spawn Duke Fishron", "spawn_duke_fishron", BossFolder),
                new Effect("Spawn Empress of Light", "spawn_empress_of_light", BossFolder),
                new Effect("Spawn Moon Lord", "spawn_moon_lord", BossFolder),
                
                // --- Screen effects
                new Effect("Screen effects", ScreenFolder, ItemKind.Folder),
                new Effect("Flip the screen", "flip_screen", ScreenFolder),
                new Effect("Drunk mode", "drunk_mode", ScreenFolder),
                new Effect("Zoom in", "zoom_in", ScreenFolder),
                new Effect("Zoom out", "zoom_out", ScreenFolder),
                new Effect("Wall of fish", "wall_of_fish", ScreenFolder),
                
                // --- Challenge effects
                new Effect("Challenges", ChallengesFolder, ItemKind.Folder),
                new Effect("Issue a challenge", "random_challenge", ChallengesFolder),
                new Effect("Start swim challenge", "swim_challenge", ChallengesFolder),
                new Effect("Start stand on block challenge", "stand_on_block_challenge", ChallengesFolder),
                new Effect("Start craft item challenge", "craft_item_challenge", ChallengesFolder),
                new Effect("Start sleep challenge", "sleep_challenge", ChallengesFolder),
            };
        }
    }
    
    #endregion
}