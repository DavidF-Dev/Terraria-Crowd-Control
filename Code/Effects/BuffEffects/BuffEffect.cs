using System;
using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CrowdControlMod.Effects.BuffEffects;

/// <summary>
///     Timed effect that forces a set of buffs upon the player.
/// </summary>
public sealed class BuffEffect : CrowdControlEffect
{
    #region Fields

    private readonly short _itemId;
    private readonly Action<CrowdControlPlayer>? _onStart;
    private readonly HashSet<int> _buffs;
    private readonly bool _hasConfusedBuff;
    private readonly bool _hasFrozenBuff;
    private readonly bool _hasInvisibilityBuff;
    private readonly bool _hasMiningBuff;
    private readonly bool _hasOnFireBuff;
    private readonly bool _hasCursedBuff;

    #endregion

    #region Constructors

    public BuffEffect(string id, EffectSeverity severity, int duration, short itemId, int emoteId, Action<CrowdControlPlayer>? onStart, params int[] buffs) : base(id, duration, severity)
    {
        _itemId = itemId;
        StartEmote = emoteId;
        _onStart = onStart;
        _buffs = new HashSet<int>(buffs);
        _hasConfusedBuff = _buffs.Contains(BuffID.Confused);
        _hasFrozenBuff = _buffs.Contains(BuffID.Frozen);
        _hasInvisibilityBuff = _buffs.Contains(BuffID.Invisibility);
        _hasMiningBuff = _buffs.Contains(BuffID.Mining);
        _hasOnFireBuff = _buffs.Contains(BuffID.OnFire) || _buffs.Contains(BuffID.OnFire3);
        _hasCursedBuff = _buffs.Contains(BuffID.Cursed);
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Buff;

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
        TerrariaUtils.WriteEffectMessage(_itemId, LangUtils.GetEffectStartText(Id, viewerString, playerString, _), Severity);
    }

    private void PreUpdateBuffs()
    {
        var player = GetLocalPlayer();

        if (_hasMiningBuff)
        {
            // Increase mining speed further
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

            if (_hasOnFireBuff)
            {
                player.Player.buffImmune[BuffID.OnFire] = false;
                player.Player.buffImmune[BuffID.OnFire3] = false;
            }

            if (_hasCursedBuff)
            {
                player.Player.buffImmune[BuffID.Cursed] = false;
            }
        }

        // Add buffs if needed
        var targetDuration = (int)Math.Ceiling(60 * TimeLeft);
        foreach (var buffId in _buffs)
        {
            // Check the existing buff's time left
            var buffIndex = player.Player.FindBuffIndex(buffId);
            if (buffIndex != -1)
            {
                // Ignore if the buff's time left is over the target duration (with a bit of leeway)
                if (player.Player.buffTime[buffIndex] >= targetDuration - 30)
                {
                    continue;
                }

                player.Player.buffTime[buffIndex] = targetDuration;
                TerrariaUtils.WriteDebug($"Updated buff's time left: '{Lang.GetBuffName(buffId)}'");
            }
            // Otherwise, add the buff and explicitly set its time left to the target duration
            else
            {
                player.Player.AddBuff(buffId, 2, false);
                TerrariaUtils.WriteDebug($"Added buff: '{Lang.GetBuffName(buffId)}'");

                buffIndex = player.Player.FindBuffIndex(buffId);
                if (buffId != -1)
                {
                    player.Player.buffTime[buffIndex] = targetDuration;
                }
            }
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