using System;
using System.Collections.Generic;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Effects.Interfaces;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Draw a wall of fish over the player's screen (client-side).
/// </summary>
public sealed class WallOfFishEffect : CrowdControlEffect, IMusicEffect
{
    #region Static Fields and Constants

    private const float DrawOffset = 0.85f;

    #endregion

    #region Fields

    private readonly List<int> _fishIds;
    private readonly bool _koiEasterEgg;

    #endregion

    #region Constructors

    public WallOfFishEffect(float duration) : base(EffectID.WallOfFish, duration, EffectSeverity.Negative)
    {
        if (NetUtils.IsServer)
        {
            _koiEasterEgg = false;
            _fishIds = new List<int>();
            return;
        }

        // Easter egg :-)
        _koiEasterEgg = SteamUtils.IsMrKaiga;
        if (_koiEasterEgg)
        {
            Main.instance.LoadItem(ItemID.KiteKoi);
            Main.instance.LoadItem(ItemID.Ebonkoi);
            Main.instance.LoadItem(ItemID.FlarefinKoi);
            _fishIds = new List<int> {ItemID.KiteKoi, ItemID.KiteKoi, ItemID.KiteKoi, ItemID.Ebonkoi, ItemID.KiteKoi, ItemID.KiteKoi, ItemID.KiteKoi, ItemID.FlarefinKoi};
            return;
        }

        // Add fish ids, so that their textures are loaded
        _fishIds = new List<int>(Math.Abs(2297 - 2321) + Math.Abs(2450 - 2488));
        for (var i = 2297; i <= 2321; i++)
        {
            Main.instance.LoadItem(i);
            _fishIds.Add(i);
        }

        for (var i = 2450; i <= 2488; i++)
        {
            Main.instance.LoadItem(i);
            _fishIds.Add(i);
        }
    }

    #endregion

    #region Properties

    int IMusicEffect.MusicId => MusicID.Mushrooms;

    int IMusicEffect.MusicPriority => 0;

    protected override int StartEmote => EmoteID.ItemCookedFish;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        if (_koiEasterEgg)
        {
            GetLocalPlayer().Player.QuickSpawnItem(null, ItemID.KiteKoi);
        }

        CrowdControlModSystem.PostDrawTilesHook += PostDrawTiles;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        CrowdControlModSystem.PostDrawTilesHook -= PostDrawTiles;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        var item = _koiEasterEgg ? ItemID.KiteKoi : ItemID.Tuna;
        var locKey = _koiEasterEgg ? $"{Id}_egg" : Id;
        TerrariaUtils.WriteEffectMessage(item, LangUtils.GetEffectStartText(locKey, viewerString, playerString, durationString), Severity);
    }

    private void PostDrawTiles()
    {
        Main.spriteBatch.Begin();
        var maxFish = Main.screenWidth / 140;
        try
        {
            for (var i = 0; i < maxFish; i++)
            {
                DrawWallOfFish(i * DrawOffset);
            }
        }
        catch (Exception e)
        {
            TerrariaUtils.WriteDebug($"Exception whilst drawing the wall of fish: {e.Message}");
        }
        finally
        {
            Main.spriteBatch.End();
        }
    }

    private void DrawWallOfFish(float offset = 0f)
    {
        // Draw a wall of fish (offset is applied to Main.GlobalTimeWrappedHourly)
        // Extracted and edited from the Terraria source code
        var scale = !_koiEasterEgg ? 1f : 1.3f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.3f;
        for (var i = 0; i < 5; i++)
        {
            var num3 = 10f + offset;
            var vector = new Vector2(Main.screenWidth / num3 * (Main.GlobalTimeWrappedHourly % num3), -100f);
            vector.X += 14 * i;
            vector.Y += i % 2 * 14;
            var num2 = 30 * i;
            while (vector.Y < Main.screenHeight + 100)
            {
                if (++num2 >= _fishIds.Count)
                {
                    num2 = 0;
                }

                vector.Y += 26f;
                var texture2D = TextureAssets.Item[_fishIds[num2]].Value;
                Main.spriteBatch.Draw(texture2D, vector, null, Color.White, (float)Math.PI / 4f, texture2D.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }
    }

    #endregion
}