using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extensions;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Token: 0x02000266 RID: 614
public class GameOverUI : MonoBehaviour
{
	// Token: 0x060015CB RID: 5579 RVA: 0x0005DD7F File Offset: 0x0005BF7F
	private void Awake()
	{
		this.SetReferences();
	}

	// Token: 0x060015CC RID: 5580 RVA: 0x0005DD88 File Offset: 0x0005BF88
	private void SetReferences()
	{
		if (!this._moneyManager)
		{
			this._moneyManager = NetworkSingleton<MoneyManager>.Instance;
		}
		if (!this._gameManager)
		{
			this._gameManager = NetworkSingleton<GameManager>.Instance;
		}
		if (!this._moneyDisplayAndFeedbacks)
		{
			this._moneyDisplayAndFeedbacks = NetworkSingleton<MoneyDisplayAndFeedbacks>.Instance;
		}
		if (!this._lobbySettings)
		{
			this._lobbySettings = Resources.Load<LobbySettings>("LobbySettings");
		}
	}

	// Token: 0x060015CD RID: 5581 RVA: 0x0005DDFC File Offset: 0x0005BFFC
	private void OnEnable()
	{
		SkipUI skipUI = this.skipUI;
		skipUI.OnSkipped = (UnityAction)Delegate.Combine(skipUI.OnSkipped, new UnityAction(this.OnSkip));
		InputEvents.OnSkipUIEvent = (Action<bool>)Delegate.Combine(InputEvents.OnSkipUIEvent, new Action<bool>(this.OnSkipUIEvent));
	}

	// Token: 0x060015CE RID: 5582 RVA: 0x0005DE50 File Offset: 0x0005C050
	private void OnDisable()
	{
		SkipUI skipUI = this.skipUI;
		skipUI.OnSkipped = (UnityAction)Delegate.Remove(skipUI.OnSkipped, new UnityAction(this.OnSkip));
		InputEvents.OnSkipUIEvent = (Action<bool>)Delegate.Remove(InputEvents.OnSkipUIEvent, new Action<bool>(this.OnSkipUIEvent));
	}

	// Token: 0x060015CF RID: 5583 RVA: 0x0005DEA4 File Offset: 0x0005C0A4
	private void OnSkipUIEvent(bool isPressed)
	{
		if (isPressed)
		{
			this._pendingSegmentSkips++;
		}
	}

