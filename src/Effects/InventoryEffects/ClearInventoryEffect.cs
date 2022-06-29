﻿using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Clear the contents of the player's main inventory (excluding the hot bar).
/// </summary>
public sealed class ClearInventoryEffect : CrowdControlEffect
{
    #region Constructors

    public ClearInventoryEffect() : base(EffectID.ClearInventory, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        // Iterate over a portion of the player's inventory and add the slot indexes to be cleared
        // 10 is the first slot in the main inventory (after the hot bar)
        // 49 is the last main inventory slot
        // https://tshock.readme.io/docs/slot-indexes (out-dated)
        List<int> slotsToClear = new();
        for (var i = 10; i < 50; i++)
        {
            if (player.Player.inventory[i].active)
            {
                slotsToClear.Add(i);
            }
        }

        if (!slotsToClear.Any())
        {
            // There are no items to be cleared
            return CrowdControlResponseStatus.Failure;
        }

        // Clear the slots
        foreach (var slot in slotsToClear)
        {
            player.Player.inventory[slot].TurnToAir();
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GhostMask, $"{viewerString} cleared {playerString}'s inventory", Severity);
    }

    #endregion
}