using System.Collections.Generic;
using System.Linq;
using CrowdControlMod.Utilities;
using Terraria;
using Terraria.ID;

namespace CrowdControlMod.Features;

/// <summary>
///     Despawn npcs if all players are dead (only if at least one player has the config option enabled).
/// </summary>
public sealed class DespawnNPCFeature : IFeature
{
    #region Fields

    private readonly List<(int whoAmI, int type)> _managedNpcs = new(4);

    #endregion

    #region Constructors

    public DespawnNPCFeature()
    {
        CrowdControlModSystem.PostUpdateNPCsHook += PostUpdateNPCs;
    }

    #endregion

    #region Methods

    public void SessionStarted()
    {
    }

    public void SessionStopped()
    {
    }

    public void Dispose()
    {
        _managedNpcs.Clear();
        CrowdControlModSystem.PostUpdateNPCsHook -= PostUpdateNPCs;
    }

    /// <summary>
    ///     Register an NPC spawned by an effect that *could* despawn if all the players are dead.
    /// </summary>
    public void RegisterNPC(int npcWhoAmI)
    {
        if (NetUtils.IsClient)
        {
            TerrariaUtils.WriteDebug($"{nameof(RegisterNPC)} cannot be called on multiplayer client");
            return;
        }

        if (npcWhoAmI < 0 || npcWhoAmI >= Main.npc.Length || _managedNpcs.Any(x => x.whoAmI == npcWhoAmI))
        {
            // Ignore
            return;
        }

        var npc = Main.npc[npcWhoAmI];
        if (!npc.active || npc.life <= 0)
        {
            // Ignore
            return;
        }

        _managedNpcs.Add((npcWhoAmI, npc.type));
        TerrariaUtils.WriteDebug($"Registered spawned npc with {nameof(DespawnNPCFeature)}: {npc.FullName} ({npcWhoAmI})");
    }

    private void PostUpdateNPCs()
    {
        if (_managedNpcs.Count == 0 || NetUtils.IsClient)
        {
            // Ignore
            return;
        }

        // Determine if the npcs should despawn due to all players being dead
        var shouldDespawn = Main.player.Any(p => p.active && p.GetModPlayer<CrowdControlPlayer>().DespawnForcefullyBoss) &&
                            Main.player.All(p => !p.active || p.dead);

        // Iterate over managed npcs and check if any should be removed
        for (var i = 0; i < _managedNpcs.Count; i++)
        {
            var npc = Main.npc[_managedNpcs[i].whoAmI];
            if (!npc.active || npc.life <= 0 || npc.type != _managedNpcs[i].type)
            {
                // Remove (due to natural reasons)
                _managedNpcs.RemoveAt(i);
                i -= 1;
                TerrariaUtils.WriteDebug($"Spawned npc removed from {nameof(DespawnNPCFeature)} by natural means: {npc.FullName} ({npc.whoAmI})");
                continue;
            }

            if (!shouldDespawn)
            {
                continue;
            }

            if (NetUtils.IsSinglePlayer)
            {
                // Spawn gore but no loot (single-player only)
                var cachedLife = npc.life;
                npc.life = 0;
                npc.HitEffect(0, double.MaxValue);
                npc.life = cachedLife;
            }
            else
            {
                // Spawn confetti (multi-player only)
                var projIndex = Projectile.NewProjectile(null, npc.Center, npc.velocity, ProjectileID.ConfettiGun, 1, 0f);
                Main.projectile[projIndex].friendly = true;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projIndex);
            }

            // Forcefully despawn the npc
            npc.active = false;
            if (NetUtils.IsServer)
            {
                // Notify clients
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
            }

            // Spawn bunny in-place (with boss health)
            var bunnyIndex = NPC.NewNPC(null, (int)npc.Center.X, (int)npc.Center.Y, NPCID.GemBunnyDiamond);
            var bunny = Main.npc[bunnyIndex];
            bunny.lifeMax = npc.lifeMax;
            bunny.life = npc.life;
            if (NetUtils.IsServer)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, bunnyIndex);
                NetUtils.SyncNPCSpecial(bunnyIndex);
            }

            _managedNpcs.RemoveAt(i);
            i -= 1;
            TerrariaUtils.WriteDebug($"Spawned npc removed from {nameof(DespawnNPCFeature)} due to all players being dead: {npc.FullName} ({npc.whoAmI})");
        }
    }

    #endregion
}