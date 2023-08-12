using System;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Goes through the player's inventory and drops their items in random directions.
/// </summary>
public sealed class ExplodeInventoryEffect : CrowdControlEffect
{
    #region Constructors

    public ExplodeInventoryEffect() : base(EffectID.ExplodeInventory, 0, EffectSeverity.Negative)
    {
    }

    #endregion

    private const float DropPercent = 0.8f;

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Remember the player's properties before we change them, so that they can be reverted at the end
        var player = GetLocalPlayer().Player;
        var oldSel = player.selectedItem;
        var oldXSpeed = player.velocity.X;
        var oldDir = player.direction;

        const int startIndex = 10;
        const int endIndex = 50;

        // Retrieve a collection of the player's filled inventory slots, excluding the hot-bar, and determine how many should be dropped
        var filledSlots = Enumerable.Range(startIndex, endIndex - startIndex).Where(i => player.inventory[i].active && !player.inventory[i].IsAir).ToList();
        var targetDrop = (int)Math.Ceiling(filledSlots.Count * DropPercent);
        if (targetDrop == 0 || targetDrop > filledSlots.Count)
        {
            return CrowdControlResponseStatus.Failure;
        }

        const int minThrowSpeed = 2;
        const int maxThrowSpeed = 15;

        // Iterate over the filled slots at random, dropping a percentage of the player's inventory
        foreach (var i in filledSlots.OrderBy(_ => Main.rand.Next()).Take(targetDrop))
        {
            // Drop the item in a random direction
            player.inventory[i].favorited = false;
            player.selectedItem = i;
            player.direction = Main.rand.Next(100) > 50 ? -1 : 1;
            player.velocity.X = Main.rand.Next(minThrowSpeed, maxThrowSpeed) * player.direction;
            player.DropSelectedItem();
        }

        // Revert the player's properties
        player.selectedItem = oldSel;
        player.velocity.X = oldXSpeed;
        player.direction = oldDir;
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SmokeBomb, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion
}