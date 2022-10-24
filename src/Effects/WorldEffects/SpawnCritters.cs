using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
            Spawn(GetLocalPlayer(), SteamUtils.IsTheJayrBayr);
        }
        else
        {
            // Notify the server
            SendPacket(PacketID.HandleEffect, SteamUtils.IsTheJayrBayr);
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.BugNet, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        // Spawn the critters on the server
        Spawn(player, reader.ReadBoolean());
    }

    private void Spawn(ModPlayer player, bool spawnMagikarp)
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

            if (npc.friendly || Main.hardMode)
            {
                continue;
            }

            // Reduce life of evil critters significantly in pre-hardmode
            npc.lifeMax /= 4;
            npc.life = npc.lifeMax;

            if (Main.netMode == NetmodeID.Server)
            {
                // Sync the new max life for clients
                WorldUtils.SyncNPCSpecial(npc);
            }
        }

        if (!spawnMagikarp || player.Player.HasBuff<MagikarpPetBuff>() || !Main.rand.NextBool(3))
        {
            return;
        }

        // Spawn a magikarp
        var magikarpIndex = NPC.NewNPC(null, x + Main.rand.Next(-16, 16), y - 16, ModContent.NPCType<MagikarpNPC>());
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, magikarpIndex);
        }
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MagikarpNPC : ModNPC
    {
        #region Properties

        public override string Texture => $"{nameof(CrowdControlMod)}/src/Assets/ShinyMagikarp";

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shiny Magikarp");
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.Dolphin);
            AIType = NPCID.Dolphin;

            Main.npcFrameCount[Type] = 1;
            NPC.width = 44;
            NPC.height = 57;

            NPC.lifeMax = 129;
            NPC.defense = 100;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            TerrariaUtils.WriteMessage("A Shiny Magikarp appeared!");
            NPC.AddBuff(BuffID.Wet, int.MaxValue);
            SoundEngine.PlaySound(SoundID.SplashWeak, NPC.position);
        }

        public override bool CanChat()
        {
            return true;
        }

        public override string GetChat()
        {
            NPC.AddBuff(BuffID.Confused, 60 * 3);
            return "*glub... glub...*";
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (string.IsNullOrEmpty(button))
            {
                button = "Catch";
            }
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (!firstButton)
            {
                return;
            }

            // Attempt to catch
            if (Main.rand.NextBool(6))
            {
                Main.LocalPlayer.AddBuff(ModContent.BuffType<MagikarpPetBuff>(), 3600);
                TerrariaUtils.WriteMessage($"{Main.LocalPlayer.name} caught a Shiny Magikarp!");
            }
            else
            {
                TerrariaUtils.WriteMessage("The Shiny Magikarp got away!");
            }

            // Despawn
            NPC.active = false;
            HitEffect(NPC.Center.X < Main.LocalPlayer.Center.X ? -1 : 1, 0d);
            SoundEngine.PlaySound(SoundID.Drown, NPC.position);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }

            var packet = Mod.GetPacket(2);
            packet.Write((byte)PacketID.DespawnNPC);
            packet.Write(NPC.whoAmI);
            packet.Send();
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            // Water explosion
            for (var n = 0; n < 20; n++)
            {
                Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Wet, hitDirection * Main.rand.NextFloat(6f, 8f), Main.rand.NextFloatDirection() * 6f);
            }
        }

        #endregion
    }

    /// <summary>
    ///     https://github.com/tModLoader/tModLoader/blob/1.4/ExampleMod/Content/Pets/ExamplePet/ExamplePetBuff.cs
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MagikarpPetBuff : ModBuff
    {
        #region Properties

        public override string Texture => $"{nameof(CrowdControlMod)}/src/Assets/ShinyMagikarpBuff";

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shiny Magikarp");
            Description.SetDefault("*glub... glub...*");

            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            var projType = ModContent.ProjectileType<MagikarpPetProjectile>();

            // If the player is local, and there hasn't been a pet projectile spawned yet - spawn it
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
            }
        }

        #endregion
    }

    /// <summary>
    ///     https://github.com/tModLoader/tModLoader/blob/1.4/ExampleMod/Content/Pets/ExamplePet/ExamplePetProjectile.cs
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class MagikarpPetProjectile : ModProjectile
    {
        #region Static Fields and Constants

        private const int DripDelay = 10;

        #endregion

        #region Fields

        private int _dripTimer;

        #endregion

        #region Properties

        public override string Texture => $"{nameof(CrowdControlMod)}/src/Assets/ShinyMagikarp";

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Projectile.width = 44;
            Projectile.height = 57;

            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.CompanionCube);
            AIType = ProjectileID.CompanionCube;
        }

        public override bool PreAI()
        {
            var player = Main.player[Projectile.owner];
            player.companionCube = false;
            return true;
        }

        public override void AI()
        {
            // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff
            var player = Main.player[Projectile.owner];
            if (!player.dead && player.HasBuff(ModContent.BuffType<MagikarpPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            if (_dripTimer-- > 0)
            {
                return;
            }

            // Water drip
            Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Wet);
            _dripTimer = DripDelay;
        }

        #endregion
    }

    #endregion
}