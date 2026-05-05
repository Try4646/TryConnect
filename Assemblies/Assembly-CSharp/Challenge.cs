using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using UnityEngine;

// Token: 0x02000123 RID: 291
[CreateAssetMenu(fileName = "NewChallenge", menuName = "Challenges/Challenge")]
public class Challenge : ScriptableObject
{
	// Token: 0x06000BC5 RID: 3013 RVA: 0x0002FFE0 File Offset: 0x0002E1E0
	public HashSet<CasinoGameType> GetRequiredGameTypes()
	{
		HashSet<CasinoGameType> hashSet = new HashSet<CasinoGameType>();
		if (this.conditions == null)
		{
			return hashSet;
		}
		foreach (ChallengeConditionData challengeConditionData in this.conditions)
		{
			if (challengeConditionData != null)
			{
				GameTypeConditionData gameTypeConditionData = challengeConditionData as GameTypeConditionData;
				if (gameTypeConditionData != null)
				{
					hashSet.Add(gameTypeConditionData.requiredGameType);
				}
				WinCountConditionData winCountConditionData = challengeConditionData as WinCountConditionData;
				if (winCountConditionData != null && winCountConditionData.useSpecificGameType)
				{
					hashSet.Add(winCountConditionData.specificGameType);
				}
				LossCountConditionData lossCountConditionData = challengeConditionData as LossCountConditionData;
				if (lossCountConditionData != null && lossCountConditionData.useSpecificGameType)
				{
					hashSet.Add(lossCountConditionData.specificGameType);
				}
				if (challengeConditionData is BlackjackHandValueConditionData)
				{
					hashSet.Add(CasinoGameType.Blackjack);
				}
				if (challengeConditionData is CrapsSequenceConditionData)
				{
					hashSet.Add(CasinoGameType.Craps);
				}
				if (challengeConditionData is DuckRaceWinConditionData)
				{
					hashSet.Add(CasinoGameType.DuckRace);
				}
				if (challengeConditionData is PokerHandConditionData)
				{
					hashSet.Add(CasinoGameType.Poker);
				}
			}
		}
		return hashSet;
	}

