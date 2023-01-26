using System.Collections.Generic;
using CrowdControlMod.Utilities;
using On.Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Main = Terraria.Main;

namespace CrowdControlMod.Features;

/// <summary>
///     Remove player tombstones if enabled in the config.
/// </summary>
public sealed class RemoveTombstoneFeature : IFeature
{
    #region Static Fields and Constants

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

    #region Static Methods

    private static int NewProjectile(Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float orig, IEntitySource spawnSource, float x, float y, float speedX, float speedY, int type, int damage, float knockback, int owner, float ai0, float ai1)
    {
        // Default spawning behaviour
        var proj = Main.projectile[orig.Invoke(spawnSource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1)];

        // Check if the projectile is a tombstone that should be removed straight away
        if (TombstoneProjectileIds.Contains(proj.type) &&
            ((NetUtils.IsSinglePlayer && proj.owner != -1 && Main.player[proj.owner].GetModPlayer<CrowdControlPlayer>().DisableTombstones) ||
             (NetUtils.IsServer && PlayerUtils.TryFindClosestPlayer(proj.Center, out var playerIndex, out _) && Main.player[playerIndex].GetModPlayer<CrowdControlPlayer>().DisableTombstones)))
        {
            // Kill the projectile (this will automatically let the clients know if on a server)
            proj.Kill();
        }

        // Make sure to return the projectile's index
        return proj.whoAmI;
    }

    #endregion

    #region Constructors

    public RemoveTombstoneFeature()
    {
        Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float += NewProjectile;
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
        Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float -= NewProjectile;
    }

    #endregion
}