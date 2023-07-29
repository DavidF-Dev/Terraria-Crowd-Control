using System;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Features;

/// <summary>
///     Easter egg feature for https://www.twitch.tv/official_conduit.
/// </summary>
public class OfficialConduitEggFeature : IFeature
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
    private sealed class OfficialConduitNPC : GlobalNPC
    {
        #region Properties

        private static bool IsEggEnabled => SteamUtils.IsOfficialConduit && DateTime.Now.Month == 6;

        #endregion

        #region Methods

        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            // 1/5 chance to spawn an item if killed by birthday player
            if (npc.life > 0 || npc.lastInteraction < 0 || npc.lastInteraction >= Main.maxPlayers || Main.myPlayer != npc.lastInteraction || !IsEggEnabled || !Main.rand.NextBool(5))
            {
                return;
            }

            // Spawn birthday-related item
            int itemType = Main.rand.NextFromList(ItemID.PartyPresent, ItemID.PartyHat, ItemID.PartyBalloonAnimal,
                ItemID.PartyBundleOfBalloonTile, ItemID.FlaskofParty, ItemID.BubbleMachine, ItemID.FireworkFountain,
                ItemID.FireworksBox, ItemID.SliceOfCake, ItemID.RainbowCampfire, ItemID.Cog, ItemID.SlapHand,
                ItemID.Maggot);
            var itemIndex = Item.NewItem(null, npc.position, npc.width, npc.height, itemType);
            Main.item[itemIndex].SetItemOwner("Conduit");
            if (NetUtils.IsClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, Main.item[itemIndex].stack);
            }

            // Spawn confetti on death
            var projIndex = Projectile.NewProjectile(null, npc.position, Vector2.UnitY * Main.rand.NextFloat(3f, 5f) * -1f, ProjectileID.ConfettiGun, 1, 0f, Main.myPlayer);
            Main.projectile[projIndex].friendly = true;
            if (NetUtils.IsClient)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projIndex);
            }

            Main.LocalPlayer.Emote(EmoteID.PartyPresent);
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!IsEggEnabled)
            {
                return;
            }

            // Draw a party hat on the NPC
            Main.instance.LoadItem(ItemID.PartyHat);
            var tex = TextureAssets.Item[ItemID.PartyHat].Value;
            spriteBatch.Draw(
                tex,
                npc.Top - Main.screenPosition + new Vector2(npc.spriteDirection == 1 ? -2f : 4f, 4),
                null,
                Lighting.GetColor((int)(npc.position.X / 16f), (int)(npc.position.Y / 16f)),
                0f,
                new Vector2(tex.Width / 2f, tex.Height),
                1f,
                npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0f);
        }

        #endregion
    }

    #endregion
}