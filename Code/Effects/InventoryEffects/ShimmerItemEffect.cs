using System;
using System.IO;
using System.Reflection;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Shimmers the player's held item if possible.
/// </summary>
public sealed class ShimmerItemEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int ShimmerStackMax = int.MaxValue;
    private static readonly MethodInfo? ShimmerItemMethod;

    #endregion

    #region Static Methods

    private static void ShimmerItem(Player player, int type, int stack)
    {
        if (NetUtils.IsClient || ShimmerItemMethod == null)
        {
            return;
        }

        // Drop an item to be shimmered in the world
        var droppedIndex = Item.NewItem(null, player.getRect(), type, stack, true);
        var droppedItem = Main.item[droppedIndex];
        droppedItem.velocity = Vector2.Zero;
        droppedItem.noGrabDelay = 100;
        droppedItem.newAndShiny = false;
        if (NetUtils.IsServer)
        {
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, droppedIndex, 1);
        }

        // Shimmer the item
        ShimmerItemMethod.Invoke(droppedItem, Array.Empty<object>());
    }

    #endregion

    #region Fields

    private int _shimmeredType;
    private int _shimmeredStack;

    #endregion

    #region Constructors

    static ShimmerItemEffect()
    {
        ShimmerItemMethod = typeof(Item).GetMethod("GetShimmered", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public ShimmerItemEffect() : base(EffectID.ShimmerItem, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (ShimmerItemMethod == null)
        {
            return CrowdControlResponseStatus.Unavailable;
        }

        var player = GetLocalPlayer().Player;
        if (player.shimmering)
        {
            return CrowdControlResponseStatus.Retry;
        }

        var heldItem = player.HeldItem;
        if (heldItem == null || heldItem.IsAir || !heldItem.CanShimmer())
        {
            return CrowdControlResponseStatus.Retry;
        }

        _shimmeredType = heldItem.type;
        _shimmeredStack = Math.Min(heldItem.stack, ShimmerStackMax);

        // Shimmer the dropped item
        if (NetUtils.IsSinglePlayer)
        {
            ShimmerItem(player, _shimmeredType, _shimmeredStack);
        }
        else
        {
            SendPacket(PacketID.HandleEffect, (short)_shimmeredType, (short)_shimmeredStack);
        }

        // Remove the original held item
        heldItem.stack -= _shimmeredStack;
        if (heldItem.stack <= 0)
        {
            heldItem.TurnToAir();
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _shimmeredType = ItemID.None;
        _shimmeredStack = 0;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        ShimmerItem(player.Player, reader.ReadInt16(), reader.ReadInt16());
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage((short)_shimmeredType, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, Lang.GetItemName(_shimmeredType).Value), Severity);
    }

    #endregion
}