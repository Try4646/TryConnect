using System;
using System.Collections;
using System.Runtime.InteropServices;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

// Token: 0x0200005B RID: 91
public class BankKnob : InteractableBase
{
	// Token: 0x060002ED RID: 749 RVA: 0x0000ECEC File Offset: 0x0000CEEC
	protected override void OnAwake()
	{
		base.OnAwake();
		if (this.knobTransform == null)
		{
			this.knobTransform = base.transform;
		}
		if (this.knobParent == null)
		{
			this.knobParent = ((base.transform.parent != null) ? base.transform.parent : base.transform);
		}
		this.initialLocalPosition = this.knobTransform.localPosition;
	}

	// Token: 0x060002EE RID: 750 RVA: 0x0000ED64 File Offset: 0x0000CF64
	public override void OnHold(PlayerInteract playerInteract)
	{
		base.OnHold(playerInteract);
		if (playerInteract != null && playerInteract.isLocalPlayer && this.playerCamera == null)
		{
			this.playerCamera = Camera.main;
			if (this.playerCamera != null)
			{
				this.lastCameraYaw = this.playerCamera.transform.eulerAngles.y;
			}
			this.CmdStartPulling();
		}
	}

	// Token: 0x060002EF RID: 751 RVA: 0x0000EDD1 File Offset: 0x0000CFD1
	public override void OnHoldExit(PlayerInteract playerInteract)
	{
		base.OnHoldExit(playerInteract);
		if (playerInteract != null && playerInteract.isLocalPlayer)
		{
			this.playerCamera = null;
			this.CmdStopPulling();
		}
	}

	// Token: 0x060002F0 RID: 752 RVA: 0x0000EDF8 File Offset: 0x0000CFF8
	public override void ServerOnHold(PlayerInteract playerInteract)
	{
		base.ServerOnHold(playerInteract);
		if (!this.isBeingPulled && playerInteract != null)
		{
			this.StartPulling(playerInteract);
		}
	}

	// Token: 0x060002F1 RID: 753 RVA: 0x0000EE19 File Offset: 0x0000D019
	public override void ServerOnHoldExit(PlayerInteract playerInteract)
	{
		base.ServerOnHoldExit(playerInteract);
		if (this.isBeingPulled && this.currentPullingPlayer == playerInteract)
		{
			this.StopPulling();
		}
	}

