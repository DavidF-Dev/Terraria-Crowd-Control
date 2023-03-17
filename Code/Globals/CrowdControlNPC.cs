using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CrowdControlMod.Globals;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CrowdControlNPC : GlobalNPC
{
    #region Delegates

    /// <inheritdoc cref="EditSpawnRate" />
    public delegate void EditSpawnRateDelegate(Player player, ref int spawnRate, ref int maxSpawns);

    public delegate void StrikeNPCDelegate(NPC npc, Entity source, Player? player, NPC.HitInfo hit, int damageDone);

    /// <inheritdoc cref="PreDraw" />
    public delegate bool PreDrawDelegate(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColour);

    #endregion

    #region Events

    /// <inheritdoc cref="EditSpawnRate" />
    public static event EditSpawnRateDelegate? EditSpawnRateHook;

    public static event StrikeNPCDelegate? StrikeNPCHook;

    /// <inheritdoc cref="PreDraw" />
    public static event PreDrawDelegate? PreDrawHook;

    /// <inheritdoc cref="OnKill" />
    public static event Action<NPC>? OnKillHook;

    #endregion

    #region Methods

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        EditSpawnRateHook?.Invoke(player, ref spawnRate, ref maxSpawns);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        StrikeNPCHook?.Invoke(npc, item, player, hit, damageDone);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        StrikeNPCHook?.Invoke(npc, projectile, projectile.owner >= 0 && projectile.owner < Main.maxPlayers ? Main.player[projectile.owner] : null, hit, damageDone);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return PreDrawHook?.Invoke(npc, spriteBatch, screenPos, drawColor) ?? base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void OnKill(NPC npc)
    {
        OnKillHook?.Invoke(npc);
    }

    #endregion
}