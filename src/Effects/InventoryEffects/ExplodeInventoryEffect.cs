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

    public ExplodeInventoryEffect() : base(EffectID.ExplodeInventory, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Iterate over the inventory and randomly drop items
        var player = GetLocalPlayer();
        var oldSel = player.Player.selectedItem;
        var oldXSpeed = player.Player.velocity.X;
        var oldDir = player.Player.direction;
        var dropChance = 10 * Main.rand.Next(1, 10);
        for (var i = 0; i < 50; i++)
        {
            if (i < 10 || player.Player.inventory[i].type == ItemID.None)
            {
                continue;
            }

            if (Main.rand.Next(100) > dropChance)
            {
                // Increase drop chance so it becomes more likely that a drop will occur
                dropChance += 20;
                continue;
            }

            dropChance = 0;

            // Drop the item in a random direction
            player.Player.inventory[i].favorited = false;
            player.Player.selectedItem = i;
            player.Player.direction = Main.rand.Next(100) > 50 ? -1 : 1;
            player.Player.velocity.X = Main.rand.Next(8, 26) * player.Player.direction;
            player.Player.DropSelectedItem();
        }

        player.Player.selectedItem = oldSel;
        player.Player.velocity.X = oldXSpeed;
        player.Player.direction = oldDir;
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.SmokeBomb, $"{viewerString} exploded {playerString}'s inventory", Severity);
    }

    #endregion
}