using System;
using Dissonance;
using Dissonance.Integrations.FMOD_Recording;

namespace SettingsSystem
{
	// Token: 0x02000394 RID: 916
	public class InputSettingsApplier : ISettingsApplier
	{
		// Token: 0x06001E07 RID: 7687 RVA: 0x00081540 File Offset: 0x0007F740
		public void Apply(SettingItemBase entry)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.key))
			{
				return;
			}
			RebindSettingItem rebindSettingItem = entry as RebindSettingItem;
			if (rebindSettingItem != null)
			{
				if (!string.IsNullOrWhiteSpace(rebindSettingItem.overridePath))
				{
					InputReader instance = InputReader.Instance;
					if (instance == null)
					{
						return;
					}
					instance.ApplyBindingOverride(rebindSettingItem.actionName, rebindSettingItem.bindingIndex, rebindSettingItem.overridePath);
				}
				return;
			}
			string a = entry.key.Trim().ToLowerInvariant();
			if (a == "inputvolume")
			{
				SliderSettingItem sliderSettingItem = entry as SliderSettingItem;
				if (sliderSettingItem != null)
				{
					FMODMicrophoneInput.InputGain = sliderSettingItem.value;
					return;
				}
			}
			if (a == "proximityvoicechatmode")
			{
				DropdownSettingItem dropdownSettingItem = entry as DropdownSettingItem;
				if (dropdownSettingItem != null)
				{
					this.EnsurePushToTalkHook();
					InputEvents.SetProximityVoiceChatMode(dropdownSettingItem.CurrentOption);
					InputSettingsApplier.ApplyDissonanceVoiceChatMode(InputEvents.ProximityVoiceChatMode);
				}
			}
		}

		// Token: 0x06001E08 RID: 7688 RVA: 0x00081604 File Offset: 0x0007F804
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

		// Token: 0x06001E09 RID: 7689 RVA: 0x000816A0 File Offset: 0x0007F8A0
		private void EnsurePushToTalkHook()
		{
			if (this._pushToTalkHooked)
			{
				return;
			}
			InputEvents.OnPushToTalkEvent = (Action<bool>)Delegate.Combine(InputEvents.OnPushToTalkEvent, new Action<bool>(InputSettingsApplier.OnPushToTalkChanged));
			this._pushToTalkHooked = true;
		}

		// Token: 0x06001E0A RID: 7690 RVA: 0x000816D2 File Offset: 0x0007F8D2
		private static void OnPushToTalkChanged(bool isPressed)
		{
			if (InputEvents.ProximityVoiceChatMode != VoiceChatInputMode.PushToTalk)
			{
				return;
			}
			InputSettingsApplier.SetTriggersMuted(!isPressed);
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x000816E8 File Offset: 0x0007F8E8
		private static void ApplyDissonanceVoiceChatMode(VoiceChatInputMode mode)
		{
			CommActivationMode mode2 = (mode == VoiceChatInputMode.PushToTalk) ? CommActivationMode.PushToTalk : CommActivationMode.VoiceActivation;
			bool isMuted = mode == VoiceChatInputMode.PushToTalk && !InputEvents.IsPushToTalkPressed;
			VoiceBroadcastTrigger trigger;
			VoiceProximityBroadcastTrigger trigger2;
			InputSettingsApplier.GetDissonanceTriggers(out trigger, out trigger2);
			InputSettingsApplier.ApplyTriggerModeAndMute(trigger, mode2, isMuted);
			InputSettingsApplier.ApplyTriggerModeAndMute(trigger2, mode2, isMuted);
		}

		// Token: 0x06001E0C RID: 7692 RVA: 0x00081728 File Offset: 0x0007F928
		private static void SetTriggersMuted(bool isMuted)
		{
			VoiceBroadcastTrigger trigger;
			VoiceProximityBroadcastTrigger trigger2;
			InputSettingsApplier.GetDissonanceTriggers(out trigger, out trigger2);
			InputSettingsApplier.SetTriggerMuted(trigger, isMuted);
			InputSettingsApplier.SetTriggerMuted(trigger2, isMuted);
		}

		// Token: 0x06001E0D RID: 7693 RVA: 0x0008174C File Offset: 0x0007F94C
		private static void GetDissonanceTriggers(out VoiceBroadcastTrigger voiceTrigger, out VoiceProximityBroadcastTrigger proximityTrigger)
		{
			DissonanceComms singleton = DissonanceComms.GetSingleton();
			if (singleton == null)
			{
				voiceTrigger = null;
				proximityTrigger = null;
				return;
			}
			singleton.TryGetComponent<VoiceBroadcastTrigger>(out voiceTrigger);
			singleton.TryGetComponent<VoiceProximityBroadcastTrigger>(out proximityTrigger);
		}

		// Token: 0x06001E0E RID: 7694 RVA: 0x0008177F File Offset: 0x0007F97F
		private static void ApplyTriggerModeAndMute(IVoiceBroadcastTrigger trigger, CommActivationMode mode, bool isMuted)
		{
			if (trigger == null)
			{
				return;
			}
			trigger.Mode = mode;
			trigger.IsMuted = isMuted;
		}

		// Token: 0x06001E0F RID: 7695 RVA: 0x00081793 File Offset: 0x0007F993
		private static void SetTriggerMuted(IVoiceBroadcastTrigger trigger, bool isMuted)
		{
			if (trigger == null)
			{
				return;
			}
			trigger.IsMuted = isMuted;
		}

		// Token: 0x04001435 RID: 5173
		private bool _pushToTalkHooked;
	}
}
