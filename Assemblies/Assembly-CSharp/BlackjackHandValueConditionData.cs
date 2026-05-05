using System;
using Extensions;
using UnityEngine;

// Token: 0x02000136 RID: 310
[Serializable]
public class BlackjackHandValueConditionData : ChallengeConditionData
{
	// Token: 0x06000C47 RID: 3143 RVA: 0x00032DE0 File Offset: 0x00030FE0
	public override float GetProgress(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.Blackjack)
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

	// Token: 0x06000C48 RID: 3144 RVA: 0x00032E2C File Offset: 0x0003102C
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "0/1 wins";
		}
		if (conditionState.currentWinCount >= 1)
		{
			return "Complete";
		}
		string result = "Win with hand value";
		if (this.minHandValue > 0 && this.maxHandValue > 0)
		{
			result = string.Format("Win with hand value between {0} and {1}", this.minHandValue, this.maxHandValue);
		}
		else if (this.minHandValue > 0)
		{
			result = string.Format("Win with hand value >= {0}", this.minHandValue);
		}
		else if (this.maxHandValue > 0)
		{
			result = string.Format("Win with hand value < {0}", this.maxHandValue);
		}
		return result;
	}

	// Token: 0x06000C49 RID: 3145 RVA: 0x000048A7 File Offset: 0x00002AA7
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
	}

	// Token: 0x06000C4A RID: 3146 RVA: 0x00032EE0 File Offset: 0x000310E0
	public override bool Evaluate(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.Blackjack)
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
		int gameData = context.GetGameData<int>(this.checkPlayerHand ? "playerHandValue" : "dealerHandValue", -1);
		if (gameData == -1)
		{
			ChallengeManager instance3 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState3 = (instance3 != null) ? instance3.GetConditionState(this) : null;
			return conditionState3 != null && conditionState3.currentWinCount >= 1;
		}
		bool flag = this.minHandValue == 0 || gameData >= this.minHandValue;
		bool flag2 = this.maxHandValue == 0 || gameData < this.maxHandValue;
		if (flag && flag2)
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

	// Token: 0x040007BF RID: 1983
	[Header("Blackjack Hand Value Settings")]
	[Tooltip("Minimum hand value required (inclusive). Set to 0 to ignore.")]
	public int minHandValue;

	// Token: 0x040007C0 RID: 1984
	[Tooltip("Maximum hand value allowed (exclusive). Set to 0 to ignore.")]
	public int maxHandValue = 10;

	// Token: 0x040007C1 RID: 1985
	[Tooltip("Whether to check player hand or dealer hand")]
	public bool checkPlayerHand = true;
}
