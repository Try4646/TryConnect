using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x02000066 RID: 102
public class MinesweeperTile : InteractableEventTrigger
{
	// Token: 0x06000381 RID: 897 RVA: 0x00010D99 File Offset: 0x0000EF99
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		this.minesweeperGame.RevealTile(this);
	}

	// Token: 0x06000382 RID: 898 RVA: 0x00010DAE File Offset: 0x0000EFAE
	[Server]
	public void ServerSetButtonColor(int i)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MinesweeperTile::ServerSetButtonColor(System.Int32)' called when server was not active");
			return;
		}
		this.RpcSetButtonColor(i);
	}

	// Token: 0x06000383 RID: 899 RVA: 0x00010DCC File Offset: 0x0000EFCC
	[Server]
	public void ServerSetMine(bool isEnabled)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MinesweeperTile::ServerSetMine(System.Boolean)' called when server was not active");
			return;
		}
		this.RpcSetMine(isEnabled);
	}

	// Token: 0x06000384 RID: 900 RVA: 0x00010DEA File Offset: 0x0000EFEA
	[Server]
	public void ServerExplode()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void MinesweeperTile::ServerExplode()' called when server was not active");
			return;
		}
		this.Explode();
	}

	// Token: 0x06000385 RID: 901 RVA: 0x00010E08 File Offset: 0x0000F008
	[ClientRpc]
	private void Explode()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void MinesweeperTile::Explode()", 290275528, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000386 RID: 902 RVA: 0x00010E38 File Offset: 0x0000F038
	[ClientRpc]
	private void RpcSetButtonColor(int i)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVarInt(i);
		this.SendRPCInternal("System.Void MinesweeperTile::RpcSetButtonColor(System.Int32)", 1786757900, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000387 RID: 903 RVA: 0x00010E74 File Offset: 0x0000F074
	[ClientRpc]
	private void RpcSetMine(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void MinesweeperTile::RpcSetMine(System.Boolean)", -1822623430, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000389 RID: 905 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x0600038A RID: 906 RVA: 0x00010EB8 File Offset: 0x0000F0B8
	protected void UserCode_Explode()
	{
		this.explodeVfx.Play();
		SFXManager.SFXOneShot(this.explosionSFX, base.transform.position);
		NetworkIdentity localPlayer = NetworkClient.localPlayer;
		Rigidbody rigidbody;
		if (!localPlayer.TryGetComponent<Rigidbody>(out rigidbody))
		{
			return;
		}
		if ((rigidbody.worldCenterOfMass - base.transform.position).sqrMagnitude < 4f)
		{
			PlayerController playerController;
			if (!localPlayer.TryGetComponent<PlayerController>(out playerController))
			{
				return;
			}
			Vector3 position = base.transform.position;
			Vector3 horizontalProjectionOfVector = FathF.GetHorizontalProjectionOfVector(rigidbody.worldCenterOfMass - position);
			Vector3 b = Vector3.up * this.explosionUpPower;
			Vector3 a = horizontalProjectionOfVector * this.explosionPower;
			float num = Vector3.Distance(rigidbody.worldCenterOfMass, position);
			float d = 1f - Mathf.Clamp01(num / 2f);
			Vector3 force = (a + b) * d;
			Vector3 vector = rigidbody.worldCenterOfMass - position;
			Vector3 torque = Vector3.Cross(Vector3.up, vector.normalized) * (this.explosionTorquePower * horizontalProjectionOfVector.magnitude);
			playerController.LocalKnockback(force, torque);
		}
	}

	// Token: 0x0600038B RID: 907 RVA: 0x00010FDD File Offset: 0x0000F1DD
	protected static void InvokeUserCode_Explode(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC Explode called on server.");
			return;
		}
		((MinesweeperTile)obj).UserCode_Explode();
	}

	// Token: 0x0600038C RID: 908 RVA: 0x00011000 File Offset: 0x0000F200
	protected void UserCode_RpcSetButtonColor__Int32(int i)
	{
		switch (i)
		{
		case 0:
			this.meshRenderer.material = this.defaultMaterial;
			return;
		case 1:
			this.meshRenderer.material = this.enabledMaterial;
			return;
		case 2:
			this.meshRenderer.material = this.revealMaterial;
			return;
		default:
			return;
		}
	}

	// Token: 0x0600038D RID: 909 RVA: 0x00011055 File Offset: 0x0000F255
	protected static void InvokeUserCode_RpcSetButtonColor__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetButtonColor called on server.");
			return;
		}
		((MinesweeperTile)obj).UserCode_RpcSetButtonColor__Int32(reader.ReadVarInt());
	}

	// Token: 0x0600038E RID: 910 RVA: 0x0001107E File Offset: 0x0000F27E
	protected void UserCode_RpcSetMine__Boolean(bool isEnabled)
	{
		if (isEnabled)
		{
			this.mineTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			return;
		}
		this.mineTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
	}

	// Token: 0x0600038F RID: 911 RVA: 0x000110BE File Offset: 0x0000F2BE
	protected static void InvokeUserCode_RpcSetMine__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMine called on server.");
			return;
		}
		((MinesweeperTile)obj).UserCode_RpcSetMine__Boolean(reader.ReadBool());
	}

	// Token: 0x06000390 RID: 912 RVA: 0x000110E8 File Offset: 0x0000F2E8
	static MinesweeperTile()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(MinesweeperTile), "System.Void MinesweeperTile::Explode()", new RemoteCallDelegate(MinesweeperTile.InvokeUserCode_Explode));
		RemoteProcedureCalls.RegisterRpc(typeof(MinesweeperTile), "System.Void MinesweeperTile::RpcSetButtonColor(System.Int32)", new RemoteCallDelegate(MinesweeperTile.InvokeUserCode_RpcSetButtonColor__Int32));
		RemoteProcedureCalls.RegisterRpc(typeof(MinesweeperTile), "System.Void MinesweeperTile::RpcSetMine(System.Boolean)", new RemoteCallDelegate(MinesweeperTile.InvokeUserCode_RpcSetMine__Boolean));
	}

	// Token: 0x0400028D RID: 653
	[Header("Settings")]
	[SerializeField]
	private float explosionPower;

	// Token: 0x0400028E RID: 654
	[SerializeField]
	private float explosionUpPower;

	// Token: 0x0400028F RID: 655
	[SerializeField]
	private float explosionTorquePower;

	// Token: 0x04000290 RID: 656
	[Header("References")]
	[SerializeField]
	private Minesweeper minesweeperGame;

	// Token: 0x04000291 RID: 657
	[SerializeField]
	private MeshRenderer meshRenderer;

	// Token: 0x04000292 RID: 658
	[SerializeField]
	private Transform mineTransform;

	// Token: 0x04000293 RID: 659
	[SerializeField]
	private Material defaultMaterial;

	// Token: 0x04000294 RID: 660
	[SerializeField]
	private Material enabledMaterial;

	// Token: 0x04000295 RID: 661
	[SerializeField]
	private Material revealMaterial;

	// Token: 0x04000296 RID: 662
	[SerializeField]
	private ParticleSystem explodeVfx;

	// Token: 0x04000297 RID: 663
	[Header("SFX")]
	[SerializeField]
	private EventReference explosionSFX;
}
