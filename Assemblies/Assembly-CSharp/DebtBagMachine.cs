using System;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000104 RID: 260
public class DebtBagMachine : InteractableBase
{
	// Token: 0x06000AC7 RID: 2759 RVA: 0x0002B085 File Offset: 0x00029285
	public override void OnStartServer()
	{
		base.OnStartServer();
		this._debtBagMoneyAmount = (long)Math.Round((double)NetworkSingleton<MoneyManager>.Instance.balance / (double)this.spawnAmount);
	}

	// Token: 0x06000AC8 RID: 2760 RVA: 0x0002B0AC File Offset: 0x000292AC
	public override void ServerOnInteract(PlayerInteract playerInteract)
	{
		this.RpcInvokeInteract();
		if (this._spawnedBagAmount >= this.spawnAmount)
		{
			return;
		}
		this.SpawnBag();
	}

	// Token: 0x06000AC9 RID: 2761 RVA: 0x0002B0CC File Offset: 0x000292CC
	private void SpawnBag()
	{
		long num = this._debtBagMoneyAmount;
		if (this._spawnedBagAmount >= this.spawnAmount - 1)
		{
			num = NetworkSingleton<MoneyManager>.Instance.balance;
		}
		this._spawnedBagAmount++;
		if (!NetworkSingleton<MoneyManager>.Instance.TryChangeBalance(-num, null, ChangeType.Misc))
		{
			NetworkSingleton<MoneyManager>.Instance.SetBalance(0L, null, ChangeType.Misc);
		}
		Vector3 position = this.spawnPoint.position + Random.insideUnitSphere * this.spawnRadius;
		DebtBag debtBag = Object.Instantiate<DebtBag>(this.debtBagPrefab, position, Random.rotation);
		NetworkServer.Spawn(debtBag.gameObject, null);
		debtBag.ServerSetMoney(num);
		debtBag.ServerThrow(this.spawnPoint.position, Random.rotation, this.spawnPoint.forward * 5f, Random.insideUnitSphere * 10f);
	}

	// Token: 0x06000ACA RID: 2762 RVA: 0x0002B1A8 File Offset: 0x000293A8
	[ClientRpc]
	private void RpcInvokeInteract()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void DebtBagMachine::RpcInvokeInteract()", -1951586492, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000ACC RID: 2764 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000ACD RID: 2765 RVA: 0x0002B1D8 File Offset: 0x000293D8
	protected void UserCode_RpcInvokeInteract()
	{
		UnityEvent onRpcInteract = this.OnRpcInteract;
		if (onRpcInteract == null)
		{
			return;
		}
		onRpcInteract.Invoke();
	}

	// Token: 0x06000ACE RID: 2766 RVA: 0x0002B1EA File Offset: 0x000293EA
	protected static void InvokeUserCode_RpcInvokeInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeInteract called on server.");
			return;
		}
		((DebtBagMachine)obj).UserCode_RpcInvokeInteract();
	}

	// Token: 0x06000ACF RID: 2767 RVA: 0x0002B20D File Offset: 0x0002940D
	static DebtBagMachine()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DebtBagMachine), "System.Void DebtBagMachine::RpcInvokeInteract()", new RemoteCallDelegate(DebtBagMachine.InvokeUserCode_RpcInvokeInteract));
	}

	// Token: 0x040006C1 RID: 1729
	[Header("Settings")]
	[SerializeField]
	private float spawnRadius;

	// Token: 0x040006C2 RID: 1730
	[SerializeField]
	private int spawnAmount;

	// Token: 0x040006C3 RID: 1731
	[Header("References")]
	[SerializeField]
	private Transform spawnPoint;

	// Token: 0x040006C4 RID: 1732
	[SerializeField]
	private DebtBag debtBagPrefab;

	// Token: 0x040006C5 RID: 1733
	public UnityEvent OnRpcInteract;

	// Token: 0x040006C6 RID: 1734
	private int _spawnedBagAmount;

	// Token: 0x040006C7 RID: 1735
	private long _debtBagMoneyAmount;
}
