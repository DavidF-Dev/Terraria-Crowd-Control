using System;
using System.IO;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace CrowdControlMod.Code.Utilities;

public static class ResizedPlayerUtils
{
    #region Static Methods

    /// <summary>
    ///     Set the player's custom scale.
    /// </summary>
    public static void SetScale(Player player, float scale)
    {
        if (!NetUtils.IsServer && player.whoAmI != Main.myPlayer)
        {
            // Not allowed
            return;
        }

        var resizedPlayer = player.GetModPlayer<ResizedPlayer>();
        if (Math.Abs(scale - resizedPlayer.Scale) < float.Epsilon)
        {
            // No change
            return;
        }

        resizedPlayer.Scale = Math.Max(scale, 0.05f);
        resizedPlayer.OnScaleChanged();
        if (!NetUtils.IsSinglePlayer)
        {
            resizedPlayer.SyncScale();
        }
    }

    /// <summary>
    ///     Reset the player's custom scale.
    /// </summary>
    public static void ResetScale(Player player)
    {
        SetScale(player, 1);
    }

    /// <summary>
    ///     Get the player's custom scale.
    /// </summary>
    public static float GetScale(Player player)
    {
        return player.GetModPlayer<ResizedPlayer>().Scale;
    }

    /// <summary>
    ///     Check if the player has a custom scale set.
    /// </summary>
    public static bool GetIsScaled(Player player)
    {
        return player.GetModPlayer<ResizedPlayer>().IsScaled;
    }

    public static void HandleSync(BinaryReader reader)
    {
        var whoAmI = reader.ReadByte();
        var scale = reader.ReadSingle();
        var resizedPlayer = Main.player[whoAmI].GetModPlayer<ResizedPlayer>();
        resizedPlayer.Scale = scale;
        resizedPlayer.OnScaleChanged();
        if (NetUtils.IsServer)
        {
            resizedPlayer.SyncScale(-1, whoAmI);
        }
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class ResizedPlayer : ModPlayer
    {
        #region Static Methods

        private static void ResetPlayerSize(Player player)
        {
            player.position = player.BottomLeft;
            player.width = Player.defaultWidth;
            player.height = Player.defaultHeight;
            player.BottomLeft = player.position;
        }

        private static void ApplyPlayerSize(Player player, float scale)
        {
            // https://github.com/NotLe0n/Creativetools/blob/1.4.4/src/Tools/Modify/ModifyPlayer.cs
            player.position = player.BottomLeft;
            player.width = (int)(Player.defaultWidth * scale);
            player.height = (int)(Player.defaultHeight * scale);
            player.BottomLeft = player.position;
        }

        private static void OnDrawPlayer(On_LegacyPlayerRenderer.orig_DrawPlayerInternal orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float alpha, float scale, bool headOnly)
        {
            if (drawPlayer.TryGetModPlayer(out ResizedPlayer resizedPlayer) && resizedPlayer.IsScaled)
            {
                ResetPlayerSize(drawPlayer);
                ApplyPlayerSize(drawPlayer, resizedPlayer.Scale);
                scale *= resizedPlayer.Scale;
            }

            orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
        }

        #endregion

        #region Fields

        public float Scale = 1;

        #endregion

        #region Properties

        public bool IsScaled => Scale is < 1 or > 1;

        #endregion

        #region Methods

        public void OnScaleChanged()
        {
            ResetPlayerSize(Player);
        }

#if DEBUG
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (triggersSet.Hotbar8)
            {
                SetScale(Player, 0.1f);
            }
            else if (triggersSet.Hotbar9)
            {
                ResetScale(Player);
            }
            else if (triggersSet.Hotbar10)
            {
                SetScale(Player, 3f);
            }
        }
#endif

        public override void Load()
        {
            On_LegacyPlayerRenderer.DrawPlayerInternal += OnDrawPlayer;
        }

        public override void Initialize()
        {
            Scale = 1;
            OnScaleChanged();
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (IsScaled)
            {
                SyncScale(toWho, fromWho);
            }
        }

        public override void PreUpdateMovement()
        {
            if (IsScaled)
            {
                ApplyPlayerSize(Player, Scale);
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (IsScaled)
            {
                drawInfo.ItemLocation.Y += Player.defaultHeight * Scale * 0.5f * (Scale > 1 ? 1 : -1);
            }
        }

        public void SyncScale(int toClient = -1, int ignoreClient = -1)
        {
            var packet = Mod.GetPacket();
            packet.Write((byte)PacketID.SyncResizedPlayer);
            packet.Write((byte)Player.whoAmI);
            packet.Write(Scale);
            packet.Send(toClient, ignoreClient);
        }

        #endregion
    }

    #endregion
}