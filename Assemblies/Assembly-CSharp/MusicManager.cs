using System;
using FMODUnity;
using UnityEngine;

// Token: 0x0200026F RID: 623
public class MusicManager : MonoBehaviour
{
	// Token: 0x17000200 RID: 512
	// (get) Token: 0x0600160D RID: 5645 RVA: 0x0005F011 File Offset: 0x0005D211
	// (set) Token: 0x0600160E RID: 5646 RVA: 0x0005F018 File Offset: 0x0005D218
	public static MusicManager Instance { get; private set; }

	// Token: 0x0600160F RID: 5647 RVA: 0x0005F020 File Offset: 0x0005D220
	private void Awake()
	{
		if (MusicManager.Instance != null && MusicManager.Instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		MusicManager.Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		base.TryGetComponent<StudioEventEmitter>(out this.studioEventEmitter);
	}

	// Token: 0x06001610 RID: 5648 RVA: 0x0005F071 File Offset: 0x0005D271
	private void OnEnable()
	{
		if (this.enableMusic && !this.studioEventEmitter.IsPlaying())
		{
			this.studioEventEmitter.Play();
		}
	}

	// Token: 0x06001611 RID: 5649 RVA: 0x0005F094 File Offset: 0x0005D294
	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.studioEventEmitter == null)
		{
			return;
		}
		if (this.enableMusic && !this.studioEventEmitter.IsPlaying())
		{
			this.studioEventEmitter.Play();
			return;
		}
		this.studioEventEmitter.Stop();
	}

	// Token: 0x04000E75 RID: 3701
	public bool enableMusic;

	// Token: 0x04000E76 RID: 3702
	private StudioEventEmitter studioEventEmitter;
}
