using System;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;

// Token: 0x02000258 RID: 600
public class DaySummaryQuotaSlider : MonoBehaviour
{
	// Token: 0x0600156E RID: 5486 RVA: 0x0005BD9C File Offset: 0x00059F9C
	public IEnumerator SliderRoutine(long quota, long balance, int baseTicket, int overflowTicket, float duration)
	{
		this.Reset();
		float totalPercent = 0f;
		float underPercent = 0f;
		float overPercent = 0f;
		if (quota > 0L && balance > 0L)
		{
			totalPercent = (float)((double)balance / (double)quota) * 100f;
			underPercent = Mathf.Clamp(totalPercent, 0f, 100f);
			overPercent = ((totalPercent > 100f) ? (totalPercent - 100f) : 0f);
		}
		DOVirtual.Float(0f, Mathf.Max(underPercent / 100f, this.minVisualPercent), duration, new TweenCallback<float>(this.SetUnderQuotaFill)).SetEase(Ease.OutCubic).SetId(this);
		DOVirtual.Float(0f, underPercent, duration, new TweenCallback<float>(this.SetPercentageText)).SetEase(Ease.OutCubic).SetId(this);
		if (totalPercent >= 100f)
		{
			DOVirtual.Float(0f, (float)baseTicket, duration, delegate(float value)
			{
				this.ticketRewardText.text = value.ToString("0");
			}).SetEase(Ease.OutCubic).SetId(this);
		}
		SFXManager.SFXOneShot(this.firstSliderSfx, default(Vector3));
		yield return new WaitForSeconds(duration);
		if (overPercent > 0f)
		{
			yield return new WaitForSeconds(0.5f);
			this.overQuotaFill.gameObject.SetActive(true);
			SFXManager.SFXOneShot(this.secondSliderSfx, default(Vector3));
			float value2 = underPercent / totalPercent;
			DOVirtual.Float(1f, Mathf.Clamp(value2, this.minVisualPercent, 1f - this.minVisualPercent), duration, delegate(float value)
			{
				this.SetOverQuotaFill(value);
				this.SetUnderQuotaFill(value);
			}).SetEase(Ease.OutCubic).SetId(this);
			DOVirtual.Float(0f, overPercent, duration, new TweenCallback<float>(this.SetOverfillPercentageText)).SetEase(Ease.OutCubic).SetId(this);
			DOVirtual.Float(0f, (float)overflowTicket, duration, delegate(float value)
			{
				this.overfillTicketRewardText.text = value.ToString("0");
			}).SetEase(Ease.OutCubic).SetId(this);
			yield return new WaitForSeconds(duration);
		}
		yield break;
	}

	// Token: 0x0600156F RID: 5487 RVA: 0x0005BDD0 File Offset: 0x00059FD0
	public void Reset()
	{
		this.percentageText.text = "0%";
		this.overfillPercentageText.text = "+0%";
		this.ticketRewardText.text = "0";
		this.overfillTicketRewardText.text = "0";
		this.underQuotaFill.anchorMin = new Vector2(0f, this.underQuotaFill.anchorMin.y);
		this.underQuotaFill.anchorMax = new Vector2(0f, this.underQuotaFill.anchorMax.y);
		this.underQuotaFill.offsetMin = new Vector2(0f, this.underQuotaFill.offsetMin.y);
		this.underQuotaFill.offsetMax = new Vector2(this.gapPixels / 2f, this.underQuotaFill.offsetMax.y);
		this.overQuotaFill.anchorMin = new Vector2(1f, this.overQuotaFill.anchorMin.y);
		this.overQuotaFill.anchorMax = new Vector2(1f, this.overQuotaFill.anchorMax.y);
		this.overQuotaFill.offsetMin = new Vector2(this.gapPixels / 2f, this.overQuotaFill.offsetMin.y);
		this.overQuotaFill.offsetMax = new Vector2(0f, this.overQuotaFill.offsetMax.y);
		this.overQuotaFill.gameObject.SetActive(false);
	}

