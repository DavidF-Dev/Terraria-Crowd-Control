using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Shuffle the content's of the player's inventory.
/// </summary>
public sealed class ShuffleInventoryEffect : CrowdControlEffect
{
    #region Constructors

    public ShuffleInventoryEffect() : base(EffectID.ShuffleInventory, null, EffectSeverity.Negative)
    {
    }

    #endregion

    public override EffectCategory Category => EffectCategory.Inventory;
    
    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();

        // Iterate over a portion of the player's inventory and add all slot indexes to a list
        // 0 is the first slot in the the hot bar
        // 57 is the last coin slot
        // https://tshock.readme.io/docs/slot-indexes (out-dated)
        List<int> allSlots = new();
        for (var i = 0; i < 58; i++)
        {
            allSlots.Add(i);
        }

        // Shuffle and swap the items around
        var slot = 0;
        var c = 0;
        foreach (var shuffledSlot in allSlots.OrderBy(_ => Main.rand.Next()))
        {
            // Swap the items at 'slot' and 'shuffledSlot'
            (player.Player.inventory[slot], player.Player.inventory[shuffledSlot]) = (player.Player.inventory[shuffledSlot], player.Player.inventory[slot]);
            if (!player.Player.inventory[slot].IsAir || !player.Player.inventory[shuffledSlot].IsAir)
            {
                c++;
            }
            
            slot++;
        }

        return c != 0 ? CrowdControlResponseStatus.Success : CrowdControlResponseStatus.Failure;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.LavaLamp, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}