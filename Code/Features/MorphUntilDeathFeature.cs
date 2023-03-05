using CrowdControlMod.Code.Utilities;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Features;

/// <summary>
///     Feature that turns the player into a morph until death.
/// </summary>
public class MorphUntilDeathFeature : IFeature
{
    #region Properties

    /// <summary>
    ///     Local player is currently a morph until death.
    /// </summary>
    public bool IsEnabled { get; private set; }

    #endregion

    #region Methods

    public void SessionStarted()
    {
    }

    public void SessionStopped()
    {
        Disable();
    }

    public void Dispose()
    {
        Disable();
    }

    /// <summary>
    ///     Enable the morph for the local player until death.
    /// </summary>
    public void Enable(byte morph)
    {
        if (IsEnabled || Main.netMode == NetmodeID.Server || morph == MorphID.None)
        {
            return;
        }

        IsEnabled = true;
        Main.LocalPlayer.SetMorph(morph);
        CrowdControlModSystem.GameUpdateHook += OnGameUpdate;
    }

    /// <summary>
    ///     Disable the morph for the local player.
    /// </summary>
    public void Disable()
    {
        if (!IsEnabled)
        {
            return;
        }

        CrowdControlModSystem.GameUpdateHook -= OnGameUpdate;
        Main.LocalPlayer.SetMorph(MorphID.None);
        IsEnabled = false;
    }

    private void OnGameUpdate(GameTime gameTime)
    {
        // Disable after death
        if (Main.LocalPlayer.dead)
        {
            Disable();
        }
    }

    #endregion
}