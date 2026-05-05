using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000127 RID: 295
public class ChallengeEntryUI : MonoBehaviour
{
	// Token: 0x06000BE2 RID: 3042 RVA: 0x0003091C File Offset: 0x0002EB1C
	public void SetData(ChallengeProgress progress)
	{
		if (progress == null || progress.challenge == null)
		{
			return;
		}
		if (this.challengeNameText != null)
		{
			this.challengeNameText.text = progress.challenge.challengeName;
		}
		if (this.descriptionText != null)
		{
			string processedDescription = progress.challenge.GetProcessedDescription();
			this.descriptionText.text = (this.useDescriptionWrap ? ChallengeEntryUI.WrapDescription(processedDescription, this.DescriptionWrapChars) : processedDescription);
		}
		if (this.progressText != null)
		{
			this.progressText.text = progress.progressText;
		}
		bool flag = progress.challenge.ShouldShowProgress();
		float num = Mathf.Clamp01(progress.progress);
		if (this.progressValueText != null)
		{
			if (flag && num > 0f)
			{
				this.progressValueText.text = string.Format("{0}%", Mathf.RoundToInt(num * 100f));
			}
			else
			{
				this.progressValueText.text = string.Empty;
			}
		}
		if (this.progressSlider != null)
		{
			this.progressSlider.value = num;
			this.progressSlider.gameObject.SetActive(flag);
		}
		if (this.progressFillImage != null)
		{
			this.progressFillImage.fillAmount = num;
			this.progressFillImage.gameObject.SetActive(flag);
		}
		if (this.rewardText != null)
		{
			this.rewardText.text = progress.challenge.GetTicketReward().ToString();
		}
	}

	// Token: 0x06000BE3 RID: 3043 RVA: 0x00030AA8 File Offset: 0x0002ECA8
	private static string WrapDescription(string text, int maxChars)
	{
		if (string.IsNullOrEmpty(text) || text.Length <= maxChars)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		while (i < text.Length)
		{
			int num = Math.Min(maxChars, text.Length - i);
			int num2 = i + num;
			if (num2 < text.Length)
			{
				int num3 = text.LastIndexOf(' ', num2 - 1, num);
				if (num3 >= i)
				{
					num2 = num3 + 1;
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(text, i, num2 - i);
			i = num2;
			while (i < text.Length && text[i] == ' ')
			{
				i++;
			}
		}
		return stringBuilder.ToString();
	}

	// Token: 0x04000770 RID: 1904
	[Header("Description Wrapping")]
	[SerializeField]
	private bool useDescriptionWrap = true;

	// Token: 0x04000771 RID: 1905
	[SerializeField]
	private int DescriptionWrapChars = 40;

	// Token: 0x04000772 RID: 1906
	[Header("Text")]
	[SerializeField]
	private TMP_Text challengeNameText;

	// Token: 0x04000773 RID: 1907
	[SerializeField]
	private TMP_Text descriptionText;

	// Token: 0x04000774 RID: 1908
	[SerializeField]
	private TMP_Text progressText;

	// Token: 0x04000775 RID: 1909
	[SerializeField]
	private TMP_Text progressValueText;

	// Token: 0x04000776 RID: 1910
	[SerializeField]
	private TMP_Text rewardText;

	// Token: 0x04000777 RID: 1911
	[Header("Progress")]
	[SerializeField]
	private Slider progressSlider;

	// Token: 0x04000778 RID: 1912
	[SerializeField]
	private Image progressFillImage;
}
