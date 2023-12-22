using System;
using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.Globals;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CrowdControlMod.Effects.WorldEffects;

/// <summary>
///     Effect that causes the player to spawn bees (friendly and hostile) via various actions.
/// </summary>
public sealed class NotTheBeesEffect : CrowdControlEffect
{
    #region Static Methods

    private static int GetBeeDamage(int type)
    {
        var dmg = type is ProjectileID.GiantBee or ProjectileID.Wasp ? 18f : 13f;

        // Decrease damage slightly (not in vanilla)
        dmg = (int)(dmg / 2f);

        if (Main.masterMode)
        {
            dmg *= 2;
        }
        else if (Main.expertMode)
        {
            dmg *= 1.5f;
        }

        if (type is ProjectileID.GiantBee or ProjectileID.Wasp)
        {
            return (int)(dmg + Main.rand.Next(1, 4));
        }

        return (int)(dmg + Main.rand.Next(2));
    }

    private static float GetBeeKnockback(int type)
    {
        return type is ProjectileID.GiantBee or ProjectileID.Wasp ? 0.5f : 0f;
    }

    #endregion

    #region Fields

    private int _counter;

    #endregion

    #region Constructors

    public NotTheBeesEffect(int duration) : base(EffectID.NotTheBees, duration, EffectSeverity.Neutral)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.World;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.shimmering)
        {
            return CrowdControlResponseStatus.Retry;
        }

        player.ShootHook += Shoot;
        player.OnHurtHook += OnHurt;
        CrowdControlNPC.StrikeNPCHook += OnStrikeNPC;
        CrowdControlTile.KillTileHook += OnKillTile;

        // Spawn bees initially
        SpawnBees(
            Main.rand.Next(11, 14),
            true,
            player.Player.Hitbox,
            Vector2.UnitX,
            MathHelper.Pi,
            0.3f,
            (byte)player.Player.whoAmI);

        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        player.ShootHook -= Shoot;
        player.OnHurtHook -= OnHurt;
        CrowdControlNPC.StrikeNPCHook -= OnStrikeNPC;
        CrowdControlTile.KillTileHook -= OnKillTile;

        _counter = 0;
    }

    protected override void OnUpdate(float delta)
    {
        if (_counter++ % 90 != 0 || !Main.rand.NextBool(2))
        {
            return;
        }

        // Spawn bees occasionally
        var player = Main.LocalPlayer;
        SpawnBees(
            Main.rand.Next(2, 4),
            true,
            player.Hitbox,
            Vector2.UnitX,
            MathHelper.Pi,
            0.3f,
            (byte)player.whoAmI);
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.BeeMask, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, LangUtils.GetEffectStopText(Id), EffectSeverity.Neutral);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        var num = reader.ReadInt32();
        var friendly = reader.ReadBoolean();
        var rect = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        var dir = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        var angleVar = reader.ReadSingle();
        var speed = reader.ReadSingle();
        var owner = (byte)player.Player.whoAmI;
        SpawnBees(num, friendly, rect, dir, angleVar, speed, owner);
    }

    private void SpawnBees(int num, bool friendly, Rectangle rect, Vector2 dir, float angleVar, float speed, byte owner)
    {
        num = Math.Clamp(num, 0, Main.maxProjectiles / 3);
        angleVar = Math.Abs(angleVar);
        speed = Math.Max(speed, 0);
        if (num == 0 || rect.IsEmpty)
        {
            return;
        }

        // Forward packet to server if called on a client
        if (NetUtils.IsClient)
        {
            SendPacket(PacketID.HandleEffect, num, friendly, rect.X, rect.Y, rect.Width, rect.Height, dir.X, dir.Y, angleVar, speed, owner);
            return;
        }

        // Increase number of bees if spawn rate is increased
        var incEffect = CrowdControlMod.GetInstance().GetEffect(EffectID.IncreaseSpawnRate);
        if (incEffect != null && ((NetUtils.IsSinglePlayer && incEffect.IsActive) || (NetUtils.IsServer && incEffect.IsActiveOnServer())))
        {
            num += friendly ? 1 : 2;
        }

        // Small chance for increased the amount of bees
        if (Main.rand.NextBool(12))
        {
            num = (int)(num * (friendly ? 2 : 1.5f));
        }

        // Spawn the bees!
        for (var i = 0; i < num; i++)
        {
            var pos = Main.rand.NextVector2FromRectangle(rect);
            var vel = dir.RotatedBy(Main.rand.NextFloat(-angleVar, angleVar)) * speed;

            if (friendly)
            {
                var type = Main.rand.NextFromList(ProjectileID.Bee, ProjectileID.GiantBee, ProjectileID.Wasp);
                var damage = GetBeeDamage(type);
                var knockback = GetBeeKnockback(type);
                var index = Projectile.NewProjectile(null, pos, vel, type, damage, knockback, owner);

                // Sync projectile
                if (NetUtils.IsServer)
                {
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
                }
            }
            else
            {
                var type = Main.rand.NextFromList(NPCID.Bee, NPCID.BeeSmall);
                var index = NPC.NewNPC(null, (int)pos.X, (int)pos.Y, type, Target: owner);
                var npc = Main.npc[index];
                npc.velocity = vel;

                // Sync npc
                if (NetUtils.IsServer)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
                }
            }
        }
    }

    private void OnHurt(Player.HurtInfo info)
    {
        var player = Main.LocalPlayer;
        if (!player.dead)
        {
            // Spawn bees when the player takes damage
            SpawnBees(
                Main.rand.Next(2, 6),
                true,
                player.Hitbox,
                Vector2.UnitX,
                MathHelper.Pi,
                Main.rand.NextFloat(0.2f, 0.3f),
                (byte)player.whoAmI);
        }
    }

    private bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (Main.rand.NextBool(3))
        {
            // Spawn bees when the player shoots a projectile from a weapon
            SpawnBees(
                Main.rand.Next(3, 4),
                true,
                new Rectangle((int)position.X, (int)position.Y, 1, 1),
                velocity.SafeNormalize(Vector2.Zero),
                MathHelper.ToRadians(10),
                velocity.Length() * Main.rand.NextFloat(0.7f, 0.9f),
                (byte)Main.myPlayer);
        }

        return true;
    }

    private void OnStrikeNPC(NPC npc, Entity source, Player? player, NPC.HitInfo hit, int damageDone)
    {
        if (npc is {friendly: false, chaseable: true, type: not NPCID.Bee and not NPCID.BeeSmall and not NPCID.TargetDummy} &&
            player != null && player.whoAmI == Main.myPlayer && Main.rand.NextBool(2))
        {
            // Spawn hostile bees when the player hits or kills a hostile npc
            SpawnBees(
                Main.rand.Next(2, 3) + (hit.Crit ? 1 : 0),
                false,
                npc.Hitbox,
                Vector2.UnitX,
                MathHelper.Pi,
                Main.rand.NextFloat(0.2f, 0.3f),
                (byte)player.whoAmI);
        }

        if (npc is {friendly: true, CountsAsACritter: true} &&
            player != null && player.whoAmI == Main.myPlayer && npc.life <= 0)
        {
            // Spawn hostile bees when the player kills a friendly critter
            SpawnBees(
                npc.life <= 0 ? Main.rand.Next(6, 9) : Main.rand.Next(1, 2),
                false,
                npc.Hitbox,
                Vector2.UnitX,
                MathHelper.Pi,
                Main.rand.NextFloat(0.2f, 0.3f),
                (byte)player.whoAmI);
        }
    }

    private void OnKillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail || effectOnly || noItem || TileID.Sets.IsMultitile[type] || !Main.rand.NextBool(3))
        {
            return;
        }

        // Spawn bees when the player kills a tile (hostile when in evil biome)
        var player = Main.LocalPlayer;
        SpawnBees(
            Main.rand.Next(1, 3),
            !Main.LocalPlayer.ZoneCorrupt && !Main.LocalPlayer.ZoneCrimson,
            new Rectangle(i * 16, j * 16, 16, 16),
            Vector2.UnitX,
            MathHelper.Pi,
            Main.rand.NextFloat(0.2f, 0.3f),
            (byte)player.whoAmI);
    }

    #endregion
}