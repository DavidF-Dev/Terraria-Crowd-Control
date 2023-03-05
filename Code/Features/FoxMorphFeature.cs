using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Features;

/// <summary>
///     Feature that turns the player into a fox until death. This is not synced in multiplayer.
/// </summary>
public class FoxMorphFeature : IFeature
{
    #region Static Methods

    private static void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.whoAmI != Main.myPlayer)
        {
            // Only the local player should be affected
            return;
        }

        // Disable all layers except those we care about
        foreach (var layer in PlayerDrawLayerLoader.Layers)
        {
            if (layer != ModContent.GetInstance<FoxMorphLayer>() && layer != PlayerDrawLayers.HeldItem)
            {
                layer.Hide();
            }
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Local player is currently a fox morph until death.
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
    ///     Enable the fox morph for the local player until death.
    /// </summary>
    public void Enable()
    {
        if (IsEnabled || Main.netMode == NetmodeID.Server)
        {
            return;
        }

        CrowdControlModSystem.GameUpdateHook += OnGameUpdate;
        CrowdControlMod.GetLocalPlayer().HideDrawLayersHook += HideDrawLayers;
        IsEnabled = true;
    }

    /// <summary>
    ///     Disable the fox morph for the local player.
    /// </summary>
    public void Disable()
    {
        if (!IsEnabled)
        {
            return;
        }

        CrowdControlModSystem.GameUpdateHook -= OnGameUpdate;
        CrowdControlMod.GetLocalPlayer().HideDrawLayersHook -= HideDrawLayers;
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

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class FoxMorphLayer : PlayerDrawLayer
    {
        #region Methods

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.whoAmI == Main.myPlayer && (CrowdControlMod.GetInstance().GetFeature<FoxMorphFeature>(FeatureID.FoxMorph)?.IsEnabled ?? false);
        }

        public override Position GetDefaultPosition()
        {
            return new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.HeldItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            // Calculate draw position
            var position = drawInfo.Center - Main.screenPosition;
            position.X = (int)position.X;
            position.Y = (int)position.Y - 4;

            // Get texture
            Main.instance.LoadProjectile(ProjectileID.FennecFox);
            var tex = TextureAssets.Projectile[ProjectileID.FennecFox].Value;
            const int totalFrames = 17;

            // Determine current frame / animation
            var currentFrame = 0;
            if (drawInfo.drawPlayer.velocity.Y != 0f)
            {
                currentFrame = 11 + (int)(Main.GameUpdateCount / 2) % 5;
            }
            else if (drawInfo.drawPlayer.velocity.X != 0f)
            {
                currentFrame = 4 + (int)(Main.GameUpdateCount / 2) % 7;
            }

            drawInfo.DrawDataCache.Add(new DrawData(
                tex,
                position,
                new Rectangle(0, currentFrame * (tex.Height / totalFrames), tex.Width, tex.Height / totalFrames),
                new Color(0, 102, 255, 255),
                drawInfo.rotation,
                new Vector2(tex.Width, tex.Height / (float)totalFrames) * 0.5f,
                1.5f,
                drawInfo.drawPlayer.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            ));
        }

        #endregion
    }

    #endregion
}