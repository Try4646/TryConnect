using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x020000C1 RID: 193
public class FollowUsSceneManager : NetworkSingleton<FollowUsSceneManager>
{
	// Token: 0x0600073D RID: 1853 RVA: 0x0001E7B3 File Offset: 0x0001C9B3
	public override void OnStartServer()
	{
		base.OnStartServer();
		base.StartCoroutine(this.SetSkippableRoutine());
	}

	// Token: 0x0600073E RID: 1854 RVA: 0x0001E7C8 File Offset: 0x0001C9C8
	public override void OnStartClient()
	{
		base.OnStartClient();
		UICursorSimple instance = UICursorSimple.Instance;
		if (instance == null)
		{
			return;
		}
		instance.ShowCursor();
	}

	// Token: 0x0600073F RID: 1855 RVA: 0x0001E7DF File Offset: 0x0001C9DF
	private IEnumerator SetSkippableRoutine()
	{
		yield return new WaitForSeconds(this.setSkippableDelay);
		this.RpcSetSkippable();
		yield break;
	}

	// Token: 0x06000740 RID: 1856 RVA: 0x0001E7F0 File Offset: 0x0001C9F0
	[ClientRpc]
	private void RpcSetSkippable()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void FollowUsSceneManager::RpcSetSkippable()", 122257351, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x0001E828 File Offset: 0x0001CA28
	protected void UserCode_RpcSetSkippable()
	{
		this.skipUI.Reset();
		this.skipUI.SetSkippableServer();
		this.skipUI.SetSkippableForLocal();
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x0001E84B File Offset: 0x0001CA4B
	protected static void InvokeUserCode_RpcSetSkippable(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSkippable called on server.");
			return;
		}
		((FollowUsSceneManager)obj).UserCode_RpcSetSkippable();
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x0001E86E File Offset: 0x0001CA6E
	static FollowUsSceneManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(FollowUsSceneManager), "System.Void FollowUsSceneManager::RpcSetSkippable()", new RemoteCallDelegate(FollowUsSceneManager.InvokeUserCode_RpcSetSkippable));
	}

	// Token: 0x040004DF RID: 1247
	[Header("Settings")]
	[SerializeField]
	private float setSkippableDelay;

	// Token: 0x040004E0 RID: 1248
	[Header("References")]
	[SerializeField]
	private SkipUI skipUI;
}
