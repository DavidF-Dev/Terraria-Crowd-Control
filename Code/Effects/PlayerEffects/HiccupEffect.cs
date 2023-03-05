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
    private const int MaxTime = 60 * 8;

    private static readonly SoundStyle[] HiccupSounds =
    {
        SoundID.NPCHit25, SoundID.NPCHit26, SoundID.NPCHit21, SoundID.NPCHit46,
        SoundID.Item87, SoundID.NPCHit27, SoundID.NPCDeath64, SoundID.NPCDeath41,
        SoundID.NPCDeath31
    };

    #endregion

    #region Fields

    private int _timer;

    #endregion

    #region Constructors

    public HiccupEffect(float duration) : base(EffectID.Hiccup, duration, EffectSeverity.Negative)
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
    }

    private void OnPostUpdateRunSpeeds()
    {
        if (_timer-- > 0)
        {
            return;
        }

        _timer = Main.rand.Next(MinTime, MaxTime);

        var player = GetLocalPlayer();
        if (player.Player.IsGrounded())
        {
            // On-ground hiccup
            player.Player.velocity.X += Main.rand.NextFloat(20f);
            player.Player.velocity.Y = -30f;
        }
        else
        {
            // Special in-air hiccup
            player.Player.velocity.X /= -2f;
            player.Player.velocity.Y = -30f;
        }

        // Play random 'hiccup' sound
        SoundEngine.PlaySound(HiccupSounds[Main.rand.Next(HiccupSounds.Length)], player.Player.Center);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.Player.whoAmI);
        }
    }

    #endregion
}