using System;
using UnityEngine;

// Token: 0x0200033C RID: 828
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Toggle", fileName = "ToggleSetting")]
public class ToggleSettingItem : SettingItemBase
{
	// Token: 0x17000281 RID: 641
	// (get) Token: 0x06001B59 RID: 7001 RVA: 0x0006A586 File Offset: 0x00068786
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Dropdown;
		}
	}

	// Token: 0x0400121F RID: 4639
	public bool value;

	// Token: 0x04001220 RID: 4640
	public bool defaultValue;

	// Token: 0x04001221 RID: 4641
	[Tooltip("If true, this setting will be applied on every scene load from saved settings")]
	public bool loadOnSceneStart;
}
