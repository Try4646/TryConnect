using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SettingsSystem
{
	// Token: 0x02000397 RID: 919
	public class SettingsPersistence
	{
		// Token: 0x06001E20 RID: 7712 RVA: 0x00081A52 File Offset: 0x0007FC52
		public SettingsPersistence(SettingsLayout layout, string saveFileName)
		{
			this._layout = layout;
			this._saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
		}

		// Token: 0x06001E21 RID: 7713 RVA: 0x00081A74 File Offset: 0x0007FC74
		public void Save(Dictionary<string, SettingsPersistence.SettingSaveEntry> entries)
		{
			if (this._layout == null)
			{
				return;
			}
			string contents = JsonUtility.ToJson(new SettingsPersistence.SettingsSaveData
			{
				entries = new List<SettingsPersistence.SettingSaveEntry>(entries.Values),
				timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				version = "1.0"
			}, true);
			File.WriteAllText(this._saveFilePath, contents);
		}

		// Token: 0x06001E22 RID: 7714 RVA: 0x00081AD8 File Offset: 0x0007FCD8
		public Dictionary<string, SettingsPersistence.SettingSaveEntry> Load()
		{
			Dictionary<string, SettingsPersistence.SettingSaveEntry> dictionary = new Dictionary<string, SettingsPersistence.SettingSaveEntry>(StringComparer.OrdinalIgnoreCase);
			if (!File.Exists(this._saveFilePath))
			{
				return dictionary;
			}
			try
			{
				SettingsPersistence.SettingsSaveData settingsSaveData = JsonUtility.FromJson<SettingsPersistence.SettingsSaveData>(File.ReadAllText(this._saveFilePath));
				if (((settingsSaveData != null) ? settingsSaveData.entries : null) == null)
				{
					return dictionary;
				}
				foreach (SettingsPersistence.SettingSaveEntry settingSaveEntry in settingsSaveData.entries)
				{
					if (!string.IsNullOrWhiteSpace(settingSaveEntry.key))
					{
						dictionary[settingSaveEntry.key] = settingSaveEntry;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[SettingsPersistence] Failed to load settings: " + ex.Message);
			}
			return dictionary;
		}

		// Token: 0x06001E23 RID: 7715 RVA: 0x00081BAC File Offset: 0x0007FDAC
		public SettingsPersistence.SettingSaveEntry BuildSaveEntry(SettingItemBase item)
		{
			SettingsPersistence.SettingSaveEntry settingSaveEntry = new SettingsPersistence.SettingSaveEntry
			{
				key = item.key
			};
			ToggleSettingItem toggleSettingItem = item as ToggleSettingItem;
			if (toggleSettingItem != null)
			{
				settingSaveEntry.type = "toggle";
				settingSaveEntry.boolValue = toggleSettingItem.value;
			}
			else
			{
				SliderSettingItem sliderSettingItem = item as SliderSettingItem;
				if (sliderSettingItem != null)
				{
					settingSaveEntry.type = "slider";
					settingSaveEntry.floatValue = sliderSettingItem.value;
				}
				else
				{
					DropdownSettingItem dropdownSettingItem = item as DropdownSettingItem;
					if (dropdownSettingItem != null)
					{
						settingSaveEntry.type = "dropdown";
						settingSaveEntry.index = dropdownSettingItem.index;
						settingSaveEntry.stringValue = dropdownSettingItem.CurrentOption;
						if (string.IsNullOrWhiteSpace(settingSaveEntry.stringValue) && dropdownSettingItem.options != null && dropdownSettingItem.index >= 0 && dropdownSettingItem.index < dropdownSettingItem.options.Count)
						{
							settingSaveEntry.stringValue = dropdownSettingItem.options[dropdownSettingItem.index];
						}
					}
					else
					{
						ResolutionSettingItem resolutionSettingItem = item as ResolutionSettingItem;
						if (resolutionSettingItem != null)
						{
							settingSaveEntry.type = "resolution";
							settingSaveEntry.width = resolutionSettingItem.width;
							settingSaveEntry.height = resolutionSettingItem.height;
						}
						else
						{
							RebindSettingItem rebindSettingItem = item as RebindSettingItem;
							if (rebindSettingItem != null)
							{
								settingSaveEntry.type = "rebind";
								settingSaveEntry.index = rebindSettingItem.bindingIndex;
								settingSaveEntry.stringValue = rebindSettingItem.overridePath;
							}
						}
					}
				}
			}
			return settingSaveEntry;
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x00081CFC File Offset: 0x0007FEFC
		public void ApplySavedValue(SettingItemBase item, SettingsPersistence.SettingSaveEntry saved)
		{
			ToggleSettingItem toggleSettingItem = item as ToggleSettingItem;
			if (toggleSettingItem != null)
			{
				toggleSettingItem.value = saved.boolValue;
				return;
			}
			SliderSettingItem sliderSettingItem = item as SliderSettingItem;
			if (sliderSettingItem != null)
			{
				sliderSettingItem.value = saved.floatValue;
				return;
			}
			ResolutionSettingItem resolutionSettingItem = item as ResolutionSettingItem;
			if (resolutionSettingItem != null)
			{
				resolutionSettingItem.width = saved.width;
				resolutionSettingItem.height = saved.height;
				return;
			}
			DropdownSettingItem dropdownSettingItem = item as DropdownSettingItem;
			if (dropdownSettingItem != null)
			{
				List<string> list = dropdownSettingItem.options;
				if (dropdownSettingItem.useDynamicOptions)
				{
					IDropdownOptionsProvider dropdownOptionsProvider = dropdownSettingItem.optionsProvider as IDropdownOptionsProvider;
					if (dropdownOptionsProvider != null)
					{
						list = (dropdownOptionsProvider.GetOptions() ?? list);
					}
				}
				dropdownSettingItem.options = (list ?? dropdownSettingItem.options);
				if (string.Equals(saved.type, "toggle", StringComparison.OrdinalIgnoreCase))
				{
					string key = item.key;
					if (string.Equals((key != null) ? key.Trim() : null, "devconsole", StringComparison.OrdinalIgnoreCase) && list != null && list.Count > 0)
					{
						dropdownSettingItem.index = Mathf.Clamp(saved.boolValue ? 1 : 0, 0, list.Count - 1);
						return;
					}
				}
				if (!string.IsNullOrWhiteSpace(saved.stringValue) && list != null)
				{
					int num = list.FindIndex((string o) => string.Equals(o, saved.stringValue, StringComparison.OrdinalIgnoreCase));
					if (num >= 0)
					{
						dropdownSettingItem.index = num;
						return;
					}
				}
				if (list != null && list.Count > 0)
				{
					dropdownSettingItem.index = Mathf.Clamp(saved.index, 0, list.Count - 1);
				}
				return;
			}
			RebindSettingItem rebindSettingItem = item as RebindSettingItem;
			if (rebindSettingItem != null)
			{
				rebindSettingItem.bindingIndex = ((saved.index >= 0) ? saved.index : rebindSettingItem.bindingIndex);
				rebindSettingItem.overridePath = saved.stringValue;
			}
		}

		// Token: 0x0400143D RID: 5181
		private readonly string _saveFilePath;

		// Token: 0x0400143E RID: 5182
		private readonly SettingsLayout _layout;

		// Token: 0x02000398 RID: 920
		[Serializable]
		private class SettingsSaveData
		{
			// Token: 0x0400143F RID: 5183
			public List<SettingsPersistence.SettingSaveEntry> entries;

			// Token: 0x04001440 RID: 5184
			public long timestamp;

			// Token: 0x04001441 RID: 5185
			public string version;
		}

		// Token: 0x02000399 RID: 921
		[Serializable]
		public class SettingSaveEntry
		{
			// Token: 0x04001442 RID: 5186
			public string key;

			// Token: 0x04001443 RID: 5187
			public string type;

			// Token: 0x04001444 RID: 5188
			public bool boolValue;

			// Token: 0x04001445 RID: 5189
			public float floatValue;

			// Token: 0x04001446 RID: 5190
			public int index;

			// Token: 0x04001447 RID: 5191
			public string stringValue;

			// Token: 0x04001448 RID: 5192
			public int width;

			// Token: 0x04001449 RID: 5193
			public int height;
		}
	}
}
