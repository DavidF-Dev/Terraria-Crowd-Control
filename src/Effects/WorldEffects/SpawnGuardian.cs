﻿using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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
    private const float HalfRangeHeight = 16f * 70f;
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
            Spawn(GetLocalPlayer(), SteamUtils.IsTeebu);
        }
        else
        {
            // If on server, spawn on server (no need to pass arguments)
            SendPacket(PacketID.HandleEffect, SteamUtils.IsTeebu);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        if (SteamUtils.IsTeebu)
        {
            TerrariaUtils.WriteEffectMessage(ItemID.DukeFishronMask, $"{viewerString} spawned Teebu's favourite boss", Severity);
            return;
        }

        TerrariaUtils.WriteEffectMessage(ItemID.Skull, $"{viewerString} spawned a Dungeon Guardian", Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the dungeon guardian on the server
        Spawn(player, reader.ReadBoolean());
    }

    private void Spawn(CrowdControlPlayer player, bool isTeebu)
    {
        // Determine spawn position
        var circleEdge = Main.rand.NextVector2CircularEdge(HalfRangeWidth, HalfRangeHeight);
        var spawnPos = new Point((int)(player.Player.position.X + circleEdge.X), (int)(player.Player.position.Y + circleEdge.Y));

        // Spawn the dungeon guardian
        var index = NPC.NewNPC(null, spawnPos.X, spawnPos.Y, ModContent.NPCType<CrowdControlGuardian>());
        var npc = Main.npc[index];

        // Set the target
        npc.ai[NPC.maxAI - 1] = player.Player.whoAmI;
        npc.target = player.Player.whoAmI;

        // Set whether it is fake or not
        var guardian = (CrowdControlGuardian)npc.ModNPC;
        guardian.IsFake = _isFake;
        guardian.IsTeebu = isTeebu;

        if (isTeebu)
        {
            npc.AddBuff(BuffID.Wet, int.MaxValue);
        }

        // This is only invoked by whoever spawned the guardian (single-player or server)
        guardian.FakeGuardianDied += () =>
        {
            var message = isTeebu ? "Teebu's favourite boss was a phony" : "The Dungeon Guardian was a phony";
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

        if (!isTeebu)
        {
            return;
        }

        // Special life for teebu
        npc.lifeMax = 69420;
        npc.life = 69420;

        if (Main.netMode == NetmodeID.Server)
        {
            WorldUtils.SyncNPCSpecial(npc);
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

        /// <summary>
        ///     Whether the guardian is an easter egg for Teebu.
        /// </summary>
        public bool IsTeebu
        {
            get => NPC.ai[NPC.maxAI - 1] > 0f;
            set => NPC.ai[NPC.maxAI - 1] = value ? 1f : 0f;
        }

        public override string Texture => $"Terraria/Images/NPC_{NPCID.DungeonGuardian}";

        #endregion

        #region Events

        public event Action? FakeGuardianDied;

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(SteamUtils.IsTeebu ? "Teebu's Favourite Boss" : "Dungeon Guardian");
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DungeonGuardian);
            AIType = NPCID.DungeonGuardian;
            NPC.aiStyle = NPCAIStyleID.SkeletronHead;
            _timeLeft = 60 * SurvivalDuration;
        }

        public override bool PreAI()
        {
            NPC.type = NPCID.DungeonGuardian;
            NPC.ShowNameOnHover = Main.netMode == NetmodeID.SinglePlayer || !IsTeebu;

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

        public override void BossHeadSlot(ref int index)
        {
            if (IsTeebu)
            {
                index = NPCID.Sets.BossHeadTextures[NPCID.DukeFishron];
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!IsTeebu)
            {
                return base.PreDraw(spriteBatch, screenPos, drawColor);
            }

            const short id = NPCID.DukeFishron;

            // Ensure texture is loaded
            if (!TextureAssets.Npc[id].IsLoaded)
            {
                Main.instance.LoadNPC(id);
            }

            // Get texture frame
            var tex = TextureAssets.Npc[id].Value;
            var frame = new Rectangle(0, 0, tex.Width, tex.Height / Main.npcFrameCount[id]);
            frame.Y = frame.Height * (NPC.frame.Y / NPC.frame.Height % Main.npcFrameCount[id]);

            // Draw texture
            Main.EntitySpriteDraw(
                tex,
                NPC.Center - screenPos,
                frame,
                drawColor,
                NPC.rotation,
                frame.Size() / 2f,
                1.1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.1f,
                SpriteEffects.None,
                0);

            return false;
        }

        #endregion
    }

    #endregion
}