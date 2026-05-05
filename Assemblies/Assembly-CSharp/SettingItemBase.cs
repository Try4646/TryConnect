using System;
using UnityEngine;

// Token: 0x02000329 RID: 809
public abstract class SettingItemBase : ScriptableObject
{
	// Token: 0x1700027D RID: 637
	// (get) Token: 0x06001B04 RID: 6916
	public abstract SettingKind Kind { get; }

	// Token: 0x14000026 RID: 38
	// (add) Token: 0x06001B05 RID: 6917 RVA: 0x0007294C File Offset: 0x00070B4C
	// (remove) Token: 0x06001B06 RID: 6918 RVA: 0x00072980 File Offset: 0x00070B80
	public static event Action<SettingItemBase> SettingsChanged;

	// Token: 0x1700027E RID: 638
	// (get) Token: 0x06001B07 RID: 6919 RVA: 0x000729B3 File Offset: 0x00070BB3
	public string DisplayLabel
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(this.label))
			{
				return this.label;
			}
			return this.key;
		}
	}

	// Token: 0x06001B08 RID: 6920 RVA: 0x000729CF File Offset: 0x00070BCF
	public void NotifyChanged()
	{
		Action<SettingItemBase> settingsChanged = SettingItemBase.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x040011E0 RID: 4576
	public string key;

	// Token: 0x040011E1 RID: 4577
	public string label;
}
