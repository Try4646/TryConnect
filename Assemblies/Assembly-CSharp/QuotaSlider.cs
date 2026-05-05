using System;
using Extensions;
using TMPro;
using UnityEngine;

// Token: 0x0200024A RID: 586
public class QuotaSlider : MonoBehaviour
{
	// Token: 0x0600150D RID: 5389 RVA: 0x0005A594 File Offset: 0x00058794
	private void Awake()
	{
		GameSettings gameSettings = Resources.Load<GameSettings>("GameSettings");
		if (gameSettings != null)
		{
			this._startingMoney = gameSettings.startingMoney;
		}
	}

	// Token: 0x0600150E RID: 5390 RVA: 0x0005A5C1 File Offset: 0x000587C1
	private void OnEnable()
	{
		this.Subscribe();
		this.UpdateFromCurrentState();
	}

	// Token: 0x0600150F RID: 5391 RVA: 0x0005A5CF File Offset: 0x000587CF
	private void OnDisable()
	{
		this.Unsubscribe();
	}

	// Token: 0x06001510 RID: 5392 RVA: 0x0005A5D8 File Offset: 0x000587D8
	private void Subscribe()
	{
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Combine(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnBalanceChanged));
		}
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			NetworkSingleton<GameManager>.Instance.OnQuotaChangedEvent += this.OnQuotaChanged;
		}
	}

	// Token: 0x06001511 RID: 5393 RVA: 0x0005A63C File Offset: 0x0005883C
	private void Unsubscribe()
	{
		if (NetworkSingleton<MoneyManager>.Instance != null)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.OnBalanceChanged = (Action<BalanceChangeData>)Delegate.Remove(instance.OnBalanceChanged, new Action<BalanceChangeData>(this.OnBalanceChanged));
		}
		if (NetworkSingleton<GameManager>.Instance != null)
		{
			NetworkSingleton<GameManager>.Instance.OnQuotaChangedEvent -= this.OnQuotaChanged;
		}
	}

	// Token: 0x06001512 RID: 5394 RVA: 0x0005A6A0 File Offset: 0x000588A0
	private void UpdateFromCurrentState()
	{
		if (NetworkSingleton<MoneyManager>.Instance == null || NetworkSingleton<GameManager>.Instance == null)
		{
			return;
		}
		long balance = NetworkSingleton<MoneyManager>.Instance.balance;
		if (this._quotaStartBalance == 0L)
		{
			this._quotaStartBalance = balance;
		}
		this.UpdateBar(balance, NetworkSingleton<GameManager>.Instance.currentQuota);
	}

	// Token: 0x06001513 RID: 5395 RVA: 0x0005A6F4 File Offset: 0x000588F4
	private void UpdateBar(long balance, long quota)
	{
		if (this.underQuotaFill == null || this.overQuotaFill == null)
		{
			return;
		}
		this.ResetGap();
		if (balance <= 0L || quota < 0L)
		{
			QuotaSlider.SetAnchors(this.underQuotaFill, 0f, 0f);
			QuotaSlider.SetAnchors(this.overQuotaFill, 0f, 0f);
			this.overQuotaFill.gameObject.SetActive(false);
			this.SetOverflowEffectActive(false);
			this.SetUnderQuotaText(0L);
			this.SetOverQuotaText(0L);
			return;
		}
		if (quota != 0L && balance > quota)
		{
			float num = Mathf.Max(0.0001f, (float)balance);
			float num2 = Mathf.Clamp01(Mathf.Clamp((float)quota, 0f, num) / num);
			float num3 = 1f - num2;
			if (this.minOverflowWidthPixels > 0f)
			{
				RectTransform rectTransform = base.transform as RectTransform;
				if (rectTransform != null)
				{
					float width = rectTransform.rect.width;
					if (width > 0f)
					{
						float num4 = Mathf.Clamp01(this.minOverflowWidthPixels / width);
						if (num3 > 0f && num3 < num4)
						{
							num2 = Mathf.Clamp01(1f - num4);
							num3 = 1f - num2;
						}
					}
				}
			}
			QuotaSlider.SetAnchors(this.underQuotaFill, 0f, num2);
			QuotaSlider.SetAnchors(this.overQuotaFill, num2, 1f);
			this.overQuotaFill.gameObject.SetActive(true);
			this.SetOverflowEffectActive(true);
			float num5 = Mathf.Max(0f, this.gapPixels) * 0.5f;
			this.underQuotaFill.offsetMax = new Vector2(-num5, this.underQuotaFill.offsetMax.y);
			this.overQuotaFill.offsetMin = new Vector2(num5, this.overQuotaFill.offsetMin.y);
			this.SetUnderQuotaText(quota);
			this.SetOverQuotaText(balance - quota);
			return;
		}
		long num6 = this._quotaStartBalance;
		if (this._startingMoney > 0L && this._startingMoney > num6)
		{
			num6 = this._startingMoney;
		}
		if (balance <= num6)
		{
			QuotaSlider.SetAnchors(this.underQuotaFill, 0f, 0f);
			QuotaSlider.SetAnchors(this.overQuotaFill, 0f, 0f);
			this.overQuotaFill.gameObject.SetActive(false);
			this.SetOverflowEffectActive(false);
			this.SetUnderQuotaText(balance);
			this.SetOverQuotaText(0L);
			return;
		}
		float num7 = Mathf.Max(1f, (float)(quota - num6));
		float maxX = Mathf.Clamp01((float)(balance - num6) / num7);
		QuotaSlider.SetAnchors(this.underQuotaFill, 0f, maxX);
		QuotaSlider.SetAnchors(this.overQuotaFill, 0f, 0f);
		this.overQuotaFill.gameObject.SetActive(false);
		this.SetOverflowEffectActive(false);
		this.SetUnderQuotaText(balance);
		this.SetOverQuotaText(0L);
	}

	// Token: 0x06001514 RID: 5396 RVA: 0x0005A9B5 File Offset: 0x00058BB5
	private void OnBalanceChanged(BalanceChangeData _)
	{
		if (NetworkSingleton<MoneyManager>.Instance == null || NetworkSingleton<GameManager>.Instance == null)
		{
			return;
		}
		this.UpdateBar(NetworkSingleton<MoneyManager>.Instance.balance, NetworkSingleton<GameManager>.Instance.currentQuota);
	}

	// Token: 0x06001515 RID: 5397 RVA: 0x0005A9EC File Offset: 0x00058BEC
	private void OnQuotaChanged(long oldQuota, long newQuota)
	{
		if (NetworkSingleton<MoneyManager>.Instance == null)
		{
			return;
		}
		this._quotaStartBalance = NetworkSingleton<MoneyManager>.Instance.balance;
		this.UpdateBar(NetworkSingleton<MoneyManager>.Instance.balance, newQuota);
	}

	// Token: 0x06001516 RID: 5398 RVA: 0x0005AA1D File Offset: 0x00058C1D
	private void SetUnderQuotaText(long amount)
	{
		if (this.underQuotaValueText != null)
		{
			this.underQuotaValueText.text = MoneyFormatter.FormatWithDollar(amount);
		}
	}

	// Token: 0x06001517 RID: 5399 RVA: 0x0005AA3E File Offset: 0x00058C3E
	private void SetOverQuotaText(long amount)
	{
		if (this.overQuotaValueText != null)
		{
			this.overQuotaValueText.text = "+" + MoneyFormatter.FormatWithDollar(amount);
		}
	}

	// Token: 0x06001518 RID: 5400 RVA: 0x0005AA69 File Offset: 0x00058C69
	private static void SetAnchors(RectTransform rect, float minX, float maxX)
	{
		rect.anchorMin = new Vector2(minX, rect.anchorMin.y);
		rect.anchorMax = new Vector2(maxX, rect.anchorMax.y);
	}

	// Token: 0x06001519 RID: 5401 RVA: 0x0005AA99 File Offset: 0x00058C99
	private void SetOverflowEffectActive(bool active)
	{
		if (this.overflowEffect != null)
		{
			this.overflowEffect.gameObject.SetActive(active);
		}
	}

	// Token: 0x0600151A RID: 5402 RVA: 0x0005AABC File Offset: 0x00058CBC
	private void ResetGap()
	{
		if (this.underQuotaFill != null)
		{
			this.underQuotaFill.offsetMax = new Vector2(0f, this.underQuotaFill.offsetMax.y);
		}
		if (this.overQuotaFill != null)
		{
			this.overQuotaFill.offsetMin = new Vector2(0f, this.overQuotaFill.offsetMin.y);
		}
	}

	// Token: 0x04000D73 RID: 3443
	[Header("Quota Bar Parts")]
	[SerializeField]
	private RectTransform underQuotaFill;

	// Token: 0x04000D74 RID: 3444
	[SerializeField]
	private RectTransform overQuotaFill;

	// Token: 0x04000D75 RID: 3445
	[SerializeField]
	private float gapPixels = 4f;

	// Token: 0x04000D76 RID: 3446
	[SerializeField]
	private float minOverflowWidthPixels = 60f;

	// Token: 0x04000D77 RID: 3447
	[Header("Value Texts")]
	[SerializeField]
	private TextMeshProUGUI underQuotaValueText;

	// Token: 0x04000D78 RID: 3448
	[SerializeField]
	private TextMeshProUGUI overQuotaValueText;

	// Token: 0x04000D79 RID: 3449
	[Header("Overflow Effect")]
	[Tooltip("Optional UI object (e.g. Raw Image with fire material) shown above the overflow area when over quota.")]
	[SerializeField]
	private RectTransform overflowEffect;

	// Token: 0x04000D7A RID: 3450
	private long _quotaStartBalance;

	// Token: 0x04000D7B RID: 3451
	private long _startingMoney;
}
