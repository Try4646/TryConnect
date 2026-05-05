using System;
using Extensions;
using UnityEngine;

// Token: 0x0200013A RID: 314
[Serializable]
public class LossCountConditionData : ChallengeConditionData
{
	// Token: 0x06000C5A RID: 3162 RVA: 0x0003346C File Offset: 0x0003166C
	public override bool Evaluate(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return false;
		}
		if (this.consecutive)
		{
			return conditionState.consecutiveLossCount >= this.requiredLosses;
		}
		return conditionState.currentLossCount >= this.requiredLosses;
	}

	// Token: 0x06000C5B RID: 3163 RVA: 0x000334BC File Offset: 0x000316BC
	public override float GetProgress(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		if (this.consecutive)
		{
			return Mathf.Clamp01((float)conditionState.consecutiveLossCount / (float)this.requiredLosses);
		}
		return Mathf.Clamp01((float)conditionState.currentLossCount / (float)this.requiredLosses);
	}

	// Token: 0x06000C5C RID: 3164 RVA: 0x00033518 File Offset: 0x00031718
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "0/0 losses";
		}
		if (this.consecutive)
		{
			return string.Format("{0}/{1} consecutive losses", conditionState.consecutiveLossCount, this.requiredLosses);
		}
		return string.Format("{0}/{1} losses", conditionState.currentLossCount, this.requiredLosses);
	}

	// Token: 0x06000C5D RID: 3165 RVA: 0x0003358C File Offset: 0x0003178C
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return;
		}
		if (this.useSpecificGameType && gameType != this.specificGameType)
		{
			return;
		}
		if (payout < bet)
		{
			conditionState.currentLossCount++;
			conditionState.consecutiveLossCount++;
			return;
		}
		if (this.consecutive && payout > bet)
		{
			conditionState.consecutiveLossCount = 0;
		}
	}

	// Token: 0x06000C5E RID: 3166 RVA: 0x000335FC File Offset: 0x000317FC
	public override void ResetCondition()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState != null)
		{
			conditionState.currentLossCount = 0;
			conditionState.consecutiveLossCount = 0;
		}
	}

	// Token: 0x040007C7 RID: 1991
	[Header("Loss Count Settings")]
	[Tooltip("The number of losses required")]
	public int requiredLosses = 1;

	// Token: 0x040007C8 RID: 1992
	[Tooltip("Whether losses must be consecutive")]
	public bool consecutive;

	// Token: 0x040007C9 RID: 1993
	[Tooltip("Optional: Only count losses from a specific game type (None = all games)")]
	public CasinoGameType specificGameType;

	// Token: 0x040007CA RID: 1994
	[Tooltip("Whether to filter by specific game type")]
	public bool useSpecificGameType;
}
