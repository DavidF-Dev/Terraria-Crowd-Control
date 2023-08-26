using System;
using System.IO;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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

    /// <summary>
    ///     Get the morph of the provided player (server or client).
    /// </summary>
    public static byte GetMorph(this Player player)
    {
        return player.GetModPlayer<MorphPlayer>().CurrentMorph;
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
            if (!drawInfo.drawPlayer.active || drawInfo.drawPlayer.dead || drawInfo.headOnlyRender || drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph == MorphID.None)
            {
                // No morph
                return;
            }

            // Disable all layers except those we care about
            foreach (var layer in PlayerDrawLayerLoader.Layers)
            {
                if (layer != ModContent.GetInstance<MorphDrawLayer>() && layer != PlayerDrawLayers.HeldItem &&
                    layer != PlayerDrawLayers.MountFront && layer != PlayerDrawLayers.MountFront)
                {
                    layer.Hide();
                }
            }
        }

        public override void PostUpdateEquips()
        {
            if (CurrentMorph is MorphID.BlueFairy)
            {
                Lighting.AddLight(Player.Center, TorchID.Blue);
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (CurrentMorph is not MorphID.BlueFairy || !Main.rand.NextBool(3))
            {
                return;
            }

            const int dustCount = 1;
            for (var i = 0; i < dustCount; i++)
            {
                Dust.NewDust(Player.position + new Vector2(0f, 8f), Player.width, Player.height / 2, DustID.BlueFairy, Scale: 0.5f);
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (CurrentMorph is MorphID.BlueFairy or MorphID.Junimo)
            {
                modifiers.DisableSound();
            }

            if (CurrentMorph is MorphID.BlueFairy)
            {
                modifiers.DisableDust();
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (NetUtils.IsServer)
            {
                return;
            }

            if (CurrentMorph is MorphID.BlueFairy)
            {
                SoundEngine.PlaySound(SoundID.NPCHit5 with
                {
                    MaxInstances = SoundID.PlayerHit.MaxInstances,
                    SoundLimitBehavior = SoundID.PlayerHit.SoundLimitBehavior,
                    PlayOnlyIfFocused = SoundID.PlayerHit.PlayOnlyIfFocused
                }, Player.Center);

                const int dustCount = 15;
                var speed = new Vector2(info.HitDirection * info.Knockback, -3f);
                for (var i = 0; i < dustCount; i++)
                {
                    Dust.NewDust(Player.position + new Vector2(0f, 8f), Player.width, Player.height / 2, DustID.BlueFairy, speed.X, speed.Y, Scale: 0.85f);
                }
            }

            if (CurrentMorph is MorphID.Junimo)
            {
                SoundEngine.PlaySound(new SoundStyle("CrowdControlMod/Assets/Sounds/JunimoMeep")
                {
                    PitchVariance = 0.2f,
                    Volume = 0.75f,
                    MaxInstances = SoundID.PlayerHit.MaxInstances,
                    SoundLimitBehavior = SoundID.PlayerHit.SoundLimitBehavior,
                    PlayOnlyIfFocused = SoundID.PlayerHit.PlayOnlyIfFocused
                }, Player.Center);
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
            return drawInfo.drawPlayer.active && !drawInfo.drawPlayer.dead && drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph != MorphID.None;
        }

        public override Position GetDefaultPosition()
        {
            return new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.HeldItem);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var morph = drawInfo.drawPlayer.GetModPlayer<MorphPlayer>().CurrentMorph;
            if (morph == MorphID.None)
            {
                return;
            }

            // Calculate draw position
            var position = drawInfo.Center - Main.screenPosition;
            position.X = (int)position.X;
            position.Y = (int)position.Y;
            switch (morph)
            {
                case MorphID.Fox:
                    position.Y -= 4;
                    break;
                case MorphID.BlueFairy:
                    position.X += MathF.Sin(Main.GlobalTimeWrappedHourly * 1f) * 3;
                    position.Y -= 7 + MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 6;
                    break;
                case MorphID.Junimo:
                    position.Y += 2;
                    break;
            }

            // Get texture
            Texture2D? tex = null;
            var totalFrames = 0;
            switch (morph)
            {
                case MorphID.Fox:
                    Main.instance.LoadProjectile(ProjectileID.FennecFox);
                    tex = TextureAssets.Projectile[ProjectileID.FennecFox].Value;
                    totalFrames = 17;
                    break;
                case MorphID.Junimo:
                    Main.instance.LoadProjectile(ProjectileID.JunimoPet);
                    tex = TextureAssets.Projectile[ProjectileID.JunimoPet].Value;
                    totalFrames = 16;
                    break;
                case MorphID.BlueFairy:
                    Main.instance.LoadProjectile(ProjectileID.BlueFairy);
                    tex = TextureAssets.Projectile[ProjectileID.BlueFairy].Value;
                    totalFrames = 4;
                    break;
            }

            if (tex == null || totalFrames == 0)
            {
                // No texture
                return;
            }

            // Determine animation frames          
            var idleStartFrame = 0;
            var idleFrameCount = 1;
            var idleAnimSpeed = 0.25f;
            var fallingStartFrame = 0;
            var fallingFrameCount = 1;
            var fallingAnimSpeed = 0.25f;
            var walkingStartFrame = 0;
            var walkingFrameCount = 1;
            var walkingAnimSpeed = 0.25f;
            switch (morph)
            {
                case MorphID.Fox:
                    idleStartFrame = 0;
                    idleFrameCount = 3;
                    fallingStartFrame = 11;
                    fallingFrameCount = 5;
                    walkingStartFrame = 4;
                    walkingFrameCount = 6;
                    break;
                case MorphID.Junimo:
                    idleStartFrame = 0;
                    idleFrameCount = 4;
                    idleAnimSpeed = 0.1f;
                    fallingStartFrame = 13;
                    fallingFrameCount = 3;
                    fallingAnimSpeed = 0.1f;
                    walkingStartFrame = 4;
                    walkingFrameCount = 8;
                    walkingAnimSpeed = 0.1f;
                    break;
                case MorphID.BlueFairy:
                    idleStartFrame = 0;
                    idleFrameCount = 3;
                    idleAnimSpeed = 0.1f;
                    fallingStartFrame = 0;
                    fallingFrameCount = 3;
                    fallingAnimSpeed = 0.1f;
                    walkingStartFrame = 0;
                    walkingFrameCount = 3;
                    walkingAnimSpeed = 0.1f;
                    break;
            }

            // Determine current frame / animation
            int currentFrame;
            var animStartFrame = 0;
            var animFrameCount = 1;
            var animSpeed = 1f;
            if (drawInfo.drawPlayer.velocity == Vector2.Zero || drawInfo.drawPlayer.mount.Active || drawInfo.drawPlayer.grappling[0] > -1)
            {
                // Idle
                animStartFrame = idleStartFrame;
                animFrameCount = idleFrameCount;
                animSpeed = idleAnimSpeed;
            }
            else if (drawInfo.drawPlayer.velocity.Y != 0f)
            {
                // Falling
                animStartFrame = fallingStartFrame;
                animFrameCount = fallingFrameCount;
                animSpeed = fallingAnimSpeed;
            }
            else if (drawInfo.drawPlayer.velocity.X != 0f)
            {
                // Walking
                animStartFrame = walkingStartFrame;
                animFrameCount = walkingFrameCount;
                animSpeed = walkingAnimSpeed;
            }

            if (animFrameCount > 1)
            {
                currentFrame = animStartFrame + (int)(Main.GameUpdateCount * animSpeed) % animFrameCount;
            }
            else
            {
                currentFrame = animStartFrame;
            }

            // Choose colour
            var colour = Color.White;
            if (morph == MorphID.Fox && SteamUtils.IsMagicMalaraith)
            {
                colour = new Color(0, 102, 255, 255);
            }

            var scale = 1f;
            if (morph == MorphID.Fox)
            {
                scale = 1.5f;
            }

            var flipDirection = morph is MorphID.Junimo;

            drawInfo.DrawDataCache.Add(new DrawData(
                tex,
                position,
                new Rectangle(0, currentFrame * (tex.Height / totalFrames), tex.Width, tex.Height / totalFrames),
                Lighting.GetColor((int)(drawInfo.Center.X / 16f), (int)(drawInfo.Center.Y / 16f), colour),
                drawInfo.rotation,
                new Vector2(tex.Width, tex.Height / (float)totalFrames) * 0.5f,
                scale,
                drawInfo.drawPlayer.direction == (flipDirection ? 1 : -1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            ));
        }

        #endregion
    }

    #endregion
}