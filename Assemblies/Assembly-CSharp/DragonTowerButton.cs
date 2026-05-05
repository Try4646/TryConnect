using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mirror;
using Mirror.RemoteCalls;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x0200004E RID: 78
public class DragonTowerButton : InteractableBase
{
	// Token: 0x0600022D RID: 557 RVA: 0x0000C198 File Offset: 0x0000A398
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		base.ServerOnInteract(playerInteract);
		this.dragonTower.OnPressButton(this.floorIndex, this.buttonIndex, this);
	}

	// Token: 0x0600022E RID: 558 RVA: 0x0000C1B9 File Offset: 0x0000A3B9
	public override void RpcOnInteract(PlayerInteract playerInteract)
	{
		base.RpcOnInteract(playerInteract);
		this.pressFb.PlayFeedbacks();
	}

	// Token: 0x0600022F RID: 559 RVA: 0x0000C1CD File Offset: 0x0000A3CD
	[Server]
	public void ServerSetButtonState(DragonTowerButton.ButtonState state)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DragonTowerButton::ServerSetButtonState(DragonTowerButton/ButtonState)' called when server was not active");
			return;
		}
		this.SetButtonState(state);
		this.RpcSetButtonState(state);
	}

	// Token: 0x06000230 RID: 560 RVA: 0x0000C1F4 File Offset: 0x0000A3F4
	[ClientRpc]
	private void RpcSetButtonState(DragonTowerButton.ButtonState state)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		Mirror.GeneratedNetworkCode._Write_DragonTowerButton/ButtonState(writer, state);
		this.SendRPCInternal("System.Void DragonTowerButton::RpcSetButtonState(DragonTowerButton/ButtonState)", -1637390773, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000231 RID: 561 RVA: 0x0000C230 File Offset: 0x0000A430
	private void SetButtonState(DragonTowerButton.ButtonState state)
	{
		this.buttonState = state;
		switch (state)
		{
		case DragonTowerButton.ButtonState.Inactive:
			this.IsInteractable = false;
			this.meshRenderer.material = this.materials[0];
			this.eggTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(delegate
			{
				this.eggTransform.gameObject.SetActive(false);
			});
			return;
		case DragonTowerButton.ButtonState.Clickable:
			this.IsInteractable = true;
			this.meshRenderer.material = this.materials[1];
			return;
		case DragonTowerButton.ButtonState.Red:
			this.IsInteractable = false;
			this.meshRenderer.material = this.materials[2];
			this.eggTransform.gameObject.SetActive(true);
			this.eggTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			this.explosionVfx.Play();
			return;
		case DragonTowerButton.ButtonState.Green:
			this.IsInteractable = false;
			this.meshRenderer.material = this.materials[3];
			return;
		case DragonTowerButton.ButtonState.RevealEgg:
			this.eggTransform.gameObject.SetActive(true);
			this.eggTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			return;
		default:
			return;
		}
	}

	// Token: 0x06000234 RID: 564 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000235 RID: 565 RVA: 0x0000C396 File Offset: 0x0000A596
	protected void UserCode_RpcSetButtonState__ButtonState(DragonTowerButton.ButtonState state)
	{
		if (base.isServer)
		{
			return;
		}
		this.SetButtonState(state);
	}

	// Token: 0x06000236 RID: 566 RVA: 0x0000C3A8 File Offset: 0x0000A5A8
	protected static void InvokeUserCode_RpcSetButtonState__ButtonState(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetButtonState called on server.");
			return;
		}
		((DragonTowerButton)obj).UserCode_RpcSetButtonState__ButtonState(Mirror.GeneratedNetworkCode._Read_DragonTowerButton/ButtonState(reader));
	}

	// Token: 0x06000237 RID: 567 RVA: 0x0000C3D1 File Offset: 0x0000A5D1
	static DragonTowerButton()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DragonTowerButton), "System.Void DragonTowerButton::RpcSetButtonState(DragonTowerButton/ButtonState)", new RemoteCallDelegate(DragonTowerButton.InvokeUserCode_RpcSetButtonState__ButtonState));
	}

	// Token: 0x040001C1 RID: 449
	[SerializeField]
	private DragonTower dragonTower;

	// Token: 0x040001C2 RID: 450
	[SerializeField]
	private int floorIndex;

	// Token: 0x040001C3 RID: 451
	[SerializeField]
	private int buttonIndex;

	// Token: 0x040001C4 RID: 452
	[SerializeField]
	private MeshRenderer meshRenderer;

	// Token: 0x040001C5 RID: 453
	[SerializeField]
	private Transform eggTransform;

	// Token: 0x040001C6 RID: 454
	[SerializeField]
	private ParticleSystem explosionVfx;

	// Token: 0x040001C7 RID: 455
	[SerializeField]
	private MMF_Player pressFb;

	// Token: 0x040001C8 RID: 456
	[SerializeField]
	private List<Material> materials = new List<Material>();

	// Token: 0x040001C9 RID: 457
	public DragonTowerButton.ButtonState buttonState;

	// Token: 0x0200004F RID: 79
	public enum ButtonState
	{
		// Token: 0x040001CB RID: 459
		Inactive,
		// Token: 0x040001CC RID: 460
		Clickable,
		// Token: 0x040001CD RID: 461
		Red,
		// Token: 0x040001CE RID: 462
		Green,
		// Token: 0x040001CF RID: 463
		RevealEgg
	}
}
