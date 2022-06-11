namespace CrowdControlMod.ID;

// ReSharper disable once InconsistentNaming
public static class EffectID
{
    #region Static Fields and Constants

    // --- Player effects
    public const string KillPlayer = "kill_player";
    public const string ExplodePlayer = "explode_player";
    public const string HealPlayer = "heal_player";
    public const string GodModePlayer = "god_mode_player";
    public const string IncreaseSpawnRate = "increase_spawn_rate";
    
    // --- Time effects
    public const string SetTimeNoon = "time_noon";
    public const string SetTimeMidnight = "time_midnight";
    public const string SetTimeSunrise = "time_sunrise";
    public const string SetTimeSunset = "time_sunset";
    
    // --- Buff effects (positive -> negative)
    public const string BuffSurvivability = "buff_survivability";

    // --- World effects
    public const string SpawnStructure = "spawn_structure";
    
    // --- Screen effects
    public const string WallOfFish = "wall_of_fish";

    // --- Unimplemented
    public const string DamagePlayer = "damage_player";

    #endregion
}