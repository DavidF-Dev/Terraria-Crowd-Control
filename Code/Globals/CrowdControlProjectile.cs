using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlProjectile : GlobalProjectile
{
    #region Delegates

    /// <inheritdoc cref="OnKill" />
    public delegate void KillDelegate(Projectile projectile, int timeLeft);

    #endregion

    #region Events

    /// <inheritdoc cref="OnKill" />
    public static event KillDelegate? KillHook;

    #endregion

    #region Methods

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        KillHook?.Invoke(projectile, timeLeft);
    }

    #endregion
}