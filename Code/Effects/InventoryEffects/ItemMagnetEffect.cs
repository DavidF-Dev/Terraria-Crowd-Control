using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.InventoryEffects;

public sealed class ItemMagnetEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float PullRange = 16 * 16 * 48 * 48;
    private const float PullStrength = 0.4f;
    private const float PullStrengthMax = 8.5f;
    private const float PushRange = 16 * 16 * 6.5f * 6.5f;
    private const float PushStrengthMax = 10f;

    #endregion

    #region Fields

    private readonly bool _pull;

    #endregion

    #region Constructors

    public ItemMagnetEffect(bool pull, int duration) : base(pull ? EffectID.ItemPull : EffectID.ItemPush, duration, EffectSeverity.Neutral)
    {
        _pull = pull;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        return (_pull && !CrowdControlMod.GetInstance().IsEffectActive(EffectID.ItemPush)) ||
               (!_pull && !CrowdControlMod.GetInstance().IsEffectActive(EffectID.ItemPull))
            ? CrowdControlResponseStatus.Success
            : CrowdControlResponseStatus.Retry;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var itemId = _pull ? ItemID.TreasureMagnet : ItemID.PutridScent;
        TerrariaUtils.WriteEffectMessage(itemId, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once UnusedType.Local
    private sealed class ItemMagnet : GlobalItem
    {
        #region Properties

        /// <summary>
        ///     Player index of the magnet target.
        /// </summary>
        public int TargetIndex { get; private set; }

        /// <summary>
        ///     Is being pulled towards the target.
        /// </summary>
        public bool Pull { get; private set; }

        /// <summary>
        ///     Is being pushed away from the target.
        /// </summary>
        public bool Push { get; private set; }

        /// <summary>
        ///     Magnet is active.
        /// </summary>
        public bool IsActive => TargetIndex is >= 0 and < Main.maxPlayers && (Pull || Push);

        public override bool InstancePerEntity => true;

        #endregion

        #region Methods

        public override void SetDefaults(Item entity)
        {
            TargetIndex = Main.maxPlayers;
            Pull = false;
            Push = false;
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(TargetIndex);
            writer.Write(new BitsByte
            {
                [0] = Pull,
                [1] = Push
            });
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            TargetIndex = reader.Read7BitEncodedInt();
            BitsByte flags = reader.ReadByte();
            Pull = flags[0];
            Push = flags[1];
        }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (!NetUtils.IsClient && CheckMagnetState(item))
            {
                // Sync changes
                if (NetUtils.IsServer)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI, item.noGrabDelay > 0 ? 1 : 0);
                }

                // ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{TerrariaUtils.GetItemRichText((short)item.type)} Magnet state has changed (Active={IsActive})"), Color.White);
            }

            if (!IsActive)
            {
                // Ignore
                return;
            }

            // Override vanilla behaviour
            gravity = 0;
            maxFallSpeed = float.MaxValue;

            // Edit the item's velocity
            var target = Main.player[TargetIndex];
            var targetPosition = target.Center;
            if (Pull)
            {
                // Pull the item towards the target
                item.velocity += item.Center.DirectionTo(targetPosition) * PullStrength;
                if (item.velocity.Length() > PullStrengthMax)
                {
                    item.velocity = Vector2.Normalize(item.velocity) * PullStrengthMax;
                }
            }
            else
            {
                // Modify the target for a more optimal repel effect
                targetPosition.X += (item.Center.X < targetPosition.X ? 16 : -16) * 5f;
                targetPosition.Y += 5 * 16;

                // Push the item away from the target
                item.velocity = targetPosition.DirectionTo(item.Center) * PushStrengthMax;

                if ((item.position + item.velocity).DistanceSQ(target.Center) > PullRange)
                {
                    item.velocity /= 0.4f;
                }
            }

            // Sync item position periodically
            if (NetUtils.IsServer && Main.GameUpdateCount % 30 == 0)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI, item.noGrabDelay > 0 ? 1 : 0);
            }

            if (!NetUtils.IsServer)
            {
                return;
            }

            // Add client-side effects
            if (Main.GameUpdateCount % 12 == 0)
            {
                const float increment = MathHelper.TwoPi / 20f;
                for (var i = 0f; i < MathHelper.TwoPi; i += increment)
                {
                    var pos = target.Center + Vector2.One.RotatedBy(i) * 16 * 3;
                    var vel = pos.DirectionTo(target.Center) * 1f * (Pull ? 1 : -1);
                    Dust.NewDustPerfect(pos, DustID.TreasureSparkle, vel);
                }

                for (var i = 0; i < 5; i++)
                {
                    Dust.NewDust(target.position, target.width, target.height, DustID.Teleporter);
                }
            }
        }

        public override void GrabRange(Item item, Player player, ref int grabRange)
        {
            if (!IsActive || TargetIndex != player.whoAmI)
            {
                // Ignore
                return;
            }

            // Modify grab range if magnet is active
            if (Pull)
            {
                grabRange = 1;
            }
            else if (Push)
            {
                grabRange += 2;
            }
        }

        public override bool CanStackInWorld(Item destination, Item source)
        {
            return !IsActive;
        }

        public override bool OnPickup(Item item, Player player)
        {
            TargetIndex = Main.maxPlayers;
            Pull = false;
            Push = false;
            return base.OnPickup(item, player);
        }

        private bool CheckMagnetState(Item item)
        {
            // Check if the effect is active
            var pullEffect = CrowdControlMod.GetInstance().GetEffect(EffectID.ItemPull);
            var pushEffect = CrowdControlMod.GetInstance().GetEffect(EffectID.ItemPush);
            var pull = pullEffect != null && ((NetUtils.IsSinglePlayer && pullEffect.IsActive) || (NetUtils.IsServer && pullEffect.IsActiveOnServer()));
            var push = pushEffect != null && ((NetUtils.IsSinglePlayer && pushEffect.IsActive) || (NetUtils.IsServer && pushEffect.IsActiveOnServer()));
            var hasChanged = false;

            // Check if the item's pull state has changed
            if (pull != Pull)
            {
                Pull = pull;
                hasChanged = true;
            }

            // Check if the item's push state has changed
            if (push != Push)
            {
                Push = push;
                hasChanged = true;
            }

            // Ignore the target if the magnet is not active
            if (!Push && !Pull)
            {
                return hasChanged;
            }

            // Find the closest target within range
            var closestTarget = Main.maxPlayers;
            var closestDistSq = float.MaxValue;
            for (var i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (!player.active || player.dead)
                {
                    // Ignore
                    continue;
                }

                var distSq = player.Center.DistanceSQ(item.Center);
                if ((closestTarget == Main.maxPlayers && distSq > (pull ? PullRange : PushRange)) || (closestTarget != Main.maxPlayers && distSq >= closestDistSq))
                {
                    // Ignore
                    continue;
                }

                // Found a closer target
                closestTarget = player.whoAmI;
                closestDistSq = distSq;
            }

            // Check if the item's target has changed
            if (closestTarget != TargetIndex)
            {
                TargetIndex = closestTarget;
                hasChanged = true;
            }

            return hasChanged;
        }

        #endregion
    }

    #endregion
}