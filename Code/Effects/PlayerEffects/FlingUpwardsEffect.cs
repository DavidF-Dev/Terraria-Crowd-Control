using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Cause the player to fling upwards into the air at high speed.
/// </summary>
public sealed class FlingUpwardsEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float FlingSpeed = 69f;

    #endregion

    #region Constructors

    public FlingUpwardsEffect() : base(EffectID.FlingUpwards, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.mount.Active)
        {
            // Ensure the player isn't riding a mount
            player.Player.mount.Dismount(player.Player);
            if (player.Player.mount.Active)
            {
                return CrowdControlResponseStatus.Retry;
            }
        }

        // Ensure the player has a suitable area to be flung upwards
        var pos = player.Player.position;
        const float checkHorExtraRadius = 0f;
        const float checkVerHeight = 16f * 25f;
        for (var x = pos.X - checkHorExtraRadius; x <= pos.X + checkHorExtraRadius + 16f; x += 16f)
        {
            for (var y = pos.Y; y > pos.Y - checkVerHeight; y -= 16f)
            {
                if (Collision.IsWorldPointSolid(new Vector2(x, y)))
                {
                    return CrowdControlResponseStatus.Retry;
                }
            }
        }

        // Make the player immune to damage and knockback temporarily
        player.Player.immune = true;
        player.Player.immuneNoBlink = true;
        player.Player.immuneTime = 60;

        // Fling the player upwards!
        player.Player.velocity.X /= 8f;
        player.Player.velocity.Y = -FlingSpeed;
        if (NetUtils.IsClient)
        {
            // Sync with the server
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.Player.whoAmI);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.KOCannon, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}