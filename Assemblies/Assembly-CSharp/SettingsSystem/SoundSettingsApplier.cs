using System;
using FMODUnity;

namespace SettingsSystem
{
	// Token: 0x0200039B RID: 923
	public class SoundSettingsApplier : ISettingsApplier
	{
		// Token: 0x06001E29 RID: 7721 RVA: 0x00081F04 File Offset: 0x00080104
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key))
			{
				return;
			}
			string a = entry.key.Trim().ToLowerInvariant();
			if (a == "mastervolume")
			{
				SliderSettingItem sliderSettingItem = entry as SliderSettingItem;
				if (sliderSettingItem != null)
				{
					RuntimeManager.StudioSystem.setParameterByName("Master", sliderSettingItem.value, false);
					return;
				}
			}
			if (a == "musicvolume")
			{
				SliderSettingItem sliderSettingItem2 = entry as SliderSettingItem;
				if (sliderSettingItem2 != null)
				{
					RuntimeManager.StudioSystem.setParameterByName("Music", sliderSettingItem2.value, false);
					return;
				}
			}
			if (a == "sfxvolume")
			{
				SliderSettingItem sliderSettingItem3 = entry as SliderSettingItem;
				if (sliderSettingItem3 != null)
				{
					RuntimeManager.StudioSystem.setParameterByName("SFX", sliderSettingItem3.value, false);
					return;
				}
			}
			if (a == "proximitychatvolume")
			{
				SliderSettingItem sliderSettingItem4 = entry as SliderSettingItem;
				if (sliderSettingItem4 != null)
				{
					RuntimeManager.StudioSystem.setParameterByName("VOIP", sliderSettingItem4.value, false);
					return;
				}
			}
		}

		// Token: 0x06001E2A RID: 7722 RVA: 0x00082008 File Offset: 0x00080208
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
