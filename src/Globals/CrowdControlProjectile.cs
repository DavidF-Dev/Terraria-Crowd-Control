using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlProjectile : GlobalProjectile
{
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(Projectile projectile, int timeLeft);

    #endregion

    #region Events

    /// <inheritdoc cref="Kill" />
    [PublicAPI]
    public static event KillDelegate KillHook;

    #endregion

    #region Methods

    public override void Kill(Projectile projectile, int timeLeft)
    {
        KillHook?.Invoke(projectile, timeLeft);
    }

    #endregion
}