using System;
using System.Collections.Generic;
using SettingsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020002E7 RID: 743
public class SettingsApplier : MonoBehaviour
{
	// Token: 0x060019C9 RID: 6601 RVA: 0x0006C024 File Offset: 0x0006A224
	private void Awake()
	{
		this._persistence = new SettingsPersistence(this.layout, this.saveFileName);
		this._displayApplier = new DisplaySettingsApplier((string key) => this.FindDropdownSetting(key), () => this.IsWindowedDisplay());
		this._soundApplier = new SoundSettingsApplier();
		this._microphoneApplier = new MicrophoneSettingsApplier(this);
		this._inputApplier = new InputSettingsApplier();
		this._framerateApplier = new FramerateSettingsApplier();
		this._emoteWheelApplier = new EmoteWheelSettingsApplier();
		this._graphicsApplier = new GraphicsSettingsApplier(this.graphicsSettings);
		this._skipWindowModeAndResolution = BuildTypeDetector.IsLocalBuild();
	}

	// Token: 0x060019CA RID: 6602 RVA: 0x0006C0BF File Offset: 0x0006A2BF
	private void OnEnable()
	{
		this.LoadSettings();
		SettingsLayout.SettingsChanged += this.OnSettingsChanged;
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	// Token: 0x060019CB RID: 6603 RVA: 0x0006C0E9 File Offset: 0x0006A2E9
	private void OnDisable()
	{
		SettingsLayout.SettingsChanged -= this.OnSettingsChanged;
		SceneManager.sceneLoaded -= this.OnSceneLoaded;
		MicrophoneSettingsApplier microphoneApplier = this._microphoneApplier;
		if (microphoneApplier == null)
		{
			return;
		}
		microphoneApplier.StopCoroutines();
	}

	// Token: 0x060019CC RID: 6604 RVA: 0x0006C11D File Offset: 0x0006A31D
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		this.ApplySettingsWithLoadOnSceneStart();
		MicrophoneSettingsApplier microphoneApplier = this._microphoneApplier;
		if (microphoneApplier == null)
		{
			return;
		}
		microphoneApplier.ApplyOnSceneLoad();
	}

	// Token: 0x060019CD RID: 6605 RVA: 0x0006C138 File Offset: 0x0006A338
	private void OnSettingsChanged(SettingsLayout source, SettingItemBase entry)
	{
		if (source != this.layout)
		{
			return;
		}
		string text;
		if (entry == null)
		{
			text = null;
		}
		else
		{
			string key = entry.key;
			text = ((key != null) ? key.Trim().ToLowerInvariant() : null);
		}
		string key2 = text;
		if (!this._skipWindowModeAndResolution || !SettingsApplier.IsWindowModeOrResolutionKey(key2))
		{
			this._displayApplier.Apply(entry);
		}
		this._soundApplier.Apply(entry);
		this._microphoneApplier.Apply(entry);
		this._inputApplier.Apply(entry);
		this._framerateApplier.Apply(entry);
		this._emoteWheelApplier.Apply(entry);
		this._graphicsApplier.Apply(entry);
		this.SaveSettings();
	}

