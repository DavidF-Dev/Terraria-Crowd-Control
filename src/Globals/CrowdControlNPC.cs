using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once InconsistentNaming
[UsedImplicitly]
public sealed class CrowdControlNPC : GlobalNPC
{
    #region Delegates

    /// <inheritdoc cref="EditSpawnRate" />
    public delegate void EditSpawnRateDelegate(Player player, ref int spawnRate, ref int maxSpawns);

    #endregion

    #region Events
    
    /// <inheritdoc cref="EditSpawnRate" />
    [PublicAPI]
    public static event EditSpawnRateDelegate EditSpawnRateHook;

    #endregion

    #region Methods

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        EditSpawnRateHook?.Invoke(player, ref spawnRate, ref maxSpawns);
    }

    #endregion
}