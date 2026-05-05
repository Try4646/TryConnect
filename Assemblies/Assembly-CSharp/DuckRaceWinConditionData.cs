using System;
using Extensions;
using UnityEngine;

// Token: 0x02000138 RID: 312
[Serializable]
public class DuckRaceWinConditionData : ChallengeConditionData
{
	// Token: 0x06000C51 RID: 3153 RVA: 0x000331F4 File Offset: 0x000313F4
	public override bool Evaluate(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.DuckRace)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			return conditionState != null && conditionState.currentWinCount >= 1;
		}
		if (context.payout <= context.bet)
		{
			ChallengeManager instance2 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState2 = (instance2 != null) ? instance2.GetConditionState(this) : null;
			return conditionState2 != null && conditionState2.currentWinCount >= 1;
		}
		int gameData = context.GetGameData<int>("winningDuckIndex", -1);
		int gameData2 = context.GetGameData<int>("betDuckIndex", -1);
		if (gameData == -1)
		{
			ChallengeManager instance3 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState3 = (instance3 != null) ? instance3.GetConditionState(this) : null;
			return conditionState3 != null && conditionState3.currentWinCount >= 1;
		}
		bool flag = gameData == this.requiredDuckIndex;
		if (flag && this.requireBetOnDuck)
		{
			flag = (gameData2 == this.requiredDuckIndex);
		}
		if (flag)
		{
			ChallengeManager instance4 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState4 = (instance4 != null) ? instance4.GetConditionState(this) : null;
			if (conditionState4 != null && conditionState4.currentWinCount == 0)
			{
				conditionState4.currentWinCount = 1;
			}
		}
		ChallengeManager instance5 = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState5 = (instance5 != null) ? instance5.GetConditionState(this) : null;
		return conditionState5 != null && conditionState5.currentWinCount >= 1;
	}

	// Token: 0x06000C52 RID: 3154 RVA: 0x00033320 File Offset: 0x00031520
	public override float GetProgress(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.DuckRace)
		{
			return 0f;
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)conditionState.currentWinCount / 1f);
	}

	// Token: 0x06000C53 RID: 3155 RVA: 0x0003336C File Offset: 0x0003156C
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "0/1 wins";
		}
		if (conditionState.currentWinCount < 1)
		{
			return string.Format("Win with duck #{0}", this.requiredDuckIndex + 1);
		}
		return "Complete";
	}

	// Token: 0x06000C54 RID: 3156 RVA: 0x000333BB File Offset: 0x000315BB
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		if (gameType != CasinoGameType.DuckRace)
		{
			return;
		}
		if (payout <= bet)
		{
			return;
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		if (instance != null)
		{
			instance.GetConditionState(this);
		}
	}

	// Token: 0x040007C4 RID: 1988
	[Header("Duck Race Settings")]
	[Tooltip("The duck index (0-based) that must win. 0 = first duck, 1 = second duck, etc.")]
	public int requiredDuckIndex;

	// Token: 0x040007C5 RID: 1989
	[Tooltip("Whether the player must have bet on this duck")]
	public bool requireBetOnDuck = true;
}
