using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x020000E1 RID: 225
public class DebtBag : Item
{
	// Token: 0x060008FE RID: 2302 RVA: 0x00024153 File Offset: 0x00022353
	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkSingleton<WinSceneManager>.Instance.debtBags.Add(this);
	}

	// Token: 0x060008FF RID: 2303 RVA: 0x0002416B File Offset: 0x0002236B
	[Server]
	public void ServerSetMoney(long money)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DebtBag::ServerSetMoney(System.Int64)' called when server was not active");
			return;
		}
		this.moneyInBag = money;
		this.RpcSetMoneyText(MoneyFormatter.FormatWithDollar(money));
	}

	// Token: 0x06000900 RID: 2304 RVA: 0x00024198 File Offset: 0x00022398
	[ClientRpc]
	private void RpcSetMoneyText(string text)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		this.SendRPCInternal("System.Void DebtBag::RpcSetMoneyText(System.String)", -2012068234, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000901 RID: 2305 RVA: 0x000241D4 File Offset: 0x000223D4
	[Server]
	public void ServerLock()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DebtBag::ServerLock()' called when server was not active");
			return;
		}
		base.ServerDrop();
		this.IsInteractable = false;
		this.Rb.isKinematic = true;
		this.Rb.interpolation = RigidbodyInterpolation.None;
		this.RpcLock();
	}

	// Token: 0x06000902 RID: 2306 RVA: 0x00024224 File Offset: 0x00022424
	[ClientRpc]
	private void RpcLock()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DebtBag::RpcLock()", -1393171932, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000903 RID: 2307 RVA: 0x00024254 File Offset: 0x00022454
	[Server]
	public void ServerSuckToPipe(Vector3 position, Vector3 direction)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DebtBag::ServerSuckToPipe(UnityEngine.Vector3,UnityEngine.Vector3)' called when server was not active");
			return;
		}
		this.RpcDisableColliders();
		base.StartCoroutine(this.SuckRoutine(position, direction.normalized));
	}

	// Token: 0x06000904 RID: 2308 RVA: 0x00024288 File Offset: 0x00022488
	[Server]
	private IEnumerator SuckRoutine(Vector3 pipeStart, Vector3 dir)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator DebtBag::SuckRoutine(UnityEngine.Vector3,UnityEngine.Vector3)' called when server was not active");
			return null;
		}
		DebtBag.<SuckRoutine>d__13 <SuckRoutine>d__ = new DebtBag.<SuckRoutine>d__13(0);
		<SuckRoutine>d__.<>4__this = this;
		<SuckRoutine>d__.pipeStart = pipeStart;
		<SuckRoutine>d__.dir = dir;
		return <SuckRoutine>d__;
	}

	// Token: 0x06000905 RID: 2309 RVA: 0x000242D4 File Offset: 0x000224D4
	[ClientRpc]
	private void RpcDisableColliders()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DebtBag::RpcDisableColliders()", 1300717950, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000907 RID: 2311 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000908 RID: 2312 RVA: 0x00024343 File Offset: 0x00022543
	protected void UserCode_RpcSetMoneyText__String(string text)
	{
		this.moneyText.text = text;
		this.InteractableName = text;
	}

	// Token: 0x06000909 RID: 2313 RVA: 0x00024358 File Offset: 0x00022558
	protected static void InvokeUserCode_RpcSetMoneyText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMoneyText called on server.");
			return;
		}
		((DebtBag)obj).UserCode_RpcSetMoneyText__String(reader.ReadString());
	}

	// Token: 0x0600090A RID: 2314 RVA: 0x00024381 File Offset: 0x00022581
	protected void UserCode_RpcLock()
	{
		if (base.isServer)
		{
			return;
		}
		this.IsInteractable = false;
		this.Rb.isKinematic = true;
		this.Rb.interpolation = RigidbodyInterpolation.None;
	}

	// Token: 0x0600090B RID: 2315 RVA: 0x000243AB File Offset: 0x000225AB
	protected static void InvokeUserCode_RpcLock(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLock called on server.");
			return;
		}
		((DebtBag)obj).UserCode_RpcLock();
	}

	// Token: 0x0600090C RID: 2316 RVA: 0x000243D0 File Offset: 0x000225D0
	protected void UserCode_RpcDisableColliders()
	{
		Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	// Token: 0x0600090D RID: 2317 RVA: 0x000243FB File Offset: 0x000225FB
	protected static void InvokeUserCode_RpcDisableColliders(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisableColliders called on server.");
			return;
		}
		((DebtBag)obj).UserCode_RpcDisableColliders();
	}

	// Token: 0x0600090E RID: 2318 RVA: 0x00024420 File Offset: 0x00022620
	static DebtBag()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DebtBag), "System.Void DebtBag::RpcSetMoneyText(System.String)", new RemoteCallDelegate(DebtBag.InvokeUserCode_RpcSetMoneyText__String));
		RemoteProcedureCalls.RegisterRpc(typeof(DebtBag), "System.Void DebtBag::RpcLock()", new RemoteCallDelegate(DebtBag.InvokeUserCode_RpcLock));
		RemoteProcedureCalls.RegisterRpc(typeof(DebtBag), "System.Void DebtBag::RpcDisableColliders()", new RemoteCallDelegate(DebtBag.InvokeUserCode_RpcDisableColliders));
	}

	// Token: 0x040005AF RID: 1455
	[Header("Settings")]
	[SerializeField]
	private float alignSpeed = 1f;

	// Token: 0x040005B0 RID: 1456
	[SerializeField]
	private float acceleration = 10f;

	// Token: 0x040005B1 RID: 1457
	[SerializeField]
	private float maxSpeed = 50f;

	// Token: 0x040005B2 RID: 1458
	[SerializeField]
	private float duration = 1.5f;

	// Token: 0x040005B3 RID: 1459
	[SerializeField]
	private float shrinkTime = 0.5f;

	// Token: 0x040005B4 RID: 1460
	[Header("References")]
	[SerializeField]
	private TextMeshPro moneyText;

	// Token: 0x040005B5 RID: 1461
	public long moneyInBag;
}
