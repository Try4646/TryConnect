using System;
using UnityEngine;

// Token: 0x02000327 RID: 807
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Resolution", fileName = "ResolutionSetting")]
public class ResolutionSettingItem : SettingItemBase
{
	// Token: 0x1700027C RID: 636
	// (get) Token: 0x06001B02 RID: 6914 RVA: 0x0006A586 File Offset: 0x00068786
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Dropdown;
		}
	}

	// Token: 0x040011D7 RID: 4567
	public int width = 1920;

	// Token: 0x040011D8 RID: 4568
	public int height = 1080;
}
