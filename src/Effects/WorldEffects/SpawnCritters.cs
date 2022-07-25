using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Spawn a cluster of critters at the player's position.
/// </summary>
public sealed class SpawnCritters : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int ClusterChance = 8;
    private const int ClusterSpawnMin = 20;
    private const int ClusterSpawnMax = 25;
    private const int SpawnMin = 5;
    private const int SpawnMax = 10;

    private static readonly short[] VanillaCritters =
    {
        NPCID.Bird, NPCID.BirdBlue, NPCID.Buggy, NPCID.Bunny, NPCID.GemBunnyAmethyst, NPCID.GemBunnyTopaz, NPCID.GemBunnySapphire,
        NPCID.GemBunnyEmerald, NPCID.GemBunnyRuby, NPCID.GemBunnyDiamond, NPCID.BirdRed, NPCID.Duck, NPCID.Duck2, NPCID.EnchantedNightcrawler, NPCID.FairyCritterBlue,
        NPCID.FairyCritterGreen, NPCID.FairyCritterPink, NPCID.Firefly, NPCID.Frog, NPCID.GlowingSnail, NPCID.Goldfish, NPCID.Grasshopper, NPCID.Grebe,
        NPCID.LadyBug, NPCID.Lavafly, NPCID.LightningBug, NPCID.Maggot, NPCID.MagmaSnail, NPCID.Mouse, NPCID.Owl, NPCID.Penguin, NPCID.Pupfish, NPCID.Rat,
        NPCID.Scorpion, NPCID.ScorpionBlack, NPCID.Seagull, NPCID.Seagull2, NPCID.Seahorse, NPCID.Squirrel, NPCID.SquirrelRed, NPCID.GemSquirrelAmethyst,
        NPCID.GemSquirrelTopaz, NPCID.GemSquirrelSapphire, NPCID.GemSquirrelEmerald, NPCID.GemSquirrelRuby, NPCID.GemSquirrelDiamond, NPCID.GemSquirrelAmber,
        NPCID.Turtle, NPCID.TurtleJungle, NPCID.WaterStrider, NPCID.Worm, NPCID.Butterfly, NPCID.HellButterfly, NPCID.EmpressButterfly, NPCID.BlackDragonfly,
        NPCID.BlueDragonfly, NPCID.GreenDragonfly, NPCID.OrangeDragonfly, NPCID.RedDragonfly, NPCID.YellowDragonfly, NPCID.TruffleWorm, NPCID.GoldBird,
        NPCID.GoldBunny, NPCID.GoldButterfly, NPCID.GoldDragonfly, NPCID.GoldFrog, NPCID.GoldGoldfish, NPCID.GoldGrasshopper, NPCID.GoldLadyBug,
        NPCID.GoldMouse, NPCID.GoldSeahorse, NPCID.SquirrelGold, NPCID.GoldWaterStrider, NPCID.GoldWorm, NPCID.BunnySlimed, NPCID.BunnyXmas,
        NPCID.PartyBunny, NPCID.Dolphin, NPCID.SeaTurtle, NPCID.CrimsonBunny, NPCID.CorruptBunny
    };

    private static readonly string[] CalamityCritters =
    {
        "BabyFlakCrab", "BloodwormNormal", "Twinkler", "BabyGhostBell", "SeaMinnow", "Piggy", "RepairUnitCritter"
    };

    #endregion

    #region Fields

    private readonly IReadOnlyList<short> _allCritterOptions;

    #endregion

    #region Constructors

    public SpawnCritters() : base(EffectID.SpawnCritters, null, EffectSeverity.Neutral)
    {
        // Create a list of all available critter types
        var allCritterOptions = VanillaCritters.ToList();
        if (ModUtils.TryGetMod(ModUtils.Calamity.Name, out var calamity))
        {
            // Add calamity critters
            ModUtils.IterateTypes<ModNPC>(calamity, CalamityCritters, x => allCritterOptions.Add((short)x.Type));
        }

        _allCritterOptions = allCritterOptions;
    }

    #endregion

    #region Properties

    protected override int StartEmote => EmoteID.ItemBugNet;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!_allCritterOptions.Any())
        {
            return CrowdControlResponseStatus.Failure;
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Spawn in single-player
            Spawn(GetLocalPlayer());
        }
        else
        {
            // Notify the server
            SendPacket(PacketID.HandleEffect);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.BugNet, $"{viewerString} spawned a bunch of critters", Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the critters on the server
        Spawn(player);
    }

    private void Spawn(ModPlayer player)
    {
        var x = (int)player.Player.Center.X;
        var y = (int)player.Player.Center.Y;
        var n = Main.rand.Next(100) <= ClusterChance ? Main.rand.Next(ClusterSpawnMin, ClusterSpawnMax) : Main.rand.Next(SpawnMin, SpawnMax);
        for (var i = 0; i < n; i++)
        {
            var index = NPC.NewNPC(null, x + Main.rand.Next(-16, 16), y - Main.rand.Next(16), Main.rand.Next((List<short>)_allCritterOptions));
            var npc = Main.npc[index];
            npc.target = player.Player.whoAmI;
            if (npc.friendly)
            {
                npc.AddBuff(BuffID.Lovestruck, 60 * 2);
                npc.loveStruck = true;
            }

            if (Main.netMode == NetmodeID.Server)
            {
                // Notify clients if spawned on server
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
            }

            if (!npc.friendly && !Main.hardMode)
            {
                // Reduce life of evil critters significantly in pre-hardmode
                npc.lifeMax /= 4;
                npc.life = npc.lifeMax;
            }
            else
            {
                // Increase life of critters
                npc.lifeMax *= Main.rand.Next(Main.hardMode ? 100 : 30);
                npc.life = npc.lifeMax;
            }

            if (Main.netMode == NetmodeID.Server)
            {
                // Sync the new max life for clients
                WorldUtils.SyncNPCSpecial(npc);
            }
        }
    }

    #endregion
}