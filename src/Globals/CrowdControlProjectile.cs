using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlProjectile : GlobalProjectile
{
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(Projectile projectile, int timeLeft);

    #endregion

    #region Events

    /// <inheritdoc cref="Kill" />
    public static event KillDelegate? KillHook;

    #endregion

    #region Methods

    public override void Kill(Projectile projectile, int timeLeft)
    {
        KillHook?.Invoke(projectile, timeLeft);
    }

    #endregion
}