using System;
using System.Collections.Generic;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlProjectile : GlobalProjectile
{
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(Projectile projectile, int timeLeft);

    #endregion

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

    public override void OnSpawn(Projectile projectile, IEntitySource source)
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
        if (Main.netMode == NetmodeID.Server)
        {
            // Notify clients if we're running on the server
            NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, projectile.whoAmI);
            TerrariaUtils.WriteDebug($"Removed tombstone of '{player.Player.name}'");
        }

        return;
    }

    #endregion
}