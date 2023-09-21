using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Attempts to randomly reforge the held weapon.
/// </summary>
public sealed class ReforgeItemEffect : CrowdControlEffect
{
    #region Constructors

    public ReforgeItemEffect() : base(EffectID.ReforgeItem, 0, EffectSeverity.Neutral)
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

        // Get the player's selected item, if any, and check if it is possible to reforge it
        var item = player.Player.inventory[player.Player.selectedItem];
        if (item.type == ItemID.None || !item.CanHavePrefixes())
        {
            return CrowdControlResponseStatus.Retry;
        }

        // Determine a prefix to reforge the item with, ignoring the current prefix
        var currentPrefix = item.prefix;
        int rolledPrefix;
        const int maxAttempts = 10;
        var attempts = 0;
        do
        {
            if (attempts++ >= maxAttempts ||
                !PrefixLoader.Roll(item, Main.rand, out rolledPrefix, false))
            {
                return CrowdControlResponseStatus.Retry;
            }
        } while (currentPrefix == rolledPrefix);

        // Try to apply the prefix to the item
        if (!item.CanApplyPrefix(rolledPrefix) ||
            !item.Prefix(rolledPrefix))
        {
            return CrowdControlResponseStatus.Retry;
        }

        // Update the server on the changes
        if (NetUtils.IsClient)
        {
            NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.Player.whoAmI, player.Player.selectedItem, item.stack, item.prefix, item.netID);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var player = GetLocalPlayer();
        var item = player.Player.inventory[player.Player.selectedItem];
        if (item == null)
        {
            return;
        }

        var itemName = Lang.GetItemName(item.type).Value;
        var prefixName = Lang.prefix[item.prefix].Value;
        TerrariaUtils.WriteEffectMessage((short)item.type, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, itemName, prefixName), Severity);
    }

    #endregion
}