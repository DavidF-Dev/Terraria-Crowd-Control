using System;
using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria.ID;

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
        var player = CrowdControlMod.GetInstance().GetPlayer();
        ApplyBuffs(Duration.GetValueOrDefault());
        player.OnRespawnHook += OnRespawn;
        player.PreUpdateBuffsHook += PreUpdateBuffs;
        return base.OnStart();
    }

    protected override void OnStop()
    {
        var player = CrowdControlMod.GetInstance().GetPlayer();
        player.OnRespawnHook -= OnRespawn;
        player.PreUpdateBuffsHook -= PreUpdateBuffs;
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
    
    private void PreUpdateBuffs(CrowdControlPlayer player)
    {
        CheckImmunities();
    }

    private void ApplyBuffs(float duration)
    {
        var player = CrowdControlMod.GetInstance().GetPlayer();
        
        CheckImmunities();

        // Add each buff for the provided duration
        foreach (var buffId in _buffs)
        {
            if (player.Player.HasBuff(buffId))
            {
                // Clear the buff, so we can set the time ourselves
                player.Player.ClearBuff(buffId);
            }
            
            player.Player.AddBuff(buffId, (int)Math.Ceiling(60 * duration));
        }
    }

    private void CheckImmunities()
    {
        var player = CrowdControlMod.GetInstance().GetPlayer();
        
        if (_buffs.Contains(BuffID.Confused))
        {
            player.Player.buffImmune[BuffID.Confused] = false;
        }

        if (_buffs.Contains(BuffID.Frozen))
        {
            player.Player.buffImmune[BuffID.Frozen] = false;
        }
    }
    
    #endregion
}