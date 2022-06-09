﻿using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using JetBrains.Annotations;
using Terraria.ID;

namespace CrowdControlMod.Effects;

public sealed class BuffEffect : CrowdControlEffect
{
    #region Delegates

    [NotNull]
    public delegate string GetStartMessageDelegate([NotNull] string viewerString, [NotNull] string playerString);

    #endregion

    #region Fields

    private readonly short _itemId;

    private readonly GetStartMessageDelegate _getStartMessage;

    [NotNull]
    private readonly HashSet<int> _buffs;
    
    private readonly bool _hasConfusedBuff;

    private readonly bool _hasFrozenBuff;

    #endregion

    #region Constructors

    public BuffEffect([NotNull] string id, EffectSeverity severity, float duration, short itemId, [NotNull] GetStartMessageDelegate getStartMessage, [NotNull] params int[] buffs) : base(id, severity)
    {
        Duration = duration;
        _itemId = itemId;
        _getStartMessage = getStartMessage;
        _buffs = new HashSet<int>(buffs);
        _hasConfusedBuff = _buffs.Contains(BuffID.Confused);
        _hasFrozenBuff = _buffs.Contains(BuffID.Frozen);
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        // Retry if the player already has all the buffs active
        if (_buffs.All(player.Player.HasBuff))
        {
            return CrowdControlResponseStatus.Retry;
        }

        player.PreUpdateBuffsHook += PreUpdateBuffs;
        return base.OnStart();
    }

    protected override void OnStop()
    {
        // Don't bother clearing the buffs
    
        GetLocalPlayer().PreUpdateBuffsHook -= PreUpdateBuffs;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string _)
    {
        TerrariaUtils.WriteEffectMessage(_itemId, _getStartMessage(viewerString, playerString), Severity);
    }

    private void PreUpdateBuffs()
    {
        var player = GetLocalPlayer();

        // Check buff immunities
        {
            if (_hasConfusedBuff)
            {
                player.Player.buffImmune[BuffID.Confused] = false;
            }

            if (_hasFrozenBuff)
            {
                player.Player.buffImmune[BuffID.Frozen] = false;
            }
        }

        // Add buffs if needed
        var buffTime = TimeLeft.GetValueOrDefault(1f);
        foreach (var buffId in _buffs)
        {
            // Check if the player already has the buff
            if (player.Player.HasBuff(buffId))
            {
                // Check if the buff remaining time is 'fine'
                var buffIndex = player.Player.FindBuffIndex(buffId);
                if (player.Player.buffTime[buffIndex] >= buffTime)
                {
                    continue;
                }

                // Otherwise, remove the buff so that it can be added with the correct time
                player.Player.DelBuff(buffIndex);
            }

            player.Player.AddBuff(buffId, (int)Math.Ceiling(60 * buffTime));
        }
    }

    #endregion
}