using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using Terraria.DataStructures;
using Terraria.ID;

namespace CrowdControlMod.Effects.BuffEffects;

/// <summary>
///     Timed effect that forces a set of buffs upon the player.
/// </summary>
public sealed class BuffEffect : CrowdControlEffect
{
    #region Delegates

    public delegate string GetStartMessageDelegate(string viewerString, string playerString);

    #endregion

    #region Fields

    private readonly short _itemId;

    private readonly GetStartMessageDelegate _getStartMessage;

    private readonly Action<CrowdControlPlayer>? _onStart;

    private readonly HashSet<int> _buffs;

    private readonly bool _hasConfusedBuff;

    private readonly bool _hasFrozenBuff;

    private readonly bool _hasInvisibilityBuff;

    private readonly bool _hasMiningBuff;

    #endregion

    #region Constructors

    public BuffEffect(string id, EffectSeverity severity, float duration, short itemId, int emoteId, GetStartMessageDelegate getStartMessage, Action<CrowdControlPlayer>? onStart, params int[] buffs) : base(id, duration, severity)
    {
        _itemId = itemId;
        StartEmote = emoteId;
        _onStart = onStart;
        _getStartMessage = getStartMessage;
        _buffs = new HashSet<int>(buffs);
        _hasConfusedBuff = _buffs.Contains(BuffID.Confused);
        _hasFrozenBuff = _buffs.Contains(BuffID.Frozen);
        _hasInvisibilityBuff = _buffs.Contains(BuffID.Invisibility);
        _hasMiningBuff = _buffs.Contains(BuffID.Mining);
    }

    #endregion

    #region Properties

    protected override int StartEmote { get; }

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

        _onStart?.Invoke(player);
        player.PreUpdateBuffsHook += PreUpdateBuffs;
        player.ModifyDrawInfoHook += ModifyDrawInfo;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.PreUpdateBuffsHook -= PreUpdateBuffs;
        player.ModifyDrawInfoHook -= ModifyDrawInfo;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? _)
    {
        TerrariaUtils.WriteEffectMessage(_itemId, _getStartMessage(viewerString, playerString), Severity);
    }

    private void PreUpdateBuffs()
    {
        var player = GetLocalPlayer();

        if (_hasMiningBuff)
        {
            // TODO: Forcefully increase the mining speed (reduce)
            // TODO: Test
            player.Player.pickSpeed -= 0.5f;
        }
        
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
        foreach (var buffId in _buffs)
        {
            // Check if the player already has the buff
            if (player.Player.HasBuff(buffId))
            {
                // Check if the buff remaining time is 'fine'
                var buffIndex = player.Player.FindBuffIndex(buffId);
                if (player.Player.buffTime[buffIndex] >= TimeLeft)
                {
                    continue;
                }

                // Otherwise, remove the buff so that it can be added with the correct time
                player.Player.DelBuff(buffIndex);
            }

            player.Player.AddBuff(buffId, (int)Math.Ceiling(60 * TimeLeft));
        }
    }

    private void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (!_hasInvisibilityBuff)
        {
            return;
        }

        // Draw the player off the screen if the invisibility buff is active
        drawInfo.Position.X = 0f;
        drawInfo.Position.Y = 0f;
    }

    #endregion
}