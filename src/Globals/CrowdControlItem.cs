﻿using System;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlItem : GlobalItem
{
    #region Delegates

    /// <inheritdoc cref="CanPickup" />
    public delegate bool CanPickupDelegate(Item item, Player player);

    /// <inheritdoc cref="OnConsumeItem" />
    public delegate void OnItemConsumed(Item item, Player player);

    #endregion

    #region Events

    /// <summary>
    ///     Invoked when an item is crafted by the local player (client-side).
    /// </summary>
    public static event Action<Recipe>? OnCraftedHook;

    /// <inheritdoc cref="CanPickup" />
    public static event CanPickupDelegate? CanPickupHook;

    /// <inheritdoc cref="OnConsumeItem" />
    public static event OnItemConsumed? OnItemConsumedHook;

    #endregion

    #region Methods

    public override void OnCreate(Item item, ItemCreationContext context)
    {
        if (context is RecipeCreationContext recipeCreationContext)
        {
            OnCraftedHook?.Invoke(recipeCreationContext.recipe);
        }
    }

    public override bool CanPickup(Item item, Player player)
    {
        return CanPickupHook?.Invoke(item, player) ?? base.CanPickup(item, player);
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        OnItemConsumedHook?.Invoke(item, player);
    }

    #endregion
}