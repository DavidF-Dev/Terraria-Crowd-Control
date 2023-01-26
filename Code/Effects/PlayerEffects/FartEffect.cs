using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Effect that makes the local player fart. This produces a sound effect and other visuals. Works in multiplayer.
/// </summary>
public sealed class FartEffect : CrowdControlEffect
{
    #region Static Methods

    /// <summary>
    ///     Simply play a fart sound effect at the provided player's position (client-side).<br />
    ///     Also adds buff and emotes if needed.
    /// </summary>
    public static void HandleClientFart(Player player)
    {
        if (NetUtils.IsServer)
        {
            // Ignore
            return;
        }

        // Play fart sound with a bit of variation
        SoundEngine.PlaySound(SoundID.Item16 with
        {
            PlayOnlyIfFocused = false,
            MaxInstances = int.MaxValue,
            Pitch = Main.rand.NextFloat(-0.90f, 0.05f)
        }, player.Center);

        // Provide stinky buff for a short time
        player.AddBuff(BuffID.Stinky, 100);

        // Emote if our local player is close to the farting player
        if (Main.myPlayer != player.whoAmI && Main.LocalPlayer.DistanceSQ(player.Center) < 16f * 16f * 10f * 10f)
        {
            Main.LocalPlayer.Emote(EmoteID.DebuffPoison, 180);
        }
    }

    private static void HandleFart(Player player)
    {
        // Provide stinky buff for a short time (also happens client-side)
        player.AddBuff(BuffID.Stinky, 100);

        // If not well fed, then there's a chance that no poo-related effects will happen
        var isWellFed = player.HasBuff(BuffID.WellFed) || player.HasBuff(BuffID.WellFed2) || player.HasBuff(BuffID.WellFed3);
        if (!isWellFed && !Main.rand.NextBool(3))
        {
            return;
        }

        // Spawn a poo projectile
        var pooProjIndex = Projectile.NewProjectile(null, player.Center, Vector2.Zero, ProjectileID.ToiletEffect, 0, 0f, player.whoAmI);
        if (NetUtils.IsServer)
        {
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, pooProjIndex);
        }

        // Check well fed
        if (!isWellFed)
        {
            return;
        }

        // Spawn a poo item
        // TODO: Change to actual poo item in 1.4.4
        var pooItemIndex = Item.NewItem(null, player.position, player.width, player.height, ItemID.StinkPotion, noBroadcast: true);
        Main.item[pooItemIndex].velocity = new Vector2(-player.direction * 2f, -2f);
        Main.item[pooItemIndex].noGrabDelay = 60 * 6;
        Main.item[pooItemIndex].SetItemOwner(player.name);

        if (!NetUtils.IsServer)
        {
            return;
        }

        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, pooItemIndex, 1f);
        NetUtils.SyncItemSpecial(pooItemIndex);
    }

    #endregion

    #region Constructors

    public FartEffect() : base(EffectID.FartSound, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Play fart sound effect now
        HandleClientFart(GetLocalPlayer().Player);

        if (NetUtils.IsSinglePlayer)
        {
            // Trigger effects
            HandleFart(GetLocalPlayer().Player);
        }
        else
        {
            // Tell server to notify other clients that the local player has farted
            // Will tell clients to play sfx and will handle effects server-side
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Send the fart packet to other clients (handled in CrowdControlMod.HandleClientPacket)
        // This will cause clients to play the fart sfx
        NetUtils.MakeFart(player.Player, -1, player.Player.whoAmI);

        // Handle effects server-side
        HandleFart(player.Player);
    }

    #endregion
}