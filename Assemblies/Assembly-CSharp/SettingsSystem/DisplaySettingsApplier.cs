using System;
using UnityEngine;

namespace SettingsSystem
{
	// Token: 0x0200038F RID: 911
	public class DisplaySettingsApplier : ISettingsApplier
	{
		// Token: 0x06001DEB RID: 7659 RVA: 0x0008090B File Offset: 0x0007EB0B
		public DisplaySettingsApplier(Func<string, DropdownSettingItem> findDropdownSetting, Func<bool> isWindowedDisplay)
		{
			this._findDropdownSetting = findDropdownSetting;
			this._isWindowedDisplay = isWindowedDisplay;
		}

		// Token: 0x06001DEC RID: 7660 RVA: 0x00080924 File Offset: 0x0007EB24
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key))
			{
				return;
			}
			string a = entry.key.Trim().ToLowerInvariant();
			if (a == "vsync")
			{
				ToggleSettingItem toggleSettingItem = entry as ToggleSettingItem;
				if (toggleSettingItem != null)
				{
					QualitySettings.vSyncCount = (toggleSettingItem.value ? 1 : 0);
					return;
				}
			}
			if (!(a == "resolution"))
			{
				if (a == "display")
				{
					DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
					if (dropdownSettingItem != null)
					{
						FullScreenMode fullscreenMode;
						if (DisplaySettingsApplier.TryParseDisplayMode(dropdownSettingItem.CurrentOption, out fullscreenMode))
						{
							Resolution currentResolution = Screen.currentResolution;
							RefreshRate currentRefreshRate = this.GetCurrentRefreshRate();
							Screen.SetResolution(currentResolution.width, currentResolution.height, fullscreenMode, currentRefreshRate);
						}
						return;
					}
				}
				if (a == "aspectratio")
				{
					DropdownSettingItem dropdownSettingItem2 = entry as DropdownSettingItem;
					if (dropdownSettingItem2 != null)
					{
						if (!this.IsWindowedDisplay())
						{
							return;
						}
						float targetAspect;
						if (DisplaySettingsApplier.TryParseAspectRatio(dropdownSettingItem2.CurrentOption, out targetAspect))
						{
							Resolution resolution = DisplaySettingsApplier.FindHighestResolutionForAspect(targetAspect);
							RefreshRate currentRefreshRate2 = this.GetCurrentRefreshRate();
							if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
							{
								Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.Windowed, currentRefreshRate2);
							}
							Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, currentRefreshRate2);
						}
						return;
					}
				}
				if (a == "hz" || a == "refreshrate")
				{
					DropdownSettingItem dropdownSettingItem3 = entry as DropdownSettingItem;
					if (dropdownSettingItem3 != null)
					{
						RefreshRate preferredRefreshRate;
						if (DisplaySettingsApplier.TryParseRefreshRate(dropdownSettingItem3.CurrentOption, out preferredRefreshRate))
						{
							Resolution currentResolution2 = Screen.currentResolution;
							Screen.SetResolution(currentResolution2.width, currentResolution2.height, Screen.fullScreenMode, preferredRefreshRate);
						}
						return;
					}
				}
				return;
			}
			RefreshRate currentRefreshRate3 = this.GetCurrentRefreshRate();
			ResolutionSettingItem resolutionSettingItem = entry as ResolutionSettingItem;
			if (resolutionSettingItem != null)
			{
				Screen.SetResolution(resolutionSettingItem.width, resolutionSettingItem.height, Screen.fullScreenMode, currentRefreshRate3);
				return;
			}
			DropdownSettingItem dropdownSettingItem4 = entry as DropdownSettingItem;
			int width;
			int height;
			if (dropdownSettingItem4 != null && DisplaySettingsApplier.TryParseResolution(dropdownSettingItem4.CurrentOption, out width, out height))
			{
				Screen.SetResolution(width, height, Screen.fullScreenMode, currentRefreshRate3);
			}
		}

		// Token: 0x06001DED RID: 7661 RVA: 0x00080B0C File Offset: 0x0007ED0C
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

		// Token: 0x06001DEE RID: 7662 RVA: 0x00080BA8 File Offset: 0x0007EDA8
		private RefreshRate GetCurrentRefreshRate()
		{
			Func<string, DropdownSettingItem> findDropdownSetting = this._findDropdownSetting;
			DropdownSettingItem dropdownSettingItem = (findDropdownSetting != null) ? findDropdownSetting("hz") : null;
			if (dropdownSettingItem == null)
			{
				Func<string, DropdownSettingItem> findDropdownSetting2 = this._findDropdownSetting;
				dropdownSettingItem = ((findDropdownSetting2 != null) ? findDropdownSetting2("refreshrate") : null);
			}
			RefreshRate result;
			if (dropdownSettingItem != null && DisplaySettingsApplier.TryParseRefreshRate(dropdownSettingItem.CurrentOption, out result))
			{
				return result;
			}
			return Screen.currentResolution.refreshRateRatio;
		}

		// Token: 0x06001DEF RID: 7663 RVA: 0x00080C15 File Offset: 0x0007EE15
		private bool IsWindowedDisplay()
		{
			Func<bool> isWindowedDisplay = this._isWindowedDisplay;
			return isWindowedDisplay != null && isWindowedDisplay();
		}

		// Token: 0x06001DF0 RID: 7664 RVA: 0x00080C28 File Offset: 0x0007EE28
		private static bool TryParseResolution(string value, out int width, out int height)
		{
			width = 0;
			height = 0;
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string[] array = value.ToLowerInvariant().Split('x', StringSplitOptions.None);
			return array.Length == 2 && int.TryParse(array[0].Trim(), out width) && int.TryParse(array[1].Trim(), out height);
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x00080C7C File Offset: 0x0007EE7C
		private static bool TryParseDisplayMode(string value, out FullScreenMode mode)
		{
			mode = FullScreenMode.Windowed;
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string a = value.Trim().ToLowerInvariant();
			if (a == "fullscreen" || a == "windowed fullscreen" || a == "borderless" || a == "borderless fullscreen" || a == "fullscreen windowed")
			{
				mode = FullScreenMode.FullScreenWindow;
				return true;
			}
			if (!(a == "windowed"))
			{
				return false;
			}
			mode = FullScreenMode.Windowed;
			return true;
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x00080D00 File Offset: 0x0007EF00
		private static bool TryParseAspectRatio(string value, out float ratio)
		{
			ratio = 0f;
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			string[] array = value.Split(':', StringSplitOptions.None);
			if (array.Length != 2)
			{
				return false;
			}
			float num;
			if (!float.TryParse(array[0].Trim(), out num))
			{
				return false;
			}
			float num2;
			if (!float.TryParse(array[1].Trim(), out num2))
			{
				return false;
			}
			if (num2 <= 0f)
			{
				return false;
			}
			ratio = num / num2;
			return true;
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x00080D68 File Offset: 0x0007EF68
		private static Resolution FindHighestResolutionForAspect(float targetAspect)
		{
			Resolution[] resolutions = Screen.resolutions;
			if (resolutions == null || resolutions.Length == 0)
			{
				return Screen.currentResolution;
			}
			Resolution result = Screen.currentResolution;
			int num = -1;
			foreach (Resolution resolution in resolutions)
			{
				if (Mathf.Abs((float)resolution.width / (float)resolution.height - targetAspect) <= 0.01f)
				{
					int num2 = resolution.width * resolution.height;
					if (num2 > num)
					{
						num = num2;
						result = resolution;
					}
				}
			}
			return result;
		}

		// Token: 0x06001DF4 RID: 7668 RVA: 0x00080DE4 File Offset: 0x0007EFE4
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

		// Token: 0x04001432 RID: 5170
		private readonly Func<string, DropdownSettingItem> _findDropdownSetting;

		// Token: 0x04001433 RID: 5171
		private readonly Func<bool> _isWindowedDisplay;
	}
}
