﻿using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

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
        // 57 is the last ammo slot
        // https://tshock.readme.io/docs/slot-indexes
        List<int> slotsToClear = new();
        for (var i = 10; i <= 57; i++)
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
            player.Player.inventory[slot].active = false;
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GhostMask, $"{viewerString} cleared {playerString}'s inventory", Severity);
    }

    #endregion
}