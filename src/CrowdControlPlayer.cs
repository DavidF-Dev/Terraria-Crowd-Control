using System;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Events

    /// <inheritdoc cref="PreUpdateBuffs"/>
    public event Action<CrowdControlPlayer> PreUpdateBuffsHook;

    #endregion

    #region Methods

    public override void PreUpdateBuffs()
    {
        PreUpdateBuffsHook?.Invoke(this);
        base.PreUpdateBuffs();
    }

    #endregion
}