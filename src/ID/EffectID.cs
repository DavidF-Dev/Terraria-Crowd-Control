namespace CrowdControlMod.ID;

// ReSharper disable once InconsistentNaming
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
    public const string ForceMount = "force_mount";
    public const string GivePet = "give_pet";
    public const string GiveLightPet = "give_light_pet";
    public const string ChangeGender = "change_gender";
    public const string ShootBombs = "shoot_bombs";
    public const string ShootGrenades = "shoot_grenades";
    public const string JumpBoost = "jump_boost";
    public const string RunBoost = "run_boost";
    public const string IcyFeet = "icy_feet";
    public const string ZoomIn = "zoom_in";
    public const string ZoomOut = "zoom_out";
    public const string DeathTeleport = "death_tp";

    // --- Buff effects (positive)
    public const string BuffSurvivability = "buff_survivability";
    public const string BuffRegen = "buff_regen";
    public const string BuffLight = "buff_light";
    public const string BuffTreasure = "buff_treasure";
    public const string BuffMovement = "buff_movement";

    // --- Buff effects (negative)
    public const string BuffFreeze = "buff_freeze";
    public const string BuffFire = "buff_fire";
    public const string BuffDaze = "buff_daze";
    public const string BuffLevitate = "buff_levitate";
    public const string BuffConfuse = "buff_confuse";
    public const string BuffInvisible = "buff_invisible";
    public const string BuffBlind = "buff_blind";

    // --- Inventory effects
    public const string DropItem = "drop_item";
    public const string ExplodeInventory = "explode_inventory";
    public const string ReforgeItem = "item_prefix";
    public const string MoneyBoost = "boost_money";
    public const string GivePickaxe = "give_pickaxe";
    public const string GiveSword = "give_sword";
    public const string GiveArmour = "give_armour";
    public const string GiveHealingPotion = "give_healing_potion";
    public const string GivePotion = "give_potion";

    // --- World effects
    public const string SetTimeNoon = "time_noon";
    public const string SetTimeMidnight = "time_midnight";
    public const string SetTimeSunrise = "time_sunrise";
    public const string SetTimeSunset = "time_sunset";
    public const string SpawnStructure = "spawn_structure";
    public const string RandomTeleport = "random_teleport";
    public const string RainbowFeet = "rainbow_feet";
    public const string SpawnGuardian = "spawn_guardian";
    public const string SpawnFakeGuardian = "spawn_fake_guardian";
    public const string SpawnKingSlime = "spawn_king_slime";
    public const string SpawnCritters = "spawn_critters";
    public const string WallOfFish = "wall_of_fish";
    
    // --- Challenge effects
    public const string SwimChallenge = "swim_challenge";

    #endregion
}