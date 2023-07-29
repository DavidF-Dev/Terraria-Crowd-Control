using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.PlayerEffects;

public sealed class IncreaseKnockbackEffect : CrowdControlEffect
{
    #region Static Methods

    private static void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        modifiers.Knockback += Main.rand.NextFloat(3.5f, 5f);
        modifiers.KnockbackImmunityEffectiveness *= 0f;
    }

    private static void PostUpdateEquips()
    {
        var player = GetLocalPlayer();
        player.Player.noKnockback = false;
        player.Player.GetKnockback(DamageClass.Generic) += Main.rand.NextFloat(4.5f, 7f);
    }

    #endregion

    #region Constructors

    public IncreaseKnockbackEffect(float duration) : base(EffectID.IncreaseKnockback, duration, EffectSeverity.Neutral)
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
        player.ModifyHurtHook += ModifyHurt;
        player.PostUpdateEquipsHook += PostUpdateEquips;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.ModifyHurtHook -= ModifyHurt;
        player.PostUpdateEquipsHook -= PostUpdateEquips;
    }

    #endregion
}