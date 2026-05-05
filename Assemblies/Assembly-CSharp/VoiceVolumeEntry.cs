using System;
using Dissonance;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020002A7 RID: 679
public class VoiceVolumeEntry : MonoBehaviour
{
	// Token: 0x060017F2 RID: 6130 RVA: 0x00065858 File Offset: 0x00063A58
	public void Setup(VoicePlayerState state, string displayName, string steamId, float initialVolume, Action<string, float> onVolumeChanged)
	{
		this._state = state;
		this._steamId = (steamId ?? "");
		this._onVolumeChanged = onVolumeChanged;
		if (this.nameLabel != null)
		{
			this.nameLabel.text = (string.IsNullOrEmpty(displayName) ? "Player" : displayName);
		}
		if (this.volumeSlider != null)
		{
			this.volumeSlider.SetValueWithoutNotify(initialVolume);
			this.volumeSlider.onValueChanged.RemoveAllListeners();
			this.volumeSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnSliderChanged));
		}
		this.ApplyVolume(initialVolume);
	}

	// Token: 0x060017F3 RID: 6131 RVA: 0x000658FC File Offset: 0x00063AFC
	public void UpdateIdentity(string displayName, string steamId)
	{
		this._steamId = (steamId ?? "");
		if (this.nameLabel != null)
		{
			this.nameLabel.text = (string.IsNullOrEmpty(displayName) ? "Player" : displayName);
		}
	}

	// Token: 0x060017F4 RID: 6132 RVA: 0x00065937 File Offset: 0x00063B37
	private void OnSliderChanged(float value)
	{
		this.ApplyVolume(value);
		Action<string, float> onVolumeChanged = this._onVolumeChanged;
		if (onVolumeChanged == null)
		{
			return;
		}
		onVolumeChanged(this._steamId, value);
	}

	// Token: 0x060017F5 RID: 6133 RVA: 0x00065957 File Offset: 0x00063B57
	private void ApplyVolume(float value)
	{
		if (this._state != null)
		{
			this._state.Volume = value;
		}
	}

	// Token: 0x060017F6 RID: 6134 RVA: 0x0006596D File Offset: 0x00063B6D
	private void OnDestroy()
	{
		if (this.volumeSlider != null)
		{
			this.volumeSlider.onValueChanged.RemoveAllListeners();
		}
	}

	// Token: 0x04000F79 RID: 3961
	[SerializeField]
	private TMP_Text nameLabel;

	// Token: 0x04000F7A RID: 3962
	[SerializeField]
	private Slider volumeSlider;

	// Token: 0x04000F7B RID: 3963
	private VoicePlayerState _state;

	// Token: 0x04000F7C RID: 3964
	private string _steamId;

	// Token: 0x04000F7D RID: 3965
	private Action<string, float> _onVolumeChanged;
}
