﻿using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
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

    public IncreaseKnockbackEffect(int duration) : base(EffectID.IncreaseKnockback, duration, EffectSeverity.Neutral)
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

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SlapHand, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStopText(Id), EffectSeverity.Neutral);
    }

    #endregion
}