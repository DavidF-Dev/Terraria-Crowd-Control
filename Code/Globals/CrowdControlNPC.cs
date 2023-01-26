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

    /// <inheritdoc cref="StrikeNPC" />
    public delegate bool StrikeNPCDelegate(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit);

    /// <inheritdoc cref="PreDraw" />
    public delegate bool PreDrawDelegate(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColour);

    #endregion

    #region Events

    /// <inheritdoc cref="EditSpawnRate" />
    public static event EditSpawnRateDelegate? EditSpawnRateHook;

    /// <inheritdoc cref="StrikeNPC" />
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

    public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
    {
        return StrikeNPCHook?.Invoke(npc, ref damage, defense, ref knockback, hitDirection, ref crit) ?? base.StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit);
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