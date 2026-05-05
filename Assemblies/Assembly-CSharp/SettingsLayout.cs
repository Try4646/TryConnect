using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200032A RID: 810
[CreateAssetMenu(menuName = "Game Settings/Settings Layout", fileName = "SettingsLayout")]
public class SettingsLayout : ScriptableObject
{
	// Token: 0x14000027 RID: 39
	// (add) Token: 0x06001B0A RID: 6922 RVA: 0x000729E4 File Offset: 0x00070BE4
	// (remove) Token: 0x06001B0B RID: 6923 RVA: 0x00072A18 File Offset: 0x00070C18
	public static event Action<SettingsLayout, SettingItemBase> SettingsChanged;

	// Token: 0x06001B0C RID: 6924 RVA: 0x00072A4B File Offset: 0x00070C4B
	public void NotifyChanged(SettingItemBase entry)
	{
		Action<SettingsLayout, SettingItemBase> settingsChanged = SettingsLayout.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this, entry);
	}

	// Token: 0x040011E4 RID: 4580
	public List<SettingsLayout.Tab> tabs = new List<SettingsLayout.Tab>();

	// Token: 0x0200032B RID: 811
	[Serializable]
	public class Tab
	{
		// Token: 0x040011E5 RID: 4581
		public string tabName = "General";

		// Token: 0x040011E6 RID: 4582
		public List<SettingItemBase> entries = new List<SettingItemBase>();
	}
}
