/// <summary>
/// Crowd Control for Terraria. Pack for Crowd Control 2.
/// Author: David F Dev.
/// Source: https://github.com/DavidF-Dev/Terraria-Crowd-Control
/// </summary>

#define EXPOSE_CHALLENGES

using System;
using System.Collections.Generic;
using CrowdControl.Common;
using CrowdControl.Games.Packs;

public sealed class Terraria : SimpleTCPPack
{
    #region Static Fields and Constants
    
    private const string PlayerFolder = "Player effects";
    private const string BuffFolder = "Buff effects";
    private const string InventoryFolder = "Inventory effects";
    private const string WorldFolder = "World effects";
    private const string BossFolder = "Boss effects";
    private const string ScreenFolder = "Screen effects";
    private const string ChallengesFolder = "Challenges";

    #endregion
    
    #region Constructors
    
    public Terraria(UserRecord player, Func<CrowdControlBlock, bool> responseHandler, Action<object> statusUpdateHandler) : base(player, responseHandler, statusUpdateHandler)
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
    public override EffectList Effects
    {
        get
        {
            return new List<Effect>()
            {
                // --- Player effects
                new Effect("Kill", "kill_player") {Category = PlayerFolder, Price = 1500, Description = "Kill the streamer"},
                new Effect("Explode", "explode_player") {Category = PlayerFolder, Price = 1750, Description = "Explode the streamer with dynamite, also destroying nearby blocks"},
                new Effect("Heal", "heal_player") {Category = PlayerFolder, Price = 40, Description = "Fully heal the streamer"},
                new Effect("Damage", "damage_player") {Category = PlayerFolder, Price = 200, Description = "Severely damage the streamer such that they are left on only a trickle of health"},
                new Effect("God-mode", "god_mode_player") {Category = PlayerFolder, Price = 40, Description = "Temporarily provide the streamer with invulnerablity and infinite mana"},
                new Effect("Max health (+1 heart)", "increase_life") {Category = PlayerFolder, Price = 25, Description = "Increase the streamer's max health by 1 heart"},
                new Effect("Max health (-1 heart)", "decrease_life") {Category = PlayerFolder, Price = 75, Description = "Decrease the streamer's max health by 1 heart"},
                new Effect("Max mana (+1 star)", "increase_mana") {Category = PlayerFolder, Price = 25, Description = "Increase the streamer's max mana by 1 star"},
                new Effect("Max mana (-1 star)", "decrease_mana") {Category = PlayerFolder, Price = 75, Description = "Decrease the streamer's max mana by 1 star"},
                new Effect("Increase spawn rate", "increase_spawn_rate") {Category = PlayerFolder, Price = 60, Description = "Temporarily increase spawn-rates around the streamer"},
                new Effect("Infinite ammo & mana", "infinite_ammo") {Category = PlayerFolder, Price = 20, Description = "Temporarily provide the streamer with infinite ammo and increased ranged damage"},
                new Effect("Teleport to death point", "death_tp") {Category = PlayerFolder, Price = 200, Description = "Teleport the streamer to the last place they died"},
                new Effect("Give pet", "give_pet") {Category = PlayerFolder, Price = 5, Description = "Provide the streamer with a pet"},
                new Effect("Give light pet", "give_light_pet") {Category = PlayerFolder, Price = 10, Description = "Provide the streamer with a light-providing pet"},
                new Effect("Change gender", "change_gender") {Category = PlayerFolder, Price = 5, Description = "Change the streamer's gender"},
                new Effect("Force mount", "force_mount") {Category = PlayerFolder, Price = 50, Description = "Temporarily force the streamer to ride a mount"},
                new Effect("Shoot bombs", "shoot_bombs") {Category = PlayerFolder, Price = 600, Description = "Temporarily cause the streamer to drop and shoot bombs that are capable of destroying blocks"},
                new Effect("Shoot grenades", "shoot_grenades") {Category = PlayerFolder, Price = 200, Description = "Temporarily cause the streamer to drop and shoot grenades"},
                new Effect("Increase jump height", "jump_boost") {Category = PlayerFolder, Price = 40, Description = "Temporarily increase the streamer's jump height by a large amount"},
                new Effect("Increase movement speed", "run_boost") {Category = PlayerFolder, Price = 40, Description = "Temporarily increase the streamer's movement speed by a large amount"},
                new Effect("Slippery boots", "icy_feet") {Category = PlayerFolder, Price = 40, Description = "Temporarily make the ground very slippery under the streamer"},
                new Effect("Fling upwards", "fling_upwards") {Category = PlayerFolder, Price = 30, Description = "Fling the streamer upwards violently where there is space to do so"},
                new Effect("Play fart sound", "fart_sound") {Category = PlayerFolder, Price = 1, Description = "Play a fart sound in-game"},
                new Effect("Give hiccups", "hiccup") {Category = PlayerFolder, Price = 10, Description = "Temporarily give the streamer hiccups, causing them to hop"},
                
                // --- Buff effects (positive)
                new Effect("+ Boost survivability", "buff_survivability") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer survivability buffs"},
                new Effect("+ Boost health regeneration", "buff_regen") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer regeneration buffs"},
                new Effect("+ Provide light buffs", "buff_light") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer light-providing buffs"},
                new Effect("+ Help search for treasure", "buff_treasure") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer treasure-seeking buffs"},
                new Effect("+ Boost movement speed", "buff_movement") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer movement buffs"},
                new Effect("+ Provide lava immunity", "buff_obsidian_skin") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer immunity to lava"},
                new Effect("+ Boost mining speed", "buff_mining") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer increased mining speed"},
                new Effect("+ Provide swimming buffs", "buff_swim") {Category = BuffFolder, Price = 10, Description = "Temporarily give the streamer improved swimming ability"},

                // --- Buff effects (negative)
                new Effect("- Freeze", "buff_freeze") {Category = BuffFolder, Price = 20, Description = "Temporarily freeze the streamer in place"},
                new Effect("- Set on fire", "buff_fire") {Category = BuffFolder, Price = 30, Description = "Temporarily set the streamer on fire"},
                new Effect("- Daze", "buff_daze") {Category = BuffFolder, Price = 30, Description = "Temporarily daze the streamer, reducing their movement capabilities"},
                new Effect("- Levitate", "buff_levitate") {Category = BuffFolder, Price = 30, Description = "Temporarily cause the streamer to levitate"},
                new Effect("- Confuse", "buff_confuse") {Category = BuffFolder, Price = 30, Description = "Temporarily reverse the streamer's controls"},
                new Effect("- Make invisible", "buff_invisible") {Category = BuffFolder, Price = 30, Description = "Temporarily make the streamer invisible"},
                new Effect("- Blind", "buff_blind") {Category = BuffFolder, Price = 30, Description = "Temporarily decrease the streamer's vision"},
                new Effect("- Curse", "buff_curse") {Category = BuffFolder, Price = 20, Description = "Temporarily prevent the streamer from using any items"},
                new Effect("- Noclip", "buff_shimmer") {Category = BuffFolder, Price = 20, Description = "Temporarily cause the streamer to fall through blocks"},
                
                // --- Inventory effects
                new Effect("Drop item", "drop_item") {Category = InventoryFolder, Price = 15, Description = "Drop the streamer's held item"},
                new Effect("Explode inventory", "explode_inventory") {Category = InventoryFolder, Price = 100, Description = "Explode the streamer's inventory, dropping most items in random directions"},
                new Effect("Clear inventory", "clear_inventory") {Category = InventoryFolder, Price = 1250, Description = "Clear majority of the streamer's inventory, deleting the items"},
                new Effect("Shuffle inventory", "shuffle_inventory") {Category = InventoryFolder, Price = 75, Description = "Shuffle the streamer's inventory"},
                new Effect("Switch loadout", "switch_loadout") {Category = InventoryFolder, Price = 15, Description = "Randomly switch the streamer's inventory loadout to a random one"},
                new Effect("Cannot pickup items", "no_item_pickup") {Category = InventoryFolder, Price = 40, Description = "Temporarily prevent the streamer from picking up any items"},
                new Effect("Reforge item", "item_prefix") {Category = InventoryFolder, Price = 5, Description = "Reforge the streamer's held weapon or tool if possible"},
                new Effect("Boost coin drops", "boost_money") {Category = InventoryFolder, Price = 10, Description = "Temporarily increase the coin-drops from enemies"},
                new Effect("Give pickaxe", "give_pickaxe") {Category = InventoryFolder, Price = 25, Description = "Provide the streamer with a random pickaxe based on their world progression"},
                new Effect("Give sword", "give_sword") {Category = InventoryFolder, Price = 25, Description = "Provide the streamer with a random sword based on their world progression"},
                new Effect("Give armour", "give_armour") {Category = InventoryFolder, Price = 25, Description = "Provide the streamer with a random armour piece based on their world progression"},
                new Effect("Give healing potion", "give_healing_potion") {Category = InventoryFolder, Price = 5, Description = "Provide the streamer with a random healing potion based on their world progression"},
                new Effect("Give random potion", "give_potion") {Category = InventoryFolder, Price = 10, Description = "Provide the streamer with a random potion"},
                new Effect("Give food", "give_food") {Category = InventoryFolder, Price = 5, Description = "Provide the streamer with a random food item"},
                new Effect("Give random kite", "give_kite") {Category = InventoryFolder, Price = 5, Description = "Provide the streamer with a random kite"},

                // --- World effects
                new Effect("Use a sun dial", "sun_dial") {Category = WorldFolder, Price = 50, Description = "Fast-forward the time to morning"},
                new Effect("Use a moon dial", "moon_dial") {Category = WorldFolder, Price = 50, Description = "Fast-forward the time to dusk"},
                new Effect("Set to noon", "time_noon") {Category = WorldFolder, Price = 40, Description = "Set the time to noon"},
                new Effect("Set to midnight", "time_midnight") {Category = WorldFolder, Price = 40, Description = "Set the time to midnight"},
                new Effect("Set to sunrise", "time_sunrise") {Category = WorldFolder, Price = 40, Description = "Set the time to sunrise"},
                new Effect("Set to sunset", "time_sunset") {Category = WorldFolder, Price = 40, Description = "Set the time to sunset"},
                new Effect("Spawn structure", "spawn_structure") {Category = WorldFolder, Price = 100, Description = "Spawn a random structure around the streamer, such as a house"},
                new Effect("Spawn cobweb trap", "cobweb_trap") {Category = WorldFolder, Price = 40, Description = "Encase the streamer with cobwebs"},
                new Effect("Spawn sand trap", "sand_trap") {Category = WorldFolder, Price = 50, Description = "Encase the streamer in sand blocks"},
                new Effect("Spawn water trap", "water_trap") {Category = WorldFolder, Price = 40, Description = "Spawn a large pool of water"},
                new Effect("Spawn honey trap", "honey_trap") {Category = WorldFolder, Price = 40, Description = "Spawn a large pool of honey"},
                new Effect("Spawn lava trap", "lava_trap") {Category = WorldFolder, Price = 750, Description = "Spawn a large pool of lava"},
                new Effect("Random teleport", "random_teleport") {Category = WorldFolder, Price = 150, Description = "Randomly teleport the streamer to a different part of the world"},
                new Effect("Summon all NPCs", "summon_npcs") {Category = WorldFolder, Price = 100, Description = "Summon all alive NPCs to the streamer's position"},
                new Effect("Spawn random Town NPC", "spawn_town_npc") {Category = WorldFolder, Price = 20, Description = "Spawn a random Town NPC at the streamer's position"},
                new Effect("Rainbow feet", "rainbow_feet") {Category = WorldFolder, Price = 20, Description = "Temporarily cause the streamer to paint rainbows wherever they walk"},
                new Effect("Spawn a Dungeon Guardian", "spawn_guardian") {Category = WorldFolder, Price = 1000, Description = "Spawn a real Dungeon Guardian"},
                new Effect("Spawn a fake Dungeon Guardian", "spawn_fake_guardian") {Category = WorldFolder, Price = 5, Description = "Spawn a fake Dungeon Guardian to scare the streamer"},
                new Effect("Spawn critters", "spawn_critters") {Category = WorldFolder, Price = 5, Description = "Spawn a group of mostly harmless critters on the streamer"},
                new Effect("Rain Golden Slimes", "golden_slime_rain") {Category = WorldFolder, Price = 75, Description = "Rain Golden Slimes above the streamer, giving them an opportunity to gather coins"},
                new Effect("Set the weather to Clear", "weather_clear") {Category = WorldFolder, Price = 40, Description = "Set the weather to clear"},
                new Effect("Set the weather to Rainy", "weather_rain") {Category = WorldFolder, Price = 40, Description = "Set the weather to rainy"},
                new Effect("Set the weather to Stormy", "weather_storm") {Category = WorldFolder, Price = 40, Description = "Set the weather to stormy"},
                new Effect("Set the weather to Windy", "weather_windy") {Category = WorldFolder, Price = 40, Description = "Set the weather to windy"},
                new Effect("\"For the Worthy\" mode (Enable)", "enable_for_the_worthy") {Category = WorldFolder, Price = 1200, Description = "Enable \"For the Worthy\" mode in the streamer's world"},
                new Effect("\"For the Worthy\" mode (Disable)", "disable_for_the_worthy") {Category = WorldFolder, Price = 1000, Description = "Disable \"For the Worthy\" mode in the streamer's world"},
                new Effect("\"For the Worthy\" mode (Temporary)", "temp_for_the_worthy") {Category = WorldFolder, Price = 100, Description = "Temporarily enable \"For the Worthy\" mode in the streamer's world"},
                new Effect("\"Don't Starve\" mode (Enable)", "enable_the_constant") {Category = WorldFolder, Price = 1000, Description = "Enable \"Don't Starve\" mode in the streamer's world"},
                new Effect("\"Don't Starve\" mode (Disable)", "disable_the_constant") {Category = WorldFolder, Price = 800, Description = "Disable \"Don't Starve\" mode in the streamer's world"},
                new Effect("\"Don't Starve\" mode (Temporary)", "temp_the_constant") {Category = WorldFolder, Price = 100, Description = "Temporarily enable \"Don't Starve\" mode in the streamer's world"},
                new Effect("Switch soundtrack", "switch_soundtrack") {Category = WorldFolder, Price = 5, Description = "Switch between the Vanilla and Otherworld soundtrack"},
                new Effect("Shuffle sound effects", "shuffle_sfx") {Category = WorldFolder, Price = 10, Description = "Temporarily shuffle all sound effects"},
                new Effect("Mystery blocks", "mystery_blocks") {Category = WorldFolder, Price = 25, Description = "Temporarily hide the blocks on screen, such that their identity is unknown"},

                // --- Boss effects
                new Effect("Spawn a boss", "random_boss") {Category = BossFolder, Price = 500, Description = "Spawn a random boss on the streamer based on their world progression"},
                new Effect("Spawn King Slime", "spawn_king_slime") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn King Slime on the streamer"},
                new Effect("Spawn Eye of Cthulhu (Night)", "spawn_eye_of_cthulhu") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Eye of Cthulhu on the streamer (night only)"},
                new Effect("Spawn Eater of Worlds (Corruption)", "spawn_eater_of_worlds") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Eater of Worlds on the streamer (corruption only)"},
                new Effect("Spawn Brain of Cthulhu (Crimson)", "spawn_brain_of_cthulhu") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Brain of Cthulhu on the streamer (crimson only)"},
                new Effect("Spawn Queen Bee", "spawn_queen_bee") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Queen Bee on the streamer"},
                new Effect("Spawn Skeletron (Night)", "spawn_skeletron") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Skeletron on the streamer (night only)"},
                new Effect("Spawn Deerclops", "spawn_deerclops") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Deerclops on the streamer"},
                new Effect("Spawn Wall of Flesh (Hell)", "spawn_wall_of_flesh") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Wall of Flesh on the streamer (hell only)"},
                new Effect("Spawn Queen Slime", "spawn_queen_slime") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Queen Slime on the streamer"},
                new Effect("Spawn Twins (Night)", "spawn_twins") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn The Twins on the streamer (night only)"},
                new Effect("Spawn Destroyer (Night)", "spawn_destroyer") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn The Destroyer on the streamer (night only)"},
                new Effect("Spawn Skeletron Prime (Night)", "spawn_skeletron_prime") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Skeletron Prime on the streamer (night only)"},
                new Effect("Spawn Plantera", "spawn_plantera") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Plantera on the streamer"},
                new Effect("Spawn Golem", "spawn_golem") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Golem on the streamer"},
                new Effect("Spawn Duke Fishron", "spawn_duke_fishron") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Duke Fishron on the streamer"},
                new Effect("Spawn Empress of Light", "spawn_empress_of_light") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Empress of Light on the streamer"},
                new Effect("Spawn Moon Lord", "spawn_moon_lord") {Category = BossFolder, Inactive = true, Price = 1000, Description = "Spawn Moon Lord on the streamer"},

                // --- Screen effects
                new Effect("Flip the screen", "flip_screen") {Category = ScreenFolder, Price = 50, Description = "Temporarily flip the streamer's screen"},
                new Effect("Drunk mode", "drunk_mode") {Category = ScreenFolder, Price = 50, Description = "Temporarily cause the screen to become wobbly and distorted"},
                new Effect("Zoom in", "zoom_in") {Category = ScreenFolder, Price = 30, Description = "Temporarily zoom the screen in"},
                new Effect("Zoom out", "zoom_out") {Category = ScreenFolder, Price = 30, Description = "Temporarily zoom the screen out"},
                new Effect("Wall of fish", "wall_of_fish") {Category = ScreenFolder, Price = 20, Description = "Temporarily draw a wall of fish across the streamer's screen"},
                new Effect("Critter takeover", "critter_takeover") {Category = ScreenFolder, Price = 15, Description = "Temporarily alter NPC textures to look like harmless critters"},
                new Effect("Screen shake", "screen_shake") {Category = ScreenFolder, Price = 50, Description = "Temporarily shake the streamer's screen"},
                new Effect("Sniper mode", "sniper_mode") {Category = ScreenFolder, Price = 50, Description = "Temporarily cause the camera to follow the streamer's mouse"},
                new Effect("Monolith (Aether)", "monolith_shimmer") {Category = ScreenFolder, Price = 10, Description = "Temporarily enable the Aether monolith"},
                new Effect("Monolith (Void)", "monolith_moon_lord") {Category = ScreenFolder, Price = 10, Description = "Temporarily enable the Void monolith"},
                    
                // --- Challenge effects
                new Effect("Do-or-die challenge", "random_challenge")
                {
                    Category =
#if EXPOSE_CHALLENGES
                        ChallengesFolder,
#else
                        PlayerFolder,
#endif
                    Price = 20, Description = "Issue a random timed challenge to the streamer which they must complete, or they will die"
                },
#if EXPOSE_CHALLENGES
                new Effect("Start \"swim\" challenge", "swim_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to go for a swim"},
                new Effect("Start \"stand on block\" challenge", "stand_on_block_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to stand on a specific block"},
                new Effect("Start \"craft item\" challenge", "craft_item_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to craft a specific item"},
                new Effect("Start \"sleep\" challenge", "sleep_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to go to sleep"},
                new Effect("Start \"minecart\" challenge", "minecart_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to ride in a minecart"},
                new Effect("Start \"touch grass\" challenge", "touch_grass_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to touch a grass block"},
                new Effect("Start \"eat food\" challenge", "eat_food_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to consume any food"},
                new Effect("Start \"word puzzle\" challenge", "word_puzzle_challenge") {Category = ChallengesFolder, Inactive = true, Price = 20, Description = "Challenge the streamer to complete a small word puzzle"},
#endif
            };
        }
    }
    
    #endregion
}