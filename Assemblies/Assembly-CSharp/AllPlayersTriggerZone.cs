using System;
using System.Collections;
using Extensions;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x0200028C RID: 652
public class AllPlayersTriggerZone : NetworkBehaviour
{
	// Token: 0x17000210 RID: 528
	// (get) Token: 0x06001737 RID: 5943 RVA: 0x0006280B File Offset: 0x00060A0B
	// (set) Token: 0x06001738 RID: 5944 RVA: 0x00062813 File Offset: 0x00060A13
	public bool IsActive
	{
		get
		{
			return this.isActive;
		}
		set
		{
			if (this.isActive == value)
			{
				return;
			}
			this.isActive = value;
			if (!this.isActive && this._countdownRoutine != null)
			{
				base.StopCoroutine(this._countdownRoutine);
				this._countdownRoutine = null;
				this.RpcUpdateCountdownText(false);
			}
		}
	}

	// Token: 0x06001739 RID: 5945 RVA: 0x0001DDA9 File Offset: 0x0001BFA9
	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isServer)
		{
			base.enabled = false;
			return;
		}
	}

	// Token: 0x0600173A RID: 5946 RVA: 0x00062850 File Offset: 0x00060A50
	private void Update()
	{
		this.CheckPlayers();
	}

	// Token: 0x0600173B RID: 5947 RVA: 0x00062858 File Offset: 0x00060A58
	private void CheckPlayers()
	{
		if (!this.IsActive)
		{
			return;
		}
		if (this._hasTriggered)
		{
			return;
		}
		if (!MonoSingleton<LocalManager>.Instance || MonoSingleton<LocalManager>.Instance.players == null || MonoSingleton<LocalManager>.Instance.players.Count <= 0)
		{
			return;
		}
		if (Time.time - this._lastCheckTime < this.colliderCheckInterval)
		{
			return;
		}
		this._lastCheckTime = Time.time;
		Bounds bounds = this.checkCollider.bounds;
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!bounds.Contains(playerReferences.transform.position))
			{
				if (this._countdownRoutine != null)
				{
					base.StopCoroutine(this._countdownRoutine);
					this._countdownRoutine = null;
					this.RpcUpdateCountdownText(false);
				}
				return;
			}
		}
		if (this._countdownRoutine == null)
		{
			this._countdownRoutine = base.StartCoroutine(this.CountdownRoutine());
		}
	}

	// Token: 0x0600173C RID: 5948 RVA: 0x00062964 File Offset: 0x00060B64
	[Server]
	private IEnumerator CountdownRoutine()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator AllPlayersTriggerZone::CountdownRoutine()' called when server was not active");
			return null;
		}
		AllPlayersTriggerZone.<CountdownRoutine>d__19 <CountdownRoutine>d__ = new AllPlayersTriggerZone.<CountdownRoutine>d__19(0);
		<CountdownRoutine>d__.<>4__this = this;
		return <CountdownRoutine>d__;
	}

	// Token: 0x0600173D RID: 5949 RVA: 0x000629A0 File Offset: 0x00060BA0
	[ClientRpc]
	private void RpcOnCountdownEnd()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void AllPlayersTriggerZone::RpcOnCountdownEnd()", 517119164, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600173E RID: 5950 RVA: 0x000629D0 File Offset: 0x00060BD0
	[ClientRpc]
	private void RpcUpdateCountdownText(bool start)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(start);
		this.SendRPCInternal("System.Void AllPlayersTriggerZone::RpcUpdateCountdownText(System.Boolean)", 1163986795, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x0600173F RID: 5951 RVA: 0x00062A0A File Offset: 0x00060C0A
	private IEnumerator CountdownTextRoutine()
	{
		this.countdownText.text = this.delayBeforeEvent.ToString("0.0");
		float elapsed = 0f;
		while (elapsed < this.delayBeforeEvent)
		{
			elapsed += Time.deltaTime;
			float num = this.delayBeforeEvent - elapsed;
			this.countdownText.text = num.ToString("0.0");
			yield return null;
		}
		yield break;
	}

	// Token: 0x06001740 RID: 5952 RVA: 0x00062A1C File Offset: 0x00060C1C
	[ClientRpc]
	private void RpcOnSoundEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void AllPlayersTriggerZone::RpcOnSoundEffect()", 572427076, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001741 RID: 5953 RVA: 0x00062A4C File Offset: 0x00060C4C
	[ClientRpc]
	private void RpcOnLeaveCasinoSoundEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void AllPlayersTriggerZone::RpcOnLeaveCasinoSoundEffect()", 692876386, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06001743 RID: 5955 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x06001744 RID: 5956 RVA: 0x00062A9A File Offset: 0x00060C9A
	protected void UserCode_RpcOnCountdownEnd()
	{
		if (this.animator)
		{
			this.animator.SetTrigger("isReady");
		}
	}

	// Token: 0x06001745 RID: 5957 RVA: 0x00062AB9 File Offset: 0x00060CB9
	protected static void InvokeUserCode_RpcOnCountdownEnd(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnCountdownEnd called on server.");
			return;
		}
		((AllPlayersTriggerZone)obj).UserCode_RpcOnCountdownEnd();
	}

	// Token: 0x06001746 RID: 5958 RVA: 0x00062ADC File Offset: 0x00060CDC
	protected void UserCode_RpcUpdateCountdownText__Boolean(bool start)
	{
		if (!this.countdownText)
		{
			return;
		}
		if (this._countdownTextRoutine != null)
		{
			base.StopCoroutine(this._countdownTextRoutine);
			this._countdownTextRoutine = null;
			this.countdownText.text = this.delayBeforeEvent.ToString("0.0");
		}
		if (start)
		{
			this._countdownTextRoutine = base.StartCoroutine(this.CountdownTextRoutine());
		}
	}

	// Token: 0x06001747 RID: 5959 RVA: 0x00062B42 File Offset: 0x00060D42
	protected static void InvokeUserCode_RpcUpdateCountdownText__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateCountdownText called on server.");
			return;
		}
		((AllPlayersTriggerZone)obj).UserCode_RpcUpdateCountdownText__Boolean(reader.ReadBool());
	}

	// Token: 0x06001748 RID: 5960 RVA: 0x00062B6B File Offset: 0x00060D6B
	protected void UserCode_RpcOnSoundEffect()
	{
		UnityEvent unityEvent = this.onSoundEffect;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	// Token: 0x06001749 RID: 5961 RVA: 0x00062B7D File Offset: 0x00060D7D
	protected static void InvokeUserCode_RpcOnSoundEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnSoundEffect called on server.");
			return;
		}
		((AllPlayersTriggerZone)obj).UserCode_RpcOnSoundEffect();
	}

	// Token: 0x0600174A RID: 5962 RVA: 0x00062BA0 File Offset: 0x00060DA0
	protected void UserCode_RpcOnLeaveCasinoSoundEffect()
	{
		UnityEvent unityEvent = this.onLeaveCasinoSoundEffect;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	// Token: 0x0600174B RID: 5963 RVA: 0x00062BB2 File Offset: 0x00060DB2
	protected static void InvokeUserCode_RpcOnLeaveCasinoSoundEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnLeaveCasinoSoundEffect called on server.");
			return;
		}
		((AllPlayersTriggerZone)obj).UserCode_RpcOnLeaveCasinoSoundEffect();
	}

	// Token: 0x0600174C RID: 5964 RVA: 0x00062BD8 File Offset: 0x00060DD8
	static AllPlayersTriggerZone()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(AllPlayersTriggerZone), "System.Void AllPlayersTriggerZone::RpcOnCountdownEnd()", new RemoteCallDelegate(AllPlayersTriggerZone.InvokeUserCode_RpcOnCountdownEnd));
		RemoteProcedureCalls.RegisterRpc(typeof(AllPlayersTriggerZone), "System.Void AllPlayersTriggerZone::RpcUpdateCountdownText(System.Boolean)", new RemoteCallDelegate(AllPlayersTriggerZone.InvokeUserCode_RpcUpdateCountdownText__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(AllPlayersTriggerZone), "System.Void AllPlayersTriggerZone::RpcOnSoundEffect()", new RemoteCallDelegate(AllPlayersTriggerZone.InvokeUserCode_RpcOnSoundEffect));
		RemoteProcedureCalls.RegisterRpc(typeof(AllPlayersTriggerZone), "System.Void AllPlayersTriggerZone::RpcOnLeaveCasinoSoundEffect()", new RemoteCallDelegate(AllPlayersTriggerZone.InvokeUserCode_RpcOnLeaveCasinoSoundEffect));
	}

	// Token: 0x04000F13 RID: 3859
	[Header("Settings")]
	[SerializeField]
	private float delayBeforeEvent = 1f;

	// Token: 0x04000F14 RID: 3860
	[SerializeField]
	private float colliderCheckInterval = 0.1f;

	// Token: 0x04000F15 RID: 3861
	[Header("References")]
	[SerializeField]
	private TextMeshPro countdownText;

	// Token: 0x04000F16 RID: 3862
	[SerializeField]
	private Collider checkCollider;

	// Token: 0x04000F17 RID: 3863
	[SerializeField]
	private Animator animator;

	// Token: 0x04000F18 RID: 3864
	[Header("Events")]
	[SerializeField]
	private UnityEvent onCountDownEnd;

	// Token: 0x04000F19 RID: 3865
	[SerializeField]
	private UnityEvent onSoundEffect;

	// Token: 0x04000F1A RID: 3866
	[SerializeField]
	private UnityEvent onLeaveCasinoSoundEffect;

	// Token: 0x04000F1B RID: 3867
	private Coroutine _countdownRoutine;

	// Token: 0x04000F1C RID: 3868
	private Coroutine _countdownTextRoutine;

	// Token: 0x04000F1D RID: 3869
	private bool _hasTriggered;

	// Token: 0x04000F1E RID: 3870
	private float _lastCheckTime;

	// Token: 0x04000F1F RID: 3871
	[SerializeField]
	private bool isActive;
}
