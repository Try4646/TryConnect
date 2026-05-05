using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000CC RID: 204
public class ConsumableItem : Item
{
	// Token: 0x060007E5 RID: 2021 RVA: 0x0001FE28 File Offset: 0x0001E028
	[Server]
	public void DestroyItem()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConsumableItem::DestroyItem()' called when server was not active");
			return;
		}
		if (base.isServer && NetworkSingleton<GameManager>.Instance != null && NetworkSingleton<GameManager>.Instance.state == GameState.Lobby && NetworkSingleton<ItemStampManager>.Instance != null)
		{
			NetworkSingleton<ItemStampManager>.Instance.OnLobbyStampItemConsumed(base.gameObject);
		}
		if (this.spawnableSo && NetworkSingleton<GameManager>.Instance.state == GameState.Game)
		{
			NetworkSingleton<ItemManager>.Instance.ServerRemoveItem(this.spawnableSo, this);
		}
		this.DestroyItemVFX();
		this.RpcDestroyItemVFX();
		base.ServerDrop();
		this.OnDestroyItem();
		this.RpcOnDestroyItem();
		base.StartCoroutine(this.DestroyItemCoroutine());
	}

	// Token: 0x060007E6 RID: 2022 RVA: 0x0001FEDE File Offset: 0x0001E0DE
	private IEnumerator DestroyItemCoroutine()
	{
		yield return null;
		NetworkServer.Destroy(base.gameObject);
		yield break;
	}

	// Token: 0x060007E7 RID: 2023 RVA: 0x0001FEF0 File Offset: 0x0001E0F0
	[ClientRpc]
	private void RpcOnDestroyItem()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void ConsumableItem::RpcOnDestroyItem()", -1983958164, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007E8 RID: 2024 RVA: 0x0001FF20 File Offset: 0x0001E120
	private void OnDestroyItem()
	{
		this.IsInteractable = false;
		this.Rb.isKinematic = true;
		base.SetEnableColliders(false);
		this.modelTransform.gameObject.SetActive(false);
	}

	// Token: 0x060007E9 RID: 2025 RVA: 0x0001FF50 File Offset: 0x0001E150
	[ClientRpc]
	private void RpcDestroyItemVFX()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void ConsumableItem::RpcDestroyItemVFX()", 841712571, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060007EA RID: 2026 RVA: 0x0001FF80 File Offset: 0x0001E180
	private void DestroyItemVFX()
	{
		if (!this.destroyVfx)
		{
			return;
		}
		Object.Destroy(Object.Instantiate<GameObject>(this.destroyVfx, this.modelTransform.position, Quaternion.identity), 3f);
	}

	// Token: 0x060007EC RID: 2028 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x060007ED RID: 2029 RVA: 0x0001FFBD File Offset: 0x0001E1BD
	protected void UserCode_RpcOnDestroyItem()
	{
		if (base.isServer)
		{
			return;
		}
		this.OnDestroyItem();
	}

	// Token: 0x060007EE RID: 2030 RVA: 0x0001FFCE File Offset: 0x0001E1CE
	protected static void InvokeUserCode_RpcOnDestroyItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDestroyItem called on server.");
			return;
		}
		((ConsumableItem)obj).UserCode_RpcOnDestroyItem();
	}

	// Token: 0x060007EF RID: 2031 RVA: 0x0001FFF1 File Offset: 0x0001E1F1
	protected void UserCode_RpcDestroyItemVFX()
	{
		if (base.isServer)
		{
			return;
		}
		this.DestroyItemVFX();
	}

	// Token: 0x060007F0 RID: 2032 RVA: 0x00020002 File Offset: 0x0001E202
	protected static void InvokeUserCode_RpcDestroyItemVFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDestroyItemVFX called on server.");
			return;
		}
		((ConsumableItem)obj).UserCode_RpcDestroyItemVFX();
	}

	// Token: 0x060007F1 RID: 2033 RVA: 0x00020028 File Offset: 0x0001E228
	static ConsumableItem()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(ConsumableItem), "System.Void ConsumableItem::RpcOnDestroyItem()", new RemoteCallDelegate(ConsumableItem.InvokeUserCode_RpcOnDestroyItem));
		RemoteProcedureCalls.RegisterRpc(typeof(ConsumableItem), "System.Void ConsumableItem::RpcDestroyItemVFX()", new RemoteCallDelegate(ConsumableItem.InvokeUserCode_RpcDestroyItemVFX));
	}

	// Token: 0x04000525 RID: 1317
	[SerializeField]
	private GameObject destroyVfx;
}