	// Token: 0x060019CE RID: 6606 RVA: 0x0006C1DC File Offset: 0x0006A3DC
	private void ApplySettingsWithLoadOnSceneStart()
	{
		if (this.layout == null)
		{
			return;
		}
		Dictionary<string, SettingsPersistence.SettingSaveEntry> dictionary = this._persistence.Load();
		if (dictionary.Count == 0)
		{
			return;
		}
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					SettingsPersistence.SettingSaveEntry saved;
					if (!(settingItemBase == null) && !string.IsNullOrWhiteSpace(settingItemBase.key) && this.ShouldLoadOnSceneStart(settingItemBase) && dictionary.TryGetValue(settingItemBase.key, out saved))
					{
						this._persistence.ApplySavedValue(settingItemBase, saved);
						this.ApplySettingByType(settingItemBase);
					}
				}
			}
		}
	}

	// Token: 0x060019CF RID: 6607 RVA: 0x0006C2E0 File Offset: 0x0006A4E0
	private bool ShouldLoadOnSceneStart(SettingItemBase item)
	{
		SliderSettingItem sliderSettingItem = item as SliderSettingItem;
		bool result;
		if (sliderSettingItem == null)
		{
			DropdownSettingItem dropdownSettingItem = item as DropdownSettingItem;
			if (dropdownSettingItem == null)
			{
				ToggleSettingItem toggleSettingItem = item as ToggleSettingItem;
				if (toggleSettingItem == null)
				{
					RebindSettingItem rebindSettingItem = item as RebindSettingItem;
					result = (rebindSettingItem != null && rebindSettingItem.loadOnSceneStart);
				}
				else
				{
					result = toggleSettingItem.loadOnSceneStart;
				}
			}
			else
			{
				result = dropdownSettingItem.loadOnSceneStart;
			}
		}
		else
		{
			result = sliderSettingItem.loadOnSceneStart;
		}
		return result;
	}

	// Token: 0x060019D0 RID: 6608 RVA: 0x0006C344 File Offset: 0x0006A544
	private void ApplySettingByType(SettingItemBase item)
	{
		if (item == null || string.IsNullOrWhiteSpace(item.key))
		{
			return;
		}
		if (item is RebindSettingItem)
		{
			this._inputApplier.Apply(item);
			return;
		}
		string key = item.key.Trim().ToLowerInvariant();
		if (SettingsApplier.IsDisplayKey(key))
		{
			if (!this._skipWindowModeAndResolution || !SettingsApplier.IsWindowModeOrResolutionKey(key))
			{
				this._displayApplier.Apply(item);
				return;
			}
		}
		else
		{
			if (SettingsApplier.IsSoundKey(key))
			{
				this._soundApplier.Apply(item);
				return;
			}
			if (SettingsApplier.IsMicrophoneKey(key))
			{
				this._microphoneApplier.Apply(item);
				return;
			}
			if (SettingsApplier.IsInputKey(key))
			{
				this._inputApplier.Apply(item);
				return;
			}
			if (SettingsApplier.IsFramerateKey(key))
			{
				this._framerateApplier.Apply(item);
				return;
			}
			if (SettingsApplier.IsEmoteWheelKey(key))
			{
				this._emoteWheelApplier.Apply(item);
				return;
			}
			if (SettingsApplier.IsGraphicsKey(key))
			{
				this._graphicsApplier.Apply(item);
			}
		}
	}

	// Token: 0x060019D1 RID: 6609 RVA: 0x0006C434 File Offset: 0x0006A634
	private static bool IsDisplayKey(string key)
	{
		return key == "vsync" || key == "resolution" || key == "display" || key == "aspectratio" || key == "hz" || key == "refreshrate";
	}

	// Token: 0x060019D2 RID: 6610 RVA: 0x0006C48F File Offset: 0x0006A68F
	private static bool IsSoundKey(string key)
	{
		return key == "mastervolume" || key == "musicvolume" || key == "sfxvolume" || key == "proximitychatvolume";
	}

	// Token: 0x060019D3 RID: 6611 RVA: 0x0006C4C5 File Offset: 0x0006A6C5
	private static bool IsMicrophoneKey(string key)
	{
		return key == "microphonedevice";
	}

	// Token: 0x060019D4 RID: 6612 RVA: 0x0006C4D2 File Offset: 0x0006A6D2
	private static bool IsInputKey(string key)
	{
		return key == "inputvolume" || key == "proximityvoicechatmode";
	}

	// Token: 0x060019D5 RID: 6613 RVA: 0x0006C4EE File Offset: 0x0006A6EE
	private static bool IsFramerateKey(string key)
	{
		return key == "maxframerate" || key == "framerate";
	}

	// Token: 0x060019D6 RID: 6614 RVA: 0x0006C50A File Offset: 0x0006A70A
	private static bool IsEmoteWheelKey(string key)
	{
		return key == "emotewheelmode";
	}

	// Token: 0x060019D7 RID: 6615 RVA: 0x0006C518 File Offset: 0x0006A718
	private static bool IsGraphicsKey(string key)
	{
		return key == "quality" || key == "qualitylevel" || key == "renderscale" || key == "render scale" || key == "hdr" || key == "brightness" || key == "filmgrain";
	}

	// Token: 0x060019D8 RID: 6616 RVA: 0x0006C580 File Offset: 0x0006A780
	private void SaveSettings()
	{
		if (this.layout == null)
		{
			return;
		}
		Dictionary<string, SettingsPersistence.SettingSaveEntry> dictionary = new Dictionary<string, SettingsPersistence.SettingSaveEntry>();
		HashSet<SettingItemBase> hashSet = new HashSet<SettingItemBase>();
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					if (!(settingItemBase == null) && hashSet.Add(settingItemBase))
					{
						SettingsPersistence.SettingSaveEntry value = this._persistence.BuildSaveEntry(settingItemBase);
						dictionary[settingItemBase.key] = value;
					}
				}
			}
		}
		this._persistence.Save(dictionary);
	}

	// Token: 0x060019D9 RID: 6617 RVA: 0x0006C66C File Offset: 0x0006A86C
	private void LoadSettings()
	{
		if (this.layout == null)
		{
			return;
		}
		Dictionary<string, SettingsPersistence.SettingSaveEntry> dictionary = this._persistence.Load();
		if (dictionary.Count == 0)
		{
			this.ApplyDefaultSettings();
			this.ApplyAllSettings();
			this.SaveSettings();
			return;
		}
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					SettingsPersistence.SettingSaveEntry settingSaveEntry;
					if (!(settingItemBase == null) && !string.IsNullOrWhiteSpace(settingItemBase.key) && dictionary.TryGetValue(settingItemBase.key, out settingSaveEntry))
					{
						this._persistence.ApplySavedValue(settingItemBase, settingSaveEntry);
						if (SettingsApplier.IsMicrophoneKey(settingItemBase.key) && settingSaveEntry != null && !string.IsNullOrWhiteSpace(settingSaveEntry.stringValue))
						{
							this._microphoneApplier.SetSavedDeviceName(settingSaveEntry.stringValue);
						}
					}
				}
			}
		}
		this.ApplyAllSettings();
		if (!string.IsNullOrEmpty(this._microphoneApplier.GetSavedDeviceName()))
		{
			this._microphoneApplier.ApplyOnSceneLoad();
		}
	}

	// Token: 0x060019DA RID: 6618 RVA: 0x0006C7C8 File Offset: 0x0006A9C8
	private void ApplyAllSettings()
	{
		this._soundApplier.ApplyAll(this.layout);
		this._microphoneApplier.ApplyAll(this.layout);
		this._inputApplier.ApplyAll(this.layout);
		this._framerateApplier.ApplyAll(this.layout);
		this.ApplyAllDisplaySettings(this.layout);
		GraphicsSettingsApplier graphicsApplier = this._graphicsApplier;
		if (graphicsApplier == null)
		{
			return;
		}
		graphicsApplier.ApplyAllSettings();
	}

	// Token: 0x060019DB RID: 6619 RVA: 0x0006C838 File Offset: 0x0006AA38
	private void ApplyAllDisplaySettings(SettingsLayout layout)
	{
		if (layout == null)
		{
			return;
		}
		foreach (SettingsLayout.Tab tab in layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					if (!(settingItemBase == null) && !string.IsNullOrWhiteSpace(settingItemBase.key))
					{
						string key = settingItemBase.key.Trim().ToLowerInvariant();
						if (SettingsApplier.IsDisplayKey(key) && (!this._skipWindowModeAndResolution || !SettingsApplier.IsWindowModeOrResolutionKey(key)))
						{
							this._displayApplier.Apply(settingItemBase);
						}
					}
				}
			}
		}
	}

	// Token: 0x060019DC RID: 6620 RVA: 0x0006C920 File Offset: 0x0006AB20
	private static bool IsWindowModeOrResolutionKey(string key)
	{
		return !string.IsNullOrWhiteSpace(key) && (key == "resolution" || key == "display" || key == "aspectratio" || key == "hz" || key == "refreshrate");
	}

	// Token: 0x060019DD RID: 6621 RVA: 0x0006C978 File Offset: 0x0006AB78
	private void ApplyDefaultSettings()
	{
		if (this.layout == null)
		{
			return;
		}
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					if (!(settingItemBase == null))
					{
						ToggleSettingItem toggleSettingItem = settingItemBase as ToggleSettingItem;
						if (toggleSettingItem != null)
						{
							toggleSettingItem.value = toggleSettingItem.defaultValue;
						}
						else
						{
							SliderSettingItem sliderSettingItem = settingItemBase as SliderSettingItem;
							if (sliderSettingItem != null)
							{
								sliderSettingItem.value = sliderSettingItem.defaultValue;
							}
						}
						DropdownSettingItem dropdownSettingItem = settingItemBase as DropdownSettingItem;
						if (dropdownSettingItem != null && SettingsApplier.IsKey(dropdownSettingItem, "display"))
						{
							SettingsApplier.SetDisplayDefault(dropdownSettingItem);
						}
						DropdownSettingItem dropdownSettingItem2 = settingItemBase as DropdownSettingItem;
						if (dropdownSettingItem2 != null && SettingsApplier.IsKey(dropdownSettingItem2, "resolution"))
						{
							SettingsApplier.SetResolutionDefault(dropdownSettingItem2);
						}
						ResolutionSettingItem resolutionSettingItem = settingItemBase as ResolutionSettingItem;
						if (resolutionSettingItem != null && SettingsApplier.IsKey(resolutionSettingItem, "resolution"))
						{
							resolutionSettingItem.width = Screen.currentResolution.width;
							resolutionSettingItem.height = Screen.currentResolution.height;
						}
						DropdownSettingItem dropdownSettingItem3 = settingItemBase as DropdownSettingItem;
						if (dropdownSettingItem3 != null && (SettingsApplier.IsKey(dropdownSettingItem3, "maxframerate") || SettingsApplier.IsKey(dropdownSettingItem3, "framerate")))
						{
							SettingsApplier.SetFramerateDefault(dropdownSettingItem3);
						}
					}
				}
			}
		}
	}

	// Token: 0x060019DE RID: 6622 RVA: 0x0006CB34 File Offset: 0x0006AD34
	private static void SetRefreshRateDefault(DropdownSettingItem dropdown)
	{
		if (dropdown == null)
		{
			return;
		}
		RefreshRate refreshRateRatio = Screen.currentResolution.refreshRateRatio;
		float currentHz = refreshRateRatio.numerator / refreshRateRatio.denominator;
		List<string> list = dropdown.options ?? new List<string>();
		int num = list.FindIndex(delegate(string o)
		{
			RefreshRate refreshRate2;
			return SettingsApplier.TryParseRefreshRate(o, out refreshRate2) && Mathf.Approximately(refreshRate2.numerator / refreshRate2.denominator, currentHz);
		});
		if (num < 0)
		{
			int num2 = 0;
			float num3 = float.MaxValue;
			for (int i = 0; i < list.Count; i++)
			{
				RefreshRate refreshRate;
				if (SettingsApplier.TryParseRefreshRate(list[i], out refreshRate))
				{
					float num4 = Mathf.Abs(refreshRate.numerator / refreshRate.denominator - currentHz);
					if (num4 < num3)
					{
						num3 = num4;
						num2 = i;
					}
				}
			}
			num = num2;
		}
		dropdown.index = ((num >= 0) ? num : 0);
	}

	// Token: 0x060019DF RID: 6623 RVA: 0x0006CC10 File Offset: 0x0006AE10
	private static void SetFramerateDefault(DropdownSettingItem dropdown)
	{
		if (dropdown == null)
		{
			return;
		}
		int currentFramerate = Application.targetFrameRate;
		List<string> list = dropdown.options ?? new List<string>();
		if (currentFramerate == -1)
		{
			int num = list.FindIndex(delegate(string o)
			{
				string a = o.Trim().ToLowerInvariant();
				return a == "unlimited" || a == "uncapped" || a == "off";
			});
			dropdown.index = ((num >= 0) ? num : 0);
			return;
		}
		int num2 = list.FindIndex(delegate(string o)
		{
			int num7;
			return SettingsApplier.TryParseFramerate(o, out num7) && num7 == currentFramerate;
		});
		if (num2 < 0)
		{
			int num3 = 0;
			int num4 = int.MaxValue;
			for (int i = 0; i < list.Count; i++)
			{
				int num5;
				if (SettingsApplier.TryParseFramerate(list[i], out num5) && num5 > 0)
				{
					int num6 = Mathf.Abs(num5 - currentFramerate);
					if (num6 < num4)
					{
						num4 = num6;
						num3 = i;
					}
				}
			}
			num2 = num3;
		}
		dropdown.index = ((num2 >= 0) ? num2 : 0);
	}

	// Token: 0x060019E0 RID: 6624 RVA: 0x0006CD04 File Offset: 0x0006AF04
	private static void SetDisplayDefault(DropdownSettingItem dropdown)
	{
		if (dropdown == null)
		{
			return;
		}
		int num = (dropdown.options ?? new List<string>()).FindIndex((string o) => string.Equals(o, "Fullscreen", StringComparison.OrdinalIgnoreCase) || string.Equals(o, "Windowed Fullscreen", StringComparison.OrdinalIgnoreCase) || string.Equals(o, "Fullscreen Windowed", StringComparison.OrdinalIgnoreCase) || string.Equals(o, "Borderless", StringComparison.OrdinalIgnoreCase) || string.Equals(o, "Borderless Fullscreen", StringComparison.OrdinalIgnoreCase));
		dropdown.index = ((num >= 0) ? num : 0);
	}

	// Token: 0x060019E1 RID: 6625 RVA: 0x0006CD60 File Offset: 0x0006AF60
	private static void SetResolutionDefault(DropdownSettingItem dropdown)
	{
		if (dropdown == null)
		{
			return;
		}
		string label = string.Format("{0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height);
		if (dropdown.options == null)
		{
			dropdown.options = new List<string>();
		}
		int num = dropdown.options.FindIndex((string o) => string.Equals(o, label, StringComparison.OrdinalIgnoreCase));
		if (num < 0)
		{
			dropdown.options.Insert(0, label);
			num = 0;
		}
		dropdown.index = num;
	}

	// Token: 0x060019E2 RID: 6626 RVA: 0x0006CDFC File Offset: 0x0006AFFC
	public DropdownSettingItem FindDropdownSetting(string key)
	{
		if (this.layout == null)
		{
			return null;
		}
		foreach (SettingsLayout.Tab tab in this.layout.tabs)
		{
			if (tab != null)
			{
				foreach (SettingItemBase settingItemBase in tab.entries)
				{
					DropdownSettingItem dropdownSettingItem = settingItemBase as DropdownSettingItem;
					if (dropdownSettingItem != null && SettingsApplier.IsKey(dropdownSettingItem, key))
					{
						return dropdownSettingItem;
					}
				}
			}
		}
		return null;
	}

	// Token: 0x060019E3 RID: 6627 RVA: 0x0006CEB8 File Offset: 0x0006B0B8
	private bool IsWindowedDisplay()
	{
		DropdownSettingItem dropdownSettingItem = this.FindDropdownSetting("display");
		return !(dropdownSettingItem == null) && !string.IsNullOrWhiteSpace(dropdownSettingItem.CurrentOption) && string.Equals(dropdownSettingItem.CurrentOption.Trim(), "Windowed", StringComparison.OrdinalIgnoreCase);
	}

	// Token: 0x060019E4 RID: 6628 RVA: 0x0006CEFF File Offset: 0x0006B0FF
	private static bool IsKey(SettingItemBase entry, string key)
	{
		return !(entry == null) && !string.IsNullOrWhiteSpace(entry.key) && string.Equals(entry.key.Trim(), key, StringComparison.OrdinalIgnoreCase);
	}

	// Token: 0x060019E5 RID: 6629 RVA: 0x0006CF2C File Offset: 0x0006B12C
	private static bool TryParseRefreshRate(string value, out RefreshRate refreshRate)
	{
		refreshRate = new RefreshRate
		{
			numerator = 60000U,
			denominator = 1000U
		};
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		int num;
		if (int.TryParse(value.Trim().ToLowerInvariant().Replace("hz", "").Trim(), out num) && num > 0)
		{
			refreshRate = new RefreshRate
			{
				numerator = (uint)(num * 1000),
				denominator = 1000U
			};
			return true;
		}
		return false;
	}

	// Token: 0x060019E6 RID: 6630 RVA: 0x0006CFC4 File Offset: 0x0006B1C4
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

	// Token: 0x04001092 RID: 4242
	[SerializeField]
	private SettingsLayout layout;

	// Token: 0x04001093 RID: 4243
	[SerializeField]
	private string saveFileName = "settings.json";

	// Token: 0x04001094 RID: 4244
	[SerializeField]
	private GraphicsSettings graphicsSettings;

	// Token: 0x04001095 RID: 4245
	private SettingsPersistence _persistence;

	// Token: 0x04001096 RID: 4246
	private DisplaySettingsApplier _displayApplier;

	// Token: 0x04001097 RID: 4247
	private SoundSettingsApplier _soundApplier;

	// Token: 0x04001098 RID: 4248
	private MicrophoneSettingsApplier _microphoneApplier;

	// Token: 0x04001099 RID: 4249
	private InputSettingsApplier _inputApplier;

	// Token: 0x0400109A RID: 4250
	private FramerateSettingsApplier _framerateApplier;

	// Token: 0x0400109B RID: 4251
	private EmoteWheelSettingsApplier _emoteWheelApplier;

	// Token: 0x0400109C RID: 4252
	private GraphicsSettingsApplier _graphicsApplier;

	// Token: 0x0400109D RID: 4253
	private bool _skipWindowModeAndResolution;
}
