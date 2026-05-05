using System;
using System.Runtime.InteropServices;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x02000058 RID: 88
public class HiLoSlider : NetworkBehaviour
{
	// Token: 0x0600029F RID: 671 RVA: 0x0000DA9E File Offset: 0x0000BC9E
	private void OnEnable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Combine(InputEvents.OnInteractEvent, new Action<bool>(this.HandlePlayerInteract));
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x0000DAC0 File Offset: 0x0000BCC0
	private void OnDisable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Remove(InputEvents.OnInteractEvent, new Action<bool>(this.HandlePlayerInteract));
	}

	// Token: 0x060002A1 RID: 673 RVA: 0x0000DAE2 File Offset: 0x0000BCE2
	public override void OnStartServer()
	{
		base.OnStartServer();
		this.NetworkcurrentValue = 0.5f;
	}

	// Token: 0x060002A2 RID: 674 RVA: 0x0000DAF5 File Offset: 0x0000BCF5
	[Server]
	public void LockSlider(bool isLocked)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void HiLoSlider::LockSlider(System.Boolean)' called when server was not active");
			return;
		}
		if (this._isLocked == isLocked)
		{
			return;
		}
		this._isLocked = isLocked;
		if (this._isLocked)
		{
			this._currentInteract = null;
		}
	}

	// Token: 0x060002A3 RID: 675 RVA: 0x0000DB2C File Offset: 0x0000BD2C
	private void OnValueChanged(float oldValue, float newValue)
	{
		Vector3 endValue = Vector3.Lerp(this.startPoint.position, this.endPoint.position, newValue);
		this.knob.transform.DOMove(endValue, 0.1f, false);
		this.dicePercentageText.text = (newValue * 100f).ToString("0.#") + "%";
		this.underIndicator.localScale = new Vector3(newValue, this.underIndicator.localScale.y, this.underIndicator.localScale.z);
		this.overIndicator.localScale = new Vector3(1f - newValue, this.overIndicator.localScale.y, this.overIndicator.localScale.z);
		Action<float> onValueChangedAction = this.OnValueChangedAction;
		if (onValueChangedAction == null)
		{
			return;
		}
		onValueChangedAction(newValue);
	}

	// Token: 0x060002A4 RID: 676 RVA: 0x0000DC10 File Offset: 0x0000BE10
	public void OnPlayerInteract(PlayerInteract playerInteract)
	{
		this._localIsInteracting = true;
		this.CmdTrySetInteract(playerInteract.netIdentity);
	}

	// Token: 0x060002A5 RID: 677 RVA: 0x0000DC25 File Offset: 0x0000BE25
	private void HandlePlayerInteract(bool isPressed)
	{
		if (isPressed)
		{
			return;
		}
		this._localIsInteracting = false;
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x0000DC34 File Offset: 0x0000BE34
	[Command(requiresAuthority = false)]
	private void CmdTrySetInteract(NetworkIdentity player)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkIdentity(player);
		base.SendCommandInternal("System.Void HiLoSlider::CmdTrySetInteract(Mirror.NetworkIdentity)", 1086004524, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002A7 RID: 679 RVA: 0x0000DC70 File Offset: 0x0000BE70
	private void Update()
	{
		if (!this._localIsInteracting)
		{
			return;
		}
		float t;
		float num;
		FathF.NearestPointToRayOnLine(this.startPoint.position, this.endPoint.position, MonoSingleton<LocalManager>.Instance.mainCamera.transform.position, MonoSingleton<LocalManager>.Instance.mainCamera.transform.forward, out t, out num);
		if (num > 0f)
		{
			this.CmdTrySlide(NetworkClient.localPlayer, t);
		}
	}

	// Token: 0x060002A8 RID: 680 RVA: 0x0000DCE4 File Offset: 0x0000BEE4
	[Command(requiresAuthority = false)]
	private void CmdTrySlide(NetworkIdentity player, float t)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkIdentity(player);
		writer.WriteFloat(t);
		base.SendCommandInternal("System.Void HiLoSlider::CmdTrySlide(Mirror.NetworkIdentity,System.Single)", 6839220, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002A9 RID: 681 RVA: 0x0000DD28 File Offset: 0x0000BF28
	public HiLoSlider()
	{
		this._Mirror_SyncVarHookDelegate_currentValue = new Action<float, float>(this.OnValueChanged);
	}

	// Token: 0x060002AA RID: 682 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x1700004A RID: 74
	// (get) Token: 0x060002AB RID: 683 RVA: 0x0000DD58 File Offset: 0x0000BF58
	// (set) Token: 0x060002AC RID: 684 RVA: 0x0000DD6B File Offset: 0x0000BF6B
	public float NetworkcurrentValue
	{
		get
		{
			return this.currentValue;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.currentValue, 1UL, this._Mirror_SyncVarHookDelegate_currentValue);
		}
	}

	// Token: 0x060002AD RID: 685 RVA: 0x0000DD8A File Offset: 0x0000BF8A
	protected void UserCode_CmdTrySetInteract__NetworkIdentity(NetworkIdentity player)
	{
		if (this._isLocked)
		{
			return;
		}
		this._currentInteract = player;
	}

	// Token: 0x060002AE RID: 686 RVA: 0x0000DD9C File Offset: 0x0000BF9C
	protected static void InvokeUserCode_CmdTrySetInteract__NetworkIdentity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTrySetInteract called on client.");
			return;
		}
		((HiLoSlider)obj).UserCode_CmdTrySetInteract__NetworkIdentity(reader.ReadNetworkIdentity());
	}

	// Token: 0x060002AF RID: 687 RVA: 0x0000DDC5 File Offset: 0x0000BFC5
	protected void UserCode_CmdTrySlide__NetworkIdentity__Single(NetworkIdentity player, float t)
	{
		if (this._isLocked)
		{
			return;
		}
		if (this._currentInteract != player)
		{
			return;
		}
		this.NetworkcurrentValue = Mathf.Clamp(t, this.minTargetValue / 100f, this.maxTargetValue / 100f);
	}

	// Token: 0x060002B0 RID: 688 RVA: 0x0000DE03 File Offset: 0x0000C003
	protected static void InvokeUserCode_CmdTrySlide__NetworkIdentity__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTrySlide called on client.");
			return;
		}
		((HiLoSlider)obj).UserCode_CmdTrySlide__NetworkIdentity__Single(reader.ReadNetworkIdentity(), reader.ReadFloat());
	}

	// Token: 0x060002B1 RID: 689 RVA: 0x0000DE34 File Offset: 0x0000C034
	static HiLoSlider()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(HiLoSlider), "System.Void HiLoSlider::CmdTrySetInteract(Mirror.NetworkIdentity)", new RemoteCallDelegate(HiLoSlider.InvokeUserCode_CmdTrySetInteract__NetworkIdentity), false);
		RemoteProcedureCalls.RegisterCommand(typeof(HiLoSlider), "System.Void HiLoSlider::CmdTrySlide(Mirror.NetworkIdentity,System.Single)", new RemoteCallDelegate(HiLoSlider.InvokeUserCode_CmdTrySlide__NetworkIdentity__Single), false);
	}

	// Token: 0x060002B2 RID: 690 RVA: 0x0000DE84 File Offset: 0x0000C084
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this.currentValue);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteFloat(this.currentValue);
		}
	}

	// Token: 0x060002B3 RID: 691 RVA: 0x0000DEDC File Offset: 0x0000C0DC
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentValue, this._Mirror_SyncVarHookDelegate_currentValue, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentValue, this._Mirror_SyncVarHookDelegate_currentValue, reader.ReadFloat());
		}
	}

	// Token: 0x04000206 RID: 518
	[Header("References")]
	[SerializeField]
	private Transform knob;

	// Token: 0x04000207 RID: 519
	[SerializeField]
	private Transform startPoint;

	// Token: 0x04000208 RID: 520
	[SerializeField]
	private Transform endPoint;

	// Token: 0x04000209 RID: 521
	[SerializeField]
	private Transform underIndicator;

	// Token: 0x0400020A RID: 522
	[SerializeField]
	private Transform overIndicator;

	// Token: 0x0400020B RID: 523
	[SerializeField]
	private TextMeshPro dicePercentageText;

	// Token: 0x0400020C RID: 524
	[Header("Settings")]
	[SerializeField]
	private float minTargetValue = 1f;

	// Token: 0x0400020D RID: 525
	[SerializeField]
	private float maxTargetValue = 99f;

	// Token: 0x0400020E RID: 526
	[SyncVar(hook = "OnValueChanged")]
	public float currentValue;

	// Token: 0x0400020F RID: 527
	[CanBeNull]
	private NetworkIdentity _currentInteract;

	// Token: 0x04000210 RID: 528
	public Action<float> OnValueChangedAction;

	// Token: 0x04000211 RID: 529
	private bool _isLocked;

	// Token: 0x04000212 RID: 530
	private bool _localIsInteracting;

	// Token: 0x04000213 RID: 531
	public Action<float, float> _Mirror_SyncVarHookDelegate_currentValue;
}
