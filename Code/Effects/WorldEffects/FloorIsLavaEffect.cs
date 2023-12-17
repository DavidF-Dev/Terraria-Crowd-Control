using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Temporarily heat up the ground, dealing burning damage to the player.
/// </summary>
public sealed class FloorIsLavaEffect : CrowdControlEffect
{
    #region Static Methods

    private static void PreUpdateBuffs()
    {
        var player = GetLocalPlayer();

        // TODO: Check multiplayer

        // Burn the player if standing on hot ground or in hot liquid
        if (!player.Player.IsGrounded(false) && !player.Player.IsInLiquid(LiquidID.Water) && !player.Player.IsInLiquid(LiquidID.Honey))
        {
            return;
        }

        // Effect when starting to burn
        if (!IsOnFire(player.Player))
        {
            SoundEngine.PlaySound(SoundID.DD2_GoblinScream with {MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew}, player.Player.Center);
            //player.Player.Emote(EmoteID.MiscFire);   
        }

        player.Player.buffImmune[BuffID.Burning] = false;
        player.Player.AddBuff(BuffID.Burning, 2);

        /*player.Player.buffImmune[BuffID.Dazed] = false;
        player.Player.AddBuff(BuffID.Dazed, 2);*/
    }

    private static void PostUpdateEquips()
    {
        var player = GetLocalPlayer();
        if (IsOnFire(player.Player) && player.Player.IsGrounded())
        {
            player.Player.DoBootsEffect(player.Player.DoBootsEffect_PlaceFlamesOnTile);
        }
    }

    private static bool IsOnFire(Player player)
    {
        return player.HasBuff(BuffID.Burning) || player.HasBuff(BuffID.OnFire) || player.HasBuff(BuffID.OnFire3);
    }

    #endregion

    #region Constructors

    public FloorIsLavaEffect(int duration) : base(EffectID.HotFloor, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    protected override int StartEmote => EmoteID.EmoteFear;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.statLife < 50 || player.Player.IsInLiquid() || IsOnFire(player.Player))
        {
            return CrowdControlResponseStatus.Retry;
        }

        player.PreUpdateBuffsHook += PreUpdateBuffs;
        player.PostUpdateEquipsHook += PostUpdateEquips;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.PreUpdateBuffsHook -= PreUpdateBuffs;
        player.PostUpdateEquipsHook -= PostUpdateEquips;
    }

    protected override void OnUpdate(float delta)
    {
        // Effects
        var player = GetLocalPlayer();
        var tile = player.Player.Center.ToTileCoordinates();
        const int range = 30;
        for (var x = tile.X - range; x < tile.X + range; x++)
        {
            for (var y = tile.Y - range; y < tile.Y + range; y++)
            {
                if (x < 0 || x >= Main.tile.Width || y < 1 || y >= Main.tile.Height)
                {
                    continue;
                }

                // Hot ground effect
                if (Main.rand.NextBool(9) &&
                    Main.tile[x, y - 1].LiquidAmount == 0 &&
                    Collision.IsWorldPointSolid(new Vector2(x * 16, y * 16), true) &&
                    !Collision.IsWorldPointSolid(new Vector2(x * 16, (y - 1) * 16), true))
                {
                    var dustType = Main.rand.NextFromList(DustID.Torch, DustID.RedTorch, DustID.CrimsonTorch);
                    var dust = Dust.NewDust(new Vector2(x * 16, y * 16 - 4), 16, 1, dustType, SpeedY: -0.65f, Scale: Main.rand.NextFloat(0.15f, 1.25f));
                    Main.dust[dust].noGravity = true;
                }

                // Hot liquid effect
                if (Main.rand.NextBool(18) &&
                    Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType is LiquidID.Water or LiquidID.Honey)
                {
                    var dust = Dust.NewDust(new Vector2(x * 16, y * 16), 16, 16, DustID.Cloud, Scale: Main.rand.NextFloat(0.5f, 0.8f));
                    Main.dust[dust].noGravity = true;
                }

                // Hot liquid surface effect
                if (Main.rand.NextBool(8) &&
                    Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType is LiquidID.Water or LiquidID.Honey &&
                    Main.tile[x, y - 1].LiquidAmount == 0 &&
                    !Collision.IsWorldPointSolid(new Vector2(x * 16, (y - 1) * 16), true))
                {
                    Dust.NewDust(new Vector2(x * 16 + 8, 16), 16, 1, DustID.Flare, Main.rand.NextFloatDirection() * 1, -0.5f, Scale: Main.rand.NextFloat(0.9f, 1f));
                }
            }
        }
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.HellfireTreads, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}