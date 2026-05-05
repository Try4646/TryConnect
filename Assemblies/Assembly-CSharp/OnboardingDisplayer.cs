using System;
using System.Collections.Generic;
using Extensions;
using Febucci.UI;
using MoreMountains.Feedbacks;
using UnityEngine;

// Token: 0x020002D0 RID: 720
public class OnboardingDisplayer : MonoBehaviour
{
	// Token: 0x06001963 RID: 6499 RVA: 0x0006AA47 File Offset: 0x00068C47
	private void OnEnable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Combine(InputEvents.OnInteractEvent, new Action<bool>(this.DisplayOnboardingFeedbacks));
	}

	// Token: 0x06001964 RID: 6500 RVA: 0x0006AA69 File Offset: 0x00068C69
	private void OnDisable()
	{
		InputEvents.OnInteractEvent = (Action<bool>)Delegate.Remove(InputEvents.OnInteractEvent, new Action<bool>(this.DisplayOnboardingFeedbacks));
	}

	// Token: 0x06001965 RID: 6501 RVA: 0x0006AA8B File Offset: 0x00068C8B
	public void Init()
	{
		if (NetworkSingleton<GameManager>.Instance.daysPassed > 0)
		{
			base.gameObject.SetActive(false);
		}
		this.PlayNextLine();
	}

	// Token: 0x06001966 RID: 6502 RVA: 0x0006AAAC File Offset: 0x00068CAC
	private void SetText(int index)
	{
		this.textAnimator.GetComponent<TextAnimatorPlayer>().ShowText(this.lines[index]);
	}

	// Token: 0x06001967 RID: 6503 RVA: 0x0006AACA File Offset: 0x00068CCA
	private void DisplayOnboardingFeedbacks(bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		if (!this.textAnimator.allLettersShown)
		{
			this.textAnimator.GetComponent<TextAnimatorPlayer>().SkipTypewriter();
			return;
		}
		this.PlayNextLine();
	}

	// Token: 0x06001968 RID: 6504 RVA: 0x0006AAF4 File Offset: 0x00068CF4
	private void PlayNextLine()
	{
		if (this._currentLineIndex < this.lines.Count)
		{
			this.SetText(this._currentLineIndex);
			this._currentLineIndex++;
			return;
		}
		this.displayCloseFeedbacks.PlayFeedbacks();
	}

	// Token: 0x04001052 RID: 4178
	[SerializeField]
	private MMF_Player displayCloseFeedbacks;

	// Token: 0x04001053 RID: 4179
	[SerializeField]
	private TextAnimator textAnimator;

	// Token: 0x04001054 RID: 4180
	[SerializeField]
	private List<string> lines;

	// Token: 0x04001055 RID: 4181
	private int _currentLineIndex;
}
