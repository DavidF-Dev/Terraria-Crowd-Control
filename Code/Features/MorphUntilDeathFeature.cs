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
        if (Main.netMode == NetmodeID.Server || morph == MorphID.None || Main.LocalPlayer.GetMorph() == morph)
        {
            return;
        }

        if (IsEnabled)
        {
            Disable();
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

    /// <summary>
    ///     Toggle the provided morph for the local player.
    /// </summary>
    public void Toggle(byte morph)
    {
        if (Main.LocalPlayer.GetMorph() != morph)
        {
            Enable(morph);
        }
        else
        {
            Disable();
        }
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