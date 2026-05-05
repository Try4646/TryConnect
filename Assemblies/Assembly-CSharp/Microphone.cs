using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Dissonance;
using Dissonance.Integrations.MirrorIgnorance;
using Extensions;
using FMODUnity;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

// Token: 0x020000F0 RID: 240
public class Microphone : ConsumableItem
{
	// Token: 0x060009BA RID: 2490 RVA: 0x000048A7 File Offset: 0x00002AA7
	private void OnAmplitudeChanged(float oldValue, float newValue)
	{
	}

	// Token: 0x060009BB RID: 2491 RVA: 0x00027083 File Offset: 0x00025283
	private void OnChargeChanged(float oldValue, float newValue)
	{
		this.chargeIndicator.localScale = new Vector3(1f, Mathf.Clamp(newValue, 0.01f, 1f), 1f);
	}

	// Token: 0x060009BC RID: 2492 RVA: 0x000270B0 File Offset: 0x000252B0
	protected override void OnPickedUp(PlayerInventory playerInventory)
	{
		base.OnPickedUp(playerInventory);
		playerInventory.TryGetComponent<PlayerVoiceFX>(out this._playerVoiceFX);
		playerInventory.TryGetComponent<PlayerProfile>(out this._holderProfile);
		this._buffs.Clear();
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			this._buffs.Add(playerReferences.buff);
		}
		if (this._playerVoiceFX)
		{
			this._playerVoiceFX.RpcStartVoiceFX(VoipManipulationManager.VoipFX.Radio);
		}
		if (this._isActive && !this._hasEnded)
		{
			this.RpcSetArea(true);
		}
	}

	// Token: 0x060009BD RID: 2493 RVA: 0x00027170 File Offset: 0x00025370
	protected override void OnDropped(PlayerInventory playerInventory)
	{
		base.OnDropped(playerInventory);
		this.NetworkcurrentAmplitude = 0f;
		foreach (PlayerBuff playerBuff in this._buffs)
		{
			playerBuff.ResetBuffArea(PlayerBuffType.InspiringMelody, this);
		}
		this._buffs.Clear();
		if (this._playerVoiceFX)
		{
			this._playerVoiceFX.RpcResetVoiceFX();
			this._playerVoiceFX = null;
		}
		this.RpcSetArea(false);
		this.RpcOnDropped();
	}

	// Token: 0x060009BE RID: 2494 RVA: 0x0002720C File Offset: 0x0002540C
	[ClientRpc]
	private void RpcOnDropped()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Microphone::RpcOnDropped()", 1197080143, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009BF RID: 2495 RVA: 0x0002723C File Offset: 0x0002543C
	protected override void OnUseItem(bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		if (this._isActive)
		{
			return;
		}
		this._isActive = true;
		MirrorIgnorancePlayer component = NetworkClient.localPlayer.GetComponent<MirrorIgnorancePlayer>();
		if (component != null)
		{
			DissonanceComms singleton = DissonanceComms.GetSingleton();
			this._localVps = ((singleton != null) ? singleton.FindPlayer(component.PlayerId) : null);
		}
		this._localController = NetworkClient.localPlayer.GetComponent<PlayerController>();
		this.PlayMicOnSFX();
		this.anim.SetBool("IsActivated", true);
		this.buffArea.DOScale(Vector3.one * this.buffRange, 0.3f).SetEase(Ease.OutQuad);
		base.StartCoroutine(this.DurationRoutine());
		if (base.isServer)
		{
			base.StartCoroutine(this.DestroyItemCoroutine());
		}
	}

	// Token: 0x060009C0 RID: 2496 RVA: 0x000272FE File Offset: 0x000254FE
	private IEnumerator DurationRoutine()
	{
		float currentDuration = this.duration;
		while (currentDuration > 0f)
		{
			currentDuration -= Time.deltaTime;
			float y = Mathf.Clamp(currentDuration / this.duration, 0.01f, 1f);
			this.durationIndicator.localScale = new Vector3(1f, y, 1f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x060009C1 RID: 2497 RVA: 0x0002730D File Offset: 0x0002550D
	private IEnumerator DestroyItemCoroutine()
	{
		yield return new WaitForSeconds(this.duration);
		this._hasEnded = true;
		this.RpcOnDurationEnd();
		yield return new WaitForSeconds(1.5f);
		base.DestroyItem();
		yield break;
	}

	// Token: 0x060009C2 RID: 2498 RVA: 0x0002731C File Offset: 0x0002551C
	[ClientRpc]
	private void RpcOnDurationEnd()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Microphone::RpcOnDurationEnd()", 457111312, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009C3 RID: 2499 RVA: 0x0002734C File Offset: 0x0002554C
	private void Update()
	{
		if (!this._isActive)
		{
			return;
		}
		if (base.NetworkHolder)
		{
			this.CmdSendMicAmplitude(this._localController, this._localVps.Amplitude);
		}
		if (base.isServer)
		{
			if (base.NetworkHolder)
			{
				this.SetAmplitude();
			}
			this.SetCharge();
		}
	}

	// Token: 0x060009C4 RID: 2500 RVA: 0x000273A8 File Offset: 0x000255A8
	[Command(requiresAuthority = false)]
	private void CmdSendMicAmplitude(PlayerController identity, float amplitude)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(identity);
		writer.WriteFloat(amplitude);
		base.SendCommandInternal("System.Void Microphone::CmdSendMicAmplitude(PlayerController,System.Single)", 704293981, writer, 0, false);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009C5 RID: 2501 RVA: 0x000273EC File Offset: 0x000255EC
	private void SetAmplitude()
	{
		float num = 0f;
		foreach (KeyValuePair<PlayerController, float> keyValuePair in this._amplitudes)
		{
			PlayerController key = keyValuePair.Key;
			float value = keyValuePair.Value;
			if ((base.transform.position - key.head.transform.position).sqrMagnitude <= this.inputRange * this.inputRange)
			{
				num += value;
			}
		}
		num *= this.amplitudeMultiplier;
		num *= NetworkSingleton<UpgradeManager>.Instance.GetUpgradeData(this._holderProfile.steamId, PlayerUpgradeType.Stakeholder);
		this.NetworkcurrentAmplitude = Mathf.Lerp(this.currentAmplitude, num, Time.deltaTime * this.amplitudeLerp);
	}

	// Token: 0x060009C6 RID: 2502 RVA: 0x000274CC File Offset: 0x000256CC
	private void SetCharge()
	{
		if (this.currentAmplitude >= this.minThreshold)
		{
			float num = Mathf.InverseLerp(this.minThreshold, this.maxThreshold, this.currentAmplitude);
			this.NetworkcurrentCharge = Mathf.Clamp01(this.currentCharge + this.chargeIncrease * num * Time.deltaTime);
		}
		else
		{
			this.NetworkcurrentCharge = Mathf.Clamp01(this.currentCharge - this.chargeDecrease * Time.deltaTime);
		}
		foreach (PlayerBuff playerBuff in this._buffs)
		{
			BuffArea area = new BuffArea
			{
				Source = base.transform,
				Range = this.buffRange,
				Amount = this.currentCharge,
				IsActive = true
			};
			playerBuff.SetBuffArea(PlayerBuffType.InspiringMelody, this, area);
		}
	}

	// Token: 0x060009C7 RID: 2503 RVA: 0x000275B8 File Offset: 0x000257B8
	[ClientRpc]
	private void RpcSetArea(bool isEnabled)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isEnabled);
		this.SendRPCInternal("System.Void Microphone::RpcSetArea(System.Boolean)", -653807674, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009C8 RID: 2504 RVA: 0x000275F4 File Offset: 0x000257F4
	[ClientRpc]
	private void PlayMicOnSFX()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		this.SendRPCInternal("System.Void Microphone::PlayMicOnSFX()", -2037547392, writer, 0, true);
		NetworkWriterPool.Return(writer);
	}

	// Token: 0x060009C9 RID: 2505 RVA: 0x00027624 File Offset: 0x00025824
	private void MicTapSfx()
	{
		SFXManager.SFXOneShot(this.micTapSfx, base.transform.position);
	}

	// Token: 0x060009CA RID: 2506 RVA: 0x0002763C File Offset: 0x0002583C
	private void MicClickSfx()
	{
		SFXManager.SFXOneShot(this.micClickSfx, base.transform.position);
	}

	// Token: 0x060009CB RID: 2507 RVA: 0x00027654 File Offset: 0x00025854
	private void DisableMic()
	{
		this.micTransform.gameObject.SetActive(false);
	}

	// Token: 0x060009CC RID: 2508 RVA: 0x00027668 File Offset: 0x00025868
	public Microphone()
	{
		this._Mirror_SyncVarHookDelegate_currentAmplitude = new Action<float, float>(this.OnAmplitudeChanged);
		this._Mirror_SyncVarHookDelegate_currentCharge = new Action<float, float>(this.OnChargeChanged);
	}

	// Token: 0x060009CD RID: 2509 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x170000DF RID: 223
	// (get) Token: 0x060009CE RID: 2510 RVA: 0x00027718 File Offset: 0x00025918
	// (set) Token: 0x060009CF RID: 2511 RVA: 0x0002772B File Offset: 0x0002592B
	public float NetworkcurrentAmplitude
	{
		get
		{
			return this.currentAmplitude;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.currentAmplitude, 2UL, this._Mirror_SyncVarHookDelegate_currentAmplitude);
		}
	}

	// Token: 0x170000E0 RID: 224
	// (get) Token: 0x060009D0 RID: 2512 RVA: 0x0002774C File Offset: 0x0002594C
	// (set) Token: 0x060009D1 RID: 2513 RVA: 0x0002775F File Offset: 0x0002595F
	public float NetworkcurrentCharge
	{
		get
		{
			return this.currentCharge;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.currentCharge, 4UL, this._Mirror_SyncVarHookDelegate_currentCharge);
		}
	}

	// Token: 0x060009D2 RID: 2514 RVA: 0x0002777E File Offset: 0x0002597E
	protected void UserCode_RpcOnDropped()
	{
		this.anim.Play("Default", 0, 0f);
		this.anim.Update(0f);
	}

	// Token: 0x060009D3 RID: 2515 RVA: 0x000277A6 File Offset: 0x000259A6
	protected static void InvokeUserCode_RpcOnDropped(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDropped called on server.");
			return;
		}
		((global::Microphone)obj).UserCode_RpcOnDropped();
	}

	// Token: 0x060009D4 RID: 2516 RVA: 0x000277C9 File Offset: 0x000259C9
	protected void UserCode_RpcOnDurationEnd()
	{
		this.buffArea.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
		if (base.NetworkHolder)
		{
			this.anim.SetTrigger("Drop");
		}
	}

	// Token: 0x060009D5 RID: 2517 RVA: 0x00027804 File Offset: 0x00025A04
	protected static void InvokeUserCode_RpcOnDurationEnd(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnDurationEnd called on server.");
			return;
		}
		((global::Microphone)obj).UserCode_RpcOnDurationEnd();
	}

	// Token: 0x060009D6 RID: 2518 RVA: 0x00027827 File Offset: 0x00025A27
	protected void UserCode_CmdSendMicAmplitude__PlayerController__Single(PlayerController identity, float amplitude)
	{
		this._amplitudes[identity] = amplitude;
	}

	// Token: 0x060009D7 RID: 2519 RVA: 0x00027836 File Offset: 0x00025A36
	protected static void InvokeUserCode_CmdSendMicAmplitude__PlayerController__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendMicAmplitude called on client.");
			return;
		}
		((global::Microphone)obj).UserCode_CmdSendMicAmplitude__PlayerController__Single(reader.ReadNetworkBehaviour<PlayerController>(), reader.ReadFloat());
	}

	// Token: 0x060009D8 RID: 2520 RVA: 0x00027866 File Offset: 0x00025A66
	protected void UserCode_RpcSetArea__Boolean(bool isEnabled)
	{
		this.buffArea.DOScale(isEnabled ? (Vector3.one * this.buffRange) : Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
	}

	// Token: 0x060009D9 RID: 2521 RVA: 0x00027899 File Offset: 0x00025A99
	protected static void InvokeUserCode_RpcSetArea__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetArea called on server.");
			return;
		}
		((global::Microphone)obj).UserCode_RpcSetArea__Boolean(reader.ReadBool());
	}

	// Token: 0x060009DA RID: 2522 RVA: 0x000278C2 File Offset: 0x00025AC2
	protected void UserCode_PlayMicOnSFX()
	{
		SFXManager.SFXOneShot3DAttached(this.micOnSfx, base.gameObject, false);
	}

	// Token: 0x060009DB RID: 2523 RVA: 0x000278D6 File Offset: 0x00025AD6
	protected static void InvokeUserCode_PlayMicOnSFX(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayMicOnSFX called on server.");
			return;
		}
		((global::Microphone)obj).UserCode_PlayMicOnSFX();
	}

	// Token: 0x060009DC RID: 2524 RVA: 0x000278FC File Offset: 0x00025AFC
	static Microphone()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(global::Microphone), "System.Void Microphone::CmdSendMicAmplitude(PlayerController,System.Single)", new RemoteCallDelegate(global::Microphone.InvokeUserCode_CmdSendMicAmplitude__PlayerController__Single), false);
		RemoteProcedureCalls.RegisterRpc(typeof(global::Microphone), "System.Void Microphone::RpcOnDropped()", new RemoteCallDelegate(global::Microphone.InvokeUserCode_RpcOnDropped));
		RemoteProcedureCalls.RegisterRpc(typeof(global::Microphone), "System.Void Microphone::RpcOnDurationEnd()", new RemoteCallDelegate(global::Microphone.InvokeUserCode_RpcOnDurationEnd));
		RemoteProcedureCalls.RegisterRpc(typeof(global::Microphone), "System.Void Microphone::RpcSetArea(System.Boolean)", new RemoteCallDelegate(global::Microphone.InvokeUserCode_RpcSetArea__Boolean));
		RemoteProcedureCalls.RegisterRpc(typeof(global::Microphone), "System.Void Microphone::PlayMicOnSFX()", new RemoteCallDelegate(global::Microphone.InvokeUserCode_PlayMicOnSFX));
	}

	// Token: 0x060009DD RID: 2525 RVA: 0x000279AC File Offset: 0x00025BAC
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this.currentAmplitude);
			writer.WriteFloat(this.currentCharge);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 2UL) != 0UL)
		{
			writer.WriteFloat(this.currentAmplitude);
		}
		if ((this.syncVarDirtyBits & 4UL) != 0UL)
		{
			writer.WriteFloat(this.currentCharge);
		}
	}

	// Token: 0x060009DE RID: 2526 RVA: 0x00027A34 File Offset: 0x00025C34
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentAmplitude, this._Mirror_SyncVarHookDelegate_currentAmplitude, reader.ReadFloat());
			base.GeneratedSyncVarDeserialize<float>(ref this.currentCharge, this._Mirror_SyncVarHookDelegate_currentCharge, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 2L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentAmplitude, this._Mirror_SyncVarHookDelegate_currentAmplitude, reader.ReadFloat());
		}
		if ((num & 4L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.currentCharge, this._Mirror_SyncVarHookDelegate_currentCharge, reader.ReadFloat());
		}
	}

	// Token: 0x04000627 RID: 1575
	[Header("Settings")]
	[SerializeField]
	private float minThreshold = 0.1f;

	// Token: 0x04000628 RID: 1576
	[SerializeField]
	private float maxThreshold = 0.5f;

	// Token: 0x04000629 RID: 1577
	[SerializeField]
	private float chargeIncrease = 0.1f;

	// Token: 0x0400062A RID: 1578
	[SerializeField]
	private float chargeDecrease = 0.1f;

	// Token: 0x0400062B RID: 1579
	[SerializeField]
	private float amplitudeLerp = 0.01f;

	// Token: 0x0400062C RID: 1580
	[SerializeField]
	private float amplitudeMultiplier = 100f;

	// Token: 0x0400062D RID: 1581
	[SerializeField]
	private float inputRange = 2f;

	// Token: 0x0400062E RID: 1582
	[SerializeField]
	private float buffRange = 10f;

	// Token: 0x0400062F RID: 1583
	[SerializeField]
	private float duration = 30f;

	// Token: 0x04000630 RID: 1584
	[Header("References")]
	[SerializeField]
	private TextMeshPro multiplierText;

	// Token: 0x04000631 RID: 1585
	[SerializeField]
	private Transform durationIndicator;

	// Token: 0x04000632 RID: 1586
	[SerializeField]
	private Transform chargeIndicator;

	// Token: 0x04000633 RID: 1587
	[SerializeField]
	private Transform buffArea;

	// Token: 0x04000634 RID: 1588
	[SerializeField]
	private Animator anim;

	// Token: 0x04000635 RID: 1589
	[SerializeField]
	private Transform micTransform;

	// Token: 0x04000636 RID: 1590
	[Header("SFX")]
	[SerializeField]
	private EventReference micOnSfx;

	// Token: 0x04000637 RID: 1591
	[SerializeField]
	private EventReference micTapSfx;

	// Token: 0x04000638 RID: 1592
	[SerializeField]
	private EventReference micClickSfx;

	// Token: 0x04000639 RID: 1593
	private bool _isActive;

	// Token: 0x0400063A RID: 1594
	private bool _hasEnded;

	// Token: 0x0400063B RID: 1595
	[SerializeField]
	[SyncVar(hook = "OnAmplitudeChanged")]
	private float currentAmplitude;

	// Token: 0x0400063C RID: 1596
	[SerializeField]
	[SyncVar(hook = "OnChargeChanged")]
	private float currentCharge;

	// Token: 0x0400063D RID: 1597
	private Dictionary<PlayerController, float> _amplitudes = new Dictionary<PlayerController, float>();

	// Token: 0x0400063E RID: 1598
	private PlayerProfile _holderProfile;

	// Token: 0x0400063F RID: 1599
	private PlayerVoiceFX _playerVoiceFX;

	// Token: 0x04000640 RID: 1600
	private VoicePlayerState _localVps;

	// Token: 0x04000641 RID: 1601
	private PlayerController _localController;

	// Token: 0x04000642 RID: 1602
	private List<PlayerBuff> _buffs = new List<PlayerBuff>();

	// Token: 0x04000643 RID: 1603
	public Action<float, float> _Mirror_SyncVarHookDelegate_currentAmplitude;

	// Token: 0x04000644 RID: 1604
	public Action<float, float> _Mirror_SyncVarHookDelegate_currentCharge;
}
