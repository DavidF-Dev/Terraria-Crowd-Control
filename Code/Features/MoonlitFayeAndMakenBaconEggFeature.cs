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
                shop.Add(new Item(ModContent.ItemType<GreenAppleItem>()) {stack = 1});
            }

            if (SteamUtils.IsMakenBacon)
            {
                shop.Add(new Item(ModContent.ItemType<FairyTonicItem>()) {stack = 1});
            }
        }

        #endregion
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class GreenAppleItem : ModItem
    {
        #region Properties

        public override string Texture => $"{nameof(CrowdControlMod)}/Assets/Textures/GreenAppleItem";

        #endregion

        #region Methods

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item2;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 1);
        }

        public override bool CanUseItem(Player player)
        {
            return MorphUntilDeathFeature.Instance != null;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                MorphUntilDeathFeature.Instance!.Toggle(MorphID.Junimo);
            }

            return true;
        }

        #endregion
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class FairyTonicItem : ModItem
    {
        #region Properties

        public override string Texture => "Terraria/Images/Item_" + ItemID.FairyQueenPetItem;

        #endregion

        #region Methods

        public override void SetDefaults()
        {
            Item.width = ContentSamples.ItemsByType[ItemID.FairyQueenPetItem].width;
            Item.height = ContentSamples.ItemsByType[ItemID.FairyQueenPetItem].height;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 1);
        }

        public override bool CanUseItem(Player player)
        {
            return MorphUntilDeathFeature.Instance != null;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                MorphUntilDeathFeature.Instance!.Toggle(MorphID.BlueFairy);
            }

            return true;
        }

        #endregion
    }

    #endregion
}