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
        if (!TombstoneProjectileIds.Contains(projectile.type))
        {
            // Normal behaviour if the projectile is not a tombstone
            return;
        }

        switch (Main.netMode)
        {
            case NetmodeID.SinglePlayer or NetmodeID.MultiplayerClient when CrowdControlMod.GetInstance().IsSessionActive:
            {
                var player = Main.player[projectile.owner].GetModPlayer<CrowdControlPlayer>();
                if (!player.DisableTombstones)
                {
                    // Normal behaviour
                    return;
                }

                // Destroy the projectile (local)
                projectile.active = false;
                projectile.Kill();
                break;
            }
            case NetmodeID.Server:
            {
                var player = Main.player[PlayerUtils.FindClosestPlayer(projectile.Center, out _)].GetModPlayer<CrowdControlPlayer>();
                if (!player.DisableTombstones)
                {
                    // Normal behaviour
                    return;
                }

                // Destroy the projectile (server)
                projectile.active = false;
                projectile.Kill();

                // Notify clients that the projectile should be killed
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
                NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, projectile.whoAmI);
                break;
            }
        }
    }

    #endregion
}