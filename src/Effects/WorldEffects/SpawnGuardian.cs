using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Spawn a real or fake dungeon guardian at the edge of the player's screen.
///     The dungeon guardian will de-spawn after a short duration.
/// </summary>
public sealed class SpawnGuardian : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float HalfRangeWidth = 16f * 35f;
    private const float HalfRangeHeight = 16f * 90f;
    private const int SurvivalDuration = 6;

    #endregion

    #region Fields

    private readonly bool _isFake;

    #endregion

    #region Constructors

    public SpawnGuardian(bool isFake) : base(isFake ? EffectID.SpawnFakeGuardian : EffectID.SpawnGuardian, null, EffectSeverity.Negative)
    {
        _isFake = isFake;
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // In single-player, simply spawn the custom dungeon guardian
            Spawn(GetLocalPlayer());
        }
        else
        {
            // If on server, spawn on server (no need to pass arguments)
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.Skull, $"{viewerString} spawned a Dungeon Guardian", Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the dungeon guardian on the server
        Spawn(player);
    }

    private void Spawn(CrowdControlPlayer player)
    {
        // Determine spawn position
        var circleEdge = Main.rand.NextVector2CircularEdge(HalfRangeWidth, HalfRangeHeight);
        var spawnPos = new Point((int)(player.Player.position.X + circleEdge.X), (int)(player.Player.position.Y + circleEdge.Y));

        // Spawn the dungeon guardian
        var index = NPC.NewNPC(null, spawnPos.X, spawnPos.Y, CrowdControlMod.GetInstance().Find<ModNPC>(nameof(CrowdControlGuardian)).Type);
        var npc = Main.npc[index];

        // Set the target
        npc.ai[NPC.maxAI - 1] = player.Player.whoAmI;
        npc.target = player.Player.whoAmI;

        // Set whether it is fake or not
        var guardian = (CrowdControlGuardian)npc.ModNPC;
        guardian.IsFake = _isFake;

        // This is only invoked by whoever spawned the guardian (single-player or server)
        guardian.FakeGuardianDied += () =>
        {
            const string message = "The Dungeon Guardian was a phony";
            switch (Main.netMode)
            {
                case NetmodeID.SinglePlayer:
                    TerrariaUtils.WriteEffectMessage(ItemID.WhoopieCushion, message, EffectSeverity.Neutral);
                    break;
                case NetmodeID.Server:
                    TerrariaUtils.SendEffectMessage(player, ItemID.WhoopieCushion, message, EffectSeverity.Neutral);
                    break;
            }
        };

        if (Main.netMode == NetmodeID.Server)
        {
            // Notify the server
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class CrowdControlGuardian : ModNPC
    {
        #region Fields

        private int _timeLeft;

        #endregion

        #region Properties

        /// <summary>
        ///     Whether the guardian is fake (stored in the second last ai slot).
        /// </summary>
        public bool IsFake
        {
            get => NPC.ai[NPC.maxAI - 2] > 0f;
            set => NPC.ai[NPC.maxAI - 2] = value ? 1f : 0f;
        }

        public override string Texture => $"Terraria/Images/NPC_{NPCID.DungeonGuardian}";

        #endregion

        #region Events

        public event Action? FakeGuardianDied;

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dungeon Guardian");
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DungeonGuardian);
            AIType = NPCID.DungeonGuardian;
            NPC.aiStyle = 11;
            _timeLeft = 60 * SurvivalDuration;
            NPC.boss = false;
            NPC.BossBar = null;
        }

        public override bool PreAI()
        {
            NPC.type = NPCID.DungeonGuardian;

            // Reduce the time left timer
            _timeLeft--;
            if (_timeLeft != 0)
            {
                return base.PreAI();
            }

            // Kill the dungeon guardian
            NPC.ai[1] = 3f;
            if (IsFake)
            {
                FakeGuardianDied?.Invoke();
            }

            return base.PreAI();
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            // Ignore player if fake
            return !IsFake;
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Ignore others if fake
            return IsFake ? false : null;
        }

        #endregion
    }

    #endregion
}