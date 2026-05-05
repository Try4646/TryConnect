using System;
using Extensions;
using UnityEngine;

// Token: 0x02000141 RID: 321
[Serializable]
public class WinCountConditionData : ChallengeConditionData
{
	// Token: 0x06000C84 RID: 3204 RVA: 0x000341A0 File Offset: 0x000323A0
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
			return conditionState.consecutiveWinCount >= this.requiredWins;
		}
		return conditionState.currentWinCount >= this.requiredWins;
	}

	// Token: 0x06000C85 RID: 3205 RVA: 0x000341F0 File Offset: 0x000323F0
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
			return Mathf.Clamp01((float)conditionState.consecutiveWinCount / (float)this.requiredWins);
		}
		return Mathf.Clamp01((float)conditionState.currentWinCount / (float)this.requiredWins);
	}

	// Token: 0x06000C86 RID: 3206 RVA: 0x0003424C File Offset: 0x0003244C
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "0/0 wins";
		}
		if (this.consecutive)
		{
			return string.Format("{0}/{1} consecutive wins", conditionState.consecutiveWinCount, this.requiredWins);
		}
		return string.Format("{0}/{1} wins", conditionState.currentWinCount, this.requiredWins);
	}

	// Token: 0x06000C87 RID: 3207 RVA: 0x000342C0 File Offset: 0x000324C0
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
		if (payout > bet)
		{
			conditionState.currentWinCount++;
			conditionState.consecutiveWinCount++;
			return;
		}
		if (this.consecutive && payout < bet)
		{
			conditionState.consecutiveWinCount = 0;
		}
	}

	// Token: 0x06000C88 RID: 3208 RVA: 0x00034330 File Offset: 0x00032530
	public override void ResetCondition()
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState != null)
		{
			conditionState.currentWinCount = 0;
			conditionState.consecutiveWinCount = 0;
		}
	}

	// Token: 0x040007D9 RID: 2009
	[Header("Win Count Settings")]
	[Tooltip("The number of wins required")]
	public int requiredWins = 1;

	// Token: 0x040007DA RID: 2010
	[Tooltip("Whether wins must be consecutive")]
	public bool consecutive;

	// Token: 0x040007DB RID: 2011
	[Tooltip("Optional: Only count wins from a specific game type (None = all games)")]
	public CasinoGameType specificGameType;

	// Token: 0x040007DC RID: 2012
	[Tooltip("Whether to filter by specific game type")]
	public bool useSpecificGameType;
}
