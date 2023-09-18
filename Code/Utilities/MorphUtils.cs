using System.Collections.Generic;
using System.IO;
using CrowdControlMod.Code.Utilities.Morphs;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Code.Utilities;

public static class MorphUtils
{
    #region Static Fields and Constants

    private static readonly Dictionary<byte, MorphInfo> MorphInfoById = new();

    #endregion

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

    /// <summary>
    ///     Get the morph of the provided player (server or client).
    /// </summary>
    public static byte GetMorph(this Player player)
    {
        return player.GetModPlayer<MorphPlayer>().CurrentMorph;
    }

    /// <summary>
    ///     Get the morph info for the provided morph id.
    /// </summary>
    public static MorphInfo? GetMorphData(byte morph)
    {
        if (morph == MorphID.None)
        {
            return null;
        }

        // Check if an instance already exists
        if (MorphInfoById.TryGetValue(morph, out var morphInfo))
        {
            return morphInfo;
        }

        // Otherwise create a new instance and cache it for re-use
        morphInfo = morph switch
        {
            MorphID.Fox => new FoxMorphInfo(),
            MorphID.Junimo => new JunimoMorphInfo(),
            MorphID.BlueFairy => new BlueFairyMorphInfo(),
            _ => null
        };

        if (morphInfo != null)
        {
            MorphInfoById.Add(morph, morphInfo);
        }

        return morphInfo;
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
            if (!drawInfo.drawPlayer.active || drawInfo.drawPlayer.dead || drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph == MorphID.None || (drawInfo.headOnlyRender && !ModContent.GetInstance<MorphDrawLayer>().IsHeadLayer))
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

        public override void PostUpdateEquips()
        {
            GetMorphData(CurrentMorph)?.PostUpdateEquips(Player);
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            GetMorphData(CurrentMorph)?.DrawEffects(Player);
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            GetMorphData(CurrentMorph)?.ModifyHurt(Player, ref modifiers);
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            GetMorphData(CurrentMorph)?.OnHurt(Player, in info);
        }

        #endregion
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MorphDrawLayer : PlayerDrawLayer
    {
        #region Properties

        public override bool IsHeadLayer => false;

        #endregion

        #region Methods

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.active && !drawInfo.drawPlayer.dead && drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph != MorphID.None;
        }

        public override Position GetDefaultPosition()
        {
            return new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.HeldItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var morph = drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph;
            MorphInfo? morphInfo;
            if (morph == MorphID.None || (morphInfo = GetMorphData(morph)) == null || morphInfo.Texture == null || morphInfo.TotalFrames == 0)
            {
                return;
            }

            var fullRender = !drawInfo.headOnlyRender;

            // Calculate draw position
            var position = drawInfo.Center - Main.screenPosition;
            position.X = (int)position.X;
            position.Y = (int)position.Y;
            morphInfo.ModifyPosition(ref position, in drawInfo);

            // Account for step stool
            if (drawInfo.drawPlayer.portableStoolInfo.IsInUse)
            {
                position.Y += drawInfo.drawPlayer.portableStoolInfo.VisualYOffset / 2f;
            }

            // Account for sitting
            if (drawInfo.drawPlayer.sitting.isSitting)
            {
                drawInfo.drawPlayer.sitting.GetSittingOffsetInfo(drawInfo.drawPlayer, out _, out var sittingHeight);
                position.Y += sittingHeight;
            }

            // Calculate draw rotation
            var rotation = drawInfo.rotation;
            morphInfo.ModifyRotation(ref rotation, in drawInfo);

            // Account for sleeping in a bed
            if (drawInfo.drawPlayer.sleeping.isSleeping)
            {
                rotation += MathHelper.PiOver2 * drawInfo.drawPlayer.direction;
            }

            // Calculate draw scale
            var scale = 1f;
            morphInfo.ModifyScale(ref scale, in drawInfo);

            // Determine current frame group
            int currentFrame;
            var animStartFrame = 0;
            var animFrameCount = 1;
            var animSpeed = 1f;
            if (drawInfo.drawPlayer.velocity == Vector2.Zero || drawInfo.drawPlayer.mount.Active || drawInfo.drawPlayer.grappling[0] > -1)
            {
                // Idle
                animStartFrame = morphInfo.IdleStartFrame;
                animFrameCount = morphInfo.IdleFrameCount;
                animSpeed = morphInfo.IdleAnimSpeed;
            }
            else if (drawInfo.drawPlayer.velocity.Y != 0f)
            {
                // Falling
                animStartFrame = morphInfo.FallingStartFrame;
                animFrameCount = morphInfo.FallingFrameCount;
                animSpeed = morphInfo.FallingAnimSpeed;
            }
            else if (drawInfo.drawPlayer.velocity.X != 0f)
            {
                // Walking
                animStartFrame = morphInfo.WalkingStartFrame;
                animFrameCount = morphInfo.WalkingFrameCount;
                animSpeed = morphInfo.WalkingAnimSpeed;
            }

            // Animate
            if (animFrameCount > 1)
            {
                currentFrame = animStartFrame + (int)(Main.GameUpdateCount * animSpeed) % animFrameCount;
            }
            else
            {
                currentFrame = animStartFrame;
            }

            // Determine colour
            var colour = Color.White;
            morphInfo.ModifyColour(ref colour, in drawInfo);
            if (fullRender)
            {
                // Lighting / Stealth / shadow / immunity
                colour = Lighting.GetColor((int)(drawInfo.Center.X / 16f), (int)(drawInfo.Center.Y / 16f), colour);
                colour *= drawInfo.stealth;
                colour = drawInfo.drawPlayer.GetImmuneAlpha(colour, drawInfo.shadow);
            }

            // Determine direction
            var direction = drawInfo.drawPlayer.direction;
            morphInfo.ModifyDirection(ref direction, in drawInfo);

            if (fullRender)
            {
                // Draw mount (back)
                PlayerDrawLayers.MountBack.SetVisible();
                PlayerDrawLayers.MountBack.DrawWithTransformationAndChildren(ref drawInfo);

                // Draw step stool
                if (drawInfo.drawPlayer.portableStoolInfo.IsInUse)
                {
                    PlayerDrawLayers.PortableStool.SetVisible();
                    PlayerDrawLayers.PortableStool.DrawWithTransformationAndChildren(ref drawInfo);
                }
            }

            if (!fullRender || !drawInfo.hideEntirePlayer)
            {
                // Draw the morph
                drawInfo.DrawDataCache.Add(new DrawData(
                    morphInfo.Texture,
                    position,
                    new Rectangle(0, currentFrame * (morphInfo.Texture.Height / morphInfo.TotalFrames), morphInfo.Texture.Width, morphInfo.Texture.Height / morphInfo.TotalFrames),
                    colour,
                    rotation,
                    new Vector2(morphInfo.Texture.Width, morphInfo.Texture.Height / (float)morphInfo.TotalFrames) * 0.5f,
                    scale,
                    direction != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
                ));
            }

            if (!fullRender)
            {
                return;
            }

            // Draw mount (front)
            PlayerDrawLayers.MountFront.SetVisible();
            PlayerDrawLayers.MountFront.DrawWithTransformationAndChildren(ref drawInfo);

            // Draw frozen / webbed buff
            PlayerDrawLayers.FrozenOrWebbedDebuff.SetVisible();
            PlayerDrawLayers.FrozenOrWebbedDebuff.DrawWithTransformationAndChildren(ref drawInfo);
        }

        #endregion
    }

    #endregion
}