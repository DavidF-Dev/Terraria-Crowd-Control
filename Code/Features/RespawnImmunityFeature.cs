using System;
using System.Linq;
using CrowdControlMod.Config;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Features;

/// <summary>
///     Increase the respawn invincibility time if spawn is dangerous and spawn protection is enabled.
/// </summary>
public sealed class RespawnImmunityFeature : IFeature
{
    #region Static Methods

    private static void OnSpawn(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);
        if (context != PlayerSpawnContext.ReviveFromDeath || !CrowdControlConfig.GetInstance().EnableSpawnProtection)
        {
            // Ignore
            return;
        }

        var isLavaNearby = self.GetTilesAround(5).Any(t => Main.tile[t.x, t.y].LiquidType == LiquidID.Lava && Main.tile[t.x, t.y].LiquidAmount > 0);
        if (!isLavaNearby)
        {
            // Not dangerous enough!
            return;
        }

        // Make the player immune to damage and knockback temporarily
        self.immuneTime = Math.Max(self.immuneTime, 300);
        self.lavaTime = Math.Max(self.lavaTime, self.immuneTime);
        self.AddBuff(BuffID.ObsidianSkin, self.immuneTime);

        // ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Increased respawn immunity due to nearby danger!"), Color.White);
    }

    #endregion

    #region Methods

    public void SessionStarted()
    {
        On_Player.Spawn += OnSpawn;
    }

    public void SessionStopped()
    {
        On_Player.Spawn -= OnSpawn;
    }

    public void Dispose()
    {
    }

    #endregion
}