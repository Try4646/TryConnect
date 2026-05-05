using System;
using Extensions;
using UnityEngine;

// Token: 0x02000135 RID: 309
[Serializable]
public class BetAmountConditionData : ChallengeConditionData
{
	// Token: 0x06000C3F RID: 3135 RVA: 0x00032AE4 File Offset: 0x00030CE4
	public long GetMinBetAmount(long quota = 0L)
	{
		if (quota == 0L)
		{
			if (NetworkSingleton<GameManager>.Instance == null)
			{
				return 0L;
			}
			quota = NetworkSingleton<GameManager>.Instance.currentQuota;
		}
		return FathF.RoundByFirstNDigits((long)((float)quota * this.minBetMultiplier), 2);
	}

	// Token: 0x06000C40 RID: 3136 RVA: 0x00032B18 File Offset: 0x00030D18
	public long GetMaxBetAmount(long quota = 0L)
	{
		if (this.maxBetMultiplier <= 0f)
		{
			return 0L;
		}
		if (quota == 0L)
		{
			if (NetworkSingleton<GameManager>.Instance == null)
			{
				return 0L;
			}
			quota = NetworkSingleton<GameManager>.Instance.currentQuota;
		}
		return FathF.RoundByFirstNDigits((long)((float)quota * this.maxBetMultiplier), 2);
	}

	// Token: 0x06000C41 RID: 3137 RVA: 0x00032B64 File Offset: 0x00030D64
	public override bool Evaluate(ChallengeContext context)
	{
		long quota = (context.quotaAtActivation > 0L) ? context.quotaAtActivation : ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		long minBetAmount = this.GetMinBetAmount(quota);
		long maxBetAmount = this.GetMaxBetAmount(quota);
		if (this.checkTotalBet)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			return conditionState != null && conditionState.totalBetAmount >= minBetAmount && (maxBetAmount == 0L || conditionState.totalBetAmount <= maxBetAmount);
		}
		return context.bet >= minBetAmount && (maxBetAmount == 0L || context.bet <= maxBetAmount);
	}

	// Token: 0x06000C42 RID: 3138 RVA: 0x00032C08 File Offset: 0x00030E08
	public override float GetProgress(ChallengeContext context)
	{
		long quota = (context.quotaAtActivation > 0L) ? context.quotaAtActivation : ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		long minBetAmount = this.GetMinBetAmount(quota);
		if (minBetAmount <= 0L)
		{
			return 0f;
		}
		if (!this.checkTotalBet)
		{
			return Mathf.Clamp01((float)context.bet / (float)minBetAmount);
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)conditionState.totalBetAmount / (float)minBetAmount);
	}

	// Token: 0x06000C43 RID: 3139 RVA: 0x00032C9C File Offset: 0x00030E9C
	public override string GetProgressText(ChallengeContext context)
	{
		long quota = (context.quotaAtActivation > 0L) ? context.quotaAtActivation : ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		long minBetAmount = this.GetMinBetAmount(quota);
		if (this.checkTotalBet)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState == null)
			{
				return string.Format("Bet 0/{0} total", minBetAmount);
			}
			return string.Format("Bet {0}/{1} total", conditionState.totalBetAmount, minBetAmount);
		}
		else
		{
			if (context.bet >= minBetAmount)
			{
				return string.Format("Bet {0} (✓)", context.bet);
			}
			return string.Format("Bet {0}/{1}", context.bet, minBetAmount);
		}
	}

	// Token: 0x06000C44 RID: 3140 RVA: 0x00032D64 File Offset: 0x00030F64
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		if (this.checkTotalBet)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState != null)
			{
				conditionState.totalBetAmount += bet;
			}
		}
	}

	// Token: 0x06000C45 RID: 3141 RVA: 0x00032DA0 File Offset: 0x00030FA0
	public override void ResetCondition()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState != null)
		{
			conditionState.totalBetAmount = 0L;
		}
	}

	// Token: 0x040007BC RID: 1980
	[Header("Bet Amount Settings")]
	[Tooltip("The minimum bet amount multiplier (quota * this value). Enter 0.2 for 20% of quota.")]
	public float minBetMultiplier = 0.1f;

	// Token: 0x040007BD RID: 1981
	[Tooltip("The maximum bet amount multiplier (0 = no maximum). Enter 0.5 for 50% of quota.")]
	public float maxBetMultiplier;

	// Token: 0x040007BE RID: 1982
	[Tooltip("Whether to check total bet amount or single bet")]
	public bool checkTotalBet;
}
