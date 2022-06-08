using System;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod;

[UsedImplicitly]
public sealed class CrowdControlPlayer : ModPlayer
{
    #region Events

    /// <inheritdoc cref="OnRespawn" />
    public event Action<CrowdControlPlayer> OnRespawnHook;

    /// <inheritdoc cref="PreUpdateBuffs" />
    public event Action<CrowdControlPlayer> PreUpdateBuffsHook; 

    #endregion

    #region Methods

    public override void OnEnterWorld(Player player)
    {
        if (TerrariaUtils.IsLocalPlayer(player))
        {
            // Start the crowd control session upon entering a world
            CrowdControlMod.GetInstance().StartCrowdControlSession(player.GetModPlayer<CrowdControlPlayer>());
        }
        
        base.OnEnterWorld(player);
    }

    public override void PlayerDisconnect(Player player)
    {
        if (TerrariaUtils.IsLocalPlayer(player))
        {
            // Stop the crowd control session upon disconnecting from a server
            CrowdControlMod.GetInstance().StopCrowdControlSession();
        }
        
        base.PlayerDisconnect(player);
    }

    public override void OnRespawn(Player player)
    {
        OnRespawnHook?.Invoke(player.GetModPlayer<CrowdControlPlayer>());
        base.OnRespawn(player);
    }

    public override void PreUpdateBuffs()
    {
        base.PreUpdateBuffs();
    }

    #endregion
}