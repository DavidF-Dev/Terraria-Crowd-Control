using System;
using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;

namespace CrowdControlMod.Effects;

public sealed class BuffEffect : CrowdControlEffect
{
    #region Fields

    [NotNull]
    private readonly HashSet<int> _buffs;

    #endregion

    #region Constructors

    public BuffEffect([NotNull] string id, float duration, [NotNull] params int[] buffs) : base(id)
    {
        Duration = duration;
        _buffs = new HashSet<int>(buffs);
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        ApplyBuffs(Duration.GetValueOrDefault());
        CrowdControlMod.GetInstance().GetPlayer().OnRespawnHook += OnRespawn;
        return base.OnStart();
    }

    protected override void OnStop()
    {
        CrowdControlMod.GetInstance().GetPlayer().OnRespawnHook -= OnRespawn;
        base.OnStop();
    }

    private void OnRespawn(CrowdControlPlayer player)
    {
        if (!TerrariaUtils.IsLocalPlayer(player))
        {
            return;
        }

        ApplyBuffs(TimeLeft.GetValueOrDefault());
    }

    private void ApplyBuffs(float duration)
    {
        var player = CrowdControlMod.GetInstance().GetPlayer();

        // Add each buff for the provided duration
        foreach (var buffId in _buffs)
        {
            player.Player.AddBuff(buffId, (int)Math.Ceiling(60 * duration));
        }

        // TODO: Handle buff immunities
    }

    #endregion
}