namespace CrowdControlMod.ID;

public static class EffectID
{
    #region Static Fields and Constants

    // --- Player effects
    public const string KillPlayer = "kill_player";
    public const string ExplodePlayer = "explode_player";
    public const string HealPlayer = "heal_player";
    public const string DamagePlayer = "damage_player";
    public const string GodModePlayer = "god_mode_player";
    public const string IncreaseMaxLife = "increase_life";
    public const string DecreaseMaxLife = "decrease_life";
    public const string IncreaseMaxMana = "increase_mana";
    public const string DecreaseMaxMana = "decrease_mana";
    public const string IncreaseSpawnRate = "increase_spawn_rate";
    public const string InfiniteAmmo = "infinite_ammo";
    public const string DeathTeleport = "death_tp";
    public const string GivePet = "give_pet";
    public const string GiveLightPet = "give_light_pet";
    public const string ChangeGender = "change_gender";
    public const string ForceMount = "force_mount";
    public const string ShootBombs = "shoot_bombs";
    public const string ShootGrenades = "shoot_grenades";
    public const string IncreaseKnockback = "increase_knockback";
    public const string JumpBoost = "jump_boost";
    public const string RunBoost = "run_boost";
    public const string IcyFeet = "icy_feet";
    public const string FlingUpwards = "fling_upwards";
    public const string FartSound = "fart_sound";
    public const string Hiccup = "hiccup";

    // --- Buff effects (positive)
    public const string BuffSurvivability = "buff_survivability";
    public const string BuffRegen = "buff_regen";
    public const string BuffLight = "buff_light";
    public const string BuffTreasure = "buff_treasure";
    public const string BuffMovement = "buff_movement";
    public const string BuffObsidianSkin = "buff_obsidian_skin";
    public const string BuffMiningSpeed = "buff_mining";
    public const string BuffSwimming = "buff_swim";

    // --- Buff effects (negative)
    public const string BuffFreeze = "buff_freeze";
    public const string BuffFire = "buff_fire";
    public const string BuffDaze = "buff_daze";
    public const string BuffLevitate = "buff_levitate";
    public const string BuffConfuse = "buff_confuse";
    public const string BuffInvisible = "buff_invisible";
    public const string BuffBlind = "buff_blind";
    public const string BuffCurse = "buff_curse";
    public const string BuffShimmer = "buff_shimmer";

    // --- Inventory effects
    public const string DropItem = "drop_item";
    public const string ExplodeInventory = "explode_inventory";
    public const string ClearInventory = "clear_inventory";
    public const string ShuffleInventory = "shuffle_inventory";
    public const string SwitchLoadout = "switch_loadout";
    public const string NoItemPickup = "no_item_pickup";
    public const string ShimmerItem = "shimmer_item";
    public const string ItemPull = "item_pull";
    public const string ItemPush = "item_push";
    public const string ReforgeItem = "item_prefix";
    public const string MoneyBoost = "boost_money";
    public const string GivePickaxe = "give_pickaxe";
    public const string GiveSword = "give_sword";
    public const string GiveYoyo = "give_yoyo";
    public const string GiveMagicWeapon = "give_magic_weapon";
    public const string GiveSummonWeapon = "give_summon_weapon";
    public const string GiveRangedWeapon = "give_ranged_weapon";
    public const string GiveArmour = "give_armour";
    public const string GiveAccessory = "give_accessory";
    public const string GiveVanity = "give_vanity";
    public const string GiveGrapple = "give_grapple";
    public const string GiveHealingPotion = "give_healing_potion";
    public const string GivePotion = "give_potion";
    public const string GiveFood = "give_food";
    public const string GiveKite = "give_kite";
    public const string SpawnPiggyBank = "spawn_piggy_bank";
    public const string SpawnVoidVault = "spawn_void_vault";