	// Token: 0x06000BC6 RID: 3014 RVA: 0x000300E8 File Offset: 0x0002E2E8
	public bool IsCompleted(ChallengeContext context)
	{
		if (this.conditions == null || this.conditions.Count == 0)
		{
			return false;
		}
		List<ChallengeConditionData> list = (from c in this.conditions
		where c != null && !c.useAsResultFilter
		select c).ToList<ChallengeConditionData>();
		if (list.Count == 0)
		{
			list = (from c in this.conditions
			where c != null
			select c).ToList<ChallengeConditionData>();
		}
		if (this.requireAllSimultaneously)
		{
			using (List<ChallengeConditionData>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.Evaluate(context))
					{
						return false;
					}
				}
			}
			return true;
		}
		using (List<ChallengeConditionData>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Evaluate(context))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000BC7 RID: 3015 RVA: 0x00030208 File Offset: 0x0002E408
	public float GetProgress(ChallengeContext context)
	{
		if (this.conditions == null || this.conditions.Count == 0)
		{
			return 0f;
		}
		List<ChallengeConditionData> list = (from c in this.conditions
		where c != null && !c.useAsResultFilter
		select c).ToList<ChallengeConditionData>();
		if (list.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		int num2 = 0;
		foreach (ChallengeConditionData challengeConditionData in list)
		{
			if (challengeConditionData != null)
			{
				num += challengeConditionData.GetProgress(context);
				num2++;
			}
		}
		if (num2 <= 0)
		{
			return 0f;
		}
		return num / (float)num2;
	}

	// Token: 0x06000BC8 RID: 3016 RVA: 0x000302D4 File Offset: 0x0002E4D4
	public string GetProgressText(ChallengeContext context)
	{
		if (this.conditions == null || this.conditions.Count == 0)
		{
			return "No conditions";
		}
		List<ChallengeConditionData> list = (from c in this.conditions
		where c != null && !c.useAsResultFilter
		select c).ToList<ChallengeConditionData>();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		List<string> list2 = new List<string>();
		foreach (ChallengeConditionData challengeConditionData in list)
		{
			if (challengeConditionData != null)
			{
				list2.Add(challengeConditionData.GetProgressText(context));
			}
		}
		return string.Join(" | ", list2);
	}

	// Token: 0x06000BC9 RID: 3017 RVA: 0x00030398 File Offset: 0x0002E598
	public void ResetChallenge()
	{
		if (this.conditions == null)
		{
			return;
		}
		foreach (ChallengeConditionData challengeConditionData in this.conditions)
		{
			if (challengeConditionData != null)
			{
				challengeConditionData.ResetCondition();
			}
		}
	}

	// Token: 0x06000BCA RID: 3018 RVA: 0x000303F8 File Offset: 0x0002E5F8
	public void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position, ChallengeContext context = null)
	{
		if (this.conditions == null)
		{
			return;
		}
		if (context == null)
		{
			long quotaAtActivation = 0L;
			if (NetworkSingleton<ChallengeManager>.Instance != null)
			{
				ChallengeProgress challengeProgress = NetworkSingleton<ChallengeManager>.Instance.GetChallengeProgress(this);
				if (challengeProgress != null)
				{
					quotaAtActivation = challengeProgress.quotaAtActivation;
				}
			}
			context = new ChallengeContext
			{
				bet = bet,
				payout = payout,
				gameType = gameType,
				gamePosition = position,
				quotaAtActivation = quotaAtActivation,
				hadTipsyFortuneBuff = false,
				hadInspiringMelodyBuff = false,
				hadImmunityBuff = false
			};
		}
		else
		{
			context.bet = bet;
			context.payout = payout;
			context.gameType = gameType;
			context.gamePosition = position;
			if (context.quotaAtActivation == 0L && NetworkSingleton<ChallengeManager>.Instance != null)
			{
				ChallengeProgress challengeProgress2 = NetworkSingleton<ChallengeManager>.Instance.GetChallengeProgress(this);
				if (challengeProgress2 != null)
				{
					context.quotaAtActivation = challengeProgress2.quotaAtActivation;
				}
			}
		}
		foreach (ChallengeConditionData challengeConditionData in this.conditions)
		{
			if (challengeConditionData != null)
			{
				challengeConditionData.OnGameResult(bet, payout, gameType, position);
				if (!challengeConditionData.Evaluate(context))
				{
					break;
				}
			}
		}
	}

	// Token: 0x06000BCB RID: 3019 RVA: 0x0003052C File Offset: 0x0002E72C
	public int GetTicketReward()
	{
		if (this.manualTicketReward >= 0)
		{
			return this.manualTicketReward;
		}
		ChallengeSettings challengeSettings = this.GetChallengeSettings();
		if (challengeSettings != null)
		{
			return challengeSettings.GetTicketReward(this.floorIndex);
		}
		return 0;
	}

	// Token: 0x06000BCC RID: 3020 RVA: 0x00030568 File Offset: 0x0002E768
	public bool ShouldShowProgress()
	{
		if (this.conditions == null || this.conditions.Count == 0)
		{
			return false;
		}
		foreach (ChallengeConditionData challengeConditionData in this.conditions)
		{
			if (challengeConditionData != null && !challengeConditionData.useAsResultFilter)
			{
				if (challengeConditionData is WinCountConditionData)
				{
					return true;
				}
				if (challengeConditionData is LossCountConditionData)
				{
					return true;
				}
				PayoutAmountConditionData payoutAmountConditionData = challengeConditionData as PayoutAmountConditionData;
				if (payoutAmountConditionData != null && payoutAmountConditionData.checkTotalPayout)
				{
					return true;
				}
				ProfitConditionData profitConditionData = challengeConditionData as ProfitConditionData;
				if (profitConditionData != null && profitConditionData.checkTotalProfit)
				{
					return true;
				}
				BetAmountConditionData betAmountConditionData = challengeConditionData as BetAmountConditionData;
				if (betAmountConditionData != null && betAmountConditionData.checkTotalBet)
				{
					return true;
				}
				if (challengeConditionData is CrapsSequenceConditionData)
				{
					return true;
				}
				if (challengeConditionData is TimeConditionData)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000BCD RID: 3021 RVA: 0x00030668 File Offset: 0x0002E868
	public string GetProcessedDescription()
	{
		if (string.IsNullOrEmpty(this.description))
		{
			return this.description;
		}
		if (this.conditions == null)
		{
			return this.description;
		}
		string text = this.description;
		foreach (ChallengeConditionData challengeConditionData in this.conditions)
		{
			if (challengeConditionData != null)
			{
				long num = 0L;
				if (NetworkSingleton<ChallengeManager>.Instance != null)
				{
					ChallengeProgress challengeProgress = NetworkSingleton<ChallengeManager>.Instance.GetChallengeProgress(this);
					if (challengeProgress != null && challengeProgress.quotaAtActivation > 0L)
					{
						num = challengeProgress.quotaAtActivation;
					}
				}
				if (num == 0L && NetworkSingleton<GameManager>.Instance != null)
				{
					num = NetworkSingleton<GameManager>.Instance.currentQuota;
				}
				BetAmountConditionData betAmountConditionData = challengeConditionData as BetAmountConditionData;
				if (betAmountConditionData != null)
				{
					long minBetAmount = betAmountConditionData.GetMinBetAmount(num);
					long maxBetAmount = betAmountConditionData.GetMaxBetAmount(num);
					text = text.Replace("[minAmount]", MoneyFormatter.FormatWithDollar(minBetAmount));
					text = text.Replace("[maxAmount]", (maxBetAmount > 0L) ? MoneyFormatter.FormatWithDollar(maxBetAmount) : "");
				}
				else
				{
					ProfitConditionData profitConditionData = challengeConditionData as ProfitConditionData;
					if (profitConditionData != null)
					{
						long minProfit = profitConditionData.GetMinProfit(num);
						text = text.Replace("[minAmount]", MoneyFormatter.FormatWithDollar(minProfit));
					}
				}
			}
		}
		return text;
	}

	// Token: 0x06000BCE RID: 3022 RVA: 0x000307B8 File Offset: 0x0002E9B8
	private ChallengeSettings GetChallengeSettings()
	{
		if (NetworkSingleton<ChallengeManager>.Instance != null)
		{
			FieldInfo field = typeof(ChallengeManager).GetField("challengeSettings", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				ChallengeSettings challengeSettings = field.GetValue(NetworkSingleton<ChallengeManager>.Instance) as ChallengeSettings;
				if (challengeSettings != null)
				{
					return challengeSettings;
				}
			}
		}
		return Resources.FindObjectsOfTypeAll<ChallengeSettings>().FirstOrDefault<ChallengeSettings>();
	}

	// Token: 0x04000757 RID: 1879
	[UniqueID("challenges")]
	public int challengeID;

	// Token: 0x04000758 RID: 1880
	[Header("Challenge Info")]
	[Tooltip("Display name of the challenge")]
	public string challengeName;

	// Token: 0x04000759 RID: 1881
	[Header("Steam Achievement")]
	[Tooltip("Optional linked Steam Achievement. If set, this achievement will be unlocked when the challenge is completed.")]
	public SteamAchievement_SteamworksNET linkedAchievement;

	// Token: 0x0400075A RID: 1882
	[Tooltip("Description of what the player needs to do")]
	[TextArea(3, 6)]
	public string description;

	// Token: 0x0400075B RID: 1883
	[Header("Difficulty & Rewards")]
	public int floorIndex;

	// Token: 0x0400075C RID: 1884
	[Tooltip("Manual ticket reward for this challenge. Set to -1 to use floor-based reward from ChallengeSettings.")]
	public int manualTicketReward = -1;

	// Token: 0x0400075D RID: 1885
	[Header("Conditions")]
	[Tooltip("All conditions that must be met for this challenge to complete. Click the dropdown to add a condition type.")]
	[SerializeReference]
	public List<ChallengeConditionData> conditions = new List<ChallengeConditionData>();

	// Token: 0x0400075E RID: 1886
	[Header("Settings")]
	[Tooltip("Whether all conditions must be met simultaneously or sequentially")]
	public bool requireAllSimultaneously = true;

	// Token: 0x0400075F RID: 1887
	[Tooltip("Whether the challenge can be completed multiple times")]
	public bool repeatable;
}
