using System;
using UnityEngine;

// Token: 0x02000322 RID: 802
[CreateAssetMenu(menuName = "Game Settings/Setting Item/Rebind", fileName = "RebindSetting")]
public class RebindSettingItem : SettingItemBase
{
	// Token: 0x1700027A RID: 634
	// (get) Token: 0x06001AF3 RID: 6899 RVA: 0x00072211 File Offset: 0x00070411
	public override SettingKind Kind
	{
		get
		{
			return SettingKind.Rebind;
		}
	}

	// Token: 0x040011CF RID: 4559
	[Tooltip("Input action name from InputActions, e.g. Jump")]
	public string actionName;

	// Token: 0x040011D0 RID: 4560
	[Tooltip("Binding index on the action to rebind.")]
	public int bindingIndex;

	// Token: 0x040011D1 RID: 4561
	[HideInInspector]
	public string overridePath;

	// Token: 0x040011D2 RID: 4562
	[Tooltip("If true, this setting will be applied on every scene load from saved settings")]
	public bool loadOnSceneStart;
}
