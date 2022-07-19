using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Make the player invulnerable to all damage for a short duration.
/// </summary>
public sealed class GodModeEffect : CrowdControlEffect
{
    #region Constructors

    public GodModeEffect(float duration) : base(EffectID.GodModePlayer, duration, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.ItemRing;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.IsInvincible())
        {
            // Ignore if the player is invincible
            return CrowdControlResponseStatus.Retry;
        }

        player.CanBeHitByNPCHook += CanBeHitByNpc;
        player.CanBeHitByProjectileHook += CanBeHitByProjectile;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.CanBeHitByNPCHook -= CanBeHitByNpc;
        player.CanBeHitByProjectileHook -= CanBeHitByProjectile;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.TorchGodsFavor, $"{viewerString} made {playerString} invulnerable to enemy attacks for {durationString} seconds", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "No longer invulnerable to enemy attacks", EffectSeverity.Neutral);
    }

    private bool CanBeHitByNpc(NPC _, ref int __)
    {
        return !IsActive;
    }

    private bool CanBeHitByProjectile(Projectile _)
    {
        return !IsActive;
    }

    #endregion
}