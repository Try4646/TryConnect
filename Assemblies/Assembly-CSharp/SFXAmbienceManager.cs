using System;
using UnityEngine;

// Token: 0x02000278 RID: 632
public class SFXAmbienceManager : MonoBehaviour
{
	// Token: 0x17000207 RID: 519
	// (get) Token: 0x0600167E RID: 5758 RVA: 0x0006055F File Offset: 0x0005E75F
	// (set) Token: 0x0600167F RID: 5759 RVA: 0x00060566 File Offset: 0x0005E766
	public static SFXAmbienceManager Instance { get; private set; }

	// Token: 0x06001680 RID: 5760 RVA: 0x0006056E File Offset: 0x0005E76E
	private void Awake()
	{
		if (SFXAmbienceManager.Instance != null && SFXAmbienceManager.Instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		SFXAmbienceManager.Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
