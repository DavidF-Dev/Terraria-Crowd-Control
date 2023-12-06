using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
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

    public SpawnGuardian(bool isFake) : base(isFake ? EffectID.SpawnFakeGuardian : EffectID.SpawnGuardian, 0, EffectSeverity.Negative)
    {
        _isFake = isFake;
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var mode = CrowdControlGuardian.GuardianMode.None;
        if (SteamUtils.IsTeebu)
        {
            mode |= CrowdControlGuardian.GuardianMode.Fishron;
        }

        if (SteamUtils.IsKulprid && !_isFake)
        {
            mode |= CrowdControlGuardian.GuardianMode.Persistent;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // In single-player, simply spawn the custom dungeon guardian
            Spawn(GetLocalPlayer(), mode);
        }
        else
        {
            // If on server, spawn on server (no need to pass arguments)
            SendPacket(PacketID.HandleEffect, (int)mode);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        if (SteamUtils.IsTeebu)
        {
            TerrariaUtils.WriteEffectMessage(ItemID.DukeFishronMask, LangUtils.GetEffectStartText($"{EffectID.SpawnGuardian}_egg", viewerString, playerString, durationString), Severity);
            return;
        }

        var tile = GetLocalPlayer().Player.Center.ToTileCoordinates();
        if (WorldUtils.IsDungeonWall(tile.X, tile.Y) && !NPC.downedBoss3)
        {
            // Hide the chat message if the player is in the dungeon before skeletron is defeated - we do a little trolling ;)
            return;
        }

        TerrariaUtils.WriteEffectMessage(ItemID.Skull, LangUtils.GetEffectStartText(EffectID.SpawnGuardian, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the dungeon guardian on the server
        Spawn(player, (CrowdControlGuardian.GuardianMode)reader.ReadInt32());
    }

    private void Spawn(CrowdControlPlayer player, CrowdControlGuardian.GuardianMode mode)
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
        guardian.Mode = mode;
        var isTeebu = guardian.IsFishron;
        var isKulprid = guardian.IsPersistent;

        if (isTeebu)
        {
            npc.AddBuff(BuffID.Wet, int.MaxValue);
        }

        // This is only invoked by whoever spawned the guardian (single-player or server)
        guardian.FakeGuardianDied += () =>
        {
            var message = isTeebu ? LangUtils.GetEffectMiscText(Id, "PhonyEgg") : LangUtils.GetEffectMiscText(Id, "Phony");
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

        if (NetUtils.IsServer)
        {
            // Notify the server
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }

        if (!isTeebu && !isKulprid)
        {
            return;
        }

        // Special life for teebu
        npc.lifeMax = isKulprid ? Main.hardMode ? 100 : 30 : 69420;
        npc.life = npc.lifeMax;

        if (NetUtils.IsServer)
        {
            NetUtils.SyncNPCSpecial(index);
        }
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class CrowdControlGuardian : ModNPC
    {
        #region Enums

        [Flags]
        public enum GuardianMode
        {
            None = 0,
            Fishron = 1,
            Persistent = 2
        }

        #endregion

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
        ///     Guardian mode.
        /// </summary>
        public GuardianMode Mode
        {
            get => (GuardianMode)(int)NPC.ai[NPC.maxAI - 1];
            set => NPC.ai[NPC.maxAI - 1] = (int)value;
        }

        /// <summary>
        ///     Uses a fishron overlay (Teebu easter egg).
        /// </summary>
        public bool IsFishron => Mode.HasFlag(GuardianMode.Fishron);

        /// <summary>
        ///     Doesn't despawn but has reduced life (Kulprid easter egg).
        /// </summary>
        public bool IsPersistent => Mode.HasFlag(GuardianMode.Persistent);

        public override string Texture => $"Terraria/Images/NPC_{NPCID.DungeonGuardian}";

        public override LocalizedText DisplayName => Lang.GetNPCName(NPCID.DungeonGuardian);

        #endregion

        #region Events

        public event Action? FakeGuardianDied;

        #endregion

        #region Methods

        public override void ModifyTypeName(ref string typeName)
        {
            if (IsFishron)
            {
                typeName = "Teebu's Favourite Boss";
            }
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
            NPC.ShowNameOnHover = NetUtils.IsSinglePlayer || !IsFishron;

            if (IsPersistent)
            {
                // Never run out of time if persistent
                _timeLeft = 60 * SurvivalDuration;
                NPC.timeLeft = 60 * SurvivalDuration + 1;

                if (!NetUtils.IsServer && NPC.localAI[0] == 0f)
                {
                    NPC.localAI[0] = 1f;
                    Roar();
                }

                // Ensure vanilla AI doesn't despawn the guardian if all players are dead
                if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
                {
                    NPC.target = -1;
                    NPC.TargetClosest(false);
                    if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
                    {
                        NPC.target = -1;
                        NPC.rotation %= MathF.PI * 2;
                        NPC.rotation *= 0.95f;
                        NPC.velocity *= 0.98f;
                        return false;
                    }

                    // Reacquired a target, so roar!
                    if (!NetUtils.IsServer)
                    {
                        Roar();
                    }
                }

                // Move towards player (emulating vanilla behaviour)
                var speedMult = NPC.DistanceSQ(Main.player[NPC.target].position) > 16 * 16 * 180 * 180 ? 5f : 1f;
                NPC.velocity = NPC.DirectionTo(Main.player[NPC.target].position) * 8.75f * speedMult;
                NPC.rotation += MathHelper.ToRadians(11);

                return false;
            }
            
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

        public override bool CheckActive()
        {
            // Don't despawn if persistent
            if (IsPersistent && _timeLeft > 0)
            {
                return false;
            }

            return base.CheckActive();
        }

        public override void OnKill()
        {
            Item.NewItem(null, NPC.position, NPC.width, NPC.height, ItemID.GoldCoin, 2);
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            // Ignore player if fake
            return !IsFake;
        }

        public override bool CanHitNPC(NPC target)
        {
            // Ignore others if fake
            return !IsFake;
        }

        public override void BossHeadSlot(ref int index)
        {
            if (IsFishron)
            {
                index = NPCID.Sets.BossHeadTextures[NPCID.DukeFishron];
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!IsFishron)
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
                SpriteEffects.None);

            return false;
        }

        private void Roar()
        {
            var pos = Main.LocalPlayer.position + Main.LocalPlayer.DirectionTo(NPC.position) * 16f * 20f;
            SoundEngine.PlaySound(SoundID.Roar with {SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest}, pos);
        }
        
        #endregion
    }

    #endregion
}