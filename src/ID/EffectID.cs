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

    // --- Time effects
    public const string SetTimeNoon = "time_noon";
    public const string SetTimeMidnight = "time_midnight";
    public const string SetTimeSunrise = "time_sunrise";
    public const string SetTimeSunset = "time_sunset";

    // --- Buff effects
    public const string JumpBoost = "jump_boost";
    public const string RunBoost = "run_boost";
    public const string IcyFeet = "icy_feet";
    public const string BuffSurvivability = "buff_survivability";

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
    public const string SpawnStructure = "spawn_structure";
    public const string RandomTeleport = "random_teleport";

    // --- Screen effects
    public const string WallOfFish = "wall_of_fish";

    #endregion
}