	// Token: 0x060015D0 RID: 5584 RVA: 0x0005DEB8 File Offset: 0x0005C0B8
	private void OnSkip()
	{
		CanvasGroup[] array = this.canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DOFade(0f, 1f).SetDelay(1f);
		}
	}

	// Token: 0x060015D1 RID: 5585 RVA: 0x0005DEF8 File Offset: 0x0005C0F8
	public void Show()
	{
		this.Reset();
		EmoteWheelController emoteWheelController = Object.FindAnyObjectByType<EmoteWheelController>(FindObjectsInactive.Include);
		if (emoteWheelController)
		{
			emoteWheelController.SetEmoteWheelActive(false);
		}
		base.StartCoroutine(this.GameOverRoutine());
	}

	// Token: 0x060015D2 RID: 5586 RVA: 0x0005DF2E File Offset: 0x0005C12E
	private bool ConsumeSkipRequest()
	{
		if (this._pendingSegmentSkips <= 0)
		{
			return false;
		}
		this._pendingSegmentSkips--;
		return true;
	}

	// Token: 0x060015D3 RID: 5587 RVA: 0x0005DF4A File Offset: 0x0005C14A
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

	// Token: 0x060015D4 RID: 5588 RVA: 0x0005DF67 File Offset: 0x0005C167
	private IEnumerator GameOverRoutine()
	{
		base.gameObject.SetActive(true);
		SFXManager.SFXOneShot(this.enterSummaryAmbSfx, default(Vector3));
		this.SetReferences();
		if (!this._moneyManager || !this._gameManager || !this._moneyDisplayAndFeedbacks)
		{
			yield break;
		}
		this.skipUI.Reset();
		this.skipUI.SetSkippableServer();
		this._pendingSegmentSkips = 0;
		CanvasGroup[] array = this.canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = 0f;
		}
		this.titleText.text = "";
		int dayIndex = this._gameManager.daysPassed + 1;
		long balance = NetworkSingleton<PayoutTracker>.Instance.GetLifetimeNetTotal();
		long ticketBalance = this._moneyManager.ticketBalance;
		this._contributionEntries.Clear();
		for (int j = this.contributionsRoot.childCount - 1; j >= 0; j--)
		{
			Object.Destroy(this.contributionsRoot.GetChild(j).gameObject);
		}
		IEnumerable<KeyValuePair<string, long>> profitHistorySnapshot = this._moneyDisplayAndFeedbacks.GetProfitHistorySnapshot();
		HashSet<string> currentPlayerNames = this.GetCurrentPlayerNames();
		foreach (KeyValuePair<string, long> keyValuePair in from e in profitHistorySnapshot
		orderby e.Value descending
		select e)
		{
			string key = keyValuePair.Key;
			if (currentPlayerNames.Contains(key))
			{
				long value = keyValuePair.Value;
				this.AddContributionEntry(key, value);
			}
		}
		TweenerCore<float, float, FloatOptions> introTween = this.canvasGroups[0].DOFade(1f, this.canvasAlphaDuration);
		yield return this.WaitForSecondsOrSkip(this.canvasAlphaDuration, delegate
		{
			introTween.Complete();
		});
		yield return this.WaitForSecondsOrSkip(this.initialDelay, null);
		this.titleText.text = ((SceneManager.GetActiveScene().name == "LoseStateScene") ? "Game Over" : "Game Complete");
		yield return this.WaitForSecondsOrSkip(this.titleAnimationDuration, null);
		TweenerCore<float, float, FloatOptions> contributionsGroupTween = this.canvasGroups[1].DOFade(1f, this.appearanceDuration);
		yield return this.WaitForSecondsOrSkip(this.appearanceDuration + this.delayBetweenAnimations, delegate
		{
			contributionsGroupTween.Complete();
		});
		bool skipContributionAnimation = this.ConsumeSkipRequest();
		foreach (GameLostContributionEntry gameLostContributionEntry in this._contributionEntries)
		{
			if (skipContributionAnimation)
			{
				gameLostContributionEntry.SetImmediate();
			}
			else
			{
				yield return gameLostContributionEntry.Animate(this.contributionDuration);
				if (this.ConsumeSkipRequest())
				{
					skipContributionAnimation = true;
				}
				else
				{
					yield return this.WaitForSecondsOrSkip(this.delayBetweenContributions, null);
					if (this.ConsumeSkipRequest())
					{
						skipContributionAnimation = true;
					}
				}
			}
		}
		List<GameLostContributionEntry>.Enumerator enumerator2 = default(List<GameLostContributionEntry>.Enumerator);
		long previousBalance = 0L;
		Tweener totalBalanceTween = DOVirtual.Float(0f, 1f, this.totalBalanceDuration, delegate(float t)
		{
			long num = (long)Math.Round((double)((float)balance * t));
			this.totalBalanceText.text = (MoneyFormatter.FormatWithDollar(num) ?? "");
			if (num != previousBalance)
			{
				SFXManager.SFXOneShot(this.textChangeSfx, default(Vector3));
			}
			previousBalance = num;
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.totalBalanceText.transform.DOPunchScale(this.totalBalanceText.transform.localScale * 0.2f, 0.5f, 1, 1f);
			SFXManager.SFXOneShot(this.textBlopSfx, default(Vector3));
		});
		yield return this.WaitForSecondsOrSkip(this.totalBalanceDuration, delegate
		{
			totalBalanceTween.Complete();
			this.totalBalanceText.text = (MoneyFormatter.FormatWithDollar(balance) ?? "");
		});
		yield return this.WaitForSecondsOrSkip(this.delayBetweenAnimations, null);
		TweenerCore<float, float, FloatOptions> dayTicketsGroupTween = this.canvasGroups[2].DOFade(1f, this.appearanceDuration);
		yield return this.WaitForSecondsOrSkip(this.appearanceDuration + this.delayBetweenAnimations, delegate
		{
			dayTicketsGroupTween.Complete();
		});
		int previousDayIndex = 0;
		Tweener dayTween = DOVirtual.Float(0f, 1f, this.dayIndexDuration, delegate(float t)
		{
			int num = (int)Math.Round((double)((float)dayIndex * t));
			this.dayReachedText.text = num.ToString();
			if (num != previousDayIndex)
			{
				SFXManager.SFXOneShot(this.textChangeSfx, default(Vector3));
			}
			previousDayIndex = num;
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.dayReachedText.transform.DOPunchScale(this.dayReachedText.transform.localScale * 0.2f, 0.5f, 1, 1f);
			SFXManager.SFXOneShot(this.textBlopSfx, default(Vector3));
		});
		yield return this.WaitForSecondsOrSkip(this.dayIndexDuration, delegate
		{
			dayTween.Complete();
			this.dayReachedText.text = dayIndex.ToString();
		});
		yield return this.WaitForSecondsOrSkip(this.delayBetweenAnimations, null);
		int previousTicketBalance = 0;
		Tweener ticketTween = DOVirtual.Float(0f, 1f, this.ticketBalanceDuration, delegate(float t)
		{
			int num = (int)Math.Round((double)((float)ticketBalance * t));
			this.ticketsText.text = num.ToString();
			if (num != previousTicketBalance)
			{
				SFXManager.SFXOneShot(this.textChangeSfx, default(Vector3));
			}
			previousTicketBalance = num;
		}).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			this.ticketsText.transform.DOPunchScale(this.ticketsText.transform.localScale * 0.2f, 0.5f, 1, 1f);
			SFXManager.SFXOneShot(this.textBlopSfx, default(Vector3));
		});
		yield return this.WaitForSecondsOrSkip(this.ticketBalanceDuration, delegate
		{
			ticketTween.Complete();
			this.ticketsText.text = ticketBalance.ToString();
		});
		yield return this.WaitForSecondsOrSkip(this.delayBetweenAnimations, null);
		yield return this.WaitForSecondsOrSkip(this.finalDelayBeforeSkip, null);
		this.skipUI.SetSkippableForLocal();
		yield break;
		yield break;
	}

	// Token: 0x060015D5 RID: 5589 RVA: 0x0005DF78 File Offset: 0x0005C178
	private void AddContributionEntry(string playerName, long contribution)
	{
		GameLostContributionEntry gameLostContributionEntry = Object.Instantiate<GameLostContributionEntry>(this.contributionEntryPrefab, this.contributionsRoot);
		gameLostContributionEntry.Setup(playerName, contribution);
		this._contributionEntries.Add(gameLostContributionEntry);
	}

	// Token: 0x060015D6 RID: 5590 RVA: 0x0005DFAC File Offset: 0x0005C1AC
	private HashSet<string> GetCurrentPlayerNames()
	{
		if (this._lobbySettings == null || this._lobbySettings.players == null)
		{
			return new HashSet<string>();
		}
		return (from p in this._lobbySettings.players
		where !string.IsNullOrWhiteSpace(p.playerName)
		select p.playerName).ToHashSet<string>();
	}

	// Token: 0x060015D7 RID: 5591 RVA: 0x0005E034 File Offset: 0x0005C234
	public void Reset()
	{
		base.StopAllCoroutines();
		DOTween.Kill(base.gameObject, true);
		this._pendingSegmentSkips = 0;
		this.skipUI.Reset();
		this.totalBalanceText.text = "$0";
		this.dayReachedText.text = "0";
		this.ticketsText.text = "0";
		CanvasGroup[] array = this.canvasGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].alpha = 0f;
		}
	}

	// Token: 0x04000E2E RID: 3630
	[Header("Settings")]
	[SerializeField]
	private float canvasAlphaDuration = 1f;

	// Token: 0x04000E2F RID: 3631
	[SerializeField]
	private float initialDelay = 1f;

	// Token: 0x04000E30 RID: 3632
	[SerializeField]
	private float titleAnimationDuration = 2f;

	// Token: 0x04000E31 RID: 3633
	[SerializeField]
	private float contributionDuration = 1f;

	// Token: 0x04000E32 RID: 3634
	[SerializeField]
	private float delayBetweenContributions = 0.5f;

	// Token: 0x04000E33 RID: 3635
	[SerializeField]
	private float totalBalanceDuration = 1f;

	// Token: 0x04000E34 RID: 3636
	[SerializeField]
	private float dayIndexDuration = 1f;

	// Token: 0x04000E35 RID: 3637
	[SerializeField]
	private float ticketBalanceDuration = 1f;

	// Token: 0x04000E36 RID: 3638
	[SerializeField]
	private float finalDelayBeforeSkip = 1f;

	// Token: 0x04000E37 RID: 3639
	[SerializeField]
	private float appearanceDuration = 0.5f;

	// Token: 0x04000E38 RID: 3640
	[SerializeField]
	private float delayBetweenAnimations = 0.5f;

	// Token: 0x04000E39 RID: 3641
	[Header("UI")]
	[SerializeField]
	private CanvasGroup[] canvasGroups;

	// Token: 0x04000E3A RID: 3642
	[SerializeField]
	private SkipUI skipUI;

	// Token: 0x04000E3B RID: 3643
	[Header("Header")]
	[SerializeField]
	private TextMeshProUGUI titleText;

	// Token: 0x04000E3C RID: 3644
	[SerializeField]
	private TextMeshProUGUI dayReachedText;

	// Token: 0x04000E3D RID: 3645
	[Header("Totals")]
	[SerializeField]
	private TextMeshProUGUI totalBalanceText;

	// Token: 0x04000E3E RID: 3646
	[SerializeField]
	private TextMeshProUGUI ticketsText;

	// Token: 0x04000E3F RID: 3647
	[Header("Player Contributions")]
	[SerializeField]
	private Transform contributionsRoot;

	// Token: 0x04000E40 RID: 3648
	[SerializeField]
	private GameLostContributionEntry contributionEntryPrefab;

	// Token: 0x04000E41 RID: 3649
	[Header("SFX")]
	[SerializeField]
	private EventReference textChangeSfx;

	// Token: 0x04000E42 RID: 3650
	[SerializeField]
	private EventReference enterSummaryAmbSfx;

	// Token: 0x04000E43 RID: 3651
	[SerializeField]
	private EventReference textBlopSfx;

	// Token: 0x04000E44 RID: 3652
	private List<GameLostContributionEntry> _contributionEntries = new List<GameLostContributionEntry>();

	// Token: 0x04000E45 RID: 3653
	private GameManager _gameManager;

	// Token: 0x04000E46 RID: 3654
	private MoneyManager _moneyManager;

	// Token: 0x04000E47 RID: 3655
	private MoneyDisplayAndFeedbacks _moneyDisplayAndFeedbacks;

	// Token: 0x04000E48 RID: 3656
	private LobbySettings _lobbySettings;

	// Token: 0x04000E49 RID: 3657
	private int _pendingSegmentSkips;
}
