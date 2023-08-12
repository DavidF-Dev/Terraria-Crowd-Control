using System.Collections.Generic;
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

    public ClearInventoryEffect() : base(EffectID.ClearInventory, 0, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

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
            if (player.Player.inventory[i].active && !player.Player.inventory[i].IsAir)
            {
                slotsToClear.Add(i);
            }
        }

        if (slotsToClear.Count == 0)
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

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GhostMask, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}