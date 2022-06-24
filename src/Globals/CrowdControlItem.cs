using System;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

[UsedImplicitly]
public sealed class CrowdControlItem : GlobalItem
{
    #region Events

    /// <summary>
    ///     Invoked when an item is crafted by the local player (client-side).
    /// </summary>
    [PublicAPI]
    public static event Action<Recipe> OnCraftedHook;

    #endregion

    #region Methods

    public override void OnCreate(Item item, ItemCreationContext context)
    {
        if (context is RecipeCreationContext recipeCreationContext)
        {
            OnCraftedHook?.Invoke(recipeCreationContext.recipe);
        }
    }

    #endregion
}