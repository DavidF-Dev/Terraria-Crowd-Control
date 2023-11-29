using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Effects.Challenges;

public sealed class CatchCritterChallenge : ChallengeEffect
{
    #region Static Methods

    private static void SpawnNearbyCritter(Vector2 pos)
    {
        // Spawn a critter
        var index = NPC.NewNPC(null, (int)pos.X, (int)pos.Y, NPCID.Shimmerfly);
        if (NetUtils.IsServer)
        {
            // Notify the server
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }
    }

    private static bool OnCanApplyHunterPotionEffects(On_NPC.orig_CanApplyHunterPotionEffects orig, NPC self)
    {
        return self.CountsAsACritter || orig.Invoke(self);
    }

    #endregion

    #region Fields

    private bool _providedNet;

    #endregion

    #region Constructors

    public CatchCritterChallenge(int duration) : base(EffectID.CatchCritterChallenge, duration)
    {
    }

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnChallengeStart()
    {
        var player = GetLocalPlayer();
        player.OnCatchNPCHook += OnCatchNPC;
        On_NPC.CanApplyHunterPotionEffects += OnCanApplyHunterPotionEffects;

        // Provide the player with a net so that the challenge is completable
        if (!player.Player.HasItem(ItemID.BugNet))
        {
            var item = new Item(ItemID.BugNet);
            item.SetItemOwner(Viewer);
            player.Player.QuickSpawnItem(null, item);
            _providedNet = true;
        }

        // Spawn a few critters nearby
        const int maxAttempts = 300;
        const int maxSpawns = 2;
        var spawned = 0;
        var attempt = 0;
        var minDist = Math.Max(Main.LogicCheckScreenWidth, Main.LogicCheckScreenHeight) / 2f;
        while (spawned < maxSpawns)
        {
            var pos = player.Player.Center + Main.rand.NextVector2Unit() * minDist * Main.rand.NextFloat(1.1f, 1.25f);
            if (Collision.IsWorldPointSolid(pos, true))
            {
                if (++attempt >= maxAttempts)
                {
                    spawned++;
                    attempt = 0;
                }

                continue;
            }

            if (NetUtils.IsSinglePlayer)
            {
                SpawnNearbyCritter(pos);
            }
            else
            {
                SendPacket(PacketID.HandleEffect, pos.X, pos.Y);
            }

            attempt = 0;
            spawned++;
        }

        return base.OnChallengeStart();
    }

    protected override void OnChallengeStop()
    {
        var player = GetLocalPlayer();
        player.OnCatchNPCHook -= OnCatchNPC;
        On_NPC.CanApplyHunterPotionEffects -= OnCanApplyHunterPotionEffects;

        // Remove the net if one was provided
        // Note, this doesn't prevent the player from stashing in a chest first, but they would probably fail the challenge
        if (_providedNet)
        {
            var existingNet = player.Player.FindItem(ItemID.BugNet);
            if (existingNet != -1)
            {
                player.Player.inventory[existingNet].TurnToAir();
            }
            else if (player.Player.HeldItem.type == ItemID.BugNet)
            {
                player.Player.HeldItem.TurnToAir();
            }
        }

        _providedNet = false;
    }

    protected override string GetChallengeDescription()
    {
        return LangUtils.GetEffectStartText(Id, string.Empty, string.Empty, string.Empty);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        SpawnNearbyCritter(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
    }

    private void OnCatchNPC(NPC npc, Item item, bool failed)
    {
        if (failed)
        {
            return;
        }

        SetChallengeCompleted();
    }

    #endregion
}