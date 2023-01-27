using System;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace CrowdControlMod.Effects.ScreenEffects;

/// <summary>
///     Change all NPC sprites to a random critter sprite.
/// </summary>
public sealed class CritterTakeoverEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float SpawnRateFactor = 4;

    private static readonly short[] CritterOptions =
    {
        NPCID.Bunny, NPCID.CorruptBunny, NPCID.CrimsonBunny, NPCID.GoldBunny, NPCID.ExplosiveBunny, NPCID.BunnySlimed,
        NPCID.Squirrel, NPCID.SquirrelGold, NPCID.SquirrelRed,
        NPCID.TurtleJungle, NPCID.Turtle, NPCID.WaterStrider, NPCID.Scorpion, NPCID.ScorpionBlack,
        NPCID.Goldfish, NPCID.CorruptGoldfish, NPCID.CrimsonGoldfish,
        NPCID.Penguin, NPCID.CorruptPenguin, NPCID.CrimsonPenguin,
        NPCID.Owl, NPCID.Frog, NPCID.Duck, NPCID.Duck2, NPCID.DuckWhite, NPCID.DuckWhite2,
        NPCID.Dolphin, NPCID.Pupfish, NPCID.SeaTurtle, NPCID.Seahorse, NPCID.Seagull, NPCID.Seagull2,
        NPCID.EmpressButterfly, NPCID.MagmaSnail

        // TODO: 1.4.4
        /*
        NPCID.GrayCockatiel, NPCID.YellowCockatiel, NPCID.Toucan, NPCID.BlueMacaw, NPCID.ScarletMacaw,
        NPCID.TownSlimeGreen, NPCID.TownSlimePurple, NPCID.TownSlimeRainbow, NPCID.TownSlimeRed,
        NPCID.TownSlimeYellow, NPCID.TownSlimeCopper, NPCID.TownSlimeOld
        */
    };

    #endregion

    #region Fields

    private int _seed;

    #endregion

    public override EffectCategory Category => EffectCategory.Screen;
    
    #region Constructors

    public CritterTakeoverEffect(float duration) : base(EffectID.CritterTakeover, duration, EffectSeverity.Neutral)
    {
        CrowdControlNPC.EditSpawnRateHook += EditSpawnRate;

        if (NetUtils.IsServer)
        {
            return;
        }

        // Load the ids so their textures are loaded (not on server!)
        foreach (var critterId in CritterOptions)
        {
            Main.instance.LoadNPC(critterId);
        }
    }

    #endregion

    #region Methods

    protected override void OnDisposed()
    {
        CrowdControlNPC.EditSpawnRateHook -= EditSpawnRate;
    }

    protected override CrowdControlResponseStatus OnStart()
    {
        // Get a random seed so that the textures are different each time the effect is activated
        _seed = Main.rand.Next(CritterOptions.Length);

        CrowdControlNPC.PreDrawHook += PreDraw;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _seed = 0;

        CrowdControlNPC.PreDrawHook -= PreDraw;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.ExplosiveBunny, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    private bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color drawColour)
    {
        if (npc.CountsAsACritter)
        {
            // Ignore critters
            return true;
        }

        // Get the critter that will be drawn instead of the npc 
        var critterId = CritterOptions[(npc.whoAmI + _seed) % CritterOptions.Length];
        var critterTex = TextureAssets.Npc[critterId].Value;

        // Determine the frame to use (extracted from NPC source) and frame number
        var frame = new Rectangle(0, 0, critterTex.Width, critterTex.Height / Main.npcFrameCount[critterId]);
        frame.Y = frame.Height * (npc.frame.Y / npc.frame.Height % Main.npcFrameCount[critterId]);

        // Determine origin (maybe this needs to be changed to account for offset?)
        var origin = frame.Size() / 2f;

        // Use the width of the npc frame to determine the scale (same width should result in 1)
        var scale = (float)Math.Ceiling((float)npc.frame.Width / frame.Width);

        // Determine offset to correct positioning
        var offset = new Vector2(0f, 2f);

        // Draw the critter texture
        Main.EntitySpriteDraw(
            critterTex,
            npc.Center - screenPos + offset,
            frame,
            drawColour,
            npc.rotation,
            origin,
            scale,
            npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            0);

        return false;
    }

    private void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        var mod = CrowdControlMod.GetInstance();
        var spawnRateEffect = mod.GetEffect(EffectID.IncreaseSpawnRate);
        if ((NetUtils.IsSinglePlayer && mod.IsSessionActive && IsActive && !(spawnRateEffect?.IsActive ?? false)) ||
            (NetUtils.IsServer && IsActiveOnServer(player) && !(spawnRateEffect?.IsActiveOnServer(player) ?? false)))
        {
            // Set the spawn rate if the effect is active for the player
            spawnRate = (int)(spawnRate / SpawnRateFactor);
            maxSpawns *= (int)(maxSpawns * SpawnRateFactor);
        }
    }

    #endregion
}