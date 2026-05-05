using System;
using UnityEngine;

namespace SettingsSystem
{
	// Token: 0x02000390 RID: 912
	public class EmoteWheelSettingsApplier : ISettingsApplier
	{
		// Token: 0x06001DF5 RID: 7669 RVA: 0x00080E7C File Offset: 0x0007F07C
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key))
			{
				return;
			}
			if (entry.key.Trim().ToLowerInvariant() == "emotewheelmode")
			{
				ToggleSettingItem toggleSettingItem = entry as ToggleSettingItem;
				if (toggleSettingItem != null)
				{
					RMF_RadialMenu rmf_RadialMenu = Object.FindFirstObjectByType<RMF_RadialMenu>();
					if (rmf_RadialMenu != null)
					{
						bool value = toggleSettingItem.value;
						rmf_RadialMenu.useDeltaSelection = value;
						rmf_RadialMenu.useSelectionFollower = value;
						rmf_RadialMenu.UpdateSelectionFollowerState();
					}
				}
			}
		}

		// Token: 0x06001DF6 RID: 7670 RVA: 0x00080EF0 File Offset: 0x0007F0F0
		public void ApplyAll(SettingsLayout layout)
		{
			if (layout == null)
			{
				return;
			}
			foreach (SettingsLayout.Tab tab in layout.tabs)
			{
				if (tab != null)
				{
					foreach (SettingItemBase entry in tab.entries)
					{
						this.Apply(entry);
					}
				}
			}
		}
	}
}
