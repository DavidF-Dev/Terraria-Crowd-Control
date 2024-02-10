using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

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

    private static void HandleFart(Player player, bool forceSpawnPooItem = false)
    {
        // Provide stinky buff for a short time (also happens client-side)
        player.AddBuff(BuffID.Stinky, 100);

        // If not well fed, then there's a chance that no poo-related effects will happen
        var isWellFed = player.HasBuff(BuffID.WellFed) || player.HasBuff(BuffID.WellFed2) || player.HasBuff(BuffID.WellFed3);
        if (!forceSpawnPooItem && !isWellFed && !Main.rand.NextBool(3))
        {
            return;
        }

        // Spawn a poo projectile
        var pooCount = NetUtils.IsSinglePlayer && SteamUtils.IsPixyWixy ? 5 : 1;
        for (var i = 0; i < pooCount; i++)
        {
            var pooProjIndex = Projectile.NewProjectile(null, Main.rand.NextVector2FromRectangle(player.Hitbox), Vector2.Zero, ProjectileID.ToiletEffect, 0, 0f, player.whoAmI);
            if (NetUtils.IsServer)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, pooProjIndex);
            }
        }

        // Check well fed
        if (!forceSpawnPooItem && !isWellFed)
        {
            return;
        }

        // Spawn a poo item
        var pooItemIndex = Item.NewItem(null, player.position, player.width, player.height, ItemID.PoopBlock, noBroadcast: true);
        Main.item[pooItemIndex].velocity = new Vector2(-player.direction * 2f, -2f);
        Main.item[pooItemIndex].noGrabDelay = 60 * 4;
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

    public FartEffect() : base(EffectID.FartSound, 0, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Player;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        // Play fart sound effect now
        HandleClientFart(GetLocalPlayer().Player);

        if (NetUtils.IsSinglePlayer)
        {
            // Trigger effects
            HandleFart(GetLocalPlayer().Player, SteamUtils.IsPixyWixy);

            if (SteamUtils.IsPixyWixy)
            {
                ModContent.GetInstance<PixyWixyGlobalFartSystem>().Activate();
            }
        }
        else
        {
            // Tell server to notify other clients that the local player has farted
            // Will tell clients to play sfx and will handle effects server-side
            SendPacket(PacketID.HandleEffect);
        }

        // If hiccuping, enable fart hiccups for the remainder of the effect
        if (CrowdControlMod.GetInstance().GetEffect(EffectID.Hiccup) is HiccupEffect {IsActive: true} hiccupEffect)
        {
            hiccupEffect.HicFart = true;
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

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        if (!SteamUtils.IsPixyWixy || !NetUtils.IsSinglePlayer || !Main.rand.NextBool(3))
        {
            return;
        }

        TerrariaUtils.WriteEffectMessage(ItemID.PoopBlock, LangUtils.GetEffectMiscText(Id, "PixyWixyFart." + Main.rand.Next(11), playerString), Severity);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class PixyWixyGlobalFartSystem : ModSystem
    {
        #region Static Fields and Constants

        private const int MinDelay = 5;
        private const int MaxDelay = 15;

        #endregion

        #region Static Methods

        private static void SpawnFlies()
        {
            foreach (var (x, y) in Main.LocalPlayer.GetTilesAround(150))
            {
                // Note: Spawning in air causes game to crash
                if (y > 1 && Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile)
                {
                    ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.PooFly, new ParticleOrchestraSettings
                    {
                        IndexOfPlayerWhoInvokedThis = (byte)Main.myPlayer,
                        MovementVector = Vector2.Zero,
                        PositionInWorld = new Vector2(x, y) * 16,
                        UniqueInfoPiece = 0
                    });
                }
            }
        }

        private static void SpawnFartClouds()
        {
            foreach (var (x, y) in Main.LocalPlayer.GetTilesAround(150))
            {
                if (!Main.tile[x, y].HasTile && Main.rand.NextBool(6))
                {
                    Dust.NewDust(new Vector2(x, y) * 16, 16, 16, DustID.FartInAJar);
                }
            }
        }

        #endregion

        #region Fields

        private bool _enabled;
        private int _timer;
        private int _index;
        private int _processed;
        private int _nameCounter;

        #endregion

        #region Methods

        public void Activate()
        {
            if (!NetUtils.IsSinglePlayer || _enabled)
            {
                // Unsupported in multiplayer
                return;
            }

            _enabled = true;
            _timer = MinDelay;
            _index = 0;
            _processed = 0;
            _nameCounter = 0;

            // Spawn poo fly particles and fart in a jar dust around the player
            SpawnFlies();
            SpawnFartClouds();
        }

        public override void PostUpdateWorld()
        {
            if (!_enabled)
            {
                return;
            }

            _timer--;
            if (_timer > 0)
            {
                return;
            }

            do
            {
                var npc = Main.npc[_index++];
                if (!npc.active || npc.life <= 0 || npc.hide || npc.alpha == 255)
                {
                    continue;
                }

                if (npc.Center.DistanceSQ(Main.LocalPlayer.Center) >= 120 * 120 * 16 * 16)
                {
                    continue;
                }

                // Play fart sound with a bit of variation
                SoundEngine.PlaySound(SoundID.Item16 with
                {
                    PlayOnlyIfFocused = false,
                    MaxInstances = int.MaxValue,
                    Pitch = Main.rand.NextFloat(-0.90f, 0.05f)
                }, npc.Center);

                // Provide stinky buff for a short time
                npc.AddBuff(BuffID.Stinky, 60 * 4);

                // Poison hostile npcs for a short time
                if (npc is {friendly: false, damage: > 0})
                {
                    npc.AddBuff(BuffID.Poisoned, 60 * 4);
                }

                // Spawn poo projectiles
                var pooCount = Main.rand.Next(3, 6);
                for (var i = 0; i < pooCount; i++)
                {
                    Projectile.NewProjectile(null, Main.rand.NextVector2FromRectangle(npc.Hitbox), Vector2.Zero, ProjectileID.ToiletEffect, 0, 0f);
                }

                if (npc.townNPC || Main.rand.NextBool(4))
                {
                    // Choose a name for the poo
                    var names = CrowdControlMod.GetInstance().GetRememberedViewerNames();
                    var name = names.Count == 0 ? Main.LocalPlayer.name : names[names.Count - 1 - _nameCounter++ % Math.Min(names.Count, 4)];

                    // Spawn a poo item
                    var pooItemIndex = Item.NewItem(null, npc.position, npc.width, npc.height, ItemID.PoopBlock, noBroadcast: true);
                    Main.item[pooItemIndex].velocity = new Vector2(-npc.direction * 2f, -2f);
                    Main.item[pooItemIndex].noGrabDelay = 60 * 4;
                    Main.item[pooItemIndex].SetItemOwner(name);
                }

                _timer = Main.rand.Next(MinDelay, MaxDelay);
                _processed++;
                break;
            } while (_index < Main.maxNPCs);

            _enabled = _index < Main.maxNPCs;
            if (!_enabled || _processed % 6 == 0)
            {
                SpawnFartClouds();
            }
        }

        #endregion
    }

    #endregion
}