using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Paint the tiles underneath the player's feet rainbow colours for a short duration.
///     Projectiles shot by the player will also paint tiles upon de-spawning.
/// </summary>
public sealed class RainbowFeetEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int MaxTrackedPaintedTiles = 3;

    private static readonly byte[] PaintIds = {13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24};

    #endregion

    #region Fields

    private readonly List<Tile> _paintedTiles = new(MaxTrackedPaintedTiles);

    private int _paintIndex;

    private int _trackedPaintedTilesCounter;

    #endregion

    #region Constructors

    public RainbowFeetEffect(float duration) : base(EffectID.RainbowFeet, duration, EffectSeverity.Neutral)
    {
        CrowdControlNPC.OnKillHook += NPCKill;
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.WeatherRainbow;

    #endregion

    #region Methods

    protected override void OnDisposed()
    {
        CrowdControlNPC.OnKillHook -= NPCKill;
    }

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        _paintIndex = Main.rand.Next(PaintIds.Length);
        player.Player.SetHairDye(ItemID.RainbowHairDye);

        player.PostUpdateHook += PostUpdate;
        CrowdControlProjectile.KillHook += ProjectileKill;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _paintedTiles.Clear();
        _paintIndex = 0;
        _trackedPaintedTilesCounter = 0;

        GetLocalPlayer().PostUpdateHook -= PostUpdate;
        CrowdControlProjectile.KillHook -= ProjectileKill;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private void SetRainbowOnTile(int x, int y, bool trulyRandom)
    {
        if (!WorldUtils.IsTileSolid(x, y))
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
            if (_trackedPaintedTilesCounter == MaxTrackedPaintedTiles)
            {
                // If collection is full, remove the first element
                _paintedTiles.RemoveAt(0);
            }
            else
            {
                _trackedPaintedTilesCounter++;
            }
        }
        else
        {
            // Tile should be ignored
            return;
        }

        // Paint the tile and broadcast to the server
        WorldGen.paintTile(x, y, colour, NetUtils.IsClient);
    }

    private void PostUpdate()
    {
        var player = GetLocalPlayer();
        if (player.Player.velocity.Y != 0f)
        {
            return;
        }

        // Set rainbow colours on the tiles below the player
        var tile = player.Player.position.ToTileCoordinates();
        SetRainbowOnTile(tile.X, tile.Y + 3, false);
        SetRainbowOnTile(tile.X + 1, tile.Y + 3, false);
    }

    private void ProjectileKill(Projectile projectile, int timeLeft)
    {
        if (projectile.type != ProjectileID.PainterPaintball && (projectile.minion || projectile.sentry || projectile.bobber || projectile.owner != GetLocalPlayer().Player.whoAmI))
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

    private void NPCKill(NPC npc)
    {
        if ((NetUtils.IsSinglePlayer && !IsActive) ||
            (NetUtils.IsServer && !IsActiveOnServer()) ||
            NetUtils.IsClient)
        {
            // Ignore
            return;
        }

        // Spawn paint projectile in random directions from the dead npc's center
        const int minCount = 5;
        const int maxCount = 8;
        const float minSpeed = 2f;
        const float maxSpeed = 5f;
        var count = Main.rand.Next(minCount, maxCount);
        for (var i = 0; i < count; i++)
        {
            var index = Projectile.NewProjectile(
                null, npc.Center,
                Main.rand.NextVector2Unit() * Main.rand.NextFloat(minSpeed, maxSpeed),
                ProjectileID.PainterPaintball,
                npc.lifeMax / 6, 3f,
                Main.myPlayer,
                ai1: Main.rand.Next(12) / 6f);
            if (NetUtils.IsServer)
            {
                // Spawn for clients
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }
    }

    #endregion
}