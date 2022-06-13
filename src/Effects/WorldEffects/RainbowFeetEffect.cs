using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

public sealed class RainbowFeetEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly byte[] PaintIds = {13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24};

    #endregion

    #region Fields

    private readonly HashSet<Tile> _paintedTiles = new();
    private int _paintIndex;

    #endregion

    #region Constructors

    public RainbowFeetEffect(float duration) : base(EffectID.RainbowFeet, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        _paintIndex = Main.rand.Next(PaintIds.Length);
        PlayerUtils.SetHairDye(player, ItemID.RainbowHairDye);

        player.PostUpdateHook += PostUpdate;
        CrowdControlProjectile.KillHook += ProjectileKill;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _paintIndex = 0;
        _paintedTiles.Clear();

        GetLocalPlayer().PostUpdateHook -= PostUpdate;
        CrowdControlProjectile.KillHook -= ProjectileKill;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(0, $"{viewerString} caused a rainbow to form underneath {playerString} for {durationString} seconds", Severity);
    }

    private void SetRainbowOnTile(int x, int y, bool trulyRandom)
    {
        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY || !Main.tile[x, y].HasTile)
        {
            // Ignore if invalid tile
            return;
        }

        byte colour;
        if (trulyRandom)
        {
            // Choose a truly random colour
            colour = PaintIds[Main.rand.Next(PaintIds.Length)];
        }
        else if (!_paintedTiles.Contains(Main.tile[x, y]))
        {
            // Choose the next colour in the rainbow sequence
            colour = PaintIds[_paintIndex];
            _paintIndex = (_paintIndex + 1) % PaintIds.Length;
            _paintedTiles.Add(Main.tile[x, y]);
        }
        else
        {
            // Tile should be ignored
            return;
        }

        // Paint the tile and broadcast to the server
        WorldGen.paintTile(x, y, colour, Main.netMode == NetmodeID.MultiplayerClient);
    }

    private void PostUpdate()
    {
        var player = GetLocalPlayer();
        if (player.Player.velocity.Y != 0f)
        {
            return;
        }

        // Set rainbow colours on the tiles below the player
        var tileX = player.TileX;
        var tileY = player.TileY;
        SetRainbowOnTile(tileX, tileY + 3, false);
        SetRainbowOnTile(tileX + 1, tileY + 3, false);
    }

    private void ProjectileKill(Projectile projectile, int timeLeft)
    {
        if (projectile.minion || projectile.sentry || projectile.bobber || projectile.owner != GetLocalPlayer().Player.whoAmI)
        {
            // Ignored
            return;
        }

        // Set rainbow colours on tiles around the projectile
        var tileX = (int)(projectile.Center.X / 16);
        var tileY = (int)(projectile.Center.Y / 16);
        const int range = 1;
        for (var x = tileX - range; x <= tileX + range; x++)
        {
            for (var y = tileY - range; y <= tileY + range; y++)
            {
                SetRainbowOnTile(x, y, true);
            }
        }
    }

    #endregion
}