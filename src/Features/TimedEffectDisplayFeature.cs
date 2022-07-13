using System.Linq;
using CrowdControlMod.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace CrowdControlMod.Features;

/// <summary>
///     Advanced feature that shows the duration left on active timed effects on the screen.
/// </summary>
public sealed class TimedEffectDisplayFeature : IFeature
{
    #region Static Methods

    private static void PostDrawInterface(SpriteBatch spriteBatch)
    {
        if (!CrowdControlConfig.GetInstance().DeveloperMode)
        {
            // Ignore if developer mode is disabled in the mod configuration
            return;
        }
        
        const float padding = 18f;
        var pos = new Vector2(Main.screenWidth / 2f + 175f, Main.screenHeight - padding + 8f);

        // Iterate over the active effects and draw per line
        foreach (var effect in CrowdControlMod.GetInstance().GetEffects(true).Where(x => x.TimeLeft > 0f))
        {
            Utils.DrawBorderString(
                spriteBatch,
                $"{effect.Id}: {effect.TimeLeft:0.0}s",
                pos, Color.White, 0.8f, 0f, 0.5f);
            
            // Increase padding per line
            pos.Y -= padding;
        }
    }

    #endregion

    #region Methods

    public void SessionStarted()
    {
        CrowdControlModSystem.PostDrawInterfaceHook += PostDrawInterface;
    }

    public void SessionStopped()
    {
        CrowdControlModSystem.PostDrawInterfaceHook -= PostDrawInterface;
    }

    public void Dispose()
    {
    }

    #endregion
}