	// Token: 0x06001570 RID: 5488 RVA: 0x0005BF64 File Offset: 0x0005A164
	public void SetImmediate(long quota, long balance, int baseTicket, int overflowTicket)
	{
		DOTween.Kill(this, false);
		this.Reset();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (quota > 0L && balance > 0L)
		{
			num = (float)((double)balance / (double)quota) * 100f;
			num2 = Mathf.Clamp(num, 0f, 100f);
			num3 = ((num > 100f) ? (num - 100f) : 0f);
		}
		this.SetUnderQuotaFill(Mathf.Max(num2 / 100f, this.minVisualPercent));
		this.SetPercentageText(num2);
		if (num >= 100f)
		{
			this.ticketRewardText.text = baseTicket.ToString();
		}
		if (num3 > 0f)
		{
			this.overQuotaFill.gameObject.SetActive(true);
			float num4 = Mathf.Clamp(num2 / num, this.minVisualPercent, 1f - this.minVisualPercent);
			this.SetOverQuotaFill(num4);
			this.SetUnderQuotaFill(num4);
			this.SetOverfillPercentageText(num3);
			this.overfillTicketRewardText.text = overflowTicket.ToString();
		}
	}

	// Token: 0x06001571 RID: 5489 RVA: 0x0005C064 File Offset: 0x0005A264
	private void SetPercentageText(float percent)
	{
		int num = Mathf.RoundToInt(percent);
		this.percentageText.text = string.Format("{0}%", num);
	}

	// Token: 0x06001572 RID: 5490 RVA: 0x0005C094 File Offset: 0x0005A294
	private void SetOverfillPercentageText(float percent)
	{
		int num = Mathf.RoundToInt(percent);
		this.overfillPercentageText.text = string.Format("+{0}%", num);
	}

	// Token: 0x06001573 RID: 5491 RVA: 0x0005C0C4 File Offset: 0x0005A2C4
	private void SetUnderQuotaFill(float percent)
	{
		this.underQuotaFill.anchorMin = new Vector2(0f, this.underQuotaFill.anchorMin.y);
		this.underQuotaFill.anchorMax = new Vector2(percent, this.underQuotaFill.anchorMax.y);
		this.underQuotaFill.offsetMin = new Vector2(0f, this.underQuotaFill.offsetMin.y);
		this.underQuotaFill.offsetMax = new Vector2(-this.gapPixels / 2f, this.underQuotaFill.offsetMax.y);
	}

	// Token: 0x06001574 RID: 5492 RVA: 0x0005C16C File Offset: 0x0005A36C
	private void SetOverQuotaFill(float percent)
	{
		this.overQuotaFill.anchorMin = new Vector2(percent, this.underQuotaFill.anchorMin.y);
		this.overQuotaFill.anchorMax = new Vector2(1f, this.underQuotaFill.anchorMax.y);
		this.overQuotaFill.offsetMin = new Vector2(this.gapPixels / 2f, this.underQuotaFill.offsetMin.y);
		this.overQuotaFill.offsetMax = new Vector2(0f, this.underQuotaFill.offsetMax.y);
	}

	// Token: 0x04000DB8 RID: 3512
	[Header("Quota Bar Parts")]
	[SerializeField]
	private RectTransform underQuotaFill;

	// Token: 0x04000DB9 RID: 3513
	[SerializeField]
	private RectTransform overQuotaFill;

	// Token: 0x04000DBA RID: 3514
	[SerializeField]
	private float gapPixels = 4f;

	// Token: 0x04000DBB RID: 3515
	[SerializeField]
	private float minVisualPercent = 0.1f;

	// Token: 0x04000DBC RID: 3516
	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI percentageText;

	// Token: 0x04000DBD RID: 3517
	[SerializeField]
	private TextMeshProUGUI overfillPercentageText;

	// Token: 0x04000DBE RID: 3518
	[Header("Ticket Reward")]
	[SerializeField]
	private TextMeshProUGUI ticketRewardText;

	// Token: 0x04000DBF RID: 3519
	[SerializeField]
	private TextMeshProUGUI overfillTicketRewardText;

	// Token: 0x04000DC0 RID: 3520
	[Header("SFX")]
	[SerializeField]
	private EventReference firstSliderSfx;

	// Token: 0x04000DC1 RID: 3521
	[SerializeField]
	private EventReference secondSliderSfx;
}
