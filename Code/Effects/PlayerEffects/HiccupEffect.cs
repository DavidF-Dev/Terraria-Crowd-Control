using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Effect that causes the player to hiccup randomly over a duration.
/// </summary>
public sealed class HiccupEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int MinTime = 60;
    private const int MaxTime = 60 * 5;

    private static readonly SoundStyle[] HiccupSounds =
    {
        SoundID.NPCHit25, SoundID.NPCHit26, SoundID.NPCHit21, SoundID.NPCHit46,
        SoundID.Item87, SoundID.NPCHit27, SoundID.NPCDeath64, SoundID.NPCDeath41,
        SoundID.NPCDeath31
    };

    #endregion

    #region Fields

    public bool HicFart;
    private int _timer;

    #endregion

    #region Constructors

    public HiccupEffect(int duration) : base(EffectID.Hiccup, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook += OnPostUpdateRunSpeeds;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        GetLocalPlayer().PostUpdateRunSpeedsHook -= OnPostUpdateRunSpeeds;
        _timer = 0;
        HicFart = false;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Lemonade, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void OnUpdate(float delta)
    {
        if (_timer-- > 0)
        {
            return;
        }

        // Ensure player isn't grappling
        GetLocalPlayer().Player.RemoveAllGrapplingHooks();
    }

    private void OnPostUpdateRunSpeeds()
    {
        if (_timer > 0)
        {
            return;
        }

        _timer = Main.rand.Next(MinTime, MaxTime);

        var moreVolatile = CrowdControlMod.GetInstance().IsEffectActive(EffectID.IncreaseKnockback);
        var hiccupModifierX = moreVolatile ? 3f : 1f;
        var hiccupModifierY = moreVolatile ? 1.5f : 1f;

        var player = GetLocalPlayer();
        if (player.Player.IsGrounded())
        {
            // On-ground hiccup
            player.Player.velocity.X += Main.rand.NextFloat(6f, 12f) * Main.rand.NextFloatDirection() * hiccupModifierX;
            player.Player.velocity.Y = -Main.rand.NextFloat(8f, 12f) * hiccupModifierY;
        }
        else
        {
            // Determine hiccup x direction
            var dir = -MathF.Sign(player.Player.velocity.X);
            if (dir == 0)
            {
                dir = (int)Main.rand.NextFloatDirection();
            }
            
            // Special in-air hiccup
            player.Player.velocity.X += Main.rand.NextFloat(6f, 12f) * dir * hiccupModifierX;
            player.Player.velocity.Y = -Main.rand.NextFloat(8f, 12f) * hiccupModifierY;
        }

        // Play random 'hiccup' sound
        if (!HicFart)
        {
            SoundEngine.PlaySound(HiccupSounds[Main.rand.Next(HiccupSounds.Length)], player.Player.Center);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.Item16 with
            {
                PlayOnlyIfFocused = false,
                MaxInstances = int.MaxValue,
                Pitch = Main.rand.NextFloat(-0.90f, 0.05f)
            }, player.Player.Center);

            // Provide stinky buff for a short time
            player.Player.AddBuff(BuffID.Stinky, 120);

            // Fart cloud dust
            for (var i = 0; i < 6; i++)
            {
                Dust.NewDust(player.Player.position, 16 * 2, 16 * 3, DustID.FartInAJar);
            }
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.Player.whoAmI);
        }
    }

    #endregion
}