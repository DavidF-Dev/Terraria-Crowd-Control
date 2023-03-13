using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Features;

/// <summary>
///     Easter egg feature for https://www.twitch.tv/kaylajayde.
/// </summary>
public sealed class KaylaEggFeature : IFeature
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

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class KaylaPlayer : ModPlayer
    {
        #region Static Fields and Constants

        private const string KaylaEggOwner = "Capwap";

        #endregion

        #region Properties

        public bool IsKaylaEggActive
        {
            get
            {
                for (var i = 0; i < 50; i++)
                {
                    var item = Player.inventory[i];
                    if (item.active && item.type == ItemID.ToiletGlass && item.GetItemOwner() == KaylaEggOwner)
                    {
                        return true;
                    }
                }

                return Player.HeldItem != null && Player.HeldItem.type == ItemID.ToiletGlass && Player.HeldItem.GetItemOwner() == KaylaEggOwner;
            }
        }

        #endregion

        #region Methods

        public override void PostUpdate()
        {
            if (!SteamUtils.IsKaylaJayde || Main.myPlayer != Player.whoAmI)
            {
                // Ignore
                return;
            }

            var item = Player.HeldItem;
            if (item.type != ItemID.ToiletGlass)
            {
                // Ignore
                return;
            }

            // Ensure it is named correctly
            if (string.IsNullOrEmpty(item.GetItemOwner()))
            {
                item.SetItemOwner(KaylaEggOwner);
            }
        }

        #endregion
    }

    // ReSharper disable once UnusedType.Local
    private sealed class KaylaItem : GlobalItem
    {
        #region Methods

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (!Main.LocalPlayer.GetModPlayer<KaylaPlayer>().IsKaylaEggActive)
            {
                return;
            }

            Main.instance.LoadItem(ItemID.TopHat);
            var tex = TextureAssets.Item[ItemID.TopHat].Value;
            spriteBatch.Draw(
                tex,
                item.Top - Main.screenPosition + new Vector2(0f, -2f),
                null,
                Lighting.GetColor((int)(item.position.X / 16f), (int)(item.position.Y / 16f)),
                rotation,
                new Vector2(tex.Width / 2f, tex.Height),
                scale,
                SpriteEffects.None,
                0f);
        }

        #endregion
    }

    // ReSharper disable once UnusedType.Local
    private sealed class KaylaNPC : GlobalNPC
    {
        #region Methods

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!Main.LocalPlayer.GetModPlayer<KaylaPlayer>().IsKaylaEggActive)
            {
                return;
            }

            Main.instance.LoadItem(ItemID.TopHat);
            var tex = TextureAssets.Item[ItemID.TopHat].Value;
            spriteBatch.Draw(
                tex,
                npc.Top - Main.screenPosition + new Vector2(0f, 4),
                null,
                Lighting.GetColor((int)(npc.position.X / 16f), (int)(npc.position.Y / 16f)),
                0f,
                new Vector2(tex.Width / 2f, tex.Height),
                1f,
                SpriteEffects.None,
                0f);
        }

        #endregion
    }

    #endregion
}