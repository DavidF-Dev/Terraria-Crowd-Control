using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Features;

/// <summary>
///     Easter eggs for 'moonlit_faye' and 'makenbacon07'.
/// </summary>
public sealed class MoonlitFayeAndMakenBaconEggFeature : IFeature
{
    #region Methods

    public void SessionStarted()
    {
    }

    public void SessionStopped()
    {
    }

    public void Dispose()
    {
    }

    #endregion

    #region Nested Types

    // ReSharper disable once UnusedType.Local
    private sealed class EggGlobalItem : GlobalItem
    {
        #region Methods

        public override void OnConsumeItem(Item item, Player player)
        {
            if (SteamUtils.IsMoonlitFaye && item.type is ItemID.Apple or ItemID.AppleJuice or ItemID.ApplePie or ItemID.ApplePieSlice)
            {
                CrowdControlMod.GetInstance().GetFeature<MorphUntilDeathFeature>(FeatureID.MorphUntilDeath)?.Enable(MorphID.Junimo);
            }

            if (SteamUtils.IsMakenBacon && item.type is ItemID.Lemon or ItemID.Lemonade)
            {
                CrowdControlMod.GetInstance().GetFeature<MorphUntilDeathFeature>(FeatureID.MorphUntilDeath)?.Enable(MorphID.BlueFairy);
            }
        }

        #endregion
    }

    // ReSharper disable once UnusedType.Local
    private sealed class EggGlobalNPC : GlobalNPC
    {
        #region Methods

        public override void ModifyShop(NPCShop shop)
        {
            if (SteamUtils.IsMoonlitFaye)
            {
                shop.Add(new Item(ItemID.Apple) {stack = 1, shopCustomPrice = Item.buyPrice(silver: 1)});
            }

            if (SteamUtils.IsMakenBacon)
            {
                shop.Add(new Item(ItemID.Lemon) {stack = 1, shopCustomPrice = Item.buyPrice(silver: 1)});
            }
        }

        #endregion
    }

    #endregion
}