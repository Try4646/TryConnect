using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

// Token: 0x02000137 RID: 311
[Serializable]
public class CrapsSequenceConditionData : ChallengeConditionData
{
	// Token: 0x06000C4C RID: 3148 RVA: 0x0003302C File Offset: 0x0003122C
	public override float GetProgress(ChallengeContext context)
	{
		if (context.gameType != CasinoGameType.Craps)
		{
			return 0f;
		}
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)conditionState.GetCustomInt("sequenceProgress", 0) / (float)this.requiredSequence.Count);
	}

	// Token: 0x06000C4D RID: 3149 RVA: 0x00033084 File Offset: 0x00031284
	public override string GetProgressText(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return "0/0";
		}
		int customInt = conditionState.GetCustomInt("sequenceProgress", 0);
		string arg = string.Join<int>(" → ", this.requiredSequence);
		return string.Format("{0}/{1} ({2})", customInt, this.requiredSequence.Count, arg);
	}

	// Token: 0x06000C4E RID: 3150 RVA: 0x000048A7 File Offset: 0x00002AA7
	public override void OnGameResult(long bet, long payout, CasinoGameType gameType, Vector3 position)
	{
	}

	// Token: 0x06000C4F RID: 3151 RVA: 0x000330EC File Offset: 0x000312EC
	public override bool Evaluate(ChallengeContext context)
	{
		ChallengeManager instance = NetworkSingleton<ChallengeManager>.Instance;
		ConditionState conditionState = (instance != null) ? instance.GetConditionState(this) : null;
		if (conditionState == null)
		{
			return false;
		}
		int num = conditionState.GetCustomInt("sequenceProgress", 0);
		if (context.gameType != CasinoGameType.Craps)
		{
			return num >= this.requiredSequence.Count;
		}
		int gameData = context.GetGameData<int>("lastDiceRoll", -1);
		if (gameData == -1)
		{
			return num >= this.requiredSequence.Count;
		}
		if (num < this.requiredSequence.Count && gameData == this.requiredSequence[num])
		{
			num++;
			conditionState.SetCustomInt("sequenceProgress", num);
			if (num >= this.requiredSequence.Count)
			{
				conditionState.currentWinCount++;
			}
		}
		else if (this.mustBeInSingleGame && num > 0)
		{
			conditionState.SetCustomInt("sequenceProgress", 0);
			num = 0;
		}
		return num >= this.requiredSequence.Count;
	}

	// Token: 0x040007C2 RID: 1986
	[Header("Craps Sequence Settings")]
	[Tooltip("The sequence of dice values that must be rolled in order")]
	public List<int> requiredSequence = new List<int>
	{
		6,
		7
	};

	// Token: 0x040007C3 RID: 1987
	[Tooltip("Whether the sequence must be completed in a single game")]
	public bool mustBeInSingleGame;
}
