using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000245 RID: 581
public class LobbyModeDropdownButton : MonoBehaviour
{
	// Token: 0x060014EB RID: 5355 RVA: 0x00059C55 File Offset: 0x00057E55
	private void Awake()
	{
		this.InitFromSetting();
		this.ApplyVisual(false);
	}

	// Token: 0x060014EC RID: 5356 RVA: 0x00059C64 File Offset: 0x00057E64
	private void OnEnable()
	{
		SettingItemBase.SettingsChanged += this.OnSettingChanged;
		this.InitFromSetting();
		this.ApplyVisual(false);
	}

	// Token: 0x060014ED RID: 5357 RVA: 0x00059C84 File Offset: 0x00057E84
	private void OnDisable()
	{
		SettingItemBase.SettingsChanged -= this.OnSettingChanged;
	}

	// Token: 0x060014EE RID: 5358 RVA: 0x00059C97 File Offset: 0x00057E97
	public void OnClick()
	{
		this._modeIndex = (this._modeIndex + 1) % 2;
		this.ApplyVisual(true);
	}

	// Token: 0x060014EF RID: 5359 RVA: 0x00059CB0 File Offset: 0x00057EB0
	private void InitFromSetting()
	{
		if (this.lobbyModeSetting == null)
		{
			return;
		}
		this._modeIndex = Mathf.Clamp(this.lobbyModeSetting.index, 0, 1);
	}

	// Token: 0x060014F0 RID: 5360 RVA: 0x00059CDC File Offset: 0x00057EDC
	private void ApplyVisual(bool updateSetting)
	{
		if (this.targetImage != null)
		{
			this.targetImage.sprite = ((this._modeIndex == 1) ? this.privateSprite : this.friendsOnlySprite);
		}
		string text = (this._modeIndex == 1) ? "Invite Only" : "Open To Friends";
		this.ShowModePanel(text);
		if (!updateSetting || this.lobbyModeSetting == null)
		{
			return;
		}
		if (this._modeIndex < 0 || this._modeIndex >= this.lobbyModeSetting.options.Count)
		{
			return;
		}
		this.lobbyModeSetting.index = this._modeIndex;
		if (this.settingsLayout != null)
		{
			this.settingsLayout.NotifyChanged(this.lobbyModeSetting);
		}
		this.lobbyModeSetting.NotifyChanged();
	}

	// Token: 0x060014F1 RID: 5361 RVA: 0x00059DA8 File Offset: 0x00057FA8
	private void ShowModePanel(string text)
	{
		if (this.modePanel == null || this.modeLabel == null)
		{
			return;
		}
		this.modeLabel.text = text;
		if (this._fadeRoutine != null)
		{
			base.StopCoroutine(this._fadeRoutine);
		}
		this._fadeRoutine = base.StartCoroutine(this.FadePanelRoutine());
	}

	// Token: 0x060014F2 RID: 5362 RVA: 0x00059E04 File Offset: 0x00058004
	private IEnumerator FadePanelRoutine()
	{
		if (this.modePanel == null)
		{
			yield break;
		}
		this.modePanel.gameObject.SetActive(true);
		float t = 0f;
		while (t < this.fadeDuration)
		{
			t += Time.deltaTime;
			float alpha = (this.fadeDuration > 0f) ? Mathf.Clamp01(t / this.fadeDuration) : 1f;
			this.modePanel.alpha = alpha;
			yield return null;
		}
		this.modePanel.alpha = 1f;
		if (this.visibleTime > 0f)
		{
			yield return new WaitForSeconds(this.visibleTime);
		}
		t = 0f;
		while (t < this.fadeDuration)
		{
			t += Time.deltaTime;
			float alpha2 = (this.fadeDuration > 0f) ? (1f - Mathf.Clamp01(t / this.fadeDuration)) : 0f;
			this.modePanel.alpha = alpha2;
			yield return null;
		}
		this.modePanel.alpha = 0f;
		this.modePanel.gameObject.SetActive(false);
		this._fadeRoutine = null;
		yield break;
	}

	// Token: 0x060014F3 RID: 5363 RVA: 0x00059E13 File Offset: 0x00058013
	private void OnSettingChanged(SettingItemBase entry)
	{
		if (entry == null || string.IsNullOrWhiteSpace(entry.key))
		{
			return;
		}
		if (!string.Equals(entry.key.Trim(), "lobbymode", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		this.InitFromSetting();
		this.ApplyVisual(false);
	}

	// Token: 0x04000D56 RID: 3414
	[SerializeField]
	private DropdownSettingItem lobbyModeSetting;

	// Token: 0x04000D57 RID: 3415
	[SerializeField]
	private SettingsLayout settingsLayout;

	// Token: 0x04000D58 RID: 3416
	[SerializeField]
	private Image targetImage;

	// Token: 0x04000D59 RID: 3417
	[SerializeField]
	private Sprite friendsOnlySprite;

	// Token: 0x04000D5A RID: 3418
	[SerializeField]
	private Sprite privateSprite;

	// Token: 0x04000D5B RID: 3419
	[Header("Mode Panel")]
	[SerializeField]
	private CanvasGroup modePanel;

	// Token: 0x04000D5C RID: 3420
	[SerializeField]
	private TextMeshProUGUI modeLabel;

	// Token: 0x04000D5D RID: 3421
	[SerializeField]
	private float fadeDuration = 0.25f;

	// Token: 0x04000D5E RID: 3422
	[SerializeField]
	private float visibleTime = 1.5f;

	// Token: 0x04000D5F RID: 3423
	private int _modeIndex;

	// Token: 0x04000D60 RID: 3424
	private Coroutine _fadeRoutine;
}
