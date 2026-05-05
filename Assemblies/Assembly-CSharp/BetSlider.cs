using System;
using System.Collections;
using System.Runtime.InteropServices;
using Extensions;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x0200008E RID: 142
public class BetSlider : NetworkBehaviour
{
	// Token: 0x06000519 RID: 1305 RVA: 0x00017005 File Offset: 0x00015205
	private void OnEnable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Combine(InputEvents.OnInteractEvent, new Action<bool>(this.HandlePlayerInteract));
	}

	// Token: 0x0600051A RID: 1306 RVA: 0x00017027 File Offset: 0x00015227
	private void OnDisable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Remove(InputEvents.OnInteractEvent, new Action<bool>(this.HandlePlayerInteract));
	}

	// Token: 0x0600051B RID: 1307 RVA: 0x00017049 File Offset: 0x00015249
	private void Start()
	{
		base.StartCoroutine(this.DelayedSetup());
	}

	// Token: 0x0600051C RID: 1308 RVA: 0x00017058 File Offset: 0x00015258
	private IEnumerator DelayedSetup()
	{
		yield return new WaitForSeconds(1f);
		this._localInteractor = NetworkClient.localPlayer.GetComponent<PlayerInteract>();
		this.Network_currentPercentage = 0f;
		this.knob.transform.position = Vector3.Lerp(this.startPoint.position, this.endPoint.position, 0f);
		this.betAmountText.text = "$0";
		yield break;
	}

	// Token: 0x0600051D RID: 1309 RVA: 0x00017068 File Offset: 0x00015268
	private void OnPercentageChanged(float oldValue, float newValue)
	{
		this.knob.transform.position = Vector3.Lerp(this.startPoint.position, this.endPoint.position, newValue);
		this.betAmountText.text = "$" + Mathf.RoundToInt(Mathf.Lerp(0f, (float)NetworkSingleton<MoneyManager>.Instance.balance, newValue)).ToString();
	}

	// Token: 0x0600051E RID: 1310 RVA: 0x000170D9 File Offset: 0x000152D9
	private void HandlePlayerInteract(bool isPressed)
	{
		if (isPressed)
		{
			return;
		}
		this.TryStopInteracting(this._localInteractor);
	}

	// Token: 0x0600051F RID: 1311 RVA: 0x000170EC File Offset: 0x000152EC
	[Command(requiresAuthority = false)]
	private void TryStopInteracting(PlayerInteract playerInteract)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(playerInteract);
		base.SendCommandInternal("System.Void BetSlider::TryStopInteracting(PlayerInteract)", 774954605, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000520 RID: 1312 RVA: 0x00017126 File Offset: 0x00015326
	[Server]
	public void OnPlayerInteract(PlayerInteract playerInteract)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BetSlider::OnPlayerInteract(PlayerInteract)' called when server was not active");
			return;
		}
		this._interactor = playerInteract;
		this._interactorHead = playerInteract.GetComponent<PlayerController>().head;
	}

	// Token: 0x06000521 RID: 1313 RVA: 0x00017155 File Offset: 0x00015355
	private void Update()
	{
		if (!base.isServer)
		{
			return;
		}
		if (!this._interactor)
		{
			return;
		}
		if (!this._interactorHead)
		{
			return;
		}
		this.Slide();
	}

	// Token: 0x06000522 RID: 1314 RVA: 0x00017184 File Offset: 0x00015384
	[Server]
	private void Slide()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BetSlider::Slide()' called when server was not active");
			return;
		}
		float network_currentPercentage;
		float num;
		FathF.NearestPointToRayOnLine(this.startPoint.position, this.endPoint.position, this._interactorHead.transform.position, this._interactorHead.transform.forward, out network_currentPercentage, out num);
		if (num > 0f)
		{
			this.Network_currentPercentage = network_currentPercentage;
		}
	}

	// Token: 0x06000523 RID: 1315 RVA: 0x000171F5 File Offset: 0x000153F5
	public BetSlider()
	{
		this._Mirror_SyncVarHookDelegate__currentPercentage = new Action<float, float>(this.OnPercentageChanged);
	}

	// Token: 0x06000524 RID: 1316 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000085 RID: 133
	// (get) Token: 0x06000525 RID: 1317 RVA: 0x00017210 File Offset: 0x00015410
	// (set) Token: 0x06000526 RID: 1318 RVA: 0x00017223 File Offset: 0x00015423
	public float Network_currentPercentage
	{
		get
		{
			return this._currentPercentage;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this._currentPercentage, 1UL, this._Mirror_SyncVarHookDelegate__currentPercentage);
		}
	}

	// Token: 0x06000527 RID: 1319 RVA: 0x00017242 File Offset: 0x00015442
	protected void UserCode_TryStopInteracting__PlayerInteract(PlayerInteract playerInteract)
	{
		if (this._interactor == playerInteract)
		{
			this._interactor = null;
		}
	}

	// Token: 0x06000528 RID: 1320 RVA: 0x00017259 File Offset: 0x00015459
	protected static void InvokeUserCode_TryStopInteracting__PlayerInteract(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command TryStopInteracting called on client.");
			return;
		}
		((BetSlider)obj).UserCode_TryStopInteracting__PlayerInteract(reader.ReadNetworkBehaviour<PlayerInteract>());
	}

	// Token: 0x06000529 RID: 1321 RVA: 0x00017282 File Offset: 0x00015482
	static BetSlider()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(BetSlider), "System.Void BetSlider::TryStopInteracting(PlayerInteract)", new RemoteCallDelegate(BetSlider.InvokeUserCode_TryStopInteracting__PlayerInteract), false);
	}

	// Token: 0x0600052A RID: 1322 RVA: 0x000172A8 File Offset: 0x000154A8
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this._currentPercentage);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteFloat(this._currentPercentage);
		}
	}

	// Token: 0x0600052B RID: 1323 RVA: 0x00017300 File Offset: 0x00015500
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._currentPercentage, this._Mirror_SyncVarHookDelegate__currentPercentage, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this._currentPercentage, this._Mirror_SyncVarHookDelegate__currentPercentage, reader.ReadFloat());
		}
	}

	// Token: 0x04000395 RID: 917
	[Header("References")]
	[SerializeField]
	private Transform knob;

	// Token: 0x04000396 RID: 918
	[SerializeField]
	private Transform startPoint;

	// Token: 0x04000397 RID: 919
	[SerializeField]
	private Transform endPoint;

	// Token: 0x04000398 RID: 920
	[SerializeField]
	private TMP_Text betAmountText;

	// Token: 0x04000399 RID: 921
	[CanBeNull]
	private PlayerInteract _interactor;

	// Token: 0x0400039A RID: 922
	[CanBeNull]
	private PlayerHead _interactorHead;

	// Token: 0x0400039B RID: 923
	[SyncVar(hook = "OnPercentageChanged")]
	private float _currentPercentage;

	// Token: 0x0400039C RID: 924
	private PlayerInteract _localInteractor;

	// Token: 0x0400039D RID: 925
	public Action<float, float> _Mirror_SyncVarHookDelegate__currentPercentage;
}
