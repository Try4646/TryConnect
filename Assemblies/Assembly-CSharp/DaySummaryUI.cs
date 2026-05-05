using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x0200025E RID: 606
public class DaySummaryUI : MonoBehaviour
{
	// Token: 0x0600158D RID: 5517 RVA: 0x0005C730 File Offset: 0x0005A930
	private void Awake()
	{
		this.SetReferences();
	}

	// Token: 0x0600158E RID: 5518 RVA: 0x0005C738 File Offset: 0x0005A938
	private void SetReferences()
	{
		if (!this._payoutTracker)
		{
			this._payoutTracker = NetworkSingleton<PayoutTracker>.Instance;
		}
		if (!this._localManager)
		{
			this._localManager = MonoSingleton<LocalManager>.Instance;
		}
		if (!this._moneyManager)
		{
			this._moneyManager = NetworkSingleton<MoneyManager>.Instance;
		}
		if (!this._gameManager)
		{
			this._gameManager = NetworkSingleton<GameManager>.Instance;
		}
		if (!this._gameSettings)
		{
			this._gameSettings = Resources.Load<GameSettings>("GameSettings");
		}
	}

	// Token: 0x0600158F RID: 5519 RVA: 0x0005C7C4 File Offset: 0x0005A9C4
	private void OnEnable()
	{
		SkipUI skipUI = this.skipUI;
		skipUI.OnSkipped = (UnityAction)Delegate.Combine(skipUI.OnSkipped, new UnityAction(this.OnSkip));
		InputEvents.OnSkipUIEvent = (Action<bool>)Delegate.Combine(InputEvents.OnSkipUIEvent, new Action<bool>(this.OnSkipUIEvent));
	}

	// Token: 0x06001590 RID: 5520 RVA: 0x0005C818 File Offset: 0x0005AA18
	private void OnDisable()
	{
		SkipUI skipUI = this.skipUI;
		skipUI.OnSkipped = (UnityAction)Delegate.Remove(skipUI.OnSkipped, new UnityAction(this.OnSkip));
		InputEvents.OnSkipUIEvent = (Action<bool>)Delegate.Remove(InputEvents.OnSkipUIEvent, new Action<bool>(this.OnSkipUIEvent));
	}

	// Token: 0x06001591 RID: 5521 RVA: 0x0005C86C File Offset: 0x0005AA6C
	private void OnSkipUIEvent(bool isPressed)
	{
		if (isPressed)
		{
			this._pendingSegmentSkips++;
		}
	}

