using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Spawn a flying piggy bank at the player's position.
/// </summary>
public sealed class SpawnPiggyBankEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float PiggySpeedX = 2.8f;
    private const float PiggySpeedY = -2.4f;
    private const int PiggyMovementTimer = 60 * 12;
    private const int PiggyTargetOffsetX = 0;
    private const int PiggyTargetOffsetY = 16 * -2;
    private const int PiggyMoveThreshold = 16 * 16 * 16 * 16;
    private const int PiggyStopThreshold = 16 * 16 * 3 * 3;
    private const float PiggyMoveSpeed = 2.78f;
    private const float PiggyTextOffsetX = 0;
    private const float PiggyTextOffsetY = 16 * -2.5f;
    private const int PiggyTextSpawnDuration = 60 * 2;
    private const int PiggyTextStartDuration = 60 * 2;
    private const int PiggyTextStopDuration = 60 * 4;
    private static readonly Color PiggyTextColour = Color.LightPink;

    #endregion

    #region Static Methods

    private static void SpawnPiggyBank(Player player)
    {
        var spawnPos = player.Center;
        var spawnVel = new Vector2(player.direction * PiggySpeedX, PiggySpeedY);

        // Spawn a money trough that follows the player for a duration of time
        var index = Projectile.NewProjectile(null, spawnPos, spawnVel, ProjectileID.FlyingPiggyBank, 1, 1, player.whoAmI);
        var inst = Main.projectile[index].GetGlobalProjectile<FlyingPiggyBankProj>();
        inst.IsSpawnedByEffect = true;
        inst.MovementTimer = PiggyMovementTimer;

        // Sync
        if (NetUtils.IsServer)
        {
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
        }
    }

    #endregion

    #region Constructors

    public SpawnPiggyBankEffect() : base(EffectID.SpawnPiggyBank, 0, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    private static SoundStyle PiggySound => SoundID.Item59 with {MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew};

    protected override int StartEmote => EmoteID.ItemGoldpile;

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (NetUtils.IsSinglePlayer)
        {
            SpawnPiggyBank(player.Player);
        }
        else
        {
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.PiggyBank, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        SpawnPiggyBank(player.Player);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class FlyingPiggyBankProj : GlobalProjectile
    {
        #region Static Methods

        // ReSharper disable once InconsistentNaming
        private static void ResetVanillaAI(Projectile proj)
        {
            Array.Fill(proj.ai, 0);
        }

        #endregion

        #region Fields

        public bool IsSpawnedByEffect;
        public int MovementTimer;
        private bool _isMoving;
        private bool _hasPlayedSpawnEffect;

        #endregion

        #region Properties

        public override bool InstancePerEntity => true;

        #endregion

        #region Methods

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return entity.type == ProjectileID.FlyingPiggyBank;
        }

        public override bool PreAI(Projectile proj)
        {
            if (!IsSpawnedByEffect || MovementTimer <= 0)
            {
                // Vanilla behaviour
                return base.PreAI(proj);
            }

            // Check if the owner exists
            var owner = Main.player[proj.owner];
            if (!owner.active || owner.dead)
            {
                if (_isMoving)
                {
                    _isMoving = false;
                    ResetVanillaAI(proj);
                }

                MovementTimer = 0;
                return base.PreAI(proj);
            }

            // Check if just spawned
            if (!_hasPlayedSpawnEffect)
            {
                _hasPlayedSpawnEffect = true;

                // Spawn client-side effects
                if (!NetUtils.IsServer)
                {
                    SoundEngine.PlaySound(PiggySound, proj.Center);
                    PopupText.NewText(new AdvancedPopupRequest
                    {
                        Text = LangUtils.GetEffectMiscText(EffectID.SpawnPiggyBank, "Spawn"),
                        Color = PiggyTextColour,
                        DurationInFrames = PiggyTextSpawnDuration,
                        Velocity = proj.velocity + new Vector2(0.15f * Math.Sign(proj.velocity.X), -0.25f)
                    }, proj.Top + new Vector2(proj.spriteDirection * PiggyTextOffsetX, PiggyTextOffsetY));
                }
            }

            // Check if the movement state should change by checking the distance between the piggy and its owner
            var target = owner.Top + new Vector2(owner.direction * PiggyTargetOffsetX, PiggyTargetOffsetY);
            var dist = proj.Center.DistanceSQ(target);
            switch (_isMoving)
            {
                case false when dist > PiggyMoveThreshold:
                    _isMoving = true;

                    // Movement started client-side effects
                    if (!NetUtils.IsServer)
                    {
                        SoundEngine.PlaySound(PiggySound with {Pitch = 0.175f}, proj.Center);
                        PopupText.NewText(new AdvancedPopupRequest
                        {
                            Text = LangUtils.GetEffectMiscText(EffectID.SpawnPiggyBank, "WaitForMe"),
                            Color = PiggyTextColour,
                            DurationInFrames = PiggyTextStartDuration,
                            Velocity = proj.Center.DirectionTo(target) * PiggyMoveSpeed
                        }, proj.Top + new Vector2(proj.spriteDirection * PiggyTextOffsetX, PiggyTextOffsetY));
                    }

                    break;
                case true when dist < PiggyStopThreshold:
                    _isMoving = false;
                    ResetVanillaAI(proj);
                    MovementTimer = Math.Max(MovementTimer, 60 * 4);

                    // Movement stopped client-side effects
                    if (!NetUtils.IsServer)
                    {
                        SoundEngine.PlaySound(PiggySound, proj.Center);
                    }

                    break;
            }

            if (!_isMoving)
            {
                // Use the vanilla behaviour
                return base.PreAI(proj);
            }

            // Move the piggy towards the target
            proj.velocity = proj.Center.DirectionTo(target) * PiggyMoveSpeed;

            // Reduce the movement timer whilst the piggy is moving
            MovementTimer--;
            if (MovementTimer > 0)
            {
                // Flying client-side effects
                if (!NetUtils.IsServer && Main.rand.NextBool(5))
                {
                    var dust = Main.rand.NextFromList(DustID.PlatinumCoin, DustID.GoldCoin, DustID.SilverCoin, DustID.CopperCoin, DustID.GoldCritter);
                    Dust.NewDust(proj.position, proj.width, proj.height, dust, Scale: Main.rand.NextFloat(1f, 1.1f));
                }

                return base.PreAI(proj);
            }

            // Movement timer ran out - out of breath
            MovementTimer = 0;
            ResetVanillaAI(proj);

            if (NetUtils.IsServer)
            {
                return base.PreAI(proj);
            }

            // Out of breath client-side effects
            SoundEngine.PlaySound(PiggySound with {Pitch = -0.175f, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest}, proj.Center);
            PopupText.NewText(new AdvancedPopupRequest
            {
                Text = LangUtils.GetEffectMiscText(EffectID.SpawnPiggyBank, "OutOfBreath"),
                Color = PiggyTextColour,
                DurationInFrames = PiggyTextStopDuration,
                Velocity = Vector2.Zero
            }, proj.Top + new Vector2(proj.spriteDirection * PiggyTextOffsetX, PiggyTextOffsetY) + proj.velocity * 10);

            return base.PreAI(proj);
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(new BitsByte
            {
                [0] = IsSpawnedByEffect,
                [1] = _isMoving,
                [2] = _hasPlayedSpawnEffect
            });
            binaryWriter.Write7BitEncodedInt(MovementTimer);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            BitsByte flags = binaryReader.ReadByte();
            IsSpawnedByEffect = flags[0];
            _isMoving = flags[1];
            _hasPlayedSpawnEffect = flags[2];
            MovementTimer = binaryReader.Read7BitEncodedInt();
        }

        #endregion
    }

    #endregion
}