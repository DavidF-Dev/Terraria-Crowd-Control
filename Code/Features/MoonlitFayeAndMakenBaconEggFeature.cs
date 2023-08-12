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
            if (SteamUtils.IsMoonlitFaye && item.type is ItemID.Apple)
            {
                CrowdControlMod.GetInstance().GetFeature<MorphUntilDeathFeature>(FeatureID.MorphUntilDeath)?.Toggle(MorphID.Junimo);
            }

            if (SteamUtils.IsMakenBacon && item.type is ItemID.Lemon)
            {
                CrowdControlMod.GetInstance().GetFeature<MorphUntilDeathFeature>(FeatureID.MorphUntilDeath)?.Toggle(MorphID.BlueFairy);
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
            if (shop.NpcType != NPCID.Merchant)
            {
                return;
            }

            if (SteamUtils.IsMoonlitFaye)
            {
                shop.Add(new Item(ItemID.Apple)
                {
                    stack = 1,
                    shopCustomPrice = Item.buyPrice(silver: 1),
                    rare = ItemRarityID.Green
                });
            }

            if (SteamUtils.IsMakenBacon)
            {
                shop.Add(new Item(ItemID.Lemon)
                {
                    stack = 1,
                    shopCustomPrice = Item.buyPrice(silver: 1),
                    rare = ItemRarityID.Blue
                });
            }
        }

        #endregion
    }

    #endregion
}