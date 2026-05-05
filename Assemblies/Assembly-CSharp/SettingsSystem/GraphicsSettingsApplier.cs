using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SettingsSystem
{
	// Token: 0x02000392 RID: 914
	public class GraphicsSettingsApplier : ISettingsApplier
	{
		// Token: 0x06001DFC RID: 7676 RVA: 0x000810FE File Offset: 0x0007F2FE
		public GraphicsSettingsApplier(global::GraphicsSettings graphicsSettings)
		{
			this._graphicsSettings = graphicsSettings;
		}

		// Token: 0x06001DFD RID: 7677 RVA: 0x00081110 File Offset: 0x0007F310
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key) || this._graphicsSettings == null)
			{
				return;
			}
			string a = entry.key.Trim().ToLowerInvariant();
			if (a == "quality" || a == "qualitylevel")
			{
				DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
				global::GraphicsSettings.QualityLevel qualityLevel;
				if (dropdownSettingItem != null && Enum.TryParse<global::GraphicsSettings.QualityLevel>(dropdownSettingItem.CurrentOption, true, out qualityLevel))
				{
					this._graphicsSettings.qualityLevel = qualityLevel;
					this.ApplyQualityLevel();
				}
				return;
			}
			if (a == "renderscale" || a == "render scale")
			{
				SliderSettingItem sliderSettingItem = entry as SliderSettingItem;
				if (sliderSettingItem != null)
				{
					this._graphicsSettings.renderScale = sliderSettingItem.value;
					this.ApplyRenderScale();
				}
				return;
			}
			if (a == "hdr")
			{
				ToggleSettingItem toggleSettingItem = entry as ToggleSettingItem;
				if (toggleSettingItem != null)
				{
					this._graphicsSettings.enableHDR = toggleSettingItem.value;
					this.ApplyHDR();
				}
				return;
			}
			if (a == "brightness")
			{
				SliderSettingItem sliderSettingItem2 = entry as SliderSettingItem;
				if (sliderSettingItem2 != null)
				{
					this._graphicsSettings.brightness = sliderSettingItem2.value;
					this.ApplyBrightness();
				}
				return;
			}
			if (a == "filmgrain")
			{
				SliderSettingItem sliderSettingItem3 = entry as SliderSettingItem;
				if (sliderSettingItem3 != null)
				{
					this._graphicsSettings.filmGrain = sliderSettingItem3.value;
					this.ApplyFilmGrain();
				}
				return;
			}
		}

		// Token: 0x06001DFE RID: 7678 RVA: 0x0008126C File Offset: 0x0007F46C
		public void ApplyAll(SettingsLayout layout)
		{
			if (layout == null || this._graphicsSettings == null)
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
			this.ApplyAllSettings();
		}

		// Token: 0x06001DFF RID: 7679 RVA: 0x0008131C File Offset: 0x0007F51C
		public void ApplyAllSettings()
		{
			if (this._graphicsSettings == null)
			{
				return;
			}
			this.ApplyQualityLevel();
			this.ApplyRenderScale();
			this.ApplyHDR();
			this.ApplyBrightness();
			this.ApplyFilmGrain();
		}

		// Token: 0x06001E00 RID: 7680 RVA: 0x0008134C File Offset: 0x0007F54C
		private void ApplyQualityLevel()
		{
			UniversalRenderPipelineAsset qualityAsset = this._graphicsSettings.GetQualityAsset();
			if (qualityAsset != null)
			{
				int vSyncCount = QualitySettings.vSyncCount;
				int qualityLevel = (int)this._graphicsSettings.qualityLevel;
				if (qualityLevel >= 0 && qualityLevel < QualitySettings.names.Length)
				{
					QualitySettings.SetQualityLevel(qualityLevel, true);
				}
				QualitySettings.vSyncCount = vSyncCount;
				QualitySettings.renderPipeline = qualityAsset;
				UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
				if (universalRenderPipelineAsset != null)
				{
					universalRenderPipelineAsset.renderScale = this._graphicsSettings.renderScale;
					universalRenderPipelineAsset.supportsHDR = this._graphicsSettings.enableHDR;
				}
			}
		}

		// Token: 0x06001E01 RID: 7681 RVA: 0x000813D8 File Offset: 0x0007F5D8
		private void ApplyRenderScale()
		{
			UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.renderScale = this._graphicsSettings.renderScale;
			}
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x0008140C File Offset: 0x0007F60C
		private void ApplyHDR()
		{
			UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			if (universalRenderPipelineAsset != null)
			{
				universalRenderPipelineAsset.supportsHDR = this._graphicsSettings.enableHDR;
			}
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x00081440 File Offset: 0x0007F640
		private void ApplyBrightness()
		{
			foreach (Volume volume in Object.FindObjectsByType<Volume>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
			{
				ColorAdjustments colorAdjustments;
				if (volume != null && volume.profile != null && volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
				{
					colorAdjustments.postExposure.overrideState = true;
					colorAdjustments.postExposure.value = (this._graphicsSettings.brightness - 1f) * 2f;
				}
			}
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x000814BC File Offset: 0x0007F6BC
		private void ApplyFilmGrain()
		{
			Volume[] array = Object.FindObjectsByType<Volume>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			float filmGrain = this._graphicsSettings.filmGrain;
			foreach (Volume volume in array)
			{
				FilmGrain filmGrain2;
				if (!(volume == null) && !(volume.profile == null) && volume.profile.TryGet<FilmGrain>(out filmGrain2))
				{
					filmGrain2.intensity.overrideState = true;
					filmGrain2.intensity.value = filmGrain;
					filmGrain2.active = (filmGrain > 0f);
				}
			}
		}

		// Token: 0x04001434 RID: 5172
		private readonly global::GraphicsSettings _graphicsSettings;
	}
}
