using System;
using FMODUnity;
using UnityEngine;

// Token: 0x0200026E RID: 622
public class LimoMusicStarter : MonoBehaviour
{
	// Token: 0x0600160A RID: 5642 RVA: 0x0005EFC1 File Offset: 0x0005D1C1
	private void Start()
	{
		base.Invoke("TryStartingCarMusic", this._startDelay);
	}

	// Token: 0x0600160B RID: 5643 RVA: 0x0005EFD4 File Offset: 0x0005D1D4
	private void TryStartingCarMusic()
	{
		if (this.carEmitter == null)
		{
			return;
		}
		if (this.carEmitter.IsPlaying())
		{
			return;
		}
		this.carEmitter.Play();
	}

	// Token: 0x04000E72 RID: 3698
	[SerializeField]
	private StudioEventEmitter carEmitter;

	// Token: 0x04000E73 RID: 3699
	private float _startDelay = 2f;
}
