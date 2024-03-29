﻿using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod.Utilities;

public static class ItemUtils
{
    #region Static Methods

    /// <summary>
    ///     Set the owner's name for an item, so that it precedes the item's name.
    /// </summary>
    public static void SetItemOwner(this Item item, string owner)
    {
        if (owner.Equals("Chat"))
        {
            // Ignore if using anonymous names
            return;
        }

        item.GetGlobalItem<ItemOwner>().SetOwner(item, owner);
    }

    /// <summary>
    ///     Get the owner name for an item.
    /// </summary>
    public static string GetItemOwner(this Item item)
    {
        return item.GetGlobalItem<ItemOwner>().Owner;
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class ItemOwner : GlobalItem
    {
        #region Static Methods

        private static void ApplyOwner(Item item, string owner)
        {
            item.SetNameOverride($"{owner}'s {Lang.GetItemName(item.type)}");
        }

        #endregion

        #region Properties

        public override bool InstancePerEntity => true;

        /// <summary>
        ///     Name of the person who owns the item.
        /// </summary>
        public string Owner { get; private set; } = string.Empty;

        #endregion

        #region Methods

        public void SetOwner(Item item, string owner)
        {
            if (Owner.Equals(owner))
            {
                // Ignore if unchanged
                return;
            }

            Owner = owner;
            item.ClearNameOverride();
            if (!string.IsNullOrEmpty(Owner))
            {
                ApplyOwner(item, Owner);
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(item.GetGlobalItem<ItemOwner>().Owner);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            item.GetGlobalItem<ItemOwner>().SetOwner(item, reader.ReadString());
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (!string.IsNullOrEmpty(Owner))
            {
                tag.Add("Owner", Owner);
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            var owner = tag.ContainsKey("Owner") ? tag.GetString("Owner") : string.Empty;
            if (!string.IsNullOrEmpty(owner))
            {
                item.GetGlobalItem<ItemOwner>().SetOwner(item, owner);
            }
        }

        public override GlobalItem Clone(Item from, Item to)
        {
            var clone = base.Clone(from, to);
            if (from.TryGetGlobalItem(out ItemOwner owner) && !string.IsNullOrEmpty(owner.Owner))
            {
                ApplyOwner(to, owner.Owner);
            }

            return clone;
        }

        public override bool CanStack(Item destination, Item source)
        {
            var destinationOwner = destination.GetItemOwner();
            var sourceOwner = source.GetItemOwner();
            return destinationOwner.Equals(sourceOwner) || (string.IsNullOrEmpty(destinationOwner) && string.IsNullOrEmpty(sourceOwner));
        }

        #endregion
    }

    #endregion
}