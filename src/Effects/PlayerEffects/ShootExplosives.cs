using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CrowdControlMod.Effects.PlayerEffects;

/// <summary>
///     Make the player drop (and shoot) either tile-breaking explosives or non-tile-breaking grenades.
/// </summary>
public sealed class ShootExplosives : CrowdControlEffect
{
    #region Enums

    public enum Shoot
    {
        Bombs,
        Grenades
    }

    #endregion

    #region Static Fields and Constants

    private const float MinExplosiveSpeed = 5f;
    private const float MaxExplosiveSpeed = 11f;
    private const int ShootChance = 85;

    private static readonly Dictionary<Shoot, short[]> VanillaExplosives = new()
    {
        {
            Shoot.Bombs, new[]
            {
                ProjectileID.Bomb, ProjectileID.StickyBomb, ProjectileID.BouncyBomb, ProjectileID.Dynamite, ProjectileID.BouncyDynamite,
                ProjectileID.DirtBomb, ProjectileID.HoneyBomb, ProjectileID.LavaBomb, ProjectileID.WetBomb, ProjectileID.BombFish,
                ProjectileID.ScarabBomb, ProjectileID.SmokeBomb
            }
        },
        {
            Shoot.Grenades, new[]
            {
                ProjectileID.Grenade, ProjectileID.StickyGrenade, ProjectileID.BouncyGrenade, ProjectileID.Beenade, ProjectileID.PartyGirlGrenade,
                ProjectileID.SmokeBomb, ProjectileID.ConfettiGun, ProjectileID.FlowerPetal, ProjectileID.OrnamentFriendly,
                ProjectileID.SantaBombs, ProjectileID.DD2GoblinBomb, ProjectileID.DryGrenade, ProjectileID.WetGrenade, ProjectileID.HoneyGrenade,
                ProjectileID.LavaGrenade
            }
        }
    };

    private static readonly Dictionary<Shoot, string[]> CalamityExplosives = new()
    {
        {
            Shoot.Bombs, new[]
            {
                "PlasmaGrenadeProjectile", "PenumbraBomb", "SealedSingularityProj", "SupernovaBomb", "WavePounderProjectile"
            }
        },
        {
            Shoot.Grenades, new[]
            {
                "BallisticPoisonBombProj", "BlastBarrelProjectile", "BettyExplosion", "BrackishFlaskProj", "ContaminatedBileFlask",
                "DesecratedWaterProj", "DuststormInABottleProj", "ExorcismProj", "MeteorFistProj", "PlaguenadeProj",
                "SeafoamBombProj", "ShockGrenadeProjectile", "DestructionStar", "TotalityFlask"
            }
        }
    };

    private static readonly Dictionary<Shoot, (int, int)> SpawnDelays = new()
    {
        {Shoot.Bombs, (60, 120)},
        {Shoot.Grenades, (10, 35)}
    };

    #endregion

    #region Static Methods

    [Pure]
    private static string GetId(Shoot shoot)
    {
        return shoot switch
        {
            Shoot.Bombs => EffectID.ShootBombs,
            Shoot.Grenades => EffectID.ShootGrenades,
            _ => throw new ArgumentOutOfRangeException(nameof(shoot), shoot, null)
        };
    }

    [Pure]
    private static EffectSeverity GetSeverity(Shoot shoot)
    {
        return shoot switch
        {
            Shoot.Bombs => EffectSeverity.Negative,
            Shoot.Grenades => EffectSeverity.Neutral,
            _ => EffectSeverity.Neutral
        };
    }

    #endregion

    #region Fields

    private readonly Dictionary<Shoot, IReadOnlyList<short>> _allExplosiveOptions = new();
    private readonly Shoot _shoot;
    private int _delay;

    #endregion

    #region Constructors

    public ShootExplosives(float duration, Shoot shoot) : base(GetId(shoot), duration, GetSeverity(shoot))
    {
        _shoot = shoot;

        var allBombOptions = VanillaExplosives[Shoot.Bombs].ToList();
        if (ModLoader.TryGetMod(ModID.Calamity, out var calamity))
        {
            // Add calamity bombs
            foreach (var calamityProjName in CalamityExplosives[Shoot.Bombs])
            {
                if(calamity.TryFind<ModProjectile>(calamityProjName, out var calamityProj))
                {
                    allBombOptions.Add((short)calamityProj.Type);
                }
            }
        }
        
        _allExplosiveOptions[Shoot.Bombs] = allBombOptions;
        
        var allGrenadeOptions = VanillaExplosives[Shoot.Grenades].ToList();
        if (calamity != null)
        {
            // Add calamity grenades
            foreach (var calamityProjName in CalamityExplosives[Shoot.Grenades])
            {
                if(calamity.TryFind<ModProjectile>(calamityProjName, out var calamityProj))
                {
                    allGrenadeOptions.Add((short)calamityProj.Type);
                }
            }
        }
        
        _allExplosiveOptions[Shoot.Grenades] = allGrenadeOptions;
    }

    #endregion

    #region Methods

    public override bool ShouldUpdate()
    {
        return !GetLocalPlayer().Player.IsWithinSpawnProtection();
    }

    protected override CrowdControlResponseStatus OnStart()
    {
        if (!_allExplosiveOptions.ContainsKey(_shoot) || !SpawnDelays.ContainsKey(_shoot))
        {
            // Unsupported
            return CrowdControlResponseStatus.Failure;
        }

        var player = GetLocalPlayer();
        if (player.Player.IsWithinSpawnProtection())
        {
            return CrowdControlResponseStatus.Retry;
        }

        player.PostUpdateHook += PostUpdate;
        player.ShootHook += PlayerShoot;
        return CrowdControlResponseStatus.Success;
    }

    protected override void OnStop()
    {
        var player = GetLocalPlayer();
        _delay = 0;
        player.PostUpdateHook -= PostUpdate;
        player.ShootHook -= PlayerShoot;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        short itemId = _shoot switch
        {
            Shoot.Bombs => ItemID.Bomb,
            Shoot.Grenades => ItemID.Grenade,
            _ => 0
        };

        var descriptor = _shoot switch
        {
            Shoot.Bombs => "bombs",
            Shoot.Grenades => "grenades",
            _ => "unknown"
        };

        TerrariaUtils.WriteEffectMessage(itemId, $"{viewerString} caused {playerString} to shoot {descriptor} for {durationString} seconds", Severity);
    }

    protected override void SendStopMessage()
    {
        TerrariaUtils.WriteEffectMessage(0, "No longer shooting explosives", EffectSeverity.Neutral);
    }

    private void PostUpdate()
    {
        var player = GetLocalPlayer();
        if (player.Player.IsWithinSpawnProtection())
        {
            return;
        }

        _delay--;
        if (_delay > 0)
        {
            return;
        }

        // Reset spawn delay
        var spawnDelay = SpawnDelays[_shoot];
        _delay = Main.rand.Next(spawnDelay.Item1, spawnDelay.Item2);

        // Choose explosive settings
        var explosiveIds = _allExplosiveOptions[_shoot];
        var explosiveId = explosiveIds[Main.rand.Next(explosiveIds.Count)];
        var speed = Main.rand.NextFloat(MinExplosiveSpeed, MaxExplosiveSpeed);

        // Spawn the explosive
        Projectile.NewProjectile(null, player.Player.Center, Main.rand.NextVector2Unit() * speed, explosiveId, 10, 1f, player.Player.whoAmI);
    }

    private bool PlayerShoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var projectile = new Projectile();
        projectile.SetDefaults(type);
        if (projectile.bobber || projectile.minion || projectile.sentry)
        {
            // Ignore for special projectiles
            return true;
        }

        if (Main.rand.Next(100) >= ShootChance)
        {
            // Random chance to occur
            return true;
        }

        var player = GetLocalPlayer();

        // Choose explosive settings
        var explosiveIds = _allExplosiveOptions[_shoot];
        var explosiveId = explosiveIds[Main.rand.Next(explosiveIds.Count)];
        var speed = Main.rand.NextFloat(MinExplosiveSpeed, MaxExplosiveSpeed);

        // Spawn the explosive
        Projectile.NewProjectile(null, player.Player.Center, Main.rand.NextVector2Unit() * speed, explosiveId, 10, 1f, player.Player.whoAmI);

        // Prevent vanilla item from shooting
        return false;
    }

    #endregion
}