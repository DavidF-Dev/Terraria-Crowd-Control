using System;
using System.Collections.Generic;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Temporarily heat up the ground, dealing burning damage to the player.
/// </summary>
public sealed class FloorIsLavaEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int Range = 65; // X tiles

    #endregion

    #region Constructors

    public FloorIsLavaEffect(int duration) : base(EffectID.HotFloor, duration, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    protected override int StartEmote => EmoteID.EmoteFear;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.statLife < 20 || player.Player.IsInLiquid() || player.Player.shimmering)
        {
            return CrowdControlResponseStatus.Retry;
        }

        if (NetUtils.IsSinglePlayer)
        {
            player.Player.GetModPlayer<FloorIsLavaPlayer>().EnableFloorIsLava();
        }
        else
        {
            SendPacket(PacketID.HandleEffect, true);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        if (NetUtils.IsSinglePlayer)
        {
            GetLocalPlayer().Player.GetModPlayer<FloorIsLavaPlayer>().DisableFloorIsLava();
        }
        else
        {
            SendPacket(PacketID.HandleEffect, false);
        }
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        var enable = reader.ReadBoolean();
        if (enable)
        {
            player.Player.GetModPlayer<FloorIsLavaPlayer>().EnableFloorIsLava();
        }
        else
        {
            player.Player.GetModPlayer<FloorIsLavaPlayer>().DisableFloorIsLava();
        }
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.HellfireTreads, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class FloorIsLavaPlayer : ModPlayer
    {
        #region Static Fields and Constants

        private const int BounceDuration = 15;
        private const int HotButtDuration = 60 * 3;
        private const int BounceDamage = 10;
        private const float LavaBounceSpeedMin = 12;
        private const float LavaBounceSpeedMax = 14;
        private const float ButtBounceSpeedMin = 5;
        private const float ButtBounceSpeedMax = 9;

        #endregion

        #region Static Methods

        /// <summary>
        ///     Check if the player is currently on fire.
        /// </summary>
        public static bool IsOnFire(Player player)
        {
            return player.HasBuff(BuffID.Burning) || player.HasBuff(BuffID.OnFire) || player.HasBuff(BuffID.OnFire3);
        }

        #endregion

        #region Fields

        // Not synced but duration is so short that it shouldn't matter (for joining players)
        private bool _proxyFloorIsLavaEnabled;
        private bool _skipHurtSound;
        private int _bounceTime;
        private int _hotButtTime;

        #endregion

        #region Methods

        /// <summary>
        ///     Enable "Floor Is Lava" for the player (single-player and server).
        /// </summary>
        public void EnableFloorIsLava()
        {
            ModContent.GetInstance<FloorIsLavaSystem>().EnableForPlayer(Player);
        }

        /// <summary>
        ///     Disable "Floor Is Lava" for the player (single-player and server).
        /// </summary>
        public void DisableFloorIsLava()
        {
            ModContent.GetInstance<FloorIsLavaSystem>().DisableForPlayer(Player);
        }

        /// <summary>
        ///     Check if "Floor Is Lava" is enabled for the player.
        /// </summary>
        public bool IsFloorIsLavaEnabled()
        {
            return ModContent.GetInstance<FloorIsLavaSystem>().IsEnabledForPlayer(Player);
        }

        public override void PreUpdate()
        {
            if (IsFloorIsLavaEnabled())
            {
                _proxyFloorIsLavaEnabled = true;
                return;
            }

            var oldProxyFloorIsLava = _proxyFloorIsLavaEnabled;
            _proxyFloorIsLavaEnabled = false;
            for (var i = 0; i < Main.maxPlayers; i++)
            {
                var other = Main.player[i];
                if (!other.active || other.dead ||
                    !other.GetModPlayer<FloorIsLavaPlayer>().IsFloorIsLavaEnabled() ||
                    Player.Center.DistanceSQ(other.Center) >= Range * Range * 16 * 16)
                {
                    continue;
                }

                _proxyFloorIsLavaEnabled = true;
                break;
            }

            if (!NetUtils.IsServer && _proxyFloorIsLavaEnabled && !oldProxyFloorIsLava)
            {
                Player.Emote(EmoteID.EmoteFear);
            }
        }

        public override void PostUpdateBuffs()
        {
            // Decrease the bounce timer
            if (_bounceTime > 0)
            {
                _bounceTime--;
            }

            // Decrease the hot butt timer
            if (_hotButtTime > 0)
            {
                _hotButtTime--;

                // Set on fire
                Player.buffImmune[BuffID.OnFire] = false;
                Player.AddBuff(BuffID.OnFire, 2);

                // Force the player to keep moving
                Player.moveSpeed *= 0.9f; // -10%
                if (!Player.controlLeft && !Player.controlRight)
                {
                    if (Player.direction < 0)
                    {
                        Player.controlLeft = true;
                    }
                    else
                    {
                        Player.controlRight = true;
                    }
                }

                // Bouncy on land with a diminishing strength
                if (Player.IsGrounded() && Player.velocity.Y > 0)
                {
                    Player.velocity.Y = MathHelper.Lerp(ButtBounceSpeedMin, ButtBounceSpeedMax, Math.Clamp(_hotButtTime / (float)HotButtDuration, 0, 1)) * -1;

                    // Bounce effect
                    if (!NetUtils.IsServer)
                    {
                        for (var i = 0; i < 12; i++)
                        {
                            var dust = Dust.NewDust(Player.BottomLeft, 16 * 2, 1, DustID.Cloud, Scale: Main.rand.NextFloat(0.9f, 1.1f));
                            Main.dust[dust].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.GlommerBounce with {Volume = 0.6f}, Player.Bottom);
                    }
                }
            }

            if (!_proxyFloorIsLavaEnabled)
            {
                return;
            }

            // Cause the player to bounce off of lava
            // Use vanilla lava collision so it is exact
            if (!Player.shimmering && Collision.LavaCollision(Player.position, Player.width, Player.waterWalk ? Player.height - 6 : Player.height))
            {
                // Prevent normal lava damage whilst bouncing
                if (_bounceTime > 0)
                {
                    Player.lavaImmune = true;
                    Player.lavaTime = Math.Max(Player.lavaTime, 1);
                }

                // Only initiate a bounce if falling on top of lava
                if (Player.velocity.Y <= 0f ||
                    Main.tile[Player.TopLeft.ToTileCoordinates()].LiquidAmount != 0 ||
                    Main.tile[Player.TopRight.ToTileCoordinates()].LiquidAmount != 0)
                {
                    return;
                }

                // Reset bounce and hot butt timer
                var wasBouncing = _bounceTime > 0;
                _bounceTime = BounceDuration;
                _hotButtTime = HotButtDuration;

                if (Player.whoAmI == Main.myPlayer)
                {
                    Player.SetNoControl(new PlayerUtils.NoControlSetting
                    {
                        TimeLeft = _hotButtTime,
                        AllowHorizontalMovement = true
                    });
                }

                // Bounce on the lava (like in Mario 64)
                Player.position.Y -= Player.oldVelocity.Y;
                Player.velocity.Y = Main.rand.NextFloat(LavaBounceSpeedMin, LavaBounceSpeedMax) * -1;
                Player.RemoveAllGrapplingHooks();

                // Prevent normal lava damage
                Player.lavaImmune = true;
                Player.lavaTime = Math.Max(Player.lavaTime, 1);

                // Deal custom lava damage
                _skipHurtSound = !wasBouncing;
                Player.Hurt(PlayerDeathReason.ByOther(2), BounceDamage, 0, quiet: true, cooldownCounter: 4);
                _skipHurtSound = false;

                // Bounce effect
                if (!NetUtils.IsServer && !wasBouncing)
                {
                    SoundEngine.PlaySound(new SoundStyle("CrowdControlMod/Assets/Sounds/MarioBurn")
                    {
                        PitchRange = (-0.075f, 0.075f),
                        Volume = 0.8f,
                        MaxInstances = 2,
                        SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
                    }, Player.Center);
                }

                return;
            }

            // Burn the player if standing on hot ground or in hot liquid
            if (!Player.IsGrounded(false) && !Player.IsInLiquid(LiquidID.Water) && !Player.IsInLiquid(LiquidID.Honey))
            {
                return;
            }

            // Burn the player (faster when higher health)
            var wasOnFire = IsOnFire(Player);
            if (Player.statLife > 20)
            {
                Player.buffImmune[BuffID.Burning] = false;
                Player.AddBuff(BuffID.Burning, 2);
            }
            else
            {
                Player.buffImmune[BuffID.OnFire] = false;
                Player.AddBuff(BuffID.OnFire, 2);
            }

            // Burn effect
            if (!NetUtils.IsServer && !wasOnFire)
            {
                SoundEngine.PlaySound(SoundID.DD2_GoblinScream with
                {
                    MaxInstances = 1,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                }, Player.Center);
            }
        }

        public override void PostUpdateEquips()
        {
            if (!_proxyFloorIsLavaEnabled)
            {
                return;
            }

            if (!NetUtils.IsServer && IsOnFire(Player) && Player.IsGrounded())
            {
                Player.DoBootsEffect(Player.DoBootsEffect_PlaceFlamesOnTile);
            }
        }

        public override void PostUpdate()
        {
            if (!NetUtils.IsServer && _proxyFloorIsLavaEnabled)
            {
                Player.ManageSpecialBiomeVisuals("HeatDistortion", Main.UseHeatDistortion);
            }

            if (NetUtils.IsServer || !IsFloorIsLavaEnabled())
            {
                return;
            }

            // Effects
            var tile = Player.Center.ToTileCoordinates();
            for (var x = tile.X - Range; x < tile.X + Range; x++)
            {
                for (var y = tile.Y - Range; y < tile.Y + Range; y++)
                {
                    if (x < 0 || x >= Main.tile.Width || y < 1 || y >= Main.tile.Height)
                    {
                        continue;
                    }

                    if (Vector2.DistanceSquared(Player.Center, new Vector2(x * 16, y * 16)) >= Range * Range * 16 * 16)
                    {
                        continue;
                    }

                    // Hot ground effect
                    if (Main.rand.NextBool(8) &&
                        Main.tile[x, y - 1].LiquidAmount == 0 &&
                        Collision.IsWorldPointSolid(new Vector2(x * 16, y * 16), true) &&
                        !Collision.IsWorldPointSolid(new Vector2(x * 16, (y - 1) * 16), true))
                    {
                        var dustType = Main.rand.NextFromList(DustID.Torch, DustID.RedTorch, DustID.CrimsonTorch);
                        var dust = Dust.NewDust(new Vector2(x * 16, y * 16 - 4), 16, 1, dustType, SpeedY: -0.65f, Scale: Main.rand.NextFloat(1.15f, 1.25f));
                        Main.dust[dust].noGravity = true;
                    }

                    // Hot liquid effect
                    if (Main.rand.NextBool(18) &&
                        Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType is LiquidID.Water or LiquidID.Honey)
                    {
                        var dust = Dust.NewDust(new Vector2(x * 16, y * 16), 16, 16, DustID.Cloud, Scale: Main.rand.NextFloat(0.5f, 0.8f));
                        Main.dust[dust].noGravity = true;
                    }
                }
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!_skipHurtSound)
            {
                return;
            }

            _skipHurtSound = false;
            modifiers.DisableSound();
        }

        #endregion
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class FloorIsLavaSystem : ModSystem
    {
        #region Fields

        private readonly HashSet<byte> _enabledPlayers = new();
        private readonly HashSet<byte> _toRemovePlayers = new();

        #endregion

        #region Methods

        public void EnableForPlayer(Player player)
        {
            if (NetUtils.IsClient || !_enabledPlayers.Add((byte)player.whoAmI))
            {
                return;
            }

            if (NetUtils.IsServer)
            {
                NetMessage.SendData(MessageID.WorldData);
            }

            // TerrariaUtils.WriteDebug($"Floor Is Lava enabled for player: {player.name}");
        }

        public void DisableForPlayer(Player player)
        {
            if (NetUtils.IsClient || !_enabledPlayers.Remove((byte)player.whoAmI))
            {
                return;
            }

            if (NetUtils.IsServer)
            {
                NetMessage.SendData(MessageID.WorldData);
            }

            // TerrariaUtils.WriteDebug($"Floor Is Lava disabled for player: {player.name}");
        }

        public bool IsEnabledForPlayer(Player player)
        {
            return _enabledPlayers.Contains((byte)player.whoAmI);
        }

        public override void ClearWorld()
        {
            _enabledPlayers.Clear();
            _toRemovePlayers.Clear();
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(_enabledPlayers.Count);
            foreach (var whoAmI in _enabledPlayers)
            {
                writer.Write(whoAmI);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            _enabledPlayers.Clear();
            var c = reader.ReadInt32();
            for (; c > 0; c--)
            {
                _enabledPlayers.Add(reader.ReadByte());
            }
        }

        public override void PostUpdateEverything()
        {
            if (_enabledPlayers.Count == 0)
            {
                return;
            }

            foreach (var whoAmI in _enabledPlayers)
            {
                if (!Main.player[whoAmI].active)
                {
                    _toRemovePlayers.Add(whoAmI);
                }
            }

            if (_toRemovePlayers.Count == 0)
            {
                return;
            }

            foreach (var whoAmI in _toRemovePlayers)
            {
                _enabledPlayers.Remove(whoAmI);
            }

            _toRemovePlayers.Clear();
        }

        #endregion
    }

    #endregion
}