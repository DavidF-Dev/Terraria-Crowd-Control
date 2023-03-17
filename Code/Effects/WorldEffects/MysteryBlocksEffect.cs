using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Temporarily hide all tiles, except a few around the player.
/// </summary>
public sealed class MysteryBlocksEffect : CrowdControlEffect
{
    #region Static Methods

    private static bool PreDrawTile(int i, int j, int type, SpriteBatch spriteBatch)
    {
        if (!Main.tile[i, j].HasTile)
        {
            // Ignore empty tiles
            return true;
        }

        if (ShouldReveal(i, j))
        {
            // Ignore if the tile SHOULD be revealed
            return true;
        }

        // Draw coloured square instead
        // https://github.com/tModLoader/tModLoader/blob/1.4/ExampleMod/Content/Tiles/ExampleAnimatedTile.cs
        var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
            new Rectangle(0, 0, 1, 1),
            Lighting.GetColor(i, j).MultiplyRGB(Main.hslToRgb((Main.GlobalTimeWrappedHourly * 0.15f + (i + j + type) / 2000f) % 1, 0.80f, 0.56f)),
            0f,
            default,
            16f,
            SpriteEffects.None,
            0);

        return false;
    }

    private static bool PreDrawWall(int i, int j, int type, SpriteBatch spriteBatch)
    {
        if (Main.tile[i, j].WallType == 0)
        {
            // Ignore empty walls
            return true;
        }

        if (ShouldReveal(i, j))
        {
            // Ignore if the wall SHOULD be revealed
            return true;
        }

        // Draw coloured square instead
        var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
            new Rectangle(0, 0, 1, 1),
            Lighting.GetColor(i, j).MultiplyRGB(Main.hslToRgb((Main.GlobalTimeWrappedHourly * 0.15f + (i + j + type) / 2000f) % 1, 0.72f, 0.38f)),
            0f,
            default,
            16f,
            SpriteEffects.None,
            0);

        return false;
    }

    private static bool ShouldReveal(int i, int j)
    {
        // Check if the tile is within the player reveal range
        const int revealRange = 2;
        return Main.player
            .Where(x => x.active)
            .Select(player => player.GetModPlayer<CrowdControlPlayer>())
            .Any(moddedPlayer =>
            {
                var tile = moddedPlayer.Player.position.ToTileCoordinates();
                return i >= tile.X - revealRange && i <= tile.X + 1 + revealRange &&
                       j >= tile.Y - revealRange && j <= tile.Y + 2 + revealRange;
            });
    }

    #endregion

    #region Constructors

    public MysteryBlocksEffect(float duration) : base(EffectID.MysteryBlocks, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        CrowdControlTile.PreDrawHook += PreDrawTile;
        CrowdControlWall.PreDrawHook += PreDrawWall;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        CrowdControlTile.PreDrawHook -= PreDrawTile;
        CrowdControlWall.PreDrawHook -= PreDrawWall;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Actuator, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}