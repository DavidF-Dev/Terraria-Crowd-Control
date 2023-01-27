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
    private const float DropGoldCoins = 1f;
    private const float DropHardModeGoldCoins = 2.5f;
    private static readonly short[] ReplacementNPCIds = {NPCID.Goldfish, NPCID.GemBunnyTopaz, NPCID.GemSquirrelTopaz, NPCID.GemBunnyAmber, NPCID.GemSquirrelAmber};

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
        npc.value = 10000 * (Main.hardMode ? DropHardModeGoldCoins : DropGoldCoins);

        if (NetUtils.IsServer)
        {
            // Let clients know about the NPC
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }
    }

    private static void Despawn()
    {
        for (var i = 0; i < Main.maxNPCs; i++)
        {
            // Only affect damaged golden slimes
            if (!Main.npc[i].active || Main.npc[i].netID != NPCID.GoldenSlime || Main.npc[i].life < Main.npc[i].lifeMax / 2)
            {
                continue;
            }

            // Replace golden slime
            var slimeNPC = Main.npc[i];
            slimeNPC.SetDefaults(ReplacementNPCIds[Main.rand.Next(ReplacementNPCIds.Length)]);

            if (NetUtils.IsClient)
            {
                // Notify clients
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
            }
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

    int IMusicEffect.MusicId => MusicID.DayRemix;

    int IMusicEffect.MusicPriority => 0;

    public override EffectCategory Category => EffectCategory.World;
    
    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        if (NetUtils.IsSinglePlayer)
        {
            Despawn();
        }
        else
        {
            SendPacket(PacketID.HandleEffect, false);
        }
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        var spawn = reader.ReadBoolean();
        if (spawn)
        {
            Spawn(player);
        }
        else
        {
            Despawn();
        }
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.GoldDust, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStopText(Id), EffectSeverity.Neutral);
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
        if (CrowdControlMod.GetInstance().IsEffectActive(EffectID.IncreaseSpawnRate))
        {
            // Decrease spawn time drastically if spawn rate is increased via an effect
            _spawnTime /= 3;
        }

        if (NetUtils.IsSinglePlayer)
        {
            // Spawn the slime in single-player
            Spawn(GetLocalPlayer());
        }
        else
        {
            // Spawn the slime on the server
            SendPacket(PacketID.HandleEffect, true);
        }
    }

    #endregion
}