using System;
using System.IO;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Effect that spawns a random Town NPC at the player's position based on world progression.
/// </summary>
public sealed class SpawnTownNPCEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private static readonly short[] PreEyeTownNPCs =
    {
        NPCID.Guide, NPCID.Merchant, NPCID.Nurse, NPCID.Demolitionist,
        NPCID.TravellingMerchant, NPCID.OldMan, NPCID.SkeletonMerchant
    };

    private static readonly short[] PreSkeletronTownNPCs =
    {
        NPCID.DyeTrader, NPCID.Angler, NPCID.BestiaryGirl, NPCID.Dryad, NPCID.Painter,
        NPCID.Golfer, NPCID.ArmsDealer, NPCID.DD2Bartender, NPCID.Stylist
    };

    private static readonly short[] PreWallTownNPCs =
    {
        NPCID.GoblinTinkerer, NPCID.WitchDoctor, NPCID.Clothier, NPCID.Mechanic, NPCID.PartyGirl
    };

    private static readonly short[] PreMechTownNPCs = {NPCID.Wizard, NPCID.TaxCollector};
    private static readonly short[] PreGolemTownNPCs = {NPCID.Truffle, NPCID.Pirate, NPCID.Steampunker};
    private static readonly short[] PreLunarTownNPCs = {NPCID.Cyborg};
    private static readonly short[] PreMoonLordTownNPCs = Array.Empty<short>();
    private static readonly short[] PostGameTownNPCs = {NPCID.Princess};

    #endregion

    #region Static Methods

    private static int GetEmoteId(short npcType)
    {
        return npcType switch
        {
            NPCID.Merchant => EmoteID.TownMerchant,
            NPCID.Nurse => EmoteID.TownNurse,
            NPCID.ArmsDealer => EmoteID.TownArmsDealer,
            NPCID.Dryad => EmoteID.TownDryad,
            NPCID.Guide => EmoteID.TownGuide,
            NPCID.OldMan => EmoteID.TownOldman,
            NPCID.Demolitionist => EmoteID.TownDemolitionist,
            NPCID.Clothier => EmoteID.TownClothier,
            NPCID.GoblinTinkerer => EmoteID.TownGoblinTinkerer,
            NPCID.Wizard => EmoteID.TownWizard,
            NPCID.Mechanic => EmoteID.TownMechanic,
            NPCID.Truffle => EmoteID.TownTruffle,
            NPCID.Steampunker => EmoteID.TownSteampunker,
            NPCID.DyeTrader => EmoteID.TownDyeTrader,
            NPCID.PartyGirl => EmoteID.TownPartyGirl,
            NPCID.Cyborg => EmoteID.TownCyborg,
            NPCID.Painter => EmoteID.TownPainter,
            NPCID.Stylist => EmoteID.TownStylist,
            NPCID.TravellingMerchant => EmoteID.TownTravellingMerchant,
            NPCID.Angler => EmoteID.TownAngler,
            NPCID.SkeletonMerchant => EmoteID.TownSkeletonMerchant,
            NPCID.TaxCollector => EmoteID.TownTaxCollector,
            NPCID.DD2Bartender => EmoteID.TownBartender,
            NPCID.Golfer => EmoteID.TownGolfer,
            NPCID.BestiaryGirl => Main.dayTime ? EmoteID.TownBestiaryGirl : EmoteID.TownBestiaryGirlFox,
            NPCID.Princess => EmoteID.TownPrincess,
            _ => -1
        };
    }

    private static void Spawn(Vector2 spawnPos, int npcId, string givenName)
    {
        // Spawn the Town NPC
        var index = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, npcId);

        // Attempt to assign the town NPC a given name
        var newName = false;
        if (!string.IsNullOrEmpty(givenName))
        {
            Main.npc[index].GivenName = givenName;
            newName = true;
        }

        if (!NetUtils.IsServer)
        {
            return;
        }

        // Notify clients
        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        if (newName)
        {
            NetMessage.SendData(MessageID.UniqueTownNPCInfoSyncRequest, -1, -1, null, index);
        }
    }

    #endregion

    #region Fields

    private short _chosenNPC = -1;

    #endregion

    #region Constructors

    public SpawnTownNPCEffect() : base(EffectID.SpawnTownNpc, null, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    protected override int StartEmote => GetEmoteId(_chosenNPC);

    #endregion

    #region Methods

    protected override void OnSessionStopped()
    {
        _chosenNPC = -1;
    }

    protected override CrowdControlResponseStatus OnStart()
    {
        // Determine options based on progression
        var options = ProgressionUtils.ChooseUpToProgression(
                PreEyeTownNPCs, PreSkeletronTownNPCs, PreWallTownNPCs,
                PreMechTownNPCs, PreGolemTownNPCs, PreLunarTownNPCs,
                PreMoonLordTownNPCs, PostGameTownNPCs)
            .SelectMany(x => x)
            .ToList();
        if (options.Count == 0)
        {
            return CrowdControlResponseStatus.Failure;
        }

        // Choose a random town NPC to spawn (except the previously chosen one)
        short npcId;
        do
        {
            npcId = Main.rand.Next(options);
        } while (npcId == _chosenNPC);

        _chosenNPC = npcId;
        var givenName = !string.IsNullOrEmpty(Viewer) ? Viewer : string.Empty;
        var player = GetLocalPlayer();

        if (NetUtils.IsSinglePlayer)
        {
            Spawn(player.Player.Center, _chosenNPC, givenName);
        }
        else
        {
            SendPacket(PacketID.HandleEffect, _chosenNPC, givenName);
        }

        // Play sfx
        SoundEngine.PlaySound(SoundID.Meowmere, player.Player.Center);

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Handle on server
        Spawn(player.Player.Center, reader.ReadInt16(), reader.ReadString());
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.PeddlersHat, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString, Lang.GetNPCName(_chosenNPC)), Severity);
    }

    #endregion
}