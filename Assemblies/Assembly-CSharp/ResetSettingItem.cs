using System;
using UnityEngine;

// Token: 0x02000324 RID: 804
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Reset", fileName = "ResetSetting")]
public class ResetSettingItem : SettingItemBase
{
	// Token: 0x1700027B RID: 635
	// (get) Token: 0x06001AFA RID: 6906 RVA: 0x00072692 File Offset: 0x00070892
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Reset;
		}
	}
}
