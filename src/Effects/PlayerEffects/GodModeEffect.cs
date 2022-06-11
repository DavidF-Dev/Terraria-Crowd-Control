using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

public sealed class GodModeEffect : CrowdControlEffect
{
    #region Constructors

    public GodModeEffect() : base(EffectID.GodModePlayer, 20f, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (PlayerUtilities.IsInvincible(player))
        {
            // Ignore if the player ins invincible
            return CrowdControlResponseStatus.Retry;
        }

        player.CanBeHitByNpcHook += CanBeHitByNpc;
        player.CanBeHitByProjectileHook += CanBeHitByProjectile;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.CanBeHitByNpcHook -= CanBeHitByNpc;
        player.CanBeHitByProjectileHook -= CanBeHitByProjectile;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.EoCShield, $"{viewerString} made {playerString} invulnerable to enemy attacks for {durationString} seconds", Severity);
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