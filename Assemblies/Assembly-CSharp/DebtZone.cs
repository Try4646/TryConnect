using System;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x020000BB RID: 187
public class DebtZone : NetworkBehaviour
{
	// Token: 0x0600070D RID: 1805 RVA: 0x0001DD91 File Offset: 0x0001BF91
	public override void OnStartServer()
	{
		base.OnStartServer();
		this._totalDebtMoney = NetworkSingleton<MoneyManager>.Instance.balance;
	}

	// Token: 0x0600070E RID: 1806 RVA: 0x0001DDA9 File Offset: 0x0001BFA9
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isServer)
		{
			base.enabled = false;
			return;
		}
	}

	// Token: 0x0600070F RID: 1807 RVA: 0x0001DDC1 File Offset: 0x0001BFC1
	private void Start()
	{
		this.moneyText.text = "$0 / " + MoneyFormatter.FormatWithDollar(NetworkSingleton<MoneyManager>.Instance.balance);
	}

	// Token: 0x06000710 RID: 1808 RVA: 0x0001DDE7 File Offset: 0x0001BFE7
	private void Update()
	{
		this.CheckDebtBags();
	}

	// Token: 0x06000711 RID: 1809 RVA: 0x0001DDF0 File Offset: 0x0001BFF0
	private void CheckDebtBags()
	{
		if (this._hasInitialized)
		{
			return;
		}
		if (Time.time - this._lastCheckTime < this.checkInterval)
		{
			return;
		}
		if (!NetworkSingleton<WinSceneManager>.Instance)
		{
			return;
		}
		this._lastCheckTime = Time.time;
		Bounds bounds = this.checkCollider.bounds;
		int num = 0;
		long num2 = 0L;
		foreach (DebtBag debtBag in NetworkSingleton<WinSceneManager>.Instance.debtBags)
		{
			if (bounds.Contains(debtBag.transform.position))
			{
				num++;
				num2 += debtBag.moneyInBag;
			}
		}
		this._moneyInZone = num2;
		if (this._bagAmountInZone != num)
		{
			bool hasIncreased = num > this._bagAmountInZone;
			this._bagAmountInZone = num;
			this.OnBagAmountChanged(hasIncreased);
			return;
		}
		this._bagAmountInZone = num;
	}

	// Token: 0x06000712 RID: 1810 RVA: 0x0001DEE0 File Offset: 0x0001C0E0
	private void OnBagAmountChanged(bool hasIncreased)
	{
		string str = MoneyFormatter.FormatWithDollar(this._moneyInZone);
		string str2 = MoneyFormatter.FormatWithDollar(this._totalDebtMoney);
		this.RpcSetMoneyText(str + " / " + str2);
		bool isEnabled = this._bagAmountInZone >= NetworkSingleton<WinSceneManager>.Instance.debtBags.Count && this._moneyInZone >= this._totalDebtMoney;
		this.RpcSetButtonEnabled(isEnabled);
	}

	// Token: 0x06000713 RID: 1811 RVA: 0x0001DF4C File Offset: 0x0001C14C
	[ClientRpc]
	private void RpcSetMoneyText(string text)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(text);
		this.SendRPCInternal("System.Void DebtZone::RpcSetMoneyText(System.String)", -854644100, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000714 RID: 1812 RVA: 0x0001DF88 File Offset: 0x0001C188
	[ClientRpc]
	private void RpcSetButtonEnabled(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void DebtZone::RpcSetButtonEnabled(System.Boolean)", -2117910075, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000715 RID: 1813 RVA: 0x0001DFC4 File Offset: 0x0001C1C4
	[Server]
	public void ServerTryInitializeZone()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DebtZone::ServerTryInitializeZone()' called when server was not active");
			return;
		}
		if (this._hasInitialized)
		{
			return;
		}
		if (this._bagAmountInZone < NetworkSingleton<WinSceneManager>.Instance.debtBags.Count)
		{
			return;
		}
		if (this._moneyInZone < this._totalDebtMoney)
		{
			return;
		}
		this._hasInitialized = true;
		UnityEvent unityEvent = this.onZoneInitialized;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	// Token: 0x06000717 RID: 1815 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06000718 RID: 1816 RVA: 0x0001E040 File Offset: 0x0001C240
	protected void UserCode_RpcSetMoneyText__String(string text)
	{
		this.moneyText.text = text;
	}

	// Token: 0x06000719 RID: 1817 RVA: 0x0001E04E File Offset: 0x0001C24E
	protected static void InvokeUserCode_RpcSetMoneyText__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetMoneyText called on server.");
			return;
		}
		((DebtZone)obj).UserCode_RpcSetMoneyText__String(reader.ReadString());
	}

	// Token: 0x0600071A RID: 1818 RVA: 0x0001E077 File Offset: 0x0001C277
	protected void UserCode_RpcSetButtonEnabled__Boolean(bool isEnabled)
	{
		this.button.IsInteractable = isEnabled;
		this.buttonRenderer.material = (isEnabled ? this.enabledMaterial : this.disabledMaterial);
	}

	// Token: 0x0600071B RID: 1819 RVA: 0x0001E0A1 File Offset: 0x0001C2A1
	protected static void InvokeUserCode_RpcSetButtonEnabled__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetButtonEnabled called on server.");
			return;
		}
		((DebtZone)obj).UserCode_RpcSetButtonEnabled__Boolean(reader.ReadBool());
	}

	// Token: 0x0600071C RID: 1820 RVA: 0x0001E0CC File Offset: 0x0001C2CC
	static DebtZone()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(DebtZone), "System.Void DebtZone::RpcSetMoneyText(System.String)", new RemoteCallDelegate(DebtZone.InvokeUserCode_RpcSetMoneyText__String));
		RemoteProcedureCalls.RegisterRpc(typeof(DebtZone), "System.Void DebtZone::RpcSetButtonEnabled(System.Boolean)", new RemoteCallDelegate(DebtZone.InvokeUserCode_RpcSetButtonEnabled__Boolean));
	}

	// Token: 0x040004B4 RID: 1204
	[Header("Settings")]
	[SerializeField]
	private float checkInterval = 0.1f;

	// Token: 0x040004B5 RID: 1205
	[Header("References")]
	[SerializeField]
	private Collider checkCollider;

	// Token: 0x040004B6 RID: 1206
	[SerializeField]
	private TextMeshPro moneyText;

	// Token: 0x040004B7 RID: 1207
	[SerializeField]
	private InteractableEventTrigger button;

	// Token: 0x040004B8 RID: 1208
	[SerializeField]
	private MeshRenderer buttonRenderer;

	// Token: 0x040004B9 RID: 1209
	[SerializeField]
	private Material enabledMaterial;

	// Token: 0x040004BA RID: 1210
	[SerializeField]
	private Material disabledMaterial;

	// Token: 0x040004BB RID: 1211
	[SerializeField]
	private UnityEvent onZoneInitialized;

	// Token: 0x040004BC RID: 1212
	private long _totalDebtMoney;

	// Token: 0x040004BD RID: 1213
	private long _moneyInZone;

	// Token: 0x040004BE RID: 1214
	private int _bagAmountInZone;

	// Token: 0x040004BF RID: 1215
	private float _lastCheckTime;

	// Token: 0x040004C0 RID: 1216
	private bool _hasInitialized;
}
