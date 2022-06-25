using System;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlItem : GlobalItem
{
    #region Delegates

    /// <inheritdoc cref="CanPickup" />
    public delegate bool CanPickupDelegate(Item item, Player player);

    #endregion

    #region Events

    /// <summary>
    ///     Invoked when an item is crafted by the local player (client-side).
    /// </summary>
    [PublicAPI]
    public static event Action<Recipe> OnCraftedHook;

    /// <inheritdoc cref="CanPickup" />
    [PublicAPI]
    public static event CanPickupDelegate CanPickupHook;

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

    #endregion
}