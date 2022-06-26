using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;

namespace CrowdControlMod.Spawnables;

/// <summary>
///     Spawnable Terraria entity.
/// </summary>
public interface ISpawnable<out T> where T : Entity
{
    #region Methods

    /// <summary>
    ///     Check if the spawnable can be spawned for the given player.
    /// </summary>
    [PublicAPI] [Pure]
    bool CanSpawn([NotNull] CrowdControlPlayer player);

    /// <summary>
    ///     Spawn the spawnable for the given player at the provided position (single-player or server-side).
    /// </summary>
    [PublicAPI] [NotNull]
    T Spawn([NotNull] CrowdControlPlayer player, Vector2 position);

    #endregion
}