using System;
using System.Collections;
using Dissonance;
using Dissonance.Integrations.FMOD_Playback;
using FMOD;
using FMODUnity;
using UnityEngine;

// Token: 0x02000285 RID: 645
public class VoipManipulation : MonoBehaviour
{
	// Token: 0x06001700 RID: 5888 RVA: 0x00061E5F File Offset: 0x0006005F
	private void Awake()
	{
		this._playbackComponent = base.GetComponent<FMODVoicePlayback>();
		this._voipManipulationManager = base.GetComponentInParent<VoipManipulationManager>();
		this._dissonanceComms = base.GetComponentInParent<DissonanceComms>();
	}

	// Token: 0x06001701 RID: 5889 RVA: 0x00061E85 File Offset: 0x00060085
	private void Start()
	{
		this.UpdateVoipFX();
		this.UpdateVoipBus();
		this.UpdateMouthFX();
	}

	// Token: 0x06001702 RID: 5890 RVA: 0x00061E9C File Offset: 0x0006009C
	private void OnEnable()
	{
		this._playerName = this._playbackComponent.PlayerName;
		this._voipManipulationManager.OnDesiredVoipFXChanged += this.UpdateVoipFX;
		this._voipManipulationManager.OnDesiredVoipBusChanged += this.UpdateVoipBus;
		this._voipManipulationManager.OnMouthFXChanged += this.UpdateMouthFX;
		this.UpdateVoipBus();
		this.UpdateVoipFX();
	}

	// Token: 0x06001703 RID: 5891 RVA: 0x00061F0C File Offset: 0x0006010C
	private void OnDisable()
	{
		this._playerName = null;
		this._voipManipulationManager.OnDesiredVoipBusChanged -= this.UpdateVoipBus;
		this._voipManipulationManager.OnDesiredVoipFXChanged -= this.UpdateVoipFX;
		this._voipManipulationManager.OnMouthFXChanged -= this.UpdateMouthFX;
	}

	// Token: 0x06001704 RID: 5892 RVA: 0x00061F65 File Offset: 0x00060165
	private void UpdateVoipFX()
	{
		base.StartCoroutine(this.VoipFXRoutine());
	}

	// Token: 0x06001705 RID: 5893 RVA: 0x00061F74 File Offset: 0x00060174
	private IEnumerator VoipFXRoutine()
	{
		yield return new WaitForEndOfFrame();
		this.voipFX = this._voipManipulationManager.GetDesiredVoipFX(this._playerName);
		this.SetVoipFXParam(this.voipFX);
		yield break;
	}

	// Token: 0x06001706 RID: 5894 RVA: 0x00061F83 File Offset: 0x00060183
	private void UpdateMouthFX()
	{
		base.StartCoroutine(this.MouthFXRoutine());
	}

	// Token: 0x06001707 RID: 5895 RVA: 0x00061F92 File Offset: 0x00060192
	private IEnumerator MouthFXRoutine()
	{
		yield return new WaitForEndOfFrame();
		int desiredMouthFX = this._voipManipulationManager.GetDesiredMouthFX(this._playerName);
		this.SetMouthFXParam(desiredMouthFX);
		yield break;
	}

	// Token: 0x06001708 RID: 5896 RVA: 0x00061FA4 File Offset: 0x000601A4
	private void SetMouthFXParam(int i)
	{
		string name = "NO MOUTH " + this.voipBusId.ToString();
		RuntimeManager.StudioSystem.setParameterByName(name, (float)i, false);
	}

	// Token: 0x06001709 RID: 5897 RVA: 0x00061FD9 File Offset: 0x000601D9
	private void UpdateVoipBus()
	{
		base.StartCoroutine(this.VoipBusRoutine());
	}

	// Token: 0x0600170A RID: 5898 RVA: 0x00061FE8 File Offset: 0x000601E8
	private IEnumerator VoipBusRoutine()
	{
		yield return new WaitForEndOfFrame();
		this.voipBusId = this._voipManipulationManager.GetDesiredVoipBus(this._playerName);
		this._playbackComponent.OutputBusID = this.GetVoipBusID(this.voipBusId);
		yield break;
	}

	// Token: 0x0600170B RID: 5899 RVA: 0x00061FF8 File Offset: 0x000601F8
	private void SetVoipFXParam(VoipManipulationManager.VoipFX fx)
	{
		string label = this.voipFX.ToString();
		string name = "FX " + this.voipBusId.ToString();
		RuntimeManager.StudioSystem.setParameterByNameWithLabel(name, label, false);
	}

	// Token: 0x0600170C RID: 5900 RVA: 0x00062040 File Offset: 0x00060240
	private string GetVoipBusID(int i)
	{
		switch (i)
		{
		case 0:
			return "{b4f26317-56fe-4bcb-824d-48025ee6f104}";
		case 1:
			return "{2860e6e7-17e7-42e9-9d86-a4b89a6309e8}";
		case 2:
			return "{09be8ccb-eae0-40d8-949b-004ae6b6ca17}";
		case 3:
			return "{4fc37a58-191d-4c79-865e-c8de1f1b27f4}";
		case 4:
			return "{ab3422bf-2919-45db-810a-1d6650ff4d62}";
		case 5:
			return "{60c025f9-638e-4159-83ec-77e90cfe04f1}";
		case 6:
			return "{14fc2e88-3b32-4090-8c59-d4011a585e1c}";
		case 7:
			return "{b9298076-c3d3-4f65-841f-b5b0ec31d38b}";
		case 8:
			return "{ebc1d0aa-ca1e-4b6e-a43e-34c454dc1505}";
		case 9:
			return "{bc251c8f-77ac-470f-a58a-c9e27f81d2b9}";
		default:
			return "{0e0eb864-3277-43d3-8255-bfa8f493c8fc}";
		}
	}

	// Token: 0x0600170D RID: 5901 RVA: 0x000620C0 File Offset: 0x000602C0
	public float GetNormalizedDistanceParameter()
	{
		ATTRIBUTES_3D attributes_3D;
		RuntimeManager.StudioSystem.getListenerAttributes(0, out attributes_3D);
		return Mathf.Clamp01((new Vector3(attributes_3D.position.x, attributes_3D.position.y, attributes_3D.position.z) - base.transform.position).magnitude / this._playbackComponent.MaxDistance);
	}

	// Token: 0x04000EF7 RID: 3831
	private FMODVoicePlayback _playbackComponent;

	// Token: 0x04000EF8 RID: 3832
	private string _playerName;

	// Token: 0x04000EF9 RID: 3833
	private VoipManipulationManager _voipManipulationManager;

	// Token: 0x04000EFA RID: 3834
	private DissonanceComms _dissonanceComms;

	// Token: 0x04000EFB RID: 3835
	[SerializeField]
	private VoipManipulationManager.VoipFX voipFX;

	// Token: 0x04000EFC RID: 3836
	[SerializeField]
	private int voipBusId;
}
