using System;
using System.Collections;
using Dissonance;
using UnityEngine;

namespace SettingsSystem
{
	// Token: 0x02000395 RID: 917
	public class MicrophoneSettingsApplier : ISettingsApplier
	{
		// Token: 0x06001E11 RID: 7697 RVA: 0x000817A0 File Offset: 0x0007F9A0
		public MicrophoneSettingsApplier(MonoBehaviour coroutineRunner)
		{
			this._coroutineRunner = coroutineRunner;
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x000817B0 File Offset: 0x0007F9B0
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key))
			{
				return;
			}
			if (entry.key.Trim().ToLowerInvariant() == "microphonedevice")
			{
				DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
				if (dropdownSettingItem != null)
				{
					string currentOption = dropdownSettingItem.CurrentOption;
					this._savedMicrophoneDeviceName = currentOption;
					this.ApplyMicrophoneDeviceName(currentOption);
				}
			}
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x00081810 File Offset: 0x0007FA10
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

		// Token: 0x06001E14 RID: 7700 RVA: 0x000818AC File Offset: 0x0007FAAC
		public void ApplyOnSceneLoad()
		{
			if (string.IsNullOrEmpty(this._savedMicrophoneDeviceName))
			{
				return;
			}
			if (this._microphoneApplyCoroutine != null)
			{
				this._coroutineRunner.StopCoroutine(this._microphoneApplyCoroutine);
			}
			this._microphoneApplyCoroutine = this._coroutineRunner.StartCoroutine(this.ApplyMicrophoneSettingsCoroutine());
		}

		// Token: 0x06001E15 RID: 7701 RVA: 0x000818EC File Offset: 0x0007FAEC
		public void SetSavedDeviceName(string deviceName)
		{
			this._savedMicrophoneDeviceName = deviceName;
		}

		// Token: 0x06001E16 RID: 7702 RVA: 0x000818F5 File Offset: 0x0007FAF5
		public string GetSavedDeviceName()
		{
			return this._savedMicrophoneDeviceName;
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x000818FD File Offset: 0x0007FAFD
		public void StopCoroutines()
		{
			if (this._microphoneApplyCoroutine != null)
			{
				this._coroutineRunner.StopCoroutine(this._microphoneApplyCoroutine);
				this._microphoneApplyCoroutine = null;
			}
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x00081920 File Offset: 0x0007FB20
		private void ApplyMicrophoneDeviceName(string deviceName)
		{
			DissonanceComms singleton = DissonanceComms.GetSingleton();
			if (singleton == null)
			{
				Debug.LogWarning("[MicrophoneSettingsApplier] DissonanceComms not found, will retry on scene load.");
				return;
			}
			string text = (deviceName != null) ? deviceName.Trim() : null;
			if (string.IsNullOrWhiteSpace(text) || string.Equals(text, "Default", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "System Default", StringComparison.OrdinalIgnoreCase))
			{
				singleton.MicrophoneName = null;
				return;
			}
			singleton.MicrophoneName = text;
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x00081987 File Offset: 0x0007FB87
		private IEnumerator ApplyMicrophoneSettingsCoroutine()
		{
			int num;
			for (int attempt = 0; attempt < 10; attempt = num + 1)
			{
				if (DissonanceComms.GetSingleton() != null)
				{
					this.ApplyMicrophoneDeviceName(this._savedMicrophoneDeviceName);
					this._microphoneApplyCoroutine = null;
					yield break;
				}
				yield return new WaitForSeconds(0.1f);
				num = attempt;
			}
			Debug.LogWarning("[MicrophoneSettingsApplier] Failed to apply microphone setting after scene load - DissonanceComms not found after multiple attempts.");
			this._microphoneApplyCoroutine = null;
			yield break;
		}

		// Token: 0x04001436 RID: 5174
		private string _savedMicrophoneDeviceName;

		// Token: 0x04001437 RID: 5175
		private Coroutine _microphoneApplyCoroutine;

		// Token: 0x04001438 RID: 5176
		private MonoBehaviour _coroutineRunner;
	}
}