	// Token: 0x060002F2 RID: 754 RVA: 0x0000EE40 File Offset: 0x0000D040
	[Command(requiresAuthority = false)]
	private void CmdStartPulling()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void BankKnob::CmdStartPulling()", -2142497560, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002F3 RID: 755 RVA: 0x0000EE70 File Offset: 0x0000D070
	[Command(requiresAuthority = false)]
	private void CmdStopPulling()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		base.SendCommandInternal("System.Void BankKnob::CmdStopPulling()", -1716597248, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002F4 RID: 756 RVA: 0x0000EEA0 File Offset: 0x0000D0A0
	[Command(requiresAuthority = false)]
	private void CmdUpdatePullInput(float pullDelta)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(pullDelta);
		base.SendCommandInternal("System.Void BankKnob::CmdUpdatePullInput(System.Single)", 672272500, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002F5 RID: 757 RVA: 0x0000EEDA File Offset: 0x0000D0DA
	private void StartPulling(PlayerInteract playerInteract)
	{
		this.isBeingPulled = true;
		this.currentPullingPlayer = playerInteract;
		this.RpcStartPulling();
	}

	// Token: 0x060002F6 RID: 758 RVA: 0x0000EEF0 File Offset: 0x0000D0F0
	private void StopPulling()
	{
		this.isBeingPulled = false;
		this.currentPullingPlayer = null;
		this.RpcStopPulling();
	}

	// Token: 0x060002F7 RID: 759 RVA: 0x0000EF08 File Offset: 0x0000D108
	[ClientRpc]
	private void RpcStartPulling()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BankKnob::RpcStartPulling()", 1110737395, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002F8 RID: 760 RVA: 0x0000EF38 File Offset: 0x0000D138
	[ClientRpc]
	private void RpcStopPulling()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void BankKnob::RpcStopPulling()", -850882243, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060002F9 RID: 761 RVA: 0x0000EF68 File Offset: 0x0000D168
	private void Update()
	{
		if (this.IsBeingHold && this.playerCamera != null)
		{
			float y = this.playerCamera.transform.eulerAngles.y;
			float num = Mathf.DeltaAngle(this.lastCameraYaw, y);
			if (Mathf.Abs(num) > 0.01f)
			{
				float pullDelta = num * this.pullSensitivity * 0.01f;
				this.CmdUpdatePullInput(pullDelta);
				this.lastCameraYaw = y;
			}
		}
	}

	// Token: 0x060002FA RID: 762 RVA: 0x0000EFD8 File Offset: 0x0000D1D8
	private void OnPullDistanceChanged(float oldValue, float newValue)
	{
		this.UpdateKnobVisuals();
	}

	// Token: 0x060002FB RID: 763 RVA: 0x0000EFE0 File Offset: 0x0000D1E0
	private void FixedUpdate()
	{
		if (!base.isServer)
		{
			return;
		}
		if (this.knobTransform != null)
		{
			this.ClampKnobPosition();
		}
	}

	// Token: 0x060002FC RID: 764 RVA: 0x0000F000 File Offset: 0x0000D200
	private void UpdateKnobVisuals()
	{
		if (this.knobTransform == null)
		{
			return;
		}
		Vector3 localPosition = this.initialLocalPosition + this.pullAxis.normalized * this.currentPullDistance;
		this.knobTransform.localPosition = localPosition;
	}

	// Token: 0x060002FD RID: 765 RVA: 0x0000F04C File Offset: 0x0000D24C
	private void ClampKnobPosition()
	{
		if (this.knobTransform == null)
		{
			return;
		}
		float num = Vector3.Dot(this.knobTransform.localPosition - this.initialLocalPosition, this.pullAxis.normalized);
		num = Mathf.Clamp(num, 0f, this.maxPullDistance);
		this.knobTransform.localPosition = this.initialLocalPosition + this.pullAxis.normalized * num;
		this.NetworkcurrentPullDistance = num;
	}

	// Token: 0x060002FE RID: 766 RVA: 0x0000F0CF File Offset: 0x0000D2CF
	public void SetMaxValue(long max)
	{
		this.maxValue = Math.Max(this.minValue, max);
		if (this.maxValue > 0L && this.currentPullDistance > 0f)
		{
			float num = this.currentPullDistance / this.maxPullDistance;
		}
	}

	// Token: 0x060002FF RID: 767 RVA: 0x0000F108 File Offset: 0x0000D308
	public long GetCurrentValue()
	{
		double num = (double)(this.currentPullDistance / this.maxPullDistance);
		long num2 = this.maxValue - this.minValue;
		long num3 = this.minValue + (long)Math.Round(num * (double)num2);
		num3 = (long)Math.Round((double)num3 / (double)this.stepValue) * (long)this.stepValue;
		return Math.Max(this.minValue, Math.Min(num3, this.maxValue));
	}

	// Token: 0x06000300 RID: 768 RVA: 0x0000F174 File Offset: 0x0000D374
	public float GetNormalizedValue()
	{
		return this.currentPullDistance / this.maxPullDistance;
	}

	// Token: 0x06000301 RID: 769 RVA: 0x0000F184 File Offset: 0x0000D384
	[Server]
	public void SetNormalizedValue(float normalizedValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void BankKnob::SetNormalizedValue(System.Single)' called when server was not active");
			return;
		}
		normalizedValue = Mathf.Clamp01(normalizedValue);
		this.NetworkcurrentPullDistance = normalizedValue * this.maxPullDistance;
		this.UpdateKnobVisuals();
		Action<float> onKnobValueChanged = this.OnKnobValueChanged;
		if (onKnobValueChanged == null)
		{
			return;
		}
		onKnobValueChanged(normalizedValue);
	}

	// Token: 0x06000302 RID: 770 RVA: 0x0000F1D4 File Offset: 0x0000D3D4
	[Command(requiresAuthority = false)]
	public void CmdSetNormalizedValue(float normalizedValue)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(normalizedValue);
		base.SendCommandInternal("System.Void BankKnob::CmdSetNormalizedValue(System.Single)", 1520418840, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x06000303 RID: 771 RVA: 0x0000F20E File Offset: 0x0000D40E
	private IEnumerator SfxSliderRoutine()
	{
		if (this.sfxDragEvent.IsNull)
		{
			yield return null;
		}
		if (this.sfxSlideEvent.IsNull)
		{
			yield return null;
		}
		float num = 0f;
		float prev_value = num;
		bool canTick = true;
		float tickDivision = 0.1f;
		EventInstance slideInstance = RuntimeManager.CreateInstance(this.sfxSlideEvent);
		SFXParams[] sFXParams = new SFXParams[]
		{
			new SFXParams("valAmount", 0f)
		};
		while (this.isBeingPulled)
		{
			num = this.GetNormalizedValue();
			sFXParams[0].value = Math.Abs(prev_value - num) * 2f;
			if (num % tickDivision >= tickDivision - 0.01f && canTick)
			{
				SFXManager.SFXOneShotWithParameters(this.sfxDragEvent, sFXParams, base.transform.position, 1f + num);
				canTick = false;
			}
			else if (num % tickDivision <= tickDivision - 0.01f)
			{
				canTick = true;
			}
			prev_value = num;
			PLAYBACK_STATE playback_STATE;
			slideInstance.getPlaybackState(out playback_STATE);
			if (playback_STATE != PLAYBACK_STATE.PLAYING)
			{
				RuntimeManager.AttachInstanceToGameObject(slideInstance, base.gameObject, true);
				slideInstance.start();
			}
			slideInstance.setParameterByName("valAmount", sFXParams[0].value * 5f, false);
			yield return new WaitForEndOfFrame();
		}
		slideInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		slideInstance.release();
		yield return null;
		yield break;
	}

	// Token: 0x06000304 RID: 772 RVA: 0x0000F220 File Offset: 0x0000D420
	public BankKnob()
	{
		this._Mirror_SyncVarHookDelegate_currentPullDistance = new Action<float, float>(this.OnPullDistanceChanged);
	}

	// Token: 0x06000305 RID: 773 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000053 RID: 83
	// (get) Token: 0x06000306 RID: 774 RVA: 0x0000F27C File Offset: 0x0000D47C
	// (set) Token: 0x06000307 RID: 775 RVA: 0x0000F28F File Offset: 0x0000D48F
	public float NetworkcurrentPullDistance
	{
		get
		{
			return this.currentPullDistance;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.currentPullDistance, 1UL, this._Mirror_SyncVarHookDelegate_currentPullDistance);
		}
	}

	// Token: 0x06000308 RID: 776 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected void UserCode_CmdStartPulling()
	{
	}

	// Token: 0x06000309 RID: 777 RVA: 0x0000F2AE File Offset: 0x0000D4AE
	protected static void InvokeUserCode_CmdStartPulling(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartPulling called on client.");
			return;
		}
		((BankKnob)obj).UserCode_CmdStartPulling();
	}

