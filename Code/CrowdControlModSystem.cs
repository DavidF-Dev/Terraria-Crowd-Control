using System;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlModSystem : ModSystem
{
    #region Delegates

    /// <inheritdoc cref="ModifyTransformMatrix" />
    public delegate void ModifyTransformMatrixDelegate(ref SpriteViewMatrix transform);

    #endregion

    #region Events

    /// <inheritdoc cref="PostDrawTiles" />
    public static event Action? PostDrawTilesHook;

    /// <inheritdoc cref="PostDrawInterface" />
    public static event Action<SpriteBatch>? PostDrawInterfaceHook;

    /// <inheritdoc cref="ModifyTransformMatrix" />
    public static event ModifyTransformMatrixDelegate? ModifyTransformMatrixHook;

    /// <inheritdoc cref="UpdateUI" />
    public static event Action<GameTime>? GameUpdateHook;

    /// <inheritdoc cref="PostUpdateNPCs" />
    public static event Action? PostUpdateNPCsHook;

    #endregion

    #region Methods

    public override void PreSaveAndQuit()
    {
        if (NetUtils.IsServer)
        {
            return;
        }

        if (CrowdControlMod.GetInstance().IsSessionActive)
        {
            // Ensure the start messages aren't shown again for this player
            CrowdControlMod.GetLocalPlayer().IsFirstTimeUser = false;
        }

        // Stop the crowd control session upon exiting a world
        CrowdControlMod.GetInstance().StopCrowdControlSession();
    }

    public override void PostDrawTiles()
    {
        PostDrawTilesHook?.Invoke();
    }

    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        PostDrawInterfaceHook?.Invoke(spriteBatch);
    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        ModifyTransformMatrixHook?.Invoke(ref transform);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        GameUpdateHook?.Invoke(gameTime);
    }

    public override void PostUpdateNPCs()
    {
        PostUpdateNPCsHook?.Invoke();
    }

    #endregion
}