	// Token: 0x06001592 RID: 5522 RVA: 0x0005C880 File Offset: 0x0005AA80
	private void OnSkip()
	{
		CanvasGroup[] array = this.canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DOFade(0f, 1f).SetDelay(1f);
		}
		this.statsLoopComponent.LoopSFX(false);
	}

	// Token: 0x06001593 RID: 5523 RVA: 0x0005C8CC File Offset: 0x0005AACC
	public void Show()
	{
		this.Reset();
		EmoteWheelController emoteWheelController = Object.FindAnyObjectByType<EmoteWheelController>(FindObjectsInactive.Include);
		if (emoteWheelController)
		{
			emoteWheelController.SetEmoteWheelActive(false);
		}
		base.StartCoroutine(this.DaySummaryRoutine());
	}

	// Token: 0x06001594 RID: 5524 RVA: 0x0005C902 File Offset: 0x0005AB02
	private bool ConsumeSkipRequest()
	{
		if (this._pendingSegmentSkips <= 0)
		{
			return false;
		}
		this._pendingSegmentSkips--;
		return true;
	}

	// Token: 0x06001595 RID: 5525 RVA: 0x0005C91E File Offset: 0x0005AB1E
	private IEnumerator WaitForSecondsOrSkip(float duration, Action onSkip)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			if (this.ConsumeSkipRequest())
			{
				if (onSkip != null)
				{
					onSkip();
				}
				yield break;
			}
			elapsed += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	// Token: 0x06001596 RID: 5526 RVA: 0x0005C93B File Offset: 0x0005AB3B
	private IEnumerator TrackRoutine(IEnumerator routine, Action onComplete)
	{
		yield return routine;
		if (onComplete != null)
		{
			onComplete();
		}
		yield break;
	}

	// Token: 0x06001597 RID: 5527 RVA: 0x0005C951 File Offset: 0x0005AB51
	private IEnumerator DaySummaryRoutine()
	{
		base.gameObject.SetActive(true);
		this.statsLoopComponent.LoopSFX(true);
		this.SetReferences();
		if (!this._payoutTracker || !this._localManager || !this._moneyManager || !this._gameManager || !this._gameSettings)
		{
			yield break;
		}
		this.skipUI.Reset();
		this.skipUI.SetSkippableServer();
		this._pendingSegmentSkips = 0;
		this.quotaSlider.Reset();
		CanvasGroup[] array = this.canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = 0f;
		}
		this.titleText.text = "";
		int dayIndex = this._gameManager.daysPassed + 1;
		long dayStartBalance = this._moneyManager.dayStartBalance;
		long profit = this._moneyManager.balance - dayStartBalance;
		int quotaExcessTickets = 0;
		if (this._gameManager.currentQuota > 0L && this._gameManager.currentFloor >= 0 && this._gameManager.currentFloor < this._gameSettings.floorData.Count)
		{
			quotaExcessTickets = this._gameSettings.GetQuotaExcessReward(this._gameManager.currentFloor, this._gameManager.currentQuota, this._moneyManager.balance);
		}
		string text = MoneyFormatter.FormatWithDollar(this._gameManager.currentQuota) ?? "";
		TextMeshProUGUI[] array2 = this.quotaText;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].text = text;
		}
		this.balanceText.text = (MoneyFormatter.FormatWithDollar(dayStartBalance) ?? "");
		this.profitText.text = "+$0";
		this._ticketEntries.Clear();
		for (int j = this.ticketSourcesRoot.childCount - 1; j >= 0; j--)
		{
			Object.Destroy(this.ticketSourcesRoot.GetChild(j).gameObject);
		}
		int totalTickets = 0;
		this.totalTicketsText.text = "0";
		this.SkipKeyPrompt.SetActive(true);
		TweenerCore<float, float, FloatOptions> introTween = this.canvasGroups[0].DOFade(1f, this.canvasAlphaDuration);
		yield return this.WaitForSecondsOrSkip(this.canvasAlphaDuration, delegate
		{
			introTween.Complete();
		});
		yield return this.WaitForSecondsOrSkip(this.initialDelay, null);
		this.titleText.text = string.Format("Stats for day {0}", dayIndex);
		yield return this.WaitForSecondsOrSkip(this.titleAnimationDuration, null);
		TweenerCore<float, float, FloatOptions> quotaHeadersTweenA = this.canvasGroups[1].DOFade(1f, this.appearanceDuration);
		TweenerCore<float, float, FloatOptions> quotaHeadersTweenB = this.canvasGroups[2].DOFade(1f, this.appearanceDuration);
		yield return this.WaitForSecondsOrSkip(this.appearanceDuration + this.delayBetweenAnimations, delegate
		{
			quotaHeadersTweenA.Complete();
			quotaHeadersTweenB.Complete();
		});
		int baseTicketRewardForDay = this._gameSettings.GetTicketReward(this._gameManager.daysPassed);
		if (this.ConsumeSkipRequest())
		{
			this.quotaSlider.SetImmediate(this._gameManager.currentQuota, this._moneyManager.balance, baseTicketRewardForDay, quotaExcessTickets);
		}
		else
		{
			DaySummaryUI.<>c__DisplayClass44_1 CS$<>8__locals2 = new DaySummaryUI.<>c__DisplayClass44_1();
			CS$<>8__locals2.sliderDone = false;
			Coroutine sliderRoutine = base.StartCoroutine(this.TrackRoutine(this.quotaSlider.SliderRoutine(this._gameManager.currentQuota, this._moneyManager.balance, baseTicketRewardForDay, quotaExcessTickets, this.quotaBarFillDuration), delegate
			{
				CS$<>8__locals2.sliderDone = true;
			}));
			while (!CS$<>8__locals2.sliderDone)
			{
				if (this.ConsumeSkipRequest())
				{
					base.StopCoroutine(sliderRoutine);
					this.quotaSlider.SetImmediate(this._gameManager.currentQuota, this._moneyManager.balance, baseTicketRewardForDay, quotaExcessTickets);
					CS$<>8__locals2.sliderDone = true;
					break;
				}
				yield return null;
			}
			CS$<>8__locals2 = null;
			sliderRoutine = null;
		}
		TweenerCore<float, float, FloatOptions> moneyGroupTween = this.canvasGroups[3].DOFade(1f, this.appearanceDuration);
		yield return this.WaitForSecondsOrSkip(this.appearanceDuration + this.delayBetweenAnimations, delegate
		{
			moneyGroupTween.Complete();
		});
		double end = (double)this._moneyManager.balance;
		long previousBalance = 0L;
		Tweener balanceTween = DOVirtual.Float(0f, 1f, this.moneyTextsDuration, delegate(float t)
		{
			long num3 = (long)Math.Round((double)dayStartBalance + (end - (double)dayStartBalance) * (double)t);
			this.balanceText.text = (MoneyFormatter.FormatWithDollar(num3) ?? "");
			if (num3 != previousBalance)
			{
				SFXManager.SFXOneShot(this.textChangeSfx, default(Vector3));
			}
			previousBalance = num3;
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.balanceText.transform.DOPunchScale(this.balanceText.transform.localScale * 0.2f, 0.5f, 1, 1f);
			SFXManager.SFXOneShot(this.textBlopSfx, default(Vector3));
		});
		yield return this.WaitForSecondsOrSkip(this.moneyTextsDuration + 0.5f, delegate
		{
			balanceTween.Complete();
			this.balanceText.text = (MoneyFormatter.FormatWithDollar(this._moneyManager.balance) ?? "");
		});
		string sign = (profit >= 0L) ? "+" : "";
		long previousProfit = 0L;
		Tweener profitTween = DOVirtual.Float(0f, 1f, this.moneyTextsDuration, delegate(float t)
		{
			long num3 = (long)Math.Round((double)((float)profit * t));
			this.profitText.text = sign + MoneyFormatter.FormatWithDollar(num3);
			if (num3 != previousProfit)
			{
				SFXManager.SFXOneShot(this.textChangeSfx, default(Vector3));
			}
			previousProfit = num3;
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.profitText.transform.DOPunchScale(this.balanceText.transform.localScale * 0.2f, 0.5f, 1, 1f);
			SFXManager.SFXOneShot(this.textBlopSfx, default(Vector3));
		});
		yield return this.WaitForSecondsOrSkip(this.moneyTextsDuration + 0.5f, delegate
		{
			profitTween.Complete();
			this.profitText.text = sign + MoneyFormatter.FormatWithDollar(profit);
		});
		TweenerCore<float, float, FloatOptions> graphGroupTween = this.canvasGroups[4].DOFade(1f, this.appearanceDuration);
		yield return this.WaitForSecondsOrSkip(this.appearanceDuration + this.delayBetweenAnimations, delegate
		{
			graphGroupTween.Complete();
		});
		ProfitLineGraph3D profitLineGraph3D = Object.FindFirstObjectByType<ProfitLineGraph3D>();
		if (profitLineGraph3D)
		{
			profitLineGraph3D.ResetAndAnimate();
		}
		yield return this.WaitForSecondsOrSkip(this.profitGraphDuration, null);
		TweenerCore<float, float, FloatOptions> ticketGroupTween = this.canvasGroups[5].DOFade(1f, this.appearanceDuration);
		yield return this.WaitForSecondsOrSkip(this.appearanceDuration + this.delayBetweenAnimations, delegate
		{
			ticketGroupTween.Complete();
		});
		bool flag = this._gameManager.currentQuota > 0L && this._moneyManager.balance >= this._gameManager.currentQuota;
		if (flag)
		{
			int ticketReward = this._gameSettings.GetTicketReward(this._gameManager.daysPassed);
			this.AddTicketEntry("Quota (100%)", ticketReward);
			totalTickets += ticketReward;
		}
		if (flag && quotaExcessTickets > 0)
		{
			float num = Mathf.Max(0.0001f, (float)this._gameManager.currentQuota);
			float f = (float)this._moneyManager.balance / num * 100f - 100f;
			int num2 = Mathf.Max(0, Mathf.RoundToInt(f));
			this.AddTicketEntry(string.Format("Quota Excess Reward ({0}%)", num2), quotaExcessTickets);
			totalTickets += quotaExcessTickets;
		}
		if (MonoSingleton<DaySummaryRuntime>.Instance)
		{
			foreach (DaySummaryRuntime.ChallengeReward challengeReward in MonoSingleton<DaySummaryRuntime>.Instance.CompletedChallenges)
			{
				if (challengeReward.tickets > 0)
				{
					this.AddTicketEntry(challengeReward.challengeName + " (Challenge)", challengeReward.tickets);
					totalTickets += challengeReward.tickets;
				}
			}
		}
		bool skipTicketAnimation = this.ConsumeSkipRequest();
		foreach (DaySummaryTicketEntry daySummaryTicketEntry in this._ticketEntries)
		{
			if (skipTicketAnimation)
			{
				daySummaryTicketEntry.SetImmediate();
			}
			else
			{
				yield return daySummaryTicketEntry.Animate(this.ticketRewardDuration);
				if (this.ConsumeSkipRequest())
				{
					skipTicketAnimation = true;
				}
				else
				{
					yield return this.WaitForSecondsOrSkip(this.delayBetweenTicketReward, null);
					if (this.ConsumeSkipRequest())
					{
						skipTicketAnimation = true;
					}
				}
			}
		}
		List<DaySummaryTicketEntry>.Enumerator enumerator2 = default(List<DaySummaryTicketEntry>.Enumerator);
		int previousTicket = 0;
		Tweener totalTicketTween = DOVirtual.Float(0f, (float)totalTickets, this.finalTicketRewardDuration, delegate(float value)
		{
			int num3 = (int)value;
			this.totalTicketsText.text = num3.ToString();
			if (num3 != previousTicket)
			{
				SFXManager.SFXOneShot(this.ticketTextChangeSfx, default(Vector3));
			}
			previousTicket = num3;
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.totalTicketsText.transform.DOPunchScale(this.totalTicketsText.transform.localScale * 0.2f, 0.5f, 1, 1f);
			SFXManager.SFXOneShot(this.textBlopSfx, default(Vector3));
		});
		yield return this.WaitForSecondsOrSkip(this.finalTicketRewardDuration + 0.5f, delegate
		{
			totalTicketTween.Complete();
			this.totalTicketsText.text = totalTickets.ToString();
		});
		yield return this.WaitForSecondsOrSkip(this.finalDelayBeforeSkip, null);
		this.skipUI.SetSkippableForLocal();
		this.SkipKeyPrompt.SetActive(false);
		yield break;
		yield break;
	}

	// Token: 0x06001598 RID: 5528 RVA: 0x0005C960 File Offset: 0x0005AB60
	private void AddTicketEntry(string label, int tickets)
	{
		if (tickets <= 0)
		{
			return;
		}
		DaySummaryTicketEntry daySummaryTicketEntry = Object.Instantiate<DaySummaryTicketEntry>(this.ticketSourceEntryPrefab, this.ticketSourcesRoot);
		daySummaryTicketEntry.Setup(label, tickets);
		this._ticketEntries.Add(daySummaryTicketEntry);
	}

	// Token: 0x06001599 RID: 5529 RVA: 0x0005C998 File Offset: 0x0005AB98
	public void Reset()
	{
		base.StopAllCoroutines();
		DOTween.Kill(base.gameObject, true);
		this._pendingSegmentSkips = 0;
		this.statsLoopComponent.LoopSFX(false);
		this.skipUI.Reset();
		CanvasGroup[] array = this.canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = 0f;
		}
	}

	// Token: 0x04000DDA RID: 3546
	[Header("Settings")]
	[SerializeField]
	private float canvasAlphaDuration = 1f;

	// Token: 0x04000DDB RID: 3547
	[SerializeField]
	private float initialDelay = 1f;

	// Token: 0x04000DDC RID: 3548
	[SerializeField]
	private float titleAnimationDuration = 2f;

	// Token: 0x04000DDD RID: 3549
	[SerializeField]
	private float quotaBarFillDuration = 1f;

	// Token: 0x04000DDE RID: 3550
	[SerializeField]
	private float moneyTextsDuration = 1f;

	// Token: 0x04000DDF RID: 3551
	[SerializeField]
	private float profitGraphDuration = 2f;

	// Token: 0x04000DE0 RID: 3552
	[SerializeField]
	private float ticketRewardDuration = 0.5f;

	// Token: 0x04000DE1 RID: 3553
	[SerializeField]
	private float delayBetweenTicketReward = 0.5f;

	// Token: 0x04000DE2 RID: 3554
	[SerializeField]
	private float finalTicketRewardDuration = 1f;

	// Token: 0x04000DE3 RID: 3555
	[SerializeField]
	private float finalDelayBeforeSkip = 1f;

	// Token: 0x04000DE4 RID: 3556
	[SerializeField]
	private float appearanceDuration = 0.5f;

	// Token: 0x04000DE5 RID: 3557
	[SerializeField]
	private float delayBetweenAnimations = 0.5f;

	// Token: 0x04000DE6 RID: 3558
	[SerializeField]
	private GameObject SkipKeyPrompt;

	// Token: 0x04000DE7 RID: 3559
	[Header("UI")]
	[SerializeField]
	private CanvasGroup[] canvasGroups;

	// Token: 0x04000DE8 RID: 3560
	[SerializeField]
	private SkipUI skipUI;

	// Token: 0x04000DE9 RID: 3561
	[Header("Header")]
	[SerializeField]
	private TextMeshProUGUI titleText;

	// Token: 0x04000DEA RID: 3562
	[Header("Money")]
	[SerializeField]
	private TextMeshProUGUI profitText;

	// Token: 0x04000DEB RID: 3563
	[SerializeField]
	private TextMeshProUGUI balanceText;

	// Token: 0x04000DEC RID: 3564
	[Header("Quota")]
	[SerializeField]
	private TextMeshProUGUI[] quotaText;

	// Token: 0x04000DED RID: 3565
	[SerializeField]
	private DaySummaryQuotaSlider quotaSlider;

	// Token: 0x04000DEE RID: 3566
	[Header("Ticket Sources")]
	[SerializeField]
	private Transform ticketSourcesRoot;

	// Token: 0x04000DEF RID: 3567
	[SerializeField]
	private DaySummaryTicketEntry ticketSourceEntryPrefab;

	// Token: 0x04000DF0 RID: 3568
	[SerializeField]
	private TextMeshProUGUI totalTicketsText;

	// Token: 0x04000DF1 RID: 3569
	[Header("SFX")]
	[SerializeField]
	private EventReference textChangeSfx;

	// Token: 0x04000DF2 RID: 3570
	[SerializeField]
	private EventReference ticketTextChangeSfx;

	// Token: 0x04000DF3 RID: 3571
	[SerializeField]
	private EventReference textBlopSfx;

	// Token: 0x04000DF4 RID: 3572
	[SerializeField]
	private SFXLoopComponent statsLoopComponent;

	// Token: 0x04000DF5 RID: 3573
	private List<DaySummaryTicketEntry> _ticketEntries = new List<DaySummaryTicketEntry>();

	// Token: 0x04000DF6 RID: 3574
	private PayoutTracker _payoutTracker;

	// Token: 0x04000DF7 RID: 3575
	private LocalManager _localManager;

	// Token: 0x04000DF8 RID: 3576
	private MoneyManager _moneyManager;

	// Token: 0x04000DF9 RID: 3577
	private GameManager _gameManager;

	// Token: 0x04000DFA RID: 3578
	private GameSettings _gameSettings;

	// Token: 0x04000DFB RID: 3579
	private int _pendingSegmentSkips;
}
