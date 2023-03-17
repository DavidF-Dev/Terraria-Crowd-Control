using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrowdControlMod.Config;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.InventoryEffects;

/// <summary>
///     Drop the held item, or choose an item from the hot-bar to drop.
/// </summary>
public sealed class DropItemEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const int MinDropSpeed = 9;
    private const int MaxDropSpeed = 11;

    #endregion

    #region Fields

    private Item? _droppedItem;
    private bool _spawnedOwl;

    #endregion

    #region Constructors

    public DropItemEffect() : base(EffectID.DropItem, null, EffectSeverity.Negative)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.inventory[player.Player.selectedItem].type != ItemID.None)
        {
            // Drop selected item
            player.Player.inventory[player.Player.selectedItem].favorited = false;
            _droppedItem = player.Player.inventory[player.Player.selectedItem];
            var oldSpeedX = player.Player.velocity.X;
            player.Player.velocity.X = Main.rand.Next(MinDropSpeed, MaxDropSpeed) * player.Player.direction;
            player.Player.DropSelectedItem();
            player.Player.velocity.X = oldSpeedX;
        }
        else
        {
            // Find an item to drop from the hot-bar
            List<int> slots = new();
            for (var i = 0; i < 10; i++)
            {
                if (player.Player.inventory[i].type == ItemID.None)
                {
                    continue;
                }

                slots.Add(i);
            }

            if (slots.Any())
            {
                // Choose an item
                var slot = Main.rand.Next(slots);
                var oldSel = player.Player.selectedItem;
                player.Player.inventory[slot].favorited = false;
                _droppedItem = player.Player.inventory[slot];
                player.Player.selectedItem = slot;
                var oldSpeedX = player.Player.velocity.X;
                player.Player.velocity.X = Main.rand.Next(MinDropSpeed, MaxDropSpeed) * player.Player.direction;
                player.Player.DropSelectedItem();
                player.Player.velocity.X = oldSpeedX;
                player.Player.selectedItem = oldSel;
            }
            else
            {
                // No item in the hot-bar
                return CrowdControlResponseStatus.Failure;
            }
        }

        // Luna's easter egg :-)
        if (NetUtils.IsSinglePlayer && player.Player.ZoneForest && SteamUtils.IsLunadabintu && Main.rand.NextBool(3) &&
            !Main.npc.Any(x => x.active && x.type == ModContent.NPCType<LunaOwl>()))
        {
            // Spawn an owl
            var owlIndex = NPC.NewNPC(null, (int)player.Player.Center.X, (int)player.Player.Top.Y - 16, ModContent.NPCType<LunaOwl>());
            Main.npc[owlIndex].AddBuff(BuffID.Lovestruck, 90);
            Main.npc[owlIndex].loveStruck = true;
            SoundEngine.PlaySound(SoundID.Owl, player.Player.Center);
            _spawnedOwl = true;
        }

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        _droppedItem = null;
        _spawnedOwl = false;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        if (_droppedItem == null || (!_spawnedOwl && CrowdControlConfig.GetInstance().HideDropItemMessage))
        {
            return;
        }

        var locKey = _droppedItem.stack > 1 ? $"{Id}_stack" : Id;
        var owlText = _spawnedOwl ? LangUtils.GetEffectMiscText(Id, "Owl") : string.Empty;
        var msg = LangUtils.GetEffectStartText(locKey, viewerString, playerString, durationString, Lang.GetItemName(_droppedItem.type), _droppedItem.stack);
        TerrariaUtils.WriteEffectMessage((short)_droppedItem.type, msg + owlText, Severity);
    }

    #endregion

    #region Nested Types

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class LunaOwl : ModNPC
    {
        #region Static Fields and Constants

        private static readonly string[] ChatMessages =
        {
            "You're a bit of a know-it-owl",
            "He does a lot of things, he's a jack of owl trades",
            "Hoot have thought it would be this easy?",
            "I’m talon you, it wasn't me!",
            "Look hoo's talking!",
            "Owl always love you.",
            "Have you heard about the owl party? It was a real hoot.”.",
            "What’s an owl’s least favourite subject? Owlgebra.",
            "Like feather like son.",
            "Have you checked the feather forecast?",
            "What’s an unstealthy owl called? A spotted owl.",
            "What does a well-educated owl say? Whooom.",
            "So I hear you all owl puns are bad. Says who?",
            "No more owl puns? Owl see what I can do about that.",
            "Hoot hoot.",
            "Some owls like to read murder mystery novels. They’re big fans of hoo-dunnits.",
            "Who’s the most famous owl magician in the world? Hoooo-dini, of course!",
            "How did the owl feel on his first date? Owl-kward!"
        };

        #endregion

        #region Properties

        public override string Texture => $"Terraria/Images/NPC_{NPCID.Owl}";

        #endregion

        #region Methods

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Luna's Consumable Owl");
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Owl];
            Main.npcCatchable[Type] = false;
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.Owl);
            AIType = NPCID.Owl;
            AnimationType = NPCID.Owl;
            NPC.aiStyle = NPCAIStyleID.Bird;
            NPC.lifeMax = 400;
            NPC.life = 400;
            NPC.DeathSound = SoundID.DeerclopsDeath;
            NPC.HitSound = SoundID.DeerclopsHit;
            NPC.chaseable = false;
        }

        public override bool PreAI()
        {
            // Change between worm/bird AI depending on how close the player is
            const float dist = 16f * 25 * 16f * 25f;
            var withinDist = NPC.Center.DistanceSQ(Main.LocalPlayer.Center) < dist;
            switch (withinDist)
            {
                case true when AIType == NPCID.Owl:
                    AIType = NPCID.Worm;
                    NPC.aiStyle = NPCAIStyleID.CritterWorm;
                    break;
                case false when AIType == NPCID.Worm:
                    AIType = NPCID.Owl;
                    NPC.aiStyle = NPCAIStyleID.Bird;
                    break;
            }

            return base.PreAI();
        }

        public override bool CanChat()
        {
            return true;
        }

        public override string GetChat()
        {
            return ChatMessages[Main.rand.Next(ChatMessages.Length)];
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
            button2 = null!;

            Main.LocalPlayer.currentShoppingSettings.HappinessReport = string.Empty;
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton)
            {
                shop = true;
            }
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ItemID.Owl);
            shop.item[nextSlot].shopCustomPrice = Item.buyPrice(0, 0, 5);
            nextSlot += 1;

            shop.item[nextSlot].SetDefaults(ItemID.OwlStatue);
            shop.item[nextSlot].shopCustomPrice = Item.buyPrice(0, 0, 20);
            nextSlot += 1;

            shop.item[nextSlot].SetDefaults(ItemID.OwlCage);
            shop.item[nextSlot].shopCustomPrice = Item.buyPrice(0, 0, 10);
            nextSlot += 1;

            shop.item[nextSlot].SetDefaults(ItemID.NightOwlPotion);
            shop.item[nextSlot].shopCustomPrice = Item.buyPrice(0, 0, 15);
            nextSlot += 1;

            shop.item[nextSlot].SetDefaults(ItemID.Acorn);
            shop.item[nextSlot].shopCustomPrice = Item.buyPrice(0, 0, 0, 20);
            nextSlot += 1;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life > 0)
            {
                return;
            }

            Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileID.SporeCloud, 10, 0f);
            for (var i = 0; i < 30; i++)
            {
                var velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 4f);
                Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.GreenBlood, velocity.X, velocity.Y);
            }

            TerrariaUtils.WriteMessage("You monster!", doLog: false);
        }

        #endregion
    }

    #endregion
}