using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlTile : GlobalTile
{
    #region Delegates

    /// <inheritdoc cref="PreDraw" />
    public delegate bool PreDrawDelegate(int i, int j, int type, SpriteBatch spriteBatch);

    /// <inheritdoc cref="KillTile" />
    public delegate void KillTileDelegate(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);

    #endregion

    #region Events

    /// <inheritdoc cref="PreDraw" />
    public static event PreDrawDelegate? PreDrawHook;

    /// <inheritdoc cref="KillTile" />
    public static event KillTileDelegate? KillTileHook;

    #endregion

    #region Methods

    public override bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        return PreDrawHook?.Invoke(i, j, type, spriteBatch) ?? base.PreDraw(i, j, type, spriteBatch);
    }

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        KillTileHook?.Invoke(i, j, type, ref fail, ref effectOnly, ref noItem);
    }

    #endregion
}