using System.IO;
using CrowdControlMod.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Code.Utilities;

public static class MorphUtils
{
    #region Static Methods

    /// <summary>
    ///     Set the morph of the provided player (server or client).
    /// </summary>
    public static void SetMorph(this Player player, byte morph)
    {
        if (Main.netMode != NetmodeID.Server && player.whoAmI != Main.myPlayer)
        {
            // Not allowed
            return;
        }

        if (player.GetModPlayer<MorphPlayer>().CurrentMorph == morph)
        {
            // No change
            return;
        }

        // Set the morph
        player.GetModPlayer<MorphPlayer>().CurrentMorph = morph;

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            return;
        }

        // Sync with server and clients
        var packet = CrowdControlMod.GetInstance().GetPacket();
        packet.Write((byte)PacketID.SyncMorph);
        packet.Write(player.whoAmI);
        packet.Write(morph);
        packet.Send();
    }

    public static void HandleSync(BinaryReader reader)
    {
        var whoAmI = reader.ReadInt32();
        var morph = reader.ReadByte();
        Main.player[whoAmI].GetModPlayer<MorphPlayer>().CurrentMorph = morph;

        if (Main.netMode != NetmodeID.Server)
        {
            return;
        }

        var packet = CrowdControlMod.GetInstance().GetPacket();
        packet.Write((byte)PacketID.SyncMorph);
        packet.Write(whoAmI);
        packet.Write(morph);
        packet.Send(-1, whoAmI);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MorphPlayer : ModPlayer
    {
        #region Fields

        /// <summary>
        ///     Player morph.
        /// </summary>
        public byte CurrentMorph;

        #endregion

        #region Methods

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (CurrentMorph == MorphID.None)
            {
                // No morph
                return;
            }

            var packet = CrowdControlMod.GetInstance().GetPacket();
            packet.Write((byte)PacketID.SyncMorph);
            packet.Write(Player.whoAmI);
            packet.Write(CurrentMorph);
            packet.Send();
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph == MorphID.None)
            {
                // No morph
                return;
            }

            // Disable all layers except those we care about
            foreach (var layer in PlayerDrawLayerLoader.Layers)
            {
                if (layer != ModContent.GetInstance<MorphDrawLayer>() && layer != PlayerDrawLayers.HeldItem)
                {
                    layer.Hide();
                }
            }
        }

        #endregion
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MorphDrawLayer : PlayerDrawLayer
    {
        #region Methods

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph != MorphID.None;
        }

        public override Position GetDefaultPosition()
        {
            return new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.HeldItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            // Calculate draw position
            var position = drawInfo.Center - Main.screenPosition;
            position.X = (int)position.X;
            position.Y = (int)position.Y - 4;

            // Get texture
            Main.instance.LoadProjectile(ProjectileID.FennecFox);
            var tex = TextureAssets.Projectile[ProjectileID.FennecFox].Value;
            const int totalFrames = 17;

            // Determine current frame / animation
            var currentFrame = 0;
            if (drawInfo.drawPlayer.velocity.Y != 0f)
            {
                currentFrame = 11 + (int)(Main.GameUpdateCount / 2) % 5;
            }
            else if (drawInfo.drawPlayer.velocity.X != 0f)
            {
                currentFrame = 4 + (int)(Main.GameUpdateCount / 2) % 7;
            }

            drawInfo.DrawDataCache.Add(new DrawData(
                tex,
                position,
                new Rectangle(0, currentFrame * (tex.Height / totalFrames), tex.Width, tex.Height / totalFrames),
                Color.White,
                drawInfo.rotation,
                new Vector2(tex.Width, tex.Height / (float)totalFrames) * 0.5f,
                1.5f,
                drawInfo.drawPlayer.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            ));
        }

        #endregion
    }

    #endregion
}