using System;
using UnityEngine;

namespace SettingsSystem
{
	// Token: 0x02000391 RID: 913
	public class FramerateSettingsApplier : ISettingsApplier
	{
		// Token: 0x06001DF8 RID: 7672 RVA: 0x00080F8C File Offset: 0x0007F18C
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key))
			{
				return;
			}
			string a = entry.key.Trim().ToLowerInvariant();
			if (a == "maxframerate" || a == "framerate")
			{
				DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
				int targetFrameRate;
				if (dropdownSettingItem != null && FramerateSettingsApplier.TryParseFramerate(dropdownSettingItem.CurrentOption, out targetFrameRate))
				{
					Application.targetFrameRate = targetFrameRate;
				}
			}
		}

		// Token: 0x06001DF9 RID: 7673 RVA: 0x00080FFC File Offset: 0x0007F1FC
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

		// Token: 0x06001DFA RID: 7674 RVA: 0x00081098 File Offset: 0x0007F298
		private static bool TryParseFramerate(string value, out int framerate)
		{
			framerate = -1;
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string text = value.Trim().ToLowerInvariant();
			if (text == "unlimited" || text == "uncapped" || text == "off")
			{
				framerate = -1;
				return true;
			}
			int num;
			if (int.TryParse(text, out num) && num > 0)
			{
				framerate = num;
				return true;
			}
			return false;
		}
	}
}
