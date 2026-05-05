using System;
using System.Collections;
using Dissonance;
using Dissonance.Integrations.MirrorIgnorance;
using Gilzoide.UpdateManager;
using Mirror;
using UnityEngine;

// Token: 0x02000205 RID: 517
public class PlayerMouth : NetworkBehaviour, ILateUpdatable, IManagedObject
{
	// Token: 0x06001316 RID: 4886 RVA: 0x00052BFF File Offset: 0x00050DFF
	private void Awake()
	{
		this._dissonancePlayer = base.GetComponent<MirrorIgnorancePlayer>();
	}

	// Token: 0x06001317 RID: 4887 RVA: 0x00052C0D File Offset: 0x00050E0D
	public void OnEnable()
	{
		this.RegisterInManager();
	}

	// Token: 0x06001318 RID: 4888 RVA: 0x00052C15 File Offset: 0x00050E15
	public void OnDisable()
	{
		this.UnregisterInManager();
	}

	// Token: 0x06001319 RID: 4889 RVA: 0x00052C1D File Offset: 0x00050E1D
	private void Start()
	{
		this._startScale = this.mouthTransform.localScale;
		base.StartCoroutine(this.Initialization());
	}

	// Token: 0x0600131A RID: 4890 RVA: 0x00052C3D File Offset: 0x00050E3D
	private IEnumerator Initialization()
	{
		while (!this._dissonanceComms)
		{
			this._dissonanceComms = DissonanceComms.GetSingleton();
			yield return null;
		}
		while (!this._dissonancePlayer)
		{
			this._dissonancePlayer = base.GetComponent<MirrorIgnorancePlayer>();
			yield return null;
		}
		while (string.IsNullOrEmpty(this._dissonancePlayer.PlayerId))
		{
			yield return null;
		}
		for (;;)
		{
			this._voicePlayerState = this._dissonanceComms.FindPlayer(this._dissonancePlayer.PlayerId);
			if (this._voicePlayerState != null)
			{
				break;
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600131B RID: 4891 RVA: 0x00052C4C File Offset: 0x00050E4C
	public void ManagedLateUpdate()
	{
		if (this._voicePlayerState == null)
		{
			return;
		}
		float num = this._voicePlayerState.Amplitude;
		if (base.isLocalPlayer && !this.IsLocallyBroadcasting())
		{
			num = 0f;
		}
		this.currentAmplitude = num;
		Vector3 localScale = this._startScale + new Vector3(num * this.gainMultiplier, 0f, 0f);
		this.mouthTransform.localScale = localScale;
	}

	// Token: 0x0600131C RID: 4892 RVA: 0x00052CBC File Offset: 0x00050EBC
	private bool IsLocallyBroadcasting()
	{
		if (this._dissonanceComms == null)
		{
			return false;
		}
		if (this._voiceBroadcastTrigger == null && this._voiceProximityBroadcastTrigger == null)
		{
			this.RefreshVoiceTriggers();
		}
		return (this._voiceBroadcastTrigger != null && this._voiceBroadcastTrigger.IsTransmitting) || (this._voiceProximityBroadcastTrigger != null && this._voiceProximityBroadcastTrigger.IsTransmitting);
	}

	// Token: 0x0600131D RID: 4893 RVA: 0x00052D33 File Offset: 0x00050F33
	private void RefreshVoiceTriggers()
	{
		this._dissonanceComms.TryGetComponent<VoiceBroadcastTrigger>(out this._voiceBroadcastTrigger);
		this._dissonanceComms.TryGetComponent<VoiceProximityBroadcastTrigger>(out this._voiceProximityBroadcastTrigger);
	}

	// Token: 0x0600131F RID: 4895 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000C29 RID: 3113
	[SerializeField]
	private float gainMultiplier = 10f;

	// Token: 0x04000C2A RID: 3114
	[SerializeField]
	private Transform mouthTransform;

	// Token: 0x04000C2B RID: 3115
	public Transform headTransform;

	// Token: 0x04000C2C RID: 3116
	public float currentAmplitude;

	// Token: 0x04000C2D RID: 3117
	private DissonanceComms _dissonanceComms;

	// Token: 0x04000C2E RID: 3118
	private MirrorIgnorancePlayer _dissonancePlayer;

	// Token: 0x04000C2F RID: 3119
	private VoicePlayerState _voicePlayerState;

	// Token: 0x04000C30 RID: 3120
	private VoiceBroadcastTrigger _voiceBroadcastTrigger;

	// Token: 0x04000C31 RID: 3121
	private VoiceProximityBroadcastTrigger _voiceProximityBroadcastTrigger;

	// Token: 0x04000C32 RID: 3122
	private Vector3 _startScale;
}
