using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlProjectile : GlobalProjectile
{
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(Projectile projectile, int timeLeft);

    /// <inheritdoc cref="OnSpawn" />
    public delegate void OnSpawnDelegate(Projectile projectile, IEntitySource source);

    #endregion

    #region Events

    /// <inheritdoc cref="Kill" />
    [PublicAPI]
    public static event KillDelegate KillHook;

    /// <inheritdoc cref="OnSpawn" />
    [PublicAPI]
    public static event OnSpawnDelegate OnSpawnHook;

    #endregion

    #region Methods

    public override void Kill(Projectile projectile, int timeLeft)
    {
        KillHook?.Invoke(projectile, timeLeft);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        OnSpawnHook?.Invoke(projectile, source);
    }

    #endregion
}