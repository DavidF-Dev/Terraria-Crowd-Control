using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects.Interfaces;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Temporarily rain golden slimes from the sky.
/// </summary>
public sealed class GoldenSlimeRainEffect : CrowdControlEffect, IMusicEffect
{
    #region Static Fields and Constants

    private const float MinSpawnTime = 1f;
    private const float MaxSpawnTime = 4.5f;
    private const float GoldCoinDropAmount = 1f;
    private const float HardModeGoldCoinDropAmount = 2.5f;

    #endregion

    #region Static Methods

    private static void Spawn(ModPlayer player)
    {
        // Determine spawn position
        var x = (int)(player.Player.position.X - Main.LogicCheckScreenWidth / 2f + Main.rand.Next(Main.LogicCheckScreenWidth));
        var y = (int)(player.Player.position.Y - Main.LogicCheckScreenHeight / 2f - Main.rand.NextFloat(2f, 16f * 7f));

        // Spawn the golden slime
        var index = NPC.NewNPC(null, x, y, NPCID.GoldenSlime, Target: player.Player.whoAmI);
        var npc = Main.npc[index];

        // Alter the life
        npc.lifeMax = Main.rand.Next(10, 300) + (Main.hardMode ? 400 : 0);
        npc.life = npc.lifeMax;

        // Alter coin drop amount (10000 = 1 gold coin)
        npc.value = 10000 * (Main.hardMode ? HardModeGoldCoinDropAmount : GoldCoinDropAmount);
        
        if (Main.netMode == NetmodeID.Server)
        {
            // Let clients know about the NPC
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }
    }

    #endregion

    #region Fields

    private float _spawnTime;

    #endregion

    #region Constructors

    public GoldenSlimeRainEffect(float duration) : base(EffectID.GoldenSlimeRain, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public int MusicId => MusicID.DayRemix;

    public int MusicPriority => 0;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        Spawn(player);
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GoldDust, $"{viewerString} caused it to rain Golden Slimes above {playerString} for {durationString} seconds", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "No longer raining Golden Slimes", EffectSeverity.Neutral);
    }

    protected override void OnUpdate(float delta)
    {
        // Reduce the spawn timer
        _spawnTime -= delta;
        if (_spawnTime > 0)
        {
            return;
        }

        // Reset spawn timer (reduced in hard-mode)
        _spawnTime = Main.rand.NextFloat(MinSpawnTime, MaxSpawnTime) * (Main.hardMode ? 0.5f : 1f);

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Spawn the slime in single-player
            Spawn(GetLocalPlayer());
        }
        else
        {
            // Spawn the slime on the server
            SendPacket(PacketID.HandleEffect);
        }
    }

    #endregion
}