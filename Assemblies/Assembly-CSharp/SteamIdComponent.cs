using System;
using UnityEngine;

// Token: 0x0200020E RID: 526
public class SteamIdComponent : MonoBehaviour
{
	// Token: 0x170001BD RID: 445
	// (get) Token: 0x0600136B RID: 4971 RVA: 0x00053BC2 File Offset: 0x00051DC2
	public ulong SteamId
	{
		get
		{
			return this.steamId;
		}
	}

	// Token: 0x0600136C RID: 4972 RVA: 0x00053BCC File Offset: 0x00051DCC
	public SteamIdComponent SetSteamID(ulong id)
	{
		this.steamId = id;
		UIColorManager uicolorManager;
		if (base.TryGetComponent<UIColorManager>(out uicolorManager))
		{
			uicolorManager.ApplyColors();
		}
		return this;
	}

	// Token: 0x04000C61 RID: 3169
	[SerializeField]
	private ulong steamId;
}
