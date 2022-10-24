using System;
using System.Collections.Generic;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlItem : GlobalItem
{
    #region Delegates

    /// <inheritdoc cref="CanPickup" />
    public delegate bool CanPickupDelegate(Item item, Player player);

    /// <inheritdoc cref="OnConsumeItem" />
    public delegate void OnItemConsumedDelegate(Item item, Player player);

    /// <inheritdoc cref="PreDrawInInventory" />
    public delegate bool PreDrawInInventoryDelegate(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColour, Color itemColour, Vector2 origin, float scale);

    /// <inheritdoc cref=" ModifyTooltips" />
    public delegate void ModifyTooltipsDelegate(Item item, List<TooltipLine> tooltips);

    #endregion

    #region Events

    /// <summary>
    ///     Invoked when an item is crafted by the local player (client-side).
    /// </summary>
    public static event Action<Recipe>? OnCraftedHook;

    /// <inheritdoc cref="CanPickup" />
    public static event CanPickupDelegate? CanPickupHook;

    /// <inheritdoc cref="OnConsumeItem" />
    public static event OnItemConsumedDelegate? OnItemConsumedHook;

    /// <inheritdoc cref="PreDrawInInventory" />
    public static event PreDrawInInventoryDelegate? PreDrawInInventoryHook;

    /// <inheritdoc cref=" ModifyTooltips" />
    public static event ModifyTooltipsDelegate? ModifyTooltipsHook;

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

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        return PreDrawInInventoryHook?.Invoke(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale) ?? base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        ModifyTooltipsHook?.Invoke(item, tooltips);
    }

    public override bool? UseItem(Item item, Player player)
    {
        // Check if a life crystal is being blocked but should be allowed
        if (item.type == ItemID.LifeCrystal && player.ConsumedLifeCrystals == Player.LifeCrystalMax && player.GetModPlayer<CrowdControlPlayer>().LifeCrystalRemoved > 0)
        {
            // Reverse the alteration
            player.GetModPlayer<CrowdControlPlayer>().LifeCrystalRemoved--;
            player.HealEffect(20);
            TerrariaUtils.WriteDebug($"{nameof(UseItem)}: (Crystal={player.ConsumedLifeCrystals}) (Fruit={player.ConsumedLifeFruit}) (Removed={player.GetModPlayer<CrowdControlPlayer>().LifeCrystalRemoved})");
            return true;
        }

        return base.UseItem(item, player);
    }

    #endregion
}