	// Token: 0x0600030A RID: 778 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected void UserCode_CmdStopPulling()
	{
	}

	// Token: 0x0600030B RID: 779 RVA: 0x0000F2D1 File Offset: 0x0000D4D1
	protected static void InvokeUserCode_CmdStopPulling(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopPulling called on client.");
			return;
		}
		((BankKnob)obj).UserCode_CmdStopPulling();
	}

	// Token: 0x0600030C RID: 780 RVA: 0x0000F2F4 File Offset: 0x0000D4F4
	protected void UserCode_CmdUpdatePullInput__Single(float pullDelta)
	{
		if (!this.isBeingPulled)
		{
			return;
		}
		float num = this.currentPullDistance + pullDelta;
		num = Mathf.Clamp(num, 0f, this.maxPullDistance);
		if (Mathf.Abs(num - this.currentPullDistance) > 0.001f)
		{
			this.NetworkcurrentPullDistance = num;
			this.UpdateKnobVisuals();
			float obj = this.currentPullDistance / this.maxPullDistance;
			Action<float> onKnobValueChanged = this.OnKnobValueChanged;
			if (onKnobValueChanged == null)
			{
				return;
			}
			onKnobValueChanged(obj);
		}
	}

	// Token: 0x0600030D RID: 781 RVA: 0x0000F365 File Offset: 0x0000D565
	protected static void InvokeUserCode_CmdUpdatePullInput__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdatePullInput called on client.");
			return;
		}
		((BankKnob)obj).UserCode_CmdUpdatePullInput__Single(reader.ReadFloat());
	}

	// Token: 0x0600030E RID: 782 RVA: 0x0000F38F File Offset: 0x0000D58F
	protected void UserCode_RpcStartPulling()
	{
		base.StartCoroutine(this.SfxSliderRoutine());
	}

	// Token: 0x0600030F RID: 783 RVA: 0x0000F39E File Offset: 0x0000D59E
	protected static void InvokeUserCode_RpcStartPulling(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartPulling called on server.");
			return;
		}
		((BankKnob)obj).UserCode_RpcStartPulling();
	}

	// Token: 0x06000310 RID: 784 RVA: 0x000048A7 File Offset: 0x00002AA7
	protected void UserCode_RpcStopPulling()
	{
	}

	// Token: 0x06000311 RID: 785 RVA: 0x0000F3C1 File Offset: 0x0000D5C1
	protected static void InvokeUserCode_RpcStopPulling(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopPulling called on server.");
			return;
		}
		((BankKnob)obj).UserCode_RpcStopPulling();
	}

	// Token: 0x06000312 RID: 786 RVA: 0x0000F3E4 File Offset: 0x0000D5E4
	protected void UserCode_CmdSetNormalizedValue__Single(float normalizedValue)
	{
		this.SetNormalizedValue(normalizedValue);
	}

	// Token: 0x06000313 RID: 787 RVA: 0x0000F3ED File Offset: 0x0000D5ED
	protected static void InvokeUserCode_CmdSetNormalizedValue__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNormalizedValue called on client.");
			return;
		}
		((BankKnob)obj).UserCode_CmdSetNormalizedValue__Single(reader.ReadFloat());
	}

	// Token: 0x06000314 RID: 788 RVA: 0x0000F418 File Offset: 0x0000D618
	static BankKnob()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(BankKnob), "System.Void BankKnob::CmdStartPulling()", new RemoteCallDelegate(BankKnob.InvokeUserCode_CmdStartPulling), false);
		RemoteProcedureCalls.RegisterCommand(typeof(BankKnob), "System.Void BankKnob::CmdStopPulling()", new RemoteCallDelegate(BankKnob.InvokeUserCode_CmdStopPulling), false);
		RemoteProcedureCalls.RegisterCommand(typeof(BankKnob), "System.Void BankKnob::CmdUpdatePullInput(System.Single)", new RemoteCallDelegate(BankKnob.InvokeUserCode_CmdUpdatePullInput__Single), false);
		RemoteProcedureCalls.RegisterCommand(typeof(BankKnob), "System.Void BankKnob::CmdSetNormalizedValue(System.Single)", new RemoteCallDelegate(BankKnob.InvokeUserCode_CmdSetNormalizedValue__Single), false);
		RemoteProcedureCalls.RegisterRpc(typeof(BankKnob), "System.Void BankKnob::RpcStartPulling()", new RemoteCallDelegate(BankKnob.InvokeUserCode_RpcStartPulling));
		RemoteProcedureCalls.RegisterRpc(typeof(BankKnob), "System.Void BankKnob::RpcStopPulling()", new RemoteCallDelegate(BankKnob.InvokeUserCode_RpcStopPulling));
	}

	// Token: 0x06000315 RID: 789 RVA: 0x0000F4EC File Offset: 0x0000D6EC
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this.currentPullDistance);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteFloat(this.currentPullDistance);
		}
	}

	// Token: 0x06000316 RID: 790 RVA: 0x0000F544 File Offset: 0x0000D744
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentPullDistance, this._Mirror_SyncVarHookDelegate_currentPullDistance, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentPullDistance, this._Mirror_SyncVarHookDelegate_currentPullDistance, reader.ReadFloat());
		}
	}

	// Token: 0x04000233 RID: 563
	[Header("References")]
	[SerializeField]
	private Transform knobTransform;

	// Token: 0x04000234 RID: 564
	[SerializeField]
	private Transform knobParent;

	// Token: 0x04000235 RID: 565
	[SerializeField]
	private Vector3 pullAxis = Vector3.up;

	// Token: 0x04000236 RID: 566
	[SerializeField]
	private float maxPullDistance = 0.5f;

	// Token: 0x04000237 RID: 567
	[SerializeField]
	private float pullSensitivity = 1f;

	// Token: 0x04000238 RID: 568
	[Header("Settings")]
	[SerializeField]
	private long minValue;

	// Token: 0x04000239 RID: 569
	[SerializeField]
	private long maxValue = 1000L;

	// Token: 0x0400023A RID: 570
	[SerializeField]
	private int stepValue = 1;

	// Token: 0x0400023B RID: 571
	[Header("SFX")]
	[SerializeField]
	private EventReference sfxDragEvent;

	// Token: 0x0400023C RID: 572
	[SerializeField]
	private EventReference sfxSlideEvent;

	// Token: 0x0400023D RID: 573
	private Vector3 initialLocalPosition;

	// Token: 0x0400023E RID: 574
	private Vector3 currentPullDirection;

	// Token: 0x0400023F RID: 575
	[SyncVar(hook = "OnPullDistanceChanged")]
	private float currentPullDistance;

	// Token: 0x04000240 RID: 576
	private bool isBeingPulled;

	// Token: 0x04000241 RID: 577
	private PlayerInteract currentPullingPlayer;

	// Token: 0x04000242 RID: 578
	private Camera playerCamera;

	// Token: 0x04000243 RID: 579
	private float lastCameraYaw;

	// Token: 0x04000244 RID: 580
	public Action<float> OnKnobValueChanged;

	// Token: 0x04000245 RID: 581
	public Action<float, float> _Mirror_SyncVarHookDelegate_currentPullDistance;
}
