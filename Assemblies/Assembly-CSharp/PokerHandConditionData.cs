using System;
using Extensions;
using UnityEngine;

// Token: 0x0200013D RID: 317
[Serializable]
public class PokerHandConditionData : ChallengeConditionData
{
	// Token: 0x06000C6C RID: 3180 RVA: 0x00033964 File Offset: 0x00031B64
	public override bool Evaluate(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.Poker)
		{
			ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
			return conditionState != null && conditionState.currentWinCount >= 1;
		}
		if (this.requireWin && context.payout <= context.bet)
		{
			ChallengeManager instance2 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState2 = (instance2 != null) ? instance2.GetConditionState(this) : null;
			return conditionState2 != null && conditionState2.currentWinCount >= 1;
		}
		int gameData = context.GetGameData<int>("handRank", -1);
		if (gameData == -1)
		{
			ChallengeManager instance3 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState3 = (instance3 != null) ? instance3.GetConditionState(this) : null;
			return conditionState3 != null && conditionState3.currentWinCount >= 1;
		}
		if (gameData < this.minHandRank)
		{
			ChallengeManager instance4 = NetworkSingleton<ChallengeManager>.Instance;
			ConditionState conditionState4 = (instance4 != null) ? instance4.GetConditionState(this) : null;
			return conditionState4 != null && conditionState4.currentWinCount >= 1;
		}
		if (this.requireNoLockedCards)
		{
			int gameData2 = context.GetGameData<int>("lockedCardsCount", -1);
			if (gameData2 == -1 || gameData2 > 0)
			{
				ChallengeManager instance5 = NetworkSingleton<ChallengeManager>.Instance;
				ConditionState conditionState5 = (instance5 != null) ? instance5.GetConditionState(this) : null;
				return conditionState5 != null && conditionState5.currentWinCount >= 1;
			}
		}
		ChallengeManager instance6 = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState6 = (instance6 != null) ? instance6.GetConditionState(this) : null;
		if (conditionState6 != null && conditionState6.currentWinCount == 0)
		{
			conditionState6.currentWinCount = 1;
		}
		return conditionState6 != null && conditionState6.currentWinCount >= 1;
	}

	// Token: 0x06000C6D RID: 3181 RVA: 0x00033AC4 File Offset: 0x00031CC4
	public override float GetProgress(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.Poker)
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

	// Token: 0x06000C6E RID: 3182 RVA: 0x00033B10 File Offset: 0x00031D10
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "0/1";
		}
		string handRankName = this.GetHandRankName(this.minHandRank);
		string str = this.requireNoLockedCards ? " without locking cards" : "";
		string str2 = this.requireWin ? " and win" : "";
		if (conditionState.currentWinCount < 1)
		{
			return "Get " + handRankName + str + str2;
		}
		return "Complete";
	}

	// Token: 0x06000C6F RID: 3183 RVA: 0x00033B8C File Offset: 0x00031D8C
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
		if (gameType != CasinoGameType.Poker)
		{
			return;
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		if (instance != null)
		{
			instance.GetConditionState(this);
		}
	}

	// Token: 0x06000C70 RID: 3184 RVA: 0x00033BA8 File Offset: 0x00031DA8
	private string GetHandRankName(int rank)
	{
		string result;
		switch (rank)
		{
		case 1:
			result = "Pair";
			break;
		case 2:
			result = "Two Pair";
			break;
		case 3:
			result = "Three of a Kind";
			break;
		case 4:
			result = "Straight";
			break;
		case 5:
			result = "Flush";
			break;
		case 6:
			result = "Full House";
			break;
		case 7:
			result = "Four of a Kind";
			break;
		case 8:
			result = "Straight Flush";
			break;
		default:
			result = string.Format("Rank {0}", rank);
			break;
		}
		return result;
	}

	// Token: 0x040007D0 RID: 2000
	[Header("Poker Hand Settings")]
	[Tooltip("Minimum hand rank required (1=Pair, 2=TwoPair, 3=ThreeOfAKind, 4=Straight, 5=Flush, 6=FullHouse, 7=FourOfAKind, 8=StraightFlush)")]
	public int minHandRank = 3;

	// Token: 0x040007D1 RID: 2001
	[Tooltip("Whether cards must not be locked (cardsToKeep must be empty)")]
	public bool requireNoLockedCards = true;

	// Token: 0x040007D2 RID: 2002
	[Tooltip("Whether the player must win (payout > bet)")]
	public bool requireWin;
}
