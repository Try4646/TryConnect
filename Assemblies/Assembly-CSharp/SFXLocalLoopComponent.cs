using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Token: 0x0200027C RID: 636
public class SFXLocalLoopComponent : MonoBehaviour
{
	// Token: 0x060016A6 RID: 5798 RVA: 0x00060B18 File Offset: 0x0005ED18
	public void LoopSFX(bool play)
	{
		if (this.eventReference.IsNull)
		{
			return;
		}
		if (play)
		{
			if (this.loopInstance.isValid())
			{
				PLAYBACK_STATE playback_STATE;
				this.loopInstance.getPlaybackState(out playback_STATE);
				if (playback_STATE == PLAYBACK_STATE.PLAYING)
				{
					return;
				}
			}
			this.loopInstance = RuntimeManager.CreateInstance(this.eventReference);
			this.loopInstance.set3DAttributes(base.transform.position.To3DAttributes());
			RuntimeManager.AttachInstanceToGameObject(this.loopInstance, base.gameObject, true);
			this.loopInstance.start();
			return;
		}
		if (!this.loopInstance.isValid())
		{
			return;
		}
		this.loopInstance.stop(this.allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
		this.loopInstance.release();
	}

	// Token: 0x060016A7 RID: 5799 RVA: 0x00060BD3 File Offset: 0x0005EDD3
	private void OnDisable()
	{
		this.LoopSFX(false);
	}

	// Token: 0x060016A8 RID: 5800 RVA: 0x00060BDC File Offset: 0x0005EDDC
	public void ModulatePitch(float pitch)
	{
		this.loopInstance.setPitch(pitch);
	}

	// Token: 0x04000EBB RID: 3771
	[SerializeField]
	private EventReference eventReference;

	// Token: 0x04000EBC RID: 3772
	[SerializeField]
	private bool allowFadeout = true;

	// Token: 0x04000EBD RID: 3773
	public EventInstance loopInstance;
}