    // --- World effects
    public const string UseSunDial = "sun_dial";
    public const string UseMoonDial = "moon_dial";
    public const string SetTimeNoon = "time_noon";
    public const string SetTimeMidnight = "time_midnight";
    public const string SetTimeSunrise = "time_sunrise";
    public const string SetTimeSunset = "time_sunset";
    public const string SpawnStructure = "spawn_structure";
    public const string CobwebTrap = "cobweb_trap";
    public const string SandTrap = "sand_trap";
    public const string WaterTrap = "water_trap";
    public const string HoneyTrap = "honey_trap";
    public const string LavaTrap = "lava_trap";
    public const string ShimmerTrap = "shimmer_trap";
    public const string RandomTeleport = "random_teleport";
    public const string SummonNpcs = "summon_npcs";
    public const string SpawnTownNpc = "spawn_town_npc";
    public const string RainbowFeet = "rainbow_feet";
    public const string SpawnGuardian = "spawn_guardian";
    public const string SpawnFakeGuardian = "spawn_fake_guardian";
    public const string SpawnCritters = "spawn_critters";
    public const string GoldenSlimeRain = "golden_slime_rain";
    public const string SetWeatherClear = "weather_clear";
    public const string SetWeatherRain = "weather_rain";
    public const string SetWeatherStorm = "weather_storm";
    public const string SetWeatherWindy = "weather_windy";
    public const string EnableForTheWorthy = "enable_for_the_worthy";
    public const string DisableForTheWorthy = "disable_for_the_worthy";
    public const string TempForTheWorthy = "temp_for_the_worthy";
    public const string EnableTheConstant = "enable_the_constant";
    public const string DisableTheConstant = "disable_the_constant";
    public const string TempTheConstant = "temp_the_constant";
    public const string SwitchSoundtrack = "switch_soundtrack";
    public const string ShuffleSfx = "shuffle_sfx";
    public const string MysteryBlocks = "mystery_blocks";
    public const string HotFloor = "hot_floor";
    public const string NotTheBees = "not_the_bees";

    // --- Boss effects
    public const string RandomBoss = "random_boss";
    public const string SpawnKingSlime = "spawn_king_slime";
    public const string SpawnEyeOfCthulhu = "spawn_eye_of_cthulhu";
    public const string SpawnEaterOfWorlds = "spawn_eater_of_worlds";
    public const string SpawnBrainOfCthulhu = "spawn_brain_of_cthulhu";
    public const string SpawnQueenBee = "spawn_queen_bee";
    public const string SpawnSkeletron = "spawn_skeletron";
    public const string SpawnDeerclops = "spawn_deerclops";
    public const string SpawnWallOfFlesh = "spawn_wall_of_flesh";
    public const string SpawnQueenSlime = "spawn_queen_slime";
    public const string SpawnTwins = "spawn_twins";
    public const string SpawnDestroyer = "spawn_destroyer";
    public const string SpawnSkeletronPrime = "spawn_skeletron_prime";
    public const string SpawnPlantera = "spawn_plantera";
    public const string SpawnGolem = "spawn_golem";
    public const string SpawnDukeFishron = "spawn_duke_fishron";
    public const string SpawnEmpressOfLight = "spawn_empress_of_light";
    public const string SpawnMoonLord = "spawn_moon_lord";

    // --- Screen effects
    public const string FlipScreen = "flip_screen";
    public const string DrunkMode = "drunk_mode";
    public const string ZoomIn = "zoom_in";
    public const string ZoomOut = "zoom_out";
    public const string WallOfFish = "wall_of_fish";
    public const string CritterTakeover = "critter_takeover";
    public const string ScreenShake = "screen_shake";
    public const string SniperMode = "sniper_mode";
    public const string MonolithShimmer = "monolith_shimmer";
    public const string MonolithMoonLord = "monolith_moon_lord";

    // --- Challenge effects
    public const string RandomChallenge = "random_challenge";
    public const string SwimChallenge = "swim_challenge";
    public const string StandOnBlockChallenge = "stand_on_block_challenge";
    public const string CraftItemChallenge = "craft_item_challenge";
    public const string SleepChallenge = "sleep_challenge";
    public const string SitChallenge = "sit_challenge";
    public const string CatchCritterChallenge = "catch_critter_challenge";
    public const string MinecartChallenge = "minecart_challenge";
    public const string TouchGrassChallenge = "touch_grass_challenge";
    public const string EatFoodChallenge = "eat_food_challenge";
    public const string WordPuzzleChallenge = "word_puzzle_challenge";

    #endregion
}