using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Drop the held item, or choose an item from the hot-bar to drop.
/// </summary>
public sealed class DropItemEffect : CrowdControlEffect
{
    #region Fields
    
    private Item _droppedItem;

    #endregion

    #region Constructors

    public DropItemEffect() : base(EffectID.DropItem, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.inventory[player.Player.selectedItem].type != ItemID.None)
        {
            // Drop selected item
            player.Player.inventory[player.Player.selectedItem].favorited = false;
            _droppedItem = player.Player.inventory[player.Player.selectedItem];
            player.Player.DropSelectedItem();
        }
        else
        {
            // Find an item to drop from the hot-bar
            List<int> slots = new();
            for (var i = 0; i < 10; i++)
            {
                if (player.Player.inventory[i].type == ItemID.None)
                {
                    continue;
                }

                slots.Add(i);
            }

            if (slots.Any())
            {
                // Choose an item
                var slot = Main.rand.Next(slots);
                var oldSel = player.Player.selectedItem;
                player.Player.inventory[slot].favorited = false;
                _droppedItem = player.Player.inventory[slot];
                player.Player.selectedItem = slot;
                player.Player.DropSelectedItem();
                player.Player.selectedItem = oldSel;
            }
            else
            {
                // No item in the hot-bar
                return CrowdControlResponseStatus.Failure;
            }
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _droppedItem = null;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string durationString)
    {
        if (_droppedItem.stack > 1)
        {
            TerrariaUtils.WriteEffectMessage((short)_droppedItem.type, $"{viewerString} caused {playerString} to fumble and drop {_droppedItem.stack} {_droppedItem.Name}", Severity);
            return;
        }

        TerrariaUtils.WriteEffectMessage((short)_droppedItem.type, $"{viewerString} caused {playerString} to fumble and drop their {_droppedItem.Name}", Severity);
    }

    #endregion
}