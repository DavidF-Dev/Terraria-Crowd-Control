using System.Collections.Generic;
using CrowdControlMod.Globals;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CrowdControlMod.Features;

/// <summary>
///     Remove player tombstones if enabled in the config.
/// </summary>
public sealed class RemoveTombstoneFeature : IFeature
{
    #region Static Fields and Constants

    [NotNull]
    private static readonly HashSet<int> TombstoneProjectileIds = new()
    {
        ProjectileID.Tombstone,
        ProjectileID.GraveMarker,
        ProjectileID.CrossGraveMarker,
        ProjectileID.Headstone,
        ProjectileID.Gravestone,
        ProjectileID.Obelisk,
        ProjectileID.RichGravestone1,
        ProjectileID.RichGravestone2,
        ProjectileID.RichGravestone3,
        ProjectileID.RichGravestone4,
        ProjectileID.RichGravestone5
    };

    #endregion

    #region Constructors

    public RemoveTombstoneFeature()
    {
        CrowdControlProjectile.OnSpawnHook += OnSpawn;
    }

    #endregion

    #region Methods

    public void SessionStarted()
    {
    }

    public void SessionStopped()
    {
    }

    public void Dispose()
    {
        CrowdControlProjectile.OnSpawnHook -= OnSpawn;
    }

    private void OnSpawn(Projectile projectile, IEntitySource __)
    {
        if ((Main.netMode == NetmodeID.SinglePlayer && !CrowdControlMod.GetInstance().IsSessionActive) || Main.netMode == NetmodeID.MultiplayerClient || !TombstoneProjectileIds.Contains(projectile.type))
        {
            // Normal behaviour
            return;
        }

        // Check if the the tombstone should be disabled (in single-player or on server)
        var playerIndex = Main.netMode == NetmodeID.Server ? PlayerUtils.FindClosestPlayer(projectile.Center, out _) : projectile.owner;
        var player = Main.player[playerIndex].GetModPlayer<CrowdControlPlayer>();
        if (!player.DisableTombstones)
        {
            // Normal behaviour
            return;
        }

        // Destroy the projectile
        projectile.active = false;
        projectile.timeLeft = 1;
        projectile.Kill();
        if (Main.netMode != NetmodeID.Server)
        {
            return;
        }

        // Notify clients if we're running on the server
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
        NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, projectile.whoAmI);
        TerrariaUtils.WriteDebug($"Removed tombstone of '{player.Player.name}'");
    }

    #endregion
}