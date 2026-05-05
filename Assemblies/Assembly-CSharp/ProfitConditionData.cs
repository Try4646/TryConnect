using System;
using Extensions;
using UnityEngine;

// Token: 0x0200013E RID: 318
[Serializable]
public class ProfitConditionData : ChallengeConditionData
{
	// Token: 0x06000C72 RID: 3186 RVA: 0x00033C47 File Offset: 0x00031E47
	public long GetMinProfit(long quota = 0L)
	{
		if (quota == 0L)
		{
			if (NetworkSingleton<GameManager>.Instance == null)
			{
				return 0L;
			}
			quota = NetworkSingleton<GameManager>.Instance.currentQuota;
		}
		return FathF.RoundByFirstNDigits((long)((float)quota * this.minProfitMultiplier), 2);
	}

	// Token: 0x06000C73 RID: 3187 RVA: 0x00033C78 File Offset: 0x00031E78
	public override bool Evaluate(ChallengeContext context)
	{
		long quota = (context.quotaAtActivation > 0L) ? context.quotaAtActivation : ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		long minProfit = this.GetMinProfit(quota);
		if (this.checkTotalProfit)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			return conditionState != null && conditionState.totalProfit >= minProfit;
		}
		return context.profit >= minProfit;
	}

	// Token: 0x06000C74 RID: 3188 RVA: 0x00033CF4 File Offset: 0x00031EF4
	public override float GetProgress(ChallengeContext context)
	{
		long quota = (context.quotaAtActivation > 0L) ? context.quotaAtActivation : ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		long minProfit = this.GetMinProfit(quota);
		if (minProfit <= 0L)
		{
			return 0f;
		}
		if (!this.checkTotalProfit)
		{
			return Mathf.Clamp01((float)context.profit / (float)minProfit);
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)conditionState.totalProfit / (float)minProfit);
	}

	// Token: 0x06000C75 RID: 3189 RVA: 0x00033D88 File Offset: 0x00031F88
	public override string GetProgressText(ChallengeContext context)
	{
		long quota = (context.quotaAtActivation > 0L) ? context.quotaAtActivation : ((NetworkSingleton<GameManager>.Instance != null) ? NetworkSingleton<GameManager>.Instance.currentQuota : 0L);
		long minProfit = this.GetMinProfit(quota);
		if (this.checkTotalProfit)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState == null)
			{
				return string.Format("Profit 0/{0}", minProfit);
			}
			return string.Format("Profit {0}/{1}", conditionState.totalProfit, minProfit);
		}
		else
		{
			if (context.profit >= minProfit)
			{
				return string.Format("Profit {0} (✓)", context.profit);
			}
			return string.Format("Profit {0}/{1}", context.profit, minProfit);
		}
	}

	// Token: 0x06000C76 RID: 3190 RVA: 0x00033E50 File Offset: 0x00032050
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		if (this.checkTotalProfit)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			if (conditionState != null)
			{
				conditionState.totalProfit += payout - bet;
			}
		}
	}

	// Token: 0x06000C77 RID: 3191 RVA: 0x00033E8C File Offset: 0x0003208C
	public override void ResetCondition()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState != null)
		{
			conditionState.totalProfit = 0L;
		}
	}

	// Token: 0x040007D3 RID: 2003
	[Header("Profit Settings")]
	[Tooltip("The minimum profit multiplier (quota * this value). Enter 0.2 for 20% of quota.")]
	public float minProfitMultiplier = 0.1f;

	// Token: 0x040007D4 RID: 2004
	[Tooltip("Whether to check total profit or single game profit")]
	public bool checkTotalProfit = true;
}
