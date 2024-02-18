using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Give a "Zenith" clone which deals minimal damage.
/// </summary>
public sealed class GiveNadirEffect : CrowdControlEffect
{
    #region Constructors

    public GiveNadirEffect() : base(EffectID.GiveNadir, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Create an instance that will be spawned in the world
        var type = ModContent.ItemType<NadirItem>();
        Item item = new(type);
        if (!string.IsNullOrEmpty(Viewer))
        {
            item.SetItemOwner(Viewer);
        }

        // Spawn the item
        item = GetLocalPlayer().Player.QuickSpawnItemDirect(null, item, item.stack);
        if (item.whoAmI == Main.maxItems || !item.active || item.type != type)
        {
            return CrowdControlResponseStatus.Retry;
        }

        return CrowdControlResponseStatus.Success;
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class NadirItem : ModItem
    {
        #region Properties

        public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.CopperShortsword;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Zenith);
            Item.damage = 1;
            Item.crit = 0;
            Item.knockBack = 0;
            Item.useAnimation = 60;
            Item.useTime = Item.useAnimation / 3;
            Item.shootSpeed /= 2;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(copper: 10);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, Main.rand.Next(-50, 51), 4956);
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.itemTime > 0)
            {
                player.AddBuff(BuffID.Dazed, 2);
            }
        }

        #endregion
    }

    #endregion
}