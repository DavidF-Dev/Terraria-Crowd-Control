using System.IO;
using CrowdControlMod.CrowdControlService;
using CrowdControlMod.ID;
using CrowdControlMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace CrowdControlMod.Effects.InventoryEffects;

public sealed class SpawnVoidVaultEffect : CrowdControlEffect
{
    #region Static Fields and Constants

    private const float ProjSpeedX = 2.8f;
    private const float ProjSpeedY = -2.4f;

    #endregion

    #region Static Methods

    private static void SpawnVoidVault(Player player)
    {
        var spawnPos = player.Center;
        var spawnVel = new Vector2(player.direction * ProjSpeedX, ProjSpeedY);

        // Spawn a void bag portal thing
        var index = Projectile.NewProjectile(null, spawnPos, spawnVel, ProjectileID.VoidLens, 1, 1, player.whoAmI);

        // Sync
        if (NetUtils.IsServer)
        {
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
        }
    }

    #endregion

    #region Constructors

    public SpawnVoidVaultEffect() : base(EffectID.SpawnVoidVault, 0, EffectSeverity.Positive)
    {
    }

    #endregion

    #region Properties

    public override EffectCategory Category => EffectCategory.Inventory;

    protected override int StartEmote => EmoteID.BossMoonmoon;

    #endregion

    #region Methods

    protected override CrowdControlResponseStatus OnStart()
    {
        var player = GetLocalPlayer();
        if (player.Player.shimmering)
        {
            return CrowdControlResponseStatus.Retry;
        }

        if (NetUtils.IsSinglePlayer)
        {
            SpawnVoidVault(player.Player);
        }
        else
        {
            SendPacket(PacketID.HandleEffect);
        }

        SoundEngine.PlaySound(SoundID.Item130, player.Player.Center);
        return CrowdControlResponseStatus.Success;
    }

    protected override void SendStartMessage(string viewerString, string playerString, string? durationString)
    {
        TerrariaUtils.WriteEffectMessage(ItemID.VoidLens, LangUtils.GetEffectStartText(Id, viewerString, playerString, durationString), Severity);
    }

    protected override void OnReceivePacket(CrowdControlPlayer player, BinaryReader reader)
    {
        SpawnVoidVault(player.Player);
    }

    #endregion
}