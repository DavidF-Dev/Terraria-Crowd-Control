using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlProjectile : GlobalProjectile
{
    private static readonly HashSet<int> TombstoneProjectileIds = new ()
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
    
    #region Delegates

    /// <inheritdoc cref="Kill" />
    public delegate void KillDelegate(Projectile projectile, int timeLeft);

    #endregion

    #region Events

    /// <inheritdoc cref="Kill" />
    [PublicAPI]
    public static event KillDelegate KillHook;

    /// <inheritdoc cref="PreAI" />
    [PublicAPI]
    public static event Func<Projectile, bool> PreAiHook;

    #endregion

    #region Methods

    public override void Kill(Projectile projectile, int timeLeft)
    {
        KillHook?.Invoke(projectile, timeLeft);
    }

    public override bool PreAI(Projectile projectile)
    {
        if (!CrowdControlMod.GetInstance().IsSessionActive || Main.netMode == NetmodeID.MultiplayerClient || !TombstoneProjectileIds.Contains(projectile.type))
        {
            // Normal behaviour
            return PreAiHook?.Invoke(projectile) ?? base.PreAI(projectile);
        }

        // Check if the the tombstone should be disabled (in single-player or on server)
        var player = Main.player[projectile.owner].GetModPlayer<CrowdControlPlayer>();
        if (!player.DisableTombstones)
        {
            // Normal behaviour
            return PreAiHook?.Invoke(projectile) ?? base.PreAI(projectile);
        }

        // Destroy the projectile
        projectile.active = false;
        if (Main.netMode == NetmodeID.Server)
        {
            // Notify clients if we're running on the server
            NetMessage.SendData(MessageID.SyncNPC, -1, player.Player.whoAmI, null, projectile.whoAmI);
        }
            
        return false;
    }

    #endregion
}