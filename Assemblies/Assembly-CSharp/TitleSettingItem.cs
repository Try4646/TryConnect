using System;
using UnityEngine;

// Token: 0x0200033B RID: 827
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Title", fileName = "TitleSetting")]
public class TitleSettingItem : SettingItemBase
{
	// Token: 0x17000280 RID: 640
	// (get) Token: 0x06001B57 RID: 6999 RVA: 0x000747A3 File Offset: 0x000729A3
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Title;
		}
	